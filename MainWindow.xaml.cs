using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Pages;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;

namespace WinDurango.UI
{
    public sealed partial class MainWindow : Window
    {
        public readonly string AppName = "WinDurango";
        public static readonly WdSettings Settings = new();
        public AppsListPage AppsListPage;
        public SettingsPage SettingsPage;
        public AboutPage AboutPage;


        private void NavigationInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                if (contentFrame.Content?.GetType() != typeof(SettingsPage))
                {
                    _ = contentFrame.Navigate(typeof(SettingsPage));
                }
            }

            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                string tag = item.Tag.ToString();
                Type pageType = tag switch
                {
                    "AppsListPage" => typeof(AppsListPage),
                    "AboutPage" => typeof(AboutPage),
                    _ => typeof(AppsListPage)
                };

                if (contentFrame.Content?.GetType() != pageType)
                {
                    if (contentFrame.Navigate(pageType))
                    {
                        if (contentFrame.Content is AppsListPage appsList)
                        {
                            AppsListPage = appsList;
                        }
                    }
                }
            }
        }

        public void ReloadAppList()
        {
            AppsListPage?.InitAppList();
        }

        public void LoadSettings()
        {
            ExtendsContentIntoTitleBar = Settings.Settings.Theme != WdSettingsData.ThemeSetting.System;
            switch (Settings.Settings.Theme)
            {
                case WdSettingsData.ThemeSetting.Mica:
                    this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };
                    break;
                case WdSettingsData.ThemeSetting.MicaAlt:
                    this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
                    break;
                case WdSettingsData.ThemeSetting.Fluent:
                    this.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
                case WdSettingsData.ThemeSetting.FluentThin:
                    this.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
                case WdSettingsData.ThemeSetting.System:
                    this.SystemBackdrop = null;
                    break;
            }
        }

        private void OnNavigate(object sender, NavigatingCancelEventArgs e)
        {
            Logger.WriteDebug($"Switching to page {e.SourcePageType.Name}");
        }

        public MainWindow()
        {
            Title = AppName;
            AppWindow.SetIcon("ms-appx:///Assets/icon.ico");
            this.Activate();
            LoadSettings();

            this.InitializeComponent();
            contentFrame.Navigating += OnNavigate;

            contentFrame.Navigate(typeof(AppsListPage));
            AppsListPage = (AppsListPage)contentFrame.Content;
        }

        private async void appTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            // have to do it here otherwise instance error
            var devNotice = new NoticeDialog($"This UI is very early in development, and mainly developed by a C# learner... There WILL be bugs, and some things will NOT work...\n\nDevelopers, check Readme.md in the repo for the todolist.", "Important");
            await devNotice.Show();
            if (ExtendsContentIntoTitleBar)
            {
                SetupTitleBar();
            }
        }

        private void appTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ExtendsContentIntoTitleBar)
            {
                SetupTitleBar();
            }
        }

        private void SetupTitleBar()
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;
            double scaleAdjustment = appTitleBar.XamlRoot.RasterizationScale;
            rightPaddingColumn.Width = new GridLength(titleBar.RightInset / scaleAdjustment);
            leftPaddingColumn.Width = new GridLength(titleBar.LeftInset / scaleAdjustment);
        }
    }
}
