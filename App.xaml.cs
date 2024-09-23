using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WinDurango.UI
{
    public partial class App : Application
    {
        private static readonly FileVersionInfo Fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        public static readonly string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinDurango");
        public static readonly MainWindow MainWindow = new();
        public static readonly uint Major = (uint)Fvi.ProductMajorPart;
        public static readonly uint Minor = (uint)Fvi.ProductMinorPart;
        public static readonly uint Patch = (uint)Fvi.ProductBuildPart;
        public static readonly string Hash = Fvi.ProductVersion.Split("+")[1].Substring(0, 7);
        public static readonly uint VerPacked = (Major << 22) | (Minor << 12) | Patch;
        public static readonly string Version = $"{Fvi.ProductMajorPart}.{Fvi.ProductMinorPart}.{Fvi.ProductBuildPart}_{Hash}"; // 1.0 will be when bugs are squashed and everything works correctly.

        public static (uint major, uint minor, uint patch) UnpackVersion(uint verPacked)
        {
            uint major = (verPacked >> 22) & 0x3FF;
            uint minor = (verPacked >> 12) & 0x3FF;
            uint patch = verPacked & 0xFFF;

            return (major, minor, patch);
        }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow.Activate();
        }
    }
}
