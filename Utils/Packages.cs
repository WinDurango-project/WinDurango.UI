using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Settings;
using WinUI3Localizer;
using static WinDurango.UI.Localization.Locale;

namespace WinDurango.UI.Utils
{
    public abstract class Packages
    {
        // TODO: Make these methods not use the GUI, instead just throw an exception and catch it in the area where the method is actually invoked.
        public static IEnumerable<Package> GetInstalledPackages()
        {
            var sid = WindowsIdentity.GetCurrent().User?.Value;

            var pm = new PackageManager();
            return pm.FindPackagesForUser(sid);
        }

        public static (string? DisplayName, string? PublisherDisplayName, string? Logo, string? Description) GetPropertiesFromManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
                return (null, null, null, null);

            string manifest;
            using (var stream = File.OpenRead(manifestPath))
            {
                var reader = new StreamReader(stream);
                manifest = reader.ReadToEnd();
            }

            XDocument doc = XDocument.Parse(manifest);
            XElement package = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Package");
            if (package == null) return (null, null, null, null);

            XElement properties = package.Descendants().FirstOrDefault(e => e.Name.LocalName == "Properties");
            if (properties == null) return (null, null, null, null);

            string logo = properties?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Logo").Value ?? null;
            string displayName = properties?.Descendants().FirstOrDefault(e => e.Name.LocalName == "DisplayName").Value ?? null;
            string publisherDisplayName = properties?.Descendants().FirstOrDefault(e => e.Name.LocalName == "PublisherDisplayName").Value ?? null;
            string description = properties?.Descendants()?.FirstOrDefault(e => e.Name.LocalName == "Description")?.Value ?? null;
            return (displayName, publisherDisplayName, logo, description);
        }

        public enum XvdMode
        {
            CreateSymlinks,
            DontUse
        };

        public static async Task InstallXPackageAsync(string dir, XvdMode mode = XvdMode.CreateSymlinks, bool addInstalledPackage = true)
        {
            string mountDir = Path.Combine(dir, "Mount");
            string exvdDir = Path.Combine(dir, "EmbeddedXvd");

            bool hasExvd = Directory.Exists(exvdDir);

            if (!Directory.Exists(mountDir))
            {
                await new NoticeDialog(GetLocalizedText($"mountNotFound", mountDir), "Error").Show();
                return;
            }

            string package = await InstallPackageAsync(new Uri(mountDir + "\\AppxManifest.xml", UriKind.Absolute), addInstalledPackage);

            if (package == null || !addInstalledPackage)
                return;

            var (familyName, installedPackage) = InstalledPackages.GetInstalledPackage(package).Value;

            if (hasExvd)
            {
                foreach (string filePath in Directory.GetFiles(Path.Combine(exvdDir + "\\Windows\\System32")))
                {
                    string fileName = Path.GetFileName(filePath);

                    if (File.Exists(Path.Combine(mountDir, fileName)))
                        continue;

                    FileSystemInfo symlink = File.CreateSymbolicLink(Path.Combine(mountDir, fileName), filePath);
                    installedPackage.SymlinkedDLLs.Add(symlink.Name);
                    InstalledPackages.UpdateInstalledPackage(familyName, installedPackage);
                }
            }
        }

        public static string GetSplashScreenPath(Package pkg)
        {
            try {
                string installPath = pkg.InstalledPath;
                string manifestPath = Path.Combine(installPath, "AppxManifest.xml");

                if (!File.Exists(manifestPath))
                    return null;

                string manifest;
                using (var stream = File.OpenRead(manifestPath))
                {
                    var reader = new StreamReader(stream);
                    manifest = reader.ReadToEnd();
                }

                XDocument doc = XDocument.Parse(manifest);
                XElement package = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Package");
                if (package == null) return null;

                XElement applications = package.Descendants().FirstOrDefault(e => e.Name.LocalName == "Applications");
                if (applications == null) return null;

                XElement application = applications.Descendants().FirstOrDefault(e => e.Name.LocalName == "Application");
                if (application == null) return null;

                XElement visualElements = application.Descendants().FirstOrDefault(e => e.Name.LocalName == "VisualElements");
                if (visualElements == null) return null;

                XElement splashScreen = visualElements.Descendants().FirstOrDefault(e => e.Name.LocalName == "SplashScreen");
                if (splashScreen == null) return null;

                string imagePath = splashScreen.Attribute("Image")?.Value;
                if (imagePath == null) return null;

                string splashScreenPath = Path.Combine(installPath, imagePath);
                return splashScreenPath;
            }
            catch
            {
                return null;
            }
        }


        public static async Task<string> InstallPackageAsync(Uri appxManifestUri, bool addInstalledPackage = true)
        {
            string manifestPath = Uri.UnescapeDataString(appxManifestUri.AbsolutePath);

            // TODO: strip UI
            if (!File.Exists(manifestPath))
            {
                await new NoticeDialog(GetLocalizedText("NotFound", manifestPath), "Error").Show();
                return null;
            }


            Logger.WriteInformation($"Installing package \"{manifestPath}\"...");
            var status = new ProgressDialog($"Installing package...", "Installing", false);
            _ = App.MainWindow.DispatcherQueue.TryEnqueue(async () => await status.ShowAsync());
            PackageManager pm = new();
            try
            {
                Logger.WriteInformation($"Reading manifest...");
                status.Text = "ReadingManifest".GetLocalizedString();
                string manifest;
                await using (var stream = File.OpenRead(manifestPath))
                {
                    var reader = new StreamReader(stream);
                    manifest = await reader.ReadToEndAsync();
                }

                XDocument doc = XDocument.Parse(manifest);
                XElement package = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Package");
                XElement identity = package?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Identity");

                string pkgName = identity.Attribute("Name")?.Value;
                string pkgPublisher = identity.Attribute("Publisher")?.Value;

                status.Progress = 20.0;
                status.Text = GetLocalizedText("CheckingInstallStatus", pkgName);
                var sid = WindowsIdentity.GetCurrent().User?.Value;
                var installedPackages = await Task.Run(() => pm.FindPackagesForUser(sid, pkgName, pkgPublisher));

                if (installedPackages.Any())
                {
                    status.Hide();
                    Logger.WriteError($"{pkgName} is already installed.");
                    await new NoticeDialog(GetLocalizedText("AlreadyInstalled", pkgName), "Error").Show();
                    return null;
                }


                status.Progress = 40.0;
                status.Text = GetLocalizedText("InstallingPackage", pkgName);
                Logger.WriteInformation($"Registering...");
                var deployment = await pm.RegisterPackageAsync(appxManifestUri, null, DeploymentOptions.DevelopmentMode);

                status.Progress = 60.0;

                status.Text = "GettingAppInfo".GetLocalizedString();
                var recentPkg = GetMostRecentlyInstalledPackage();

                if (addInstalledPackage)
                {
                    status.Text = $"UpdatingAppList".GetLocalizedString();
                    status.Progress = 80.0;
                    InstalledPackages.AddInstalledPackage(recentPkg);
                    status.Progress = 90.0;
                    App.MainWindow.ReloadAppList();
                    status.Progress = 100.0;
                }
                else
                {
                    status.Progress = 100.0;
                }

                status.Hide();
                Logger.WriteInformation($"{recentPkg.Id.Name} was installed.");
                await new NoticeDialog(GetLocalizedText("PackageInstalled", recentPkg.Id.Name)).Show();
                return recentPkg.Id.FamilyName;
            }
            catch (Exception e)
            {
                // we're fucked :(
                status.Hide();
                Logger.WriteError($"{appxManifestUri} failed to install");
                Logger.WriteException(e);
                await new NoticeDialog(GetLocalizedText("PackageInstallFailedEx", appxManifestUri, e.Message), "Error").Show();
                return null;
            }
        }

        public static async Task RemovePackage(Package package)
        {
            Logger.WriteError($"Uninstalling {package.DisplayName}...");
            var status = new ProgressDialog(GetLocalizedText("UninstallingPackage", package.DisplayName), "Uninstalling", false);
            _ = App.MainWindow.DispatcherQueue.TryEnqueue(async () => await status.ShowAsync());
            PackageManager pm = new();
            try
            {
                var undeployment = await pm.RemovePackageAsync(package.Id.FullName, RemovalOptions.PreserveApplicationData);

                status.Progress = 50.0;
                InstalledPackages.RemoveInstalledPackage(package);
                status.Progress = 100.0;
                status.Hide();
                Logger.WriteInformation($"{package.DisplayName} was uninstalled.");
                await new NoticeDialog(GetLocalizedText("PackageUninstalled", package.DisplayName)).Show();
                App.MainWindow.ReloadAppList();
            }
            catch (Exception ex)
            {
                status.Hide();
                Logger.WriteError($"{package.DisplayName} failed to uninstall");
                Logger.WriteException(ex);
                await new NoticeDialog(GetLocalizedText("PackageUninstallFailedEx", package.DisplayName, ex.Message), "Error!").Show();
            }
            return;
        }

        public static Package GetPackageByFamilyName(string familyName)
        {
            var packageManager = new PackageManager();
            var packages = packageManager.FindPackagesForUser(null, familyName);

            return packages == null || !packages.Any() ? null : packages.First();
        }

        public static Package GetMostRecentlyInstalledPackage()
        {
            var sid = WindowsIdentity.GetCurrent().User?.Value;
            var pm = new PackageManager();
            var packages = pm.FindPackagesForUser(sid);

            if (!packages.Any())
                return null;

            var newestPackage = packages.Last();

            return newestPackage;
        }

    }
}