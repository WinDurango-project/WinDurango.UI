using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Windows.ApplicationModel;

namespace WinDurango.UI.Utils
{
    public class XHandler
    {
        public static List<Package> getXPackages(List<Package> packages)
        {
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
