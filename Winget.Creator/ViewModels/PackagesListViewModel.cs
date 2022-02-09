using Downloader;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Core;
using Winget.Creator.Database;
using Winget.Creator.Helpers;
using Winget.Creator.Models;
using Winget.Pusher.Core.Models;
using Winget.Pusher.Core.Models.Database;

namespace Winget.Creator.ViewModels
{
    public class PackagesListViewModel : ObservableRecipient
    {
        private string loadingStatus;
        private int loadingValue;
        private Visibility isLoading;

        private ICommand updatePackageCommand;
        private ICommand importExistingCmd;

        private ObservableCollection<HWGPackageModel> availablePackages = new ObservableCollection<HWGPackageModel>();
        private HWGPackageModel selectedPackage;

        public string LoadingStatus
        {
            get { return loadingStatus; }
            set { SetProperty(ref loadingStatus, value); }
        }
        public int LoadingValue
        {
            get { return loadingValue; }
            set { SetProperty(ref loadingValue, value); }
        }

        public Visibility IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        public ICommand UpdatePackageCommand => updatePackageCommand ?? (updatePackageCommand = new RelayCommand(OnUpdatePackage,() => IsLoading != Visibility.Visible));
        public ICommand ImportExistingCmd => importExistingCmd ?? (importExistingCmd = new RelayCommand(OnImportExistingCmd));

        public ObservableCollection<HWGPackageModel> AvailablePackages
        {
            get { return availablePackages; }
            set { SetProperty(ref availablePackages, value); }
        }

        public HWGPackageModel SelectedPackage { get { return selectedPackage; } set { SetProperty(ref selectedPackage, value); } }

        private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public PackagesListViewModel()
        {
            IsLoading = Visibility.Collapsed;
            LoadingValue = 0;

            LoadingStatus = "Loading local database ...";
            AvailablePackages = new ObservableCollection<HWGPackageModel>(LoadDatabaseAsync().Result);
        }

        public void OnImportExistingCmd()
        {
            var navigation = (Application.Current as App).MainWindow as MainWindow;
            navigation.NavigateToPage("Winget.Creator.Views.PackageImport", SelectedPackage.YamlUri);

            //TODO: inject yaml url in import page
        }

        private async void OnUpdatePackage()
        {
            //btnUpdate.IsEnabled = false;
            bool isConnected = ApplicationHelper.IsConnectedToInternet();
            if (isConnected)
            {
                LoadingStatus = "Downloading MSIX Database...";
                //txtMSIXStatus.Text = string.Empty;
                LoadingValue = 0;
                IsLoading = Visibility.Visible;
                //prgMSIX.IsIndeterminate = false;
                var downloader = new DownloadService();
                downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                //TODO: put on setting page
                await downloader.DownloadFileTaskAsync(Consts.MSIXSourceUrl, new DirectoryInfo(ApplicationData.Current.TemporaryFolder.Path));
            }
            else
            {
                //UIHelpers.CreateInfoBar("Network UnAvailable", "Unable to connect to the Internet", panel, InfoBarSeverity.Error);
                //btnUpdate.IsEnabled = true;
            }
        }

        private async void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //TODO : error bar

                //await UIHelpers.RunOnMainThread(() =>
                //{
                //    prgMSIX.IsIndeterminate = false;
                //    prgMSIX.Visibility = Visibility.Collapsed;
                //    btnUpdate.IsEnabled = true;
                //    UIHelpers.CreateInfoBar("Error", e.Error.Message, panel, InfoBarSeverity.Error);
                //});
            }
            else
            {
                var downloadInfo = e.UserState as DownloadPackage;
                if (downloadInfo != null && downloadInfo.FileName != null)
                {
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        LoadingStatus = "Extracting archive ...";
                    });

                    await Task.Run(() =>
                    {
                        try
                        {
                            var tempFolder = ApplicationData.Current.TemporaryFolder.Path;
                            ZipFile.ExtractToDirectory(downloadInfo.FileName, tempFolder, true);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        
                    });

                    dispatcherQueue.TryEnqueue(async () =>
                    {
                        LoadingStatus = "Loading datas ...";
                        AvailablePackages = new ObservableCollection<HWGPackageModel>(await LoadDatabaseAsync());
                        //(await LoadDatabaseAsync()).ToList().ForEach(p => AvailablePackages.Add(p));

                        IsLoading = Visibility.Collapsed;
                    });



                    //await UIHelpers.RunOnMainThread(() =>
                    //{
                    //    txtStatus.Text = "Extracting...";
                    //    prgMSIX.IsIndeterminate = true;
                    //    ZipFile.ExtractToDirectory(downloadInfo.FileName, Consts.MSIXPath, true);
                    //});
                    //await Task.Run(() =>
                    //{
                    //    //TODO: use SQLite DB
                    //    //GenerateDatabaseAsync();

                    //}).ContinueWith(async x =>
                    //{
                    //    await UIHelpers.RunOnMainThread(() =>
                    //    {
                    //        prgMSIX.IsIndeterminate = false;
                    //        prgMSIX.Visibility = Visibility.Collapsed;
                    //        Settings.UpdatedDate = DateTime.Now;
                    //        txtUpdateDate.Text = $"Last Update: {DateTime.Now}";
                    //        btnUpdate.IsEnabled = true;
                    //    });

                    //});
                }
            }
        }

        private async Task<IEnumerable<HWGPackageModel>> LoadDatabaseAsync()
        {
            using (var msixDB = new MSIXContext(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Public\\index.db")))
            {
                using (var db = msixDB.CreateLinqToDbConnection())
                {
                    var pathCte = db.GetCte<PathPartCte>(cte => (
                    from pathPart in msixDB.PathPartsMSIXTable
                    select new PathPartCte
                    {
                        rowid = pathPart.rowid,
                        parent = pathPart.parent,
                        path = pathPart.pathpart
                    }
                )
                .Concat(
                    from pathPart in msixDB.PathPartsMSIXTable
                    from child in cte.Where(child => child.parent == pathPart.rowid)
                    select new PathPartCte
                    {
                        rowid = child.rowid,
                        parent = pathPart.parent,
                        path = pathPart.pathpart + "/" + child.path
                    }
                    )
                );


                    var query =
                    from item in msixDB.IdsMSIXTable
                    from manifest in msixDB.Set<ManifestMSIXTable>().Where(e => e.id == item.rowid)

                    from yml in msixDB.PathPartsMSIXTable.Where(e => e.rowid == manifest.pathpart)
                    from pathPartVersion in msixDB.PathPartsMSIXTable.Where(e => e.rowid == yml.parent)
                    from pathPartAppName in msixDB.PathPartsMSIXTable.Where(e => e.rowid == pathPartVersion.parent)
                    from pathPartPublisher in msixDB.PathPartsMSIXTable.Where(e => e.rowid == pathPartAppName.parent)
                    from pathPart in pathCte.Where(e => e.rowid == pathPartPublisher.parent && e.parent == null)
                    from productMap in msixDB.ProductCodesMapMSIXTable.Where(e => e.manifest == manifest.rowid).DefaultIfEmpty()
                    from prdCode in msixDB.ProductCodesMSIXTable.Where(e => e.rowid == productMap.productcode).DefaultIfEmpty()
                    from version in msixDB.VersionsMSIXTable.Where(e => e.rowid == manifest.version)
                    from names in msixDB.NameTable.Where(e => e.rowid == manifest.name)
                    from publisherMap in msixDB.PublishersMapMSIXTable.Where(e => e.manifest == manifest.rowid)
                    from publisher in msixDB.PublishersMSIXTable.Where(e => e.rowid == publisherMap.norm_publisher)
                    select new ManifestTable
                    {
                        PackageId = item.id,
                        Name = names.name,
                        Publisher = publisher.norm_publisher,
                        ProductCode = prdCode.productcode,
                        YamlUri = $@"{pathPart.path}/{pathPartPublisher.pathpart}/{pathPartAppName.pathpart}/{pathPartVersion.pathpart}/{yml.pathpart}",
                        Version = version.version
                    };

                    var data = await query.ToArrayAsyncLinqToDB();
                    return data
                        .GroupBy(x => x.PackageId)
                        .OrderBy(x => x.Key)
                        .Select(g => new HWGPackageModel
                        {
                            PackageId = g.Key,
                            Name = g.Select(x => x.Name).First(),
                            Publisher = g.Select(x => x.Publisher).First(),
                            YamlUri = g.Select(x => x.YamlUri).First(),
                            ProductCode = g.Select(x => x.ProductCode).First(),
                            PackageVersion = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderByDescending(x => x.Version).First(),
                            Versions = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderByDescending(x => x.Version).ToList()
                        }).OrderBy(p => p.Name);
                }
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                LoadingValue = (int)e.ProgressPercentage;
            });

            //await UIHelpers.RunOnMainThread(() =>
            //{
            //    prgMSIX.Value = value;
            //    txtMSIXStatus.Text = $"Downloading {BytesToMegabytes(e.ReceivedBytesSize)} MB of {BytesToMegabytes(e.TotalBytesToReceive)} MB  -  {value}%";
            //});
        }
    }
}
