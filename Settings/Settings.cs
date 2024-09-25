using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Settings;

public class WdSettingsData
{
    public enum ThemeSetting
    {
        Fluent,
        FluentThin,
        Mica,
        MicaAlt
    }

    public uint SaveVersion { get; set; } = App.VerPacked;
    public ThemeSetting Theme { get; set; } = ThemeSetting.Fluent;
}

public class WdSettings
{
    private readonly string _settingsFile = Path.Combine(App.DataDir, "settings.json");
    public WdSettingsData Settings { get; private set; }

    public WdSettings()
    {
        Settings = GetDefaults();
        if (!Directory.Exists(App.DataDir))
            Directory.CreateDirectory(App.DataDir);

        if (File.Exists(_settingsFile))
        {
            try
            {
                string json = File.ReadAllText(_settingsFile);
                WdSettingsData loadedSettings = JsonSerializer.Deserialize<WdSettingsData>(json);

                if (loadedSettings != null)
                {
                    if (loadedSettings.SaveVersion > App.VerPacked)
                    {
                        BackupSettings();
                        GenerateSettings();
                        Logger.Instance.WriteInformation($"Settings were reset due to the settings file version being too new. ({loadedSettings.SaveVersion})");
                    }
                    loadedSettings = JsonSerializer.Deserialize<WdSettingsData>(json);
                    Settings = loadedSettings;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteError("Error loading settings: " + ex.Message);
                GenerateSettings();
            }
        }
        else
        {
            Logger.Instance.WriteWarning($"Settings file doesn't exist");
            GenerateSettings();
        }
    }

    private void BackupSettings()
    {
        string settingsBackup = _settingsFile + ".old_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
        Logger.Instance.WriteInformation($"Backing up settings.json to {settingsBackup}");
        File.Move(_settingsFile, settingsBackup);
    }

    private async void GenerateSettings()
    {
        Logger.Instance.WriteInformation($"Generating settings file...");
        Settings = GetDefaults();
        await SaveSettings();
    }

    private static WdSettingsData GetDefaults() => new WdSettingsData();

    public void MigrateSettings()
    {
        // unused for now
    }

    public async Task SaveSettings()
    {
        try
        {
            Logger.Instance.WriteInformation($"Saving settings...");
            Settings.SaveVersion = App.VerPacked;
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(Settings, options);
            await File.WriteAllTextAsync(_settingsFile, json);
            App.MainWindow.LoadSettings();
        }
        catch (Exception ex)
        {
            Logger.Instance.WriteError("Error saving settings: " + ex.Message);
        }
    }

    public async Task SetSetting(string setting, object value)
    {
        PropertyInfo property = typeof(WdSettingsData).GetProperty(setting);

        if (property != null && property.CanWrite)
        {
            try
            {
                property.SetValue(Settings, Convert.ChangeType(value, property.PropertyType));
                await SaveSettings();
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteWarning($"Error setting {setting}: " + ex.Message);
            }
        }
        else
        {
            Logger.Instance.WriteError($"Setting {setting} does not exist... this shouldn't happen.");
        }
    }
}
