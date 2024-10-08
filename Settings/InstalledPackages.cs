﻿using System;
using System.Collections.Generic;
using System.IO;
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
                _ = Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, "InstalledPackages.json");

            if (!File.Exists(filePath))
            {
                Logger.WriteError("Could not get the list of installed packages!");
                using StreamWriter writer = File.CreateText(filePath);
                writer.WriteLine("{}");
            }

            string json = File.ReadAllText(filePath);
            var installedPkgs = JsonSerializer.Deserialize<Dictionary<string, InstalledPackage>>(json) ?? [];

            if (installedPkgs.TryGetValue(pkg.Id.FamilyName, out var package) && package.FullName == pkg.Id.FullName)
            {
                _ = installedPkgs.Remove(pkg.Id.FamilyName);
            }
            else
            {
                Logger.WriteError($"Couldn't uninstall {pkg.Id.FamilyName} as it was not found in the package list.");
                return;
            }

            string updated = JsonSerializer.Serialize(installedPkgs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updated);
            Logger.WriteInformation($"Removed {pkg.DisplayName} ({pkg.Id.FamilyName}) from the InstalledPackages list.");
        }


        public static Dictionary<string, InstalledPackage> GetInstalledPackages()
        {
            if (!Directory.Exists(App.DataDir))
            {
                _ = Directory.CreateDirectory(App.DataDir);
            }

            string filePath = Path.Combine(App.DataDir, "InstalledPackages.json");

            if (!File.Exists(filePath))
            {
                Logger.WriteError("Could not get the list of installed packages!");
                using StreamWriter writer = File.CreateText(filePath);
                writer.WriteLine("{}");
            }

            string json = File.ReadAllText(filePath);

            Dictionary<string, InstalledPackage> installedPkgs = JsonSerializer.Deserialize<Dictionary<string, InstalledPackage>>(json)
                                ?? [];

            return installedPkgs;
        }

        public static void SaveInstalledPackages(Dictionary<string, InstalledPackage> installedPkgs)
        {
            string json = JsonSerializer.Serialize(installedPkgs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(App.DataDir, "InstalledPackages.json"), json);
            Logger.WriteDebug($"Saved InstalledPackages.json");
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
            return installedPkgs.TryGetValue(familyName, out InstalledPackage installedPkg) ? (familyName, installedPkg) : null;
        }

        public static void AddInstalledPackage(Package package)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinDurango");
            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, "InstalledPackages.json");

            if (!File.Exists(filePath))
            {
                Logger.WriteError("Could not get the list of installed packages!");
                using StreamWriter writer = File.CreateText(filePath);
                writer.WriteLine("{}");
            }

            string json = File.ReadAllText(filePath);
            var installedPkgs = JsonSerializer.Deserialize<Dictionary<string, InstalledPackage>>(json) ?? [];

            if (installedPkgs.ContainsKey(package.Id.FamilyName))
            {
                Logger.WriteError($"Couldn't add {package.DisplayName} as it already exists in the InstalledPackages JSON.");
                return;
            }

            installedPkgs[package.Id.FamilyName] = new InstalledPackage
            {
                FullName = package.Id.FullName,
                SymlinkedDLLs = []
            };

            string updated = JsonSerializer.Serialize(installedPkgs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updated);
            Logger.WriteInformation($"Added {package.DisplayName} ({package.Id.FamilyName}) to the InstalledPackages list.");
        }
    }
}