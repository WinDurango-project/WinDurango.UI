using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Text.Json;
using Windows.ApplicationModel;
using WinDurango.UI.Utils;


namespace WinDurango.UI.Settings
{
    public class InstalledPackage
    {
        public string FullName { get; set; }
        public List<string> SymlinkedDLLs { get; set; }
    }

    public abstract class InstalledPackages
    {

        public static void RemoveInstalledPackage(Package pkg)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinDurango");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, "InstalledPackages.json");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("Could not get the list of installed packages!");
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine("{}");
                }
            }

            string json = File.ReadAllText(filePath);
            var installedPkgs = JsonSerializer.Deserialize<Dictionary<string, InstalledPackage>>(json) ?? new Dictionary<string, InstalledPackage>();

            if (installedPkgs.TryGetValue(pkg.Id.FamilyName, out var package) && package.FullName == pkg.Id.FullName)
            {
                installedPkgs.Remove(pkg.Id.FamilyName);
            }
            else
            {
                Debug.WriteLine($"Couldn't uninstall {pkg.Id.FamilyName} as it was not found in the package list.");
                return;
            }

            string updated = JsonSerializer.Serialize(installedPkgs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updated);
        }


        public static Dictionary<string, InstalledPackage> GetInstalledPackages()
        {
            if (!Directory.Exists(App.DataDir))
            {
                Directory.CreateDirectory(App.DataDir);
            }

            string filePath = Path.Combine(App.DataDir, "InstalledPackages.json");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("Could not get the list of installed packages!");
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine("{}");
                }
            }

            string json = File.ReadAllText(filePath);

            Dictionary<string, InstalledPackage> installedPkgs = JsonSerializer.Deserialize<Dictionary<string, InstalledPackage>>(json)
                                ?? new Dictionary<string, InstalledPackage>();

            return installedPkgs;
        }

        public static void SaveInstalledPackages(Dictionary<string, InstalledPackage> installedPkgs)
        {
            string json = JsonSerializer.Serialize(installedPkgs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(App.DataDir, "InstalledPackages.json"), json);
        }

        public static void UpdateInstalledPackage(string familyName, InstalledPackage installedPkg)
        {
            Dictionary<string, InstalledPackage> installedPkgs = GetInstalledPackages();

            installedPkgs[familyName] = installedPkg;
            SaveInstalledPackages(installedPkgs);
        }

        public static bool RemoveSymlinks(string familyName)
        {
            var pkg = GetInstalledPackage(familyName).Value;
            var pkgMount = Packages.GetPackageByFamilyName(pkg.familyName).EffectivePath;

            if (Directory.Exists(pkgMount))
            {
                foreach (var symlink in pkg.installedPackage.SymlinkedDLLs)
                {
                    string symlinkPath = Path.Combine(pkgMount, symlink);
                    if (File.Exists(symlinkPath))
                    {
                        var attributes = File.GetAttributes(symlinkPath);
                        if (attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            File.Delete(symlinkPath);
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static (string familyName, InstalledPackage installedPackage)? GetInstalledPackage(string familyName)
        {
            Dictionary<string, InstalledPackage> installedPkgs = GetInstalledPackages();
            if (installedPkgs.TryGetValue(familyName, out InstalledPackage installedPkg))
                return (familyName, installedPkg);

            // this should never happen (but in case it does)
            throw new KeyNotFoundException($"{familyName} was not found in the installed packages list.");
        }

        public static void AddInstalledPackage(Package package)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinDurango");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, "InstalledPackages.json");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("Could not get the list of installed packages!");
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine("{}");
                }
            }

            string json = File.ReadAllText(filePath);
            var installedPkgs = JsonSerializer.Deserialize<Dictionary<string, InstalledPackage>>(json) ?? new Dictionary<string, InstalledPackage>();

            if (installedPkgs.ContainsKey(package.Id.FamilyName))
            {
                return;
            }

            installedPkgs[package.Id.FamilyName] = new InstalledPackage
            {
                FullName = package.Id.FullName,
                SymlinkedDLLs = new List<string>()
            };

            string updated = JsonSerializer.Serialize(installedPkgs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updated);
        }
    }
}