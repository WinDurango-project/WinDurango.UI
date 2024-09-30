using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;
using WinUI3Localizer;

namespace WinDurango.UI
{
    public partial class App : Application
    {
        // constants
        public static readonly string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinDurango");
        public static readonly string AppDir = AppContext.BaseDirectory;
        private static readonly FileVersionInfo Fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        // versioning
        public static readonly uint Major = (uint)Fvi.ProductMajorPart;
        public static readonly uint Minor = (uint)Fvi.ProductMinorPart;
        public static readonly uint Patch = (uint)Fvi.ProductBuildPart;
        public static readonly string Hash = Fvi.ProductVersion.Contains("+")
                    ? Fvi.ProductVersion.Split("+")[1][..7]
                    : "HASHFAIL";
        public static readonly uint VerPacked = (Major << 22) | (Minor << 12) | Patch;
        public static readonly string Version = $"{Fvi.ProductMajorPart}.{Fvi.ProductMinorPart}.{Fvi.ProductBuildPart}_{Hash}"; // 1.0 will be when bugs are squashed and everything works correctly.
        // other
        public static readonly WdSettings Settings = new();
        public static readonly MainWindow MainWindow = new();

        public static (uint major, uint minor, uint patch) UnpackVersion(uint verPacked)
        {
            uint major = (verPacked >> 22) & 0x3FF;
            uint minor = (verPacked >> 12) & 0x3FF;
            uint patch = verPacked & 0xFFF;

            return (major, minor, patch);
        }

        private async Task InitializeLocalizer()
        {
            string StringsFolderPath = Path.Combine(AppContext.BaseDirectory, "Strings");
            StorageFolder stringsFolder = await StorageFolder.GetFolderFromPathAsync(StringsFolderPath);

            ILocalizer localizer = await new LocalizerBuilder()
                .AddStringResourcesFolderForLanguageDictionaries(StringsFolderPath)
                .SetOptions(options =>
                {
                    options.DefaultLanguage = "en-US";
                })
                .Build();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.WriteException($"{e.Exception.GetType().FullName}: {e.Exception.Message}");
        }

        public App()
        {
            this.UnhandledException += App_UnhandledException;
            InitializeComponent();
        }

        public static void OnClosed(object sender, WindowEventArgs args)
        {
            Logger.WriteInformation("Exiting");
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await InitializeLocalizer();
            Logger.WriteDebug("Showing MainWindow");
            MainWindow.Activate();
        }
    }
}
