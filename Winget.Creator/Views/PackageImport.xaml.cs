using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Winget.Creator.ViewModels;
using Winget.Pusher.Core.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Winget.Creator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PackageImport : Page
    {
        public  PackageImportViewModel ViewModel { get; }

        public PackageImport()
        {
            ViewModel = new PackageImportViewModel();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.SourcePageType == Type.GetType("Winget.Creator.Views.PackageImport"))
            {
                var param = (string)e.Parameter;
                ViewModel.LoadingUri = $"{Consts.AzureBaseUrl}/{param}";
            }

            base.OnNavigatedTo(e);
        }
    }
}
