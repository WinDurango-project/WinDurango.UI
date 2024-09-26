using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using WinDurango.UI.Settings;

namespace WinDurango.UI.Pages
{
    public sealed partial class SettingsPage : Page
    {
        // should probably merge these into one?
        private async void OnMicaSelected(object sender, RoutedEventArgs e)
        {
            await MainWindow.Settings.SetSetting("Theme", WdSettingsData.ThemeSetting.Mica);
            OnThemeButtonLoaded(sender, e);
        }

        private async void OnMicaAltSelected(object sender, RoutedEventArgs e)
        {
            await MainWindow.Settings.SetSetting("Theme", WdSettingsData.ThemeSetting.MicaAlt);
            OnThemeButtonLoaded(sender, e);
        }

        private async void OnFluentSelected(object sender, RoutedEventArgs e)
        {
            await MainWindow.Settings.SetSetting("Theme", WdSettingsData.ThemeSetting.Fluent);
            OnThemeButtonLoaded(sender, e);
        }

        private async void OnSystemSelected(object sender, RoutedEventArgs e)
        {
            await MainWindow.Settings.SetSetting("Theme", WdSettingsData.ThemeSetting.System);
            OnThemeButtonLoaded(sender, e);
        }

        private void OpenAppData(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(App.DataDir) { UseShellExecute = true });
        }

        private void OnThemeButtonLoaded(object sender, RoutedEventArgs e)
        {
            switch (MainWindow.Settings.Settings.Theme)
            {
                case WdSettingsData.ThemeSetting.Fluent:
                    themeButton.Content = "Fluent";
                    break;
                case WdSettingsData.ThemeSetting.FluentThin:
                    themeButton.Content = "Fluent (Thin)";
                    break;
                case WdSettingsData.ThemeSetting.System:
                    themeButton.Content = "System (No Theme)";
                    break;
                case WdSettingsData.ThemeSetting.Mica:
                    themeButton.Content = "Mica";
                    break;
                case WdSettingsData.ThemeSetting.MicaAlt:
                    themeButton.Content = "Mica (Alt)";
                    break;
            }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
