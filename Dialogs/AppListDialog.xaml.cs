using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;
using Image = Microsoft.UI.Xaml.Controls.Image;

namespace WinDurango.UI.Dialogs
{
    public sealed partial class AppListDialog : ContentDialog
    {
        public ObservableCollection<Package> Pkgs { get; set; } = new ObservableCollection<Package>();

        public AppListDialog()
        {
            Logger.Instance.WriteWarning($"AppList is very buggy atm, expect a crash.");
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Pkgs = new ObservableCollection<Package>(Packages.GetInstalledPackages());
                this.DataContext = this;
            });

            this.InitializeComponent();
            appListView.MaxHeight = App.MainWindow.Bounds.Height * 0.65;

        }


        private void logoFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = sender as Image;
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/testimg.png"));
            Logger.Instance.WriteWarning($"Failed to load logo.");
        }

        private void AddToAppList(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (appListView.SelectedItem is Package package)
            {
                if (InstalledPackages.GetInstalledPackage(package.Id.FamilyName) == null)
                    InstalledPackages.AddInstalledPackage(package);
                else
                    new NoticeDialog($"{package.DisplayName} already exists.", "Couldn't add app");
                App.MainWindow.AppsListPage.InitAppList();
            }
        }

        private void hideDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
