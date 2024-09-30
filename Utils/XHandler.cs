using Microsoft.Windows.ApplicationModel.DynamicDependency;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Windows.ApplicationModel;

namespace WinDurango.UI.Utils
{
    public class XHandler
    {
        // unused because sucks
        public static (string? DisplayName, string? Description, string? SplashScreen, string? SmallLogo, string? WideLogo, string? Logo) GetVisualElementsInfo(Package package)
        {
            string installPath = package.InstalledPath;
            string manifestPath = Path.Combine(installPath, "AppxManifest.xml");

            if (!File.Exists(manifestPath))
                return (null, null, null, null, null, null);

            string manifest;
            using (var stream = File.OpenRead(manifestPath))
            {
                var reader = new StreamReader(stream);
                manifest = reader.ReadToEnd();
            }

            XDocument doc = XDocument.Parse(manifest);
            XElement xmlPackage = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Package");
            XElement application = xmlPackage?.Descendants().LastOrDefault(e => e.Name.LocalName == "Application");
            XElement visualElements = application?.Descendants().FirstOrDefault(e => e.Name.LocalName == "VisualElements");
            if (visualElements == null)
                return (null, null, null, null, null, null);

            XElement ss = visualElements?.Descendants().FirstOrDefault(e => e.Name.LocalName == "SplashScreen") ?? null;
            XElement defaultTile = visualElements?.Descendants().FirstOrDefault(e => e.Name.LocalName == "DefaultTile") ?? null;

            // holy hell this is ew

            string? DisplayName = visualElements?.Attribute("DisplayName")?.Value ?? null;
            string? Description = visualElements?.Attribute("Description")?.Value ?? null;
            string? SplashScreen = ss?.Attribute("SplashScreen")?.Value ?? null;
            string? SmallLogo = visualElements?.Attribute("SmallLogo")?.Value ?? null;
            string? WideLogo = defaultTile?.Attribute("WideLogo")?.Value ?? null;
            string? Logo = visualElements?.Attribute("Logo")?.Value ?? null;

            return (
                DisplayName == null || DisplayName.StartsWith("ms-resource:") ? null : DisplayName,
                Description == null || Description.StartsWith("ms-resource:") ? null : Description,
                SplashScreen == null || SplashScreen.StartsWith("ms-resource:") ? null : SplashScreen,
                SmallLogo == null || SmallLogo.StartsWith("ms-resource:") ? null : SmallLogo,
                WideLogo == null || WideLogo.StartsWith("ms-resource:") ? null : WideLogo,
                Logo == null || Logo.StartsWith("ms-resource:") ? null : Logo
            );

        }

        public static List<Package> GetXPackages(List<Package> packages)
        {
            // first try implementation and it worked hell yeah
            List<Package> result = new();
            foreach (Package package in packages)
            {
                string installPath = package.InstalledPath;
                string manifestPath = Path.Combine(installPath, "AppxManifest.xml");

                if (!File.Exists(manifestPath))
                    continue;

                string manifest;
                using (var stream = File.OpenRead(manifestPath))
                {
                    var reader = new StreamReader(stream);
                    manifest = reader.ReadToEnd();
                }

                XDocument doc = XDocument.Parse(manifest);
                XElement xmlPackage = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Package");
                if (xmlPackage == null) continue;

                XElement prerequisites = xmlPackage.Descendants().FirstOrDefault(e => e.Name.LocalName == "Prerequisites");
                XElement dependencies = xmlPackage?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Dependencies");
                XElement osName = prerequisites?.Descendants().FirstOrDefault(e => e.Name.LocalName == "OSName");
                XElement osPackageDependency = dependencies?.Descendants().FirstOrDefault(e => e.Name.LocalName == "OSPackageDependency");

                if ((osName != null && osName.Value.ToLower() == "era") || (osPackageDependency != null && osPackageDependency.Attribute("Name")?.Value == "Microsoft.GameOs"))
                {
                    result.Add(package);
                }
            }
            Logger.WriteInformation($"Found {result.Count} Era/XbUWP packages");
            return result;
        }
    }
}
