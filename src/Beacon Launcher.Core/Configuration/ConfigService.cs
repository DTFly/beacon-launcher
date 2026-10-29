using System.Globalization;
using System.Text.Json;

namespace BeaconLaunch.Core.Configuration;

/// <summary>
/// Owns the launcher's config directory. The directory is created on first use,
/// and writes go through a temp file so a failed write cannot corrupt settings.
/// </summary>
public sealed class ConfigService
{
    public const string ConfigFileName = "config.json";

    public ConfigService(string? directoryPath = null)
    {
        DirectoryPath = directoryPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".bl");
        ConfigFilePath = Path.Combine(DirectoryPath, ConfigFileName);
    }

    public string DirectoryPath { get; }

    public string ConfigFilePath { get; }

    public LauncherConfig Load()
    {
        Directory.CreateDirectory(DirectoryPath);

        if (!File.Exists(ConfigFilePath))
        {
            var defaults = new LauncherConfig
            {
                ThemeVersion = LauncherConfig.CurrentThemeVersion,
            };
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(ConfigFilePath);
            var config = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.LauncherConfig)
                         ?? new LauncherConfig();
            var changed = false;

            if (string.IsNullOrWhiteSpace(config.LauncherName))
            {
                config.LauncherName = "Beacon Launcher";
                changed = true;
            }

            if (config.Theme is not ("Dark" or "Light"))
            {
                config.Theme = "Light";
                changed = true;
            }

            // The old accent presets belonged to the previous theme system.
            // Start everyone on the theme's own accent once, then honor custom colors.
            if (config.ThemeVersion < LauncherConfig.CurrentThemeVersion)
            {
                config.AccentColor = string.Empty;
                config.ThemeVersion = LauncherConfig.CurrentThemeVersion;
                changed = true;
            }

            if (!string.IsNullOrEmpty(config.AccentColor) &&
                !IsValidAccentColor(config.AccentColor))
            {
                config.AccentColor = string.Empty;
                changed = true;
            }

            if (!IsValidWindowSize(config.WindowWidth, config.WindowHeight))
            {
                config.WindowWidth = 0;
                config.WindowHeight = 0;
                changed = true;
            }

            if (changed)
            {
                Save(config);
            }

            return config;
        }
        catch (Exception)
        {
            var defaults = new LauncherConfig
            {
                ThemeVersion = LauncherConfig.CurrentThemeVersion,
            };
            Save(defaults);
            return defaults;
        }
    }

    public bool Save(LauncherConfig config)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);

            var tempPath = ConfigFilePath + ".tmp";
            var json = JsonSerializer.Serialize(config, ConfigJsonContext.Default.LauncherConfig);
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, ConfigFilePath, overwrite: true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsValidWindowSize(double width, double height)
    {
        if (!double.IsFinite(width) || !double.IsFinite(height))
        {
            return false;
        }

        if (width == 0 && height == 0)
        {
            return true;
        }

        return width >= LauncherConfig.MinWindowWidth &&
               width <= LauncherConfig.MaxWindowDimension &&
               height >= LauncherConfig.MinWindowHeight &&
               height <= LauncherConfig.MaxWindowDimension;
    }

    private static bool IsValidAccentColor(string? value)
    {
        return value is { Length: 7 } &&
               value[0] == '#' &&
               uint.TryParse(
                   value.AsSpan(1),
                   NumberStyles.HexNumber,
                   CultureInfo.InvariantCulture,
                   out _);
    }
}
