using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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

    public uint SaveVersion { get; set; }
    public ThemeSetting Theme { get; set; }
}

public class WdSettings
{
    private readonly string _settingsFile = Path.Combine(App.DataDir, "Settings.json");
    public WdSettingsData Settings { get; private set; }

    public WdSettings()
    {
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
                    }
                    Settings = loadedSettings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading settings: " + ex.Message);
                GenerateSettings();
            }
        }
        else
        {
            GenerateSettings();
        }
    }

    private void BackupSettings()
    {
        File.Move(_settingsFile, _settingsFile + ".old_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds().ToString());
    }

    private async void GenerateSettings()
    {
        WdSettingsData defaultSettings = GetDefaults();
        Settings = defaultSettings;
        await SaveSettings();
    }

    private static WdSettingsData GetDefaults()
    {
        return new WdSettingsData
        {
            SaveVersion = App.VerPacked,
            Theme = WdSettingsData.ThemeSetting.Fluent
        };
    }

    public void MigrateSettings()
    {
        // unused for now
    }

    public async Task SaveSettings()
    {
        try
        {
            Settings.SaveVersion = App.VerPacked;
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(Settings, options);
            await File.WriteAllTextAsync(_settingsFile, json);
            App.MainWindow.LoadSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error saving settings: " + ex.Message);
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
                Debug.WriteLine($"Error setting {setting}: " + ex.Message);
            }
        }
        else
        {
            Debug.WriteLine($"Property {setting} does not exist or is read only.");
        }
    }
}
