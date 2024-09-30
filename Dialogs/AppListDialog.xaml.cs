using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            this.Pkgs = packages;

            this.DataContext = this;

            this.InitializeComponent();

            if (multiSelect)
                appListView.SelectionMode = ListViewSelectionMode.Multiple;
            appListView.MaxHeight = App.MainWindow.Bounds.Height * 0.65;

        }

        private void AppListView_Loaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListView)sender;

            foreach (var pkg in Pkgs)
            {
                var item = new ListViewItem();
                item.MinWidth = 200;

                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                var packageLogo = new Image
                {
                    Width = 64,
                    Height = 64,
                    Margin = new Thickness(5),
                    Source = new BitmapImage(pkg.Logo ?? new Uri("ms-appx:///Assets/testimg.png"))
                };
                packageLogo.ImageFailed += LogoFailed;

                var packageInfo = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5)
                };

                string displayName;
                try
                {
                    displayName = pkg.DisplayName;
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    displayName = pkg.Id.Name;
                }

                var packageName = new TextBlock
                {
                    Text = displayName ?? "Unknown",
                    FontWeight = FontWeights.Bold
                };

                var publisherName = new TextBlock
                {
                    Text = pkg.PublisherDisplayName ?? "Unknown"
                };

                packageInfo.Children.Add(packageName);
                packageInfo.Children.Add(publisherName);

                stackPanel.Children.Add(packageLogo);
                stackPanel.Children.Add(packageInfo);

                item.Content = stackPanel;

                item.Tag = pkg;

                listView.Items.Add(item);
            }
        }



        private void LogoFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = sender as Image;
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/testimg.png"));
            Logger.WriteWarning($"Failed to load logo.");
        }

        private void AddToAppList(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            foreach (ListViewItem listViewItem in appListView.SelectedItems)
            {
                var package = listViewItem.Tag as Package;
                if (package != null && package?.Id?.FamilyName != null)
                {
                    if (InstalledPackages.GetInstalledPackage(package.Id.FamilyName) == null)
                        InstalledPackages.AddInstalledPackage(package);
                }
            }

            App.MainWindow.AppsListPage.InitAppList();
        }

        private void hideDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
