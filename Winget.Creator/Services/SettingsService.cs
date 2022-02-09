using CommunityToolkit.WinUI.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Winget.Creator.Services
{
    public class SettingsService
    {
        private static SettingsService instance;

        public static SettingsService Instance { get { if (instance == null) { instance = new SettingsService(); } return instance; } }

        public SettingsService()
        {

        }

        public string FileName { get; set; } = "";

        public virtual Version Version { get; set; } = new Version(1, 0, 0, 0);

        #region Property
        public virtual bool IsFirstRun { get; set; } = true;
        public virtual bool IsIDMEnabled { get; set; } = false;
        public virtual bool GroupByPublisher { get; set; } = false;
        public virtual bool AutoRefreshInStartup { get; set; } = false;
        public virtual bool IsStoreDataGridColumnWidth { get; set; } = false;
        public virtual bool IsShowNotifications { get; set; } = true;
        public virtual bool IsShowDetailByDoubleClick { get; set; } = true;
        public virtual bool IdentifyInstalledPackage { get; set; } = false;
        public virtual bool AutoDownloadPackage { get; set; } = false;
        public virtual DateTime UpdatedDate { get; set; } = DateTime.Now;
        //public virtual NavigationViewPaneDisplayMode PaneDisplayMode { get; set; } = NavigationViewPaneDisplayMode.Auto;
        //public virtual InstallMode InstallMode { get; set; } = InstallMode.Internal;
        //public virtual ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;
        //public virtual Brush Accent { get; set; }

        public virtual string WingetUrl { get; set; } = "";
        public virtual string WingetKey { get; set; } = "";

        private ObservableCollection<DataGridLength> _DataGridColumnWidth = new ObservableCollection<DataGridLength>();
        public virtual ObservableCollection<DataGridLength> DataGridColumnWidth
        {
            get => _DataGridColumnWidth;
            set
            {
                if (Equals(value, _DataGridColumnWidth)) return;
                _DataGridColumnWidth = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DataGridLength> _DataGridInstalledColumnWidth = new ObservableCollection<DataGridLength>();
        public virtual ObservableCollection<DataGridLength> DataGridInstalledColumnWidth
        {
            get => _DataGridInstalledColumnWidth;
            set
            {
                if (Equals(value, _DataGridInstalledColumnWidth)) return;
                _DataGridInstalledColumnWidth = value;
                OnPropertyChanged();
            }
        }
        #endregion Property


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
