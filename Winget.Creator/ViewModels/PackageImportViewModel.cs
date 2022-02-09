using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.WingetCreateCore.Models.Installer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Winget.Creator.Helpers;
using Winget.Creator.Models;
using Winget.Creator.Services;
using YamlDotNet.Serialization;

namespace Winget.Creator.ViewModels
{
    public class PackageImportViewModel : ObservableRecipient
    {
        #region Privates

        private SettingsService SettingsService => SettingsService.Instance;

        private string appName;
        private string publisher;
        private string id;
        private string version;
        private string description;
        private string homePage;
        private string license;
        private string licenseUrl;
        private string hash;

        private string installerSelectedArchitecture;
        private string installerUrl;
        private string installerSha256;

        private string loadingUri;

        private ObservableCollection<Installer> installers = new ObservableCollection<Installer>();

        private ICommand getHasFromWebCmd;
        private ICommand getHasFromLocalCmd;
        private ICommand generateScriptCmd;
        private ICommand loadFromUriCmd;
        private ICommand addInstallerCmd;
        private ICommand removeInstallerCmd;


        private string infoBarTitle;
        private string infoBarMessage;
        private bool infoBarVisible;
        private InfoBarSeverity infoBarSeverity;

        private Installer selectedInstaller;

        #endregion

        #region Publics
        public ObservableCollection<Installer> Installers
        {
            get { return installers; }
            set { SetProperty(ref installers, value); }
        }

        public Installer SelectedInstaller { get { return selectedInstaller; } set { SetProperty(ref selectedInstaller, value); } }

        public string AppName { get { return appName; } set { SetProperty(ref appName, value); } }
        public string Publisher { get { return publisher; } set { SetProperty(ref publisher, value); } }
        public string Id { get { return id; } set { SetProperty(ref id, value); } }
        public string Version { get { return version; } set { SetProperty(ref version, value); } }
        public string Description { get { return description; } set { SetProperty(ref description, value); } }
        public string HomePage { get { return homePage; } set { SetProperty(ref homePage, value); } }
        public string Licence { get { return license; } set { SetProperty(ref license, value); } }
        public string LicenceUrl { get { return licenseUrl; } set { SetProperty(ref licenseUrl, value); } }
        public string Hash { get { return hash; } set { SetProperty(ref hash, value); } }

        public ObservableCollection<string> Architectures { get { return new ObservableCollection<string> { "x64", "x86", "Arm", "Arm64", "Neutral" }; } }
        public string InstallerSelectedArchitecture { get { return installerSelectedArchitecture; } set { SetProperty(ref installerSelectedArchitecture, value); } }
        public string InstallerUrl { get { return installerUrl; } set { SetProperty(ref installerUrl, value); } }
        public string InstallerSha256 { get { return installerSha256; } set { SetProperty(ref installerSha256, value); } }

        public string LoadingUri { get { return loadingUri; } set { SetProperty(ref loadingUri, value); } }

        public string InfoBarTitle { get { return infoBarTitle; } set { SetProperty(ref infoBarTitle, value); } }
        public string InfoBarMessage { get { return infoBarMessage; } set { SetProperty(ref infoBarMessage, value); } }
        public bool InfoBarVisible { get { return infoBarVisible; } set { SetProperty(ref infoBarVisible, value); } }
        public InfoBarSeverity InfoBarSeverity { get { return infoBarSeverity; } set { SetProperty(ref infoBarSeverity, value); } }

        //public ICommand GetHasFromWebCmd => getHasFromWebCmd ?? (getHasFromWebCmd = new RelayCommand(OnGetHashFromWeb));
        //public ICommand GetHasFromLocalCmd => getHasFromLocalCmd ?? (getHasFromLocalCmd = new RelayCommand(OnGetHashFromLocal));

        public ICommand GenerateScriptCmd => generateScriptCmd ?? (generateScriptCmd = new RelayCommand(GenerateScript));

        public ICommand LoadFromUriCmd => loadFromUriCmd ?? (loadFromUriCmd = new RelayCommand(OnLoadFromUri));

        public ICommand AddInstallerCmd => addInstallerCmd ?? (addInstallerCmd = new RelayCommand(OnAddInstaller));

        public ICommand RemoveInstallerCmd => removeInstallerCmd ?? (removeInstallerCmd = new RelayCommand(OnRemoveInstaller));

        #endregion

        #region Privates Methods

        private void OnAddInstaller()
        {
            if (InstallerSelectedArchitecture != null && !string.IsNullOrEmpty(InstallerUrl) && !string.IsNullOrEmpty(InstallerSha256))
            {
                var installer = new Installer()
                {
                    InstallerUrl = installerUrl,
                    Architecture = InstallerSelectedArchitecture,
                    InstallerSha256 = InstallerSha256
                };

                Installers.Add(installer);
            }
        }

        private void OnRemoveInstaller()
        {
            if (SelectedInstaller != null)
            {
                Installers.Remove(SelectedInstaller);
            }
        }

        private async void OnLoadFromUri()
        {
            if (string.IsNullOrEmpty(loadingUri))
            {
                CreateInfoBar("Error", "Loading Url field is Empty or Invalid", InfoBarSeverity.Error);
                return;
            }

            //base url is yaml file
            // remove .yaml extension, add .install.yaml
            var baseUri = loadingUri.Substring(0, loadingUri.LastIndexOf('.'));
            var installerUri = baseUri + ".installer.yaml";
            // remove .yaml extension, add .local.en-US.yaml
            var enResourceUri = baseUri + ".locale.en-US.yaml";

            var versionSerializer = new DeserializerBuilder().Build();
            var basefilepath = DownloadFile(LoadingUri);
            var basecontent = File.ReadAllText(basefilepath);
            YamlBaseInfos baseinfos = null;
            try
            {
                // try with version way
                //baseinfos = versionSerializer.Deserialize<YamlBaseInfos>(basecontent);
                dynamic allInfos = versionSerializer.Deserialize<ExpandoObject>(basecontent);
            }
            catch (Exception)
            {
                //if not try with complete way
                try
                {
                    baseinfos = versionSerializer.Deserialize<YamlFullInfos>(basecontent);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            

            InstallerManifest installers = null;
            YamlResourceFile enresourcesinfos = null;
            if (baseinfos.ManifestType.ToUpper() == "version".ToUpper())
            {

                var enresourcesfilepath = DownloadFile(enResourceUri);
                var installersfilepath = DownloadFile(installerUri);
                var installercontent = File.ReadAllText(installersfilepath);
                installers = versionSerializer.Deserialize<InstallerManifest>(installercontent);
                var enresourcecontent = File.ReadAllText(enresourcesfilepath);
                enresourcesinfos = versionSerializer.Deserialize<YamlResourceFile>(enresourcecontent);
            }

            //TODO : fill form with info
            Id = baseinfos.PackageIdentifier;
            AppName = baseinfos.PackageIdentifier.Split('.')[0];
            Publisher = baseinfos.PackageIdentifier.Split('.')[1];
            Version = baseinfos.PackageVersion;

            LicenceUrl = enresourcesinfos?.LicenseUrl;
            Licence = enresourcesinfos?.License;

            Description = enresourcesinfos?.Description;
            HomePage = enresourcesinfos?.PublisherUrl;

            //TODO: first dl installers binaries and ul them to internal repo
            //TODO: modify installer values with internal ones
            //TODO: modify machine scope with user one

            var requestor = new HttpDataService(SettingsService.WingetUrl);
            requestor.SetHeader("x-functions-key", SettingsService.WingetKey);

            ///Adding to the /packages resource
            var result = await requestor.PostAsJsonWithResponseAsync("packages", baseinfos);
            if (!result.IsSuccessStatusCode)
            {
                CreateInfoBar("Error", result.ReasonPhrase, InfoBarSeverity.Error);
                return;
            }

            //Adding to the /versions resource
            //# Format difference: Tags are not allowed to have spaces in them.
            var payload = new { PackageVersion = baseinfos.PackageVersion, DefaultLocale = enresourcesinfos };
            result = await requestor.PostAsJsonWithResponseAsync($"packages/{Id}/versions", payload);
            if (!result.IsSuccessStatusCode)
            {
                CreateInfoBar("Error", result.ReasonPhrase, InfoBarSeverity.Error);
                return;
            }

            ///Adding to the /locales resource
            result = await requestor.PostAsJsonWithResponseAsync($"packages/{Id}/versions/{baseinfos.PackageVersion}/locales", enresourcesinfos);
            if (!result.IsSuccessStatusCode)
            {
                CreateInfoBar("Error", result.ReasonPhrase, InfoBarSeverity.Error);
                return;
            }

            ///Installers
            string errors = String.Empty;
            foreach (var item in installers.Installers)
            {
                var installerIdentifier = $"{item.Architecture}.{item.InstallerLocale}.{item.InstallerType}";
                item.InstallerIdentifier = installerIdentifier;

                result = await requestor.PostAsJsonWithResponseAsync($"packages/{Id}/versions/{baseinfos.PackageVersion}/installers", item);
                if (!result.IsSuccessStatusCode)
                {
                    errors += result.ReasonPhrase + '\n';
                }
            }

            if (!string.IsNullOrEmpty(errors))
            {
                CreateInfoBar("Error", errors, InfoBarSeverity.Error);
                return;
            }

            CreateInfoBar("Success", $"Package {Id} imported", InfoBarSeverity.Success);

        }

        public string DownloadFile(string uri)
        {
            string fileName = string.Empty;

            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                using (Stream rawStream = client.OpenRead(uri))
                {

                    string contentDisposition = client.ResponseHeaders["content-disposition"];
                    if (!string.IsNullOrEmpty(contentDisposition))
                    {
                        string lookFor = "filename=";
                        int index = contentDisposition.IndexOf(lookFor, StringComparison.CurrentCultureIgnoreCase);
                        if (index >= 0)
                            fileName = contentDisposition.Substring(index + lookFor.Length);
                    }
                    else
                    {
                        fileName = uri.Substring(uri.LastIndexOf('/') + 1);
                    }
                    if (fileName.Length > 0)
                    {
                        using (StreamReader reader = new StreamReader(rawStream))
                        {
                            File.WriteAllText(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, System.Net.WebUtility.HtmlDecode(fileName)), reader.ReadToEnd());
                            reader.Close();
                        }
                    }
                    rawStream.Close();
                }
            }

            return Path.Combine(ApplicationData.Current.TemporaryFolder.Path, fileName);
        }

        public async void GenerateScript()
        {
            //try
            //{
            //    if (!string.IsNullOrEmpty(AppName) && !string.IsNullOrEmpty(Publisher) &&
            //        !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Version) &&
            //        !string.IsNullOrEmpty(Licence) && !string.IsNullOrEmpty(InstallerUrl))// TODO: && txtUrl.Text.IsUrl())
            //    {
            //        var versionBuilder = new ExportVersionModel
            //        {
            //            PackageIdentifier = Id,
            //            PackageVersion = Version,
            //            PackageName = AppName,
            //            Publisher = Publisher,
            //            License = Licence,
            //            LicenseUrl = licenseUrl,
            //            ShortDescription = Description,
            //            PackageUrl = HomePage,
            //            ManifestType = "version",
            //            ManifestVersion = "1.0.0",
            //            DefaultLocale = "en-US"
            //        };

            //        var installerBuilder = new ExportInstallerModel
            //        {
            //            PackageIdentifier = Id,
            //            PackageVersion = Version,
            //            ManifestType = "installer",
            //            ManifestVersion = "1.0.0",
            //            Installers = Installers.ToList()
            //        };

            //        var versionSerializer = new SerializerBuilder().Build();
            //        var installerSerializer = new SerializerBuilder().Build();
            //        var installerSerializerJson = new SerializerBuilder().JsonCompatible().Build();
            //        var versionSerializerJson = new SerializerBuilder().JsonCompatible().Build();
            //        var versionYaml = versionSerializer.Serialize(versionBuilder);
            //        var versionJson = versionSerializerJson.Serialize(versionBuilder);
            //        var installerYaml = installerSerializer.Serialize(installerBuilder);
            //        var installerJson = installerSerializerJson.Serialize(installerBuilder);

            //        var path = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, $@"Packages\{Id}\{Version}");

            //        Directory.CreateDirectory(path);
            //        File.WriteAllText(path + @"\" + Id + ".yaml", versionYaml);
            //        File.WriteAllText(path + @"\" + Id + ".installer.yaml", installerYaml);
            //        File.WriteAllText(path + @"\" + Id + ".installer.json", installerJson);


            //        //https://blogs.infosupport.com/adding-a-package-to-your-private-winget-restsource-feed-using-its-api/

            //        //$endpointBaseUrl = "https://acme87azurefunction.azurewebsites.net/api/"
            //        //$hostKey = "Ho6omxS...R711ag=="
            //        //$headers = @{ "x-functions-key" = $hostKey }

            //        var requestor = new HttpDataService(SettingsService.WingetUrl);
            //        //6lHcEs3hzbW6kOppce8VZalonvBmZ0e1epQ6zCzVDXrnnS53BEgZ1Q==

            //        requestor.SetHeader("x-functions-key", SettingsService.WingetKey);

            //        //TODO: Adding to the /packages resource
            //        //$main | ConvertTo-Json -Depth 10 | Out -File "package.json" -Encoding utf8
            //        //Invoke-RestMethod -Uri "$endpointBaseUrl/packages" -Method Post -Headers $headers -InFile "package.json"
            //        var result = await requestor.PostAsJsonWithResponseAsync("packages", installerBuilder);
            //        if (!result.IsSuccessStatusCode)
            //        {
            //            CreateInfoBar("Error", result.ReasonPhrase, InfoBarSeverity.Error);
            //            return;
            //        }

            //        //TODO: Adding to the /versions resource
            //        //# Format difference: Tags are not allowed to have spaces in them.
            //        //$localeEnglish.Tags = $localeEnglish.Tags | ForEach - Object { $_ - replace ' ', '' }
            //        //$version = [PSCustomObject] @{ PackageVersion = $main.PackageVersion; DefaultLocale = $localeEnglish }
            //        //$version | ConvertTo - Json - Depth 10 | Out - File "version.json" - Encoding utf8
            //        //Invoke - RestMethod - Uri "$endpointBaseUrl/packages/$($main.PackageIdentifier)/versions" - Method Post - Headers $headers - InFile "version.json"
            //        var payload = new { PackageVersion = installerBuilder.PackageVersion, DefaultLocale = new List<string>() };
            //        result = await requestor.PostAsJsonWithResponseAsync($"packages/{Id}/versions", payload);
            //        if (!result.IsSuccessStatusCode)
            //        {
            //            CreateInfoBar("Error", result.ReasonPhrase, InfoBarSeverity.Error);
            //            return;
            //        }

            //        //TODO: Adding to the /locales resource
            //        //# Format difference: The Publisher and PackageName properties are required.
            //        //$localeEnglish | ConvertTo - Json - Depth 10 | Out - File "locale.en-US.json" - Encoding utf8
            //        //Invoke - RestMethod - Uri "$endpointBaseUrl/packages/$($main.PackageIdentifier)/versions/$($main.PackageVersion)/locales" - Method Post - Headers $headers - InFile "locale.en-US.json"


            //        //TODO: Adding to the /installers resource
            //        //# Add the installers. These need to be flattened, so take the Installers array as the base elements...
            //        //$installers = $installer.Installers
            //        //foreach ($elt in $installers)
            //        //{
            //        //#...and to each element, add all properties of the root Installer object (except for the Installers collection itself, of course).
            //        //$installer.GetEnumerator() | Where - Object { $_.Key - ne "Installers" } | ForEach - Object { $elt.Add($_.Key, $_.Value) }

            //        //# Format difference: Each installer needs a unique "InstallerIdentifier" property, so compose one like "x64.en-US.nullsoft"
            //        //$installerIdentifier = "$($elt.Architecture).$($elt.InstallerLocale).$($elt.InstallerType)"
            //        //$elt["InstallerIdentifier"] = $installerIdentifier

            //        //# Add it to the /installers collection
            //        //$elt | ConvertTo - Json - Depth 10 | Out - File "installer.$installerIdentifier.json" - Encoding utf8
            //        //Invoke - RestMethod - Uri "$endpointBaseUrl/packages/$($main.PackageIdentifier)/versions/$($main.PackageVersion)/installers" - Method Post - Headers $headers - InFile "installer.$installerIdentifier.json"
            //        //}
            //        result = await requestor.PostAsJsonWithResponseAsync($"packages/{Id}/versions/{Version}", versionBuilder);
            //        if (!result.IsSuccessStatusCode)
            //        {
            //            CreateInfoBar("Error", result.ReasonPhrase, InfoBarSeverity.Error);
            //            return;
            //        }

            //        ClearInputs();
            //        CreateInfoBar("Success", $"Files were created in {ApplicationData.Current.TemporaryFolder.Path}", InfoBarSeverity.Success);
            //    }
            //    else
            //    {
            //        CreateInfoBar("Fill Inputs", "Required fields must be filled", InfoBarSeverity.Error);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CreateInfoBar("Error", ex.Message, InfoBarSeverity.Error);
            //}
        }

        private void ClearInputs()
        {
            AppName = "";
            Publisher = "";
            Id = "";
            Version = "";
            Description = "";
            HomePage = "";
            Licence = "";
            LicenceUrl = "";
            Hash = "";
        }

        private void CreateInfoBar(string Title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            InfoBarTitle = Title;
            InfoBarVisible = true;
            InfoBarMessage = message;
            InfoBarSeverity = severity;
        }

        #endregion
    }
}
