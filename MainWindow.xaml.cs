using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Pages;
using WinDurango.UI.Settings;

namespace WinDurango.UI
{
    public sealed partial class MainWindow : Window
    {
        public static readonly WdSettings Settings = new();
        public readonly string AppName = "WinDurango";
        public AppsListPage AppsListPage;
        public SettingsPage SettingsPage;
        public AboutPage AboutPage;

        private void NavigationInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                contentFrame.Navigate(typeof(SettingsPage));
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

                if (contentFrame.Navigate(pageType))
                {
                    if (contentFrame.Content is AppsListPage appsList)
                    {
                        AppsListPage = appsList;
                    }
                }
            }
        }

        public void ReloadAppList()
        {
            if (AppsListPage != null)
                AppsListPage.InitAppList();
        }

        public void LoadSettings()
        {
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
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            Title = AppName;

            // setup theme and titlebar
            AppWindow appWindow = this.AppWindow;
            ExtendsContentIntoTitleBar = true;

            Microsoft.UI.Composition.Compositor compositor = this.Compositor;

            this.Activate();
            LoadSettings();

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
