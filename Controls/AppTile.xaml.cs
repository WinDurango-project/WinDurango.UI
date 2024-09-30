using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private string _Name;
        private string _Publisher;
        private string _Version;
        private Uri _Logo;

        private async void HandleUnregister(object sender, SplitButtonClickEventArgs e)
        {
            if ((bool)unregisterCheckbox.IsChecked)
            {
                var confirmation = new Confirmation($"Are you sure you want to uninstall {_Name}?", "Uninstall?");
                Dialog.BtnClicked answer = await confirmation.Show();
                if (answer == Dialog.BtnClicked.Yes)
                {
                    if (InstalledPackages.RemoveSymlinks(_familyName))
                        await Packages.RemovePackage(_package);
                }
            }
            else
            {
                if (InstalledPackages.RemoveSymlinks(_familyName))
                    InstalledPackages.RemoveInstalledPackage(_package);
                App.MainWindow.AppsListPage.InitAppList();
            }
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            Logger.WriteDebug($"Opening app installation folder {_package.InstalledPath}");
            _ = Process.Start(new ProcessStartInfo(_package.InstalledPath) { UseShellExecute = true });
        }

        public AppTile(string familyName)
        {
            _familyName = familyName;
            this.InitializeComponent();

            _package = Packages.GetPackageByFamilyName(_familyName);
            try
            {
                _Name = _package.DisplayName ?? _package.Id.Name;
            }
            catch
            {
                _Name = _package.Id.Name;
            }
            _Publisher = _package.PublisherDisplayName ?? _package.Id.PublisherId;
            _Version = $"{_package.Id.Version.Major.ToString() ?? "U"}.{_package.Id.Version.Minor.ToString() ?? "U"}.{_package.Id.Version.Build.ToString() ?? "U"}.{_package.Id.Version.Revision.ToString() ?? "U"}";
            _Logo = _package.Logo;
            string ss = Packages.GetSplashScreenPath(_package);
            IReadOnlyList<AppListEntry> appListEntries = null;
            try
            {
                appListEntries = _package.GetAppListEntries();
            } catch
            {
                Logger.WriteWarning($"Could not get the applist entries of \"{_Name}\"");
            }
            AppListEntry firstAppListEntry = appListEntries?.FirstOrDefault() ?? null;

            if (firstAppListEntry == null)
                Logger.WriteWarning($"Could not get the applist entry of \"{_Name}\"");

            if (ss == null || !File.Exists(ss))
            {
                try
                {
                    if (firstAppListEntry != null)
                    {
                        RandomAccessStreamReference logoStream = firstAppListEntry.DisplayInfo.GetLogo(new Size(320, 180));
                        BitmapImage logoImage = new();
                        using IRandomAccessStream stream = logoStream.OpenReadAsync().GetAwaiter().GetResult();
                        logoImage.SetSource(stream);
                        appLogo.Source = logoImage;
                    }
                    else
                    {
                        BitmapImage logoImage = new(_Logo);
                        appLogo.Source = logoImage;
                    }
                }
                catch (Exception)
                {
                    BitmapImage logoImage = new(_Logo);
                    appLogo.Source = logoImage;
                }
            }
            else
            {
                appLogo.Source = new BitmapImage(new Uri(ss));
            }
            infoExpander.Header = _Name;

            Flyout rcFlyout = new();

            expanderVersion.Text = $"Publisher: {_Publisher}\nVersion {_Version}";

            RightTapped += (sender, e) =>
            {
                rcFlyout.ShowAt(sender as FrameworkElement);
            };

            startButton.Tapped += async (s, e) =>
            {
                if (_package.Status.LicenseIssue)
                {
                    Logger.WriteError($"Could not launch {_Name} due to licensing issue.");
                    _ = new NoticeDialog($"There is a licensing issue... Do you own this package?", $"Could not launch {_Name}").Show();
                    return;
                }

                if (firstAppListEntry == null)
                {
                    _ = new NoticeDialog($"Could not get the applist entry of \"{_Name}\"", $"Could not launch {_Name}").Show();
                    return;
                }
                Logger.WriteInformation($"Launching {_Name}");
                if (await firstAppListEntry.LaunchAsync() == false)
                    _ = new NoticeDialog($"Failed to launch \"{_Name}\"!", $"Could not launch {_Name}").Show();
            };
        }
    }
}
