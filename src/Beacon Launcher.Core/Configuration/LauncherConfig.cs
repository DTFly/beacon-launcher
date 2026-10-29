namespace BeaconLaunch.Core.Configuration;

/// <summary>
/// Persistent launcher settings. Stored in %APPDATA%\.bl\config.json.
/// </summary>
public sealed class LauncherConfig
{
    public const int CurrentThemeVersion = 2;

    public const double MinWindowWidth = 960;
    public const double MinWindowHeight = 600;
    public const double MaxWindowDimension = 10000;

    /// <summary>Bumped when the theme token set changes.</summary>
    public int ThemeVersion { get; set; }

    public string LauncherName { get; set; } = "Beacon Launcher";

    /// <summary>"Light" or "Dark".</summary>
    public string Theme { get; set; } = "Light";

    /// <summary>Custom accent as #RRGGBB; empty means follow the theme.</summary>
    public string AccentColor { get; set; } = string.Empty;

    /// <summary>Last window width in device-independent pixels; 0 uses the default.</summary>
    public double WindowWidth { get; set; }

    /// <summary>Last window height in device-independent pixels; 0 uses the default.</summary>
    public double WindowHeight { get; set; }
}
