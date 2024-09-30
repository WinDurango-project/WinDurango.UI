using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinDurango.UI.Controls;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;
using static WinDurango.UI.Localization.Locale;

namespace WinDurango.UI.Pages
{
    public sealed partial class AppsListPage : Page
    {
        public void InitAppList()
        {
            appList.Children.Clear();

            Dictionary<string, InstalledPackage> installedPackages = InstalledPackages.GetInstalledPackages();
            var pm = new PackageManager();

            foreach (var installedPackage in installedPackages)
            {
                if (pm.FindPackageForUser(WindowsIdentity.GetCurrent().User?.Value, installedPackage.Value.FullName) != null)
                {
                    Grid outerGrid = new();
                    AppTile gameContainer = new(installedPackage.Key);
                    outerGrid.Children.Add(gameContainer);
                    appList.Children.Add(outerGrid);
                    Logger.WriteDebug($"Added {installedPackage.Key} to the app list");
                }
                else
                {
                    Logger.WriteError($"Couldn't find package {installedPackage.Value.FullName} in installed UWP packages list");
                }
            }
        }

        private async void ShowAppListView(object sender, RoutedEventArgs e)
        {
            AppListDialog dl = new(Packages.GetInstalledPackages().ToList(), true);
            dl.Title = "Installed UWP apps";
            dl.XamlRoot = this.Content.XamlRoot;
            await dl.ShowAsync();
        }

        private async void ShowInstalledEraApps(object sender, RoutedEventArgs e)
        {
            AppListDialog dl = new(XHandler.GetXPackages(Packages.GetInstalledPackages().ToList()), true);
            dl.Title = "Installed Era/XUWP apps";
            dl.XamlRoot = this.Content.XamlRoot;
            await dl.ShowAsync();
        }

        private void UpdateCheckboxes(object sender, RoutedEventArgs e)
        {
            if (autoSymlinkCheckBox == null || addToAppListCheckBox == null)
                return;

            autoSymlinkCheckBox.IsEnabled = (bool)addToAppListCheckBox.IsChecked;
        }

        public AppsListPage()
        {
            InitializeComponent();

            Stopwatch PlatinumWatch = new Stopwatch();

            Logger.WriteDebug("Initializing AppsListPage...");

            PlatinumWatch.Start();
            InitAppList();
            PlatinumWatch.Stop();

            Logger.WriteDebug("Initialized AppsListPage in {0:D2}:{1:D2}:{2:D2}.{3:D3}", (int)PlatinumWatch.Elapsed.TotalHours, (int)PlatinumWatch.Elapsed.TotalMinutes, (int)PlatinumWatch.Elapsed.TotalSeconds, (int)PlatinumWatch.Elapsed.TotalMilliseconds);
        }

        private async void InstallButton_Tapped(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                string manifest = Path.Combine(folder.Path + "\\AppxManifest.xml");
                string mountFolder = Path.Combine(folder.Path + "\\Mount");

                if (File.Exists(manifest))
                {
                    var dialog = new InstallConfirmationDialog(manifest);
                    dialog.PrimaryButtonClick += async (sender, e) =>
                    {
                        _ = await Packages.InstallPackageAsync(new Uri(manifest, UriKind.Absolute), (bool)addToAppListCheckBox.IsChecked);
                    };
                    await dialog.ShowAsync();
                }
                else
                {
                    // AppxManifest does not exist in that folder
                    if (Directory.Exists(mountFolder))
                    {
                        // there IS a mount folder
                        if (File.Exists(Path.Combine(mountFolder + "\\AppxManifest.xml")))
                        {
                            var dialog = new InstallConfirmationDialog(Path.Combine(mountFolder + "\\AppxManifest.xml"));
                            dialog.PrimaryButtonClick += async (sender, e) =>
                            {
                                await Packages.InstallXPackageAsync(folder.Path.ToString(), autoSymlinkCheckBox.IsEnabled && (bool)autoSymlinkCheckBox.IsChecked ? Packages.XvdMode.CreateSymlinks : Packages.XvdMode.DontUse, (bool)addToAppListCheckBox.IsChecked);
                            };
                            await dialog.ShowAsync();
                        }
                        else
                        {
                            // there is no AppxManifest inside.
                            Logger.WriteError($"Could not find AppxManifest.xml in {folder.Path} and {mountFolder}");
                            await new NoticeDialog(GetLocalizedText("ManifestNotFoundMulti", folder.Path, mountFolder), "Error").Show();
                        }
                    }
                    else
                    {
                        Logger.WriteError($"Could not find AppxManifest.xml in {folder.Path} and no Mount folder exists");
                        await new NoticeDialog(GetLocalizedText("ManifestNotFoundNoMount", folder.Path), "Error").Show();
                    }

                    return;
                }
            }
        }
    }
}
