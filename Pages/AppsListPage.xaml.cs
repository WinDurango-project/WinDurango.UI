using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinDurango.UI.Controls;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;

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
                }
            }
        }

        public AppsListPage()
        {
            InitializeComponent();

            InitAppList();
        }

        private async void installButton_Tapped(object sender, TappedRoutedEventArgs e)
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
                    _ = await Packages.InstallPackageAsync(new Uri(manifest, UriKind.Absolute));
                }
                else
                {
                    // AppxManifest does not exist in that folder
                    if (Directory.Exists(mountFolder))
                    {
                        // there IS a mount folder
                        if (File.Exists(Path.Combine(mountFolder + "\\AppxManifest.xml")))
                        {
                            // there is an AppxManifest inside.
                            var confirmation = new Confirmation($"There was no AppxManifest found inside the picked folder.\nHowever there was one found inside the Mount folder, would you like to register that?", "Install from Mount?");
                            if (await confirmation.Show() == Dialog.BtnClicked.Yes)
                                await Packages.InstallXPackageAsync(folder.Path.ToString());
                        }
                        else
                        {
                            // there is no AppxManifest inside.
                            await new NoticeDialog($"AppxManifest does not exist in both {folder.Path} and {mountFolder}", "Error").Show();
                        }
                    }
                    else
                    {
                        await new NoticeDialog($"AppxManifest does not exist in {folder.Path} and there is no \"Mount\" folder.", "Error").Show();
                    }

                    return;
                }
            }
        }
    }
}
