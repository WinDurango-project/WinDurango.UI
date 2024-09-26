using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;
using Image = Microsoft.UI.Xaml.Controls.Image;

namespace WinDurango.UI.Dialogs
{
    public sealed partial class AppListDialog : ContentDialog
    {
        public List<Package> Pkgs { get; set; } = new List<Package>();

        public AppListDialog(List<Package> packages, bool multiSelect = false)
        {
            Logger.WriteWarning($"AppList is very buggy atm, expect a crash.");

            this.Pkgs = packages;

            this.DataContext = this;

            this.InitializeComponent();

            if (multiSelect)
                appListView.SelectionMode = ListViewSelectionMode.Multiple;
            appListView.MaxHeight = App.MainWindow.Bounds.Height * 0.65;

        }


        private void logoFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = sender as Image;
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/testimg.png"));
            Logger.WriteWarning($"Failed to load logo.");
        }

        private void AddToAppList(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            foreach (Package package in appListView.SelectedItems)
            {
                if (InstalledPackages.GetInstalledPackage(package.Id.FamilyName) == null)
                    InstalledPackages.AddInstalledPackage(package);
                App.MainWindow.AppsListPage.InitAppList();
            }
        }

        private void hideDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
