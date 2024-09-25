using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Controls
{
    public sealed partial class AppTile
    {
        private Package _package;
        private readonly string _familyName;

        private async void HandleUnregister(object sender, SplitButtonClickEventArgs e)
        {
            if ((bool)unregisterCheckbox.IsChecked)
            {
                var confirmation = new Confirmation($"Are you sure you want to uninstall {_package.DisplayName}?", "Uninstall?");
                Dialog.BtnClicked answer = await confirmation.Show();
                if (answer == Dialog.BtnClicked.Yes)
                {
                    if (InstalledPackages.RemoveSymlinks(_familyName))
                        await Packages.RemovePackage(_package);
                }
            } else
            {
                if (InstalledPackages.RemoveSymlinks(_familyName))
                    InstalledPackages.RemoveInstalledPackage(_package);
                App.MainWindow.AppsListPage.InitAppList();
            }
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo(_package.InstalledPath) { UseShellExecute = true });
        }

        public AppTile(string familyName)
        {
            _familyName = familyName;

            this.InitializeComponent();

            _package = Packages.GetPackageByFamilyName(_familyName);
            var ss = Packages.getSplashScreenPath(_package);
            IReadOnlyList<AppListEntry> appListEntries = _package.GetAppListEntries();
            AppListEntry firstAppListEntry = appListEntries[0];

            if (ss == null)
            {
                RandomAccessStreamReference logoStream = firstAppListEntry.DisplayInfo.GetLogo(new Size(320, 180));
                BitmapImage logoImage = new();

                using IRandomAccessStream stream = logoStream.OpenReadAsync().GetAwaiter().GetResult();
                logoImage.SetSource(stream);

                appLogo.Source = logoImage;
            }
            else
            {
                appLogo.Source = new BitmapImage(new Uri(ss));
            }
            infoExpander.Header = _package.DisplayName;

            Flyout rcFlyout = new();

            expanderVersion.Text = $"Publisher: {_package.PublisherDisplayName}\nVersion {_package.Id.Version.Major}.{_package.Id.Version.Minor}.{_package.Id.Version.Build}";

            RightTapped += (sender, e) =>
            {
                rcFlyout.ShowAt(sender as FrameworkElement);
            };

            startButton.Tapped += async (s, e) =>
            {
                var hasLaunched = await firstAppListEntry.LaunchAsync();
                if (hasLaunched == false)
                    _ = new NoticeDialog($"Failed to launch \"{_package.DisplayName}\"!");
            };
        }
    }
}
