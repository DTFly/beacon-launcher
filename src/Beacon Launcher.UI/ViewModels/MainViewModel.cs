using BeaconLaunch.Core.Configuration;
using BeaconLaunch.Theme;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BeaconLaunch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly LauncherConfig _config;
    private bool _isInitializing;

    public MainViewModel(ConfigService configService)
    {
        _configService = configService;
        _config = configService.Load();

        _isInitializing = true;
        LauncherName = _config.LauncherName;
        Theme = _config.Theme;
        AccentColor = _config.AccentColor;
        _isInitializing = false;

        ApplyThemeAndAccent();
    }

    /// <summary>Displayed at the far left of the top bar and in the window title.</summary>
    [ObservableProperty]
    public partial string LauncherName { get; set; } = "Beacon Launcher";

    /// <summary>"Light" or "Dark"; persisted in %APPDATA%\.bl\config.json.</summary>
    [ObservableProperty]
    public partial string Theme { get; set; } = "Light";

    /// <summary>Custom accent as #RRGGBB, or empty to follow the theme palette.</summary>
    [ObservableProperty]
    public partial string AccentColor { get; set; } = string.Empty;

    public bool HasCustomAccent => !string.IsNullOrWhiteSpace(AccentColor);

    public AppTheme CurrentTheme => AppThemes.Get(Theme);

    public string EffectiveAccentHex => ThemeService.ResolveAccentHex(CurrentTheme, AccentColor);

    public string AccentStatusText => HasCustomAccent
        ? "自定义颜色"
        : $"跟随主题 · {(CurrentTheme.IsDark ? "深色" : "浅色")}";

    public double SavedWindowWidth => _config.WindowWidth;

    public double SavedWindowHeight => _config.WindowHeight;

    partial void OnLauncherNameChanged(string value) => PersistConfig();

    partial void OnThemeChanged(string value)
    {
        if (_isInitializing)
        {
            return;
        }

        ApplyThemeAndAccent();
        NotifyAccentState();
        PersistConfig();
    }

    partial void OnAccentColorChanged(string value)
    {
        if (_isInitializing)
        {
            return;
        }

        ApplyThemeAndAccent();
        NotifyAccentState();
        PersistConfig();
    }

    public void SaveWindowSize(double width, double height)
    {
        if (!double.IsFinite(width) ||
            !double.IsFinite(height) ||
            width < LauncherConfig.MinWindowWidth ||
            height < LauncherConfig.MinWindowHeight ||
            width > LauncherConfig.MaxWindowDimension ||
            height > LauncherConfig.MaxWindowDimension)
        {
            return;
        }

        _config.WindowWidth = Math.Round(width);
        _config.WindowHeight = Math.Round(height);
        _configService.Save(_config);
    }

    private void ApplyThemeAndAccent() => ThemeService.Apply(CurrentTheme, AccentColor);

    private void NotifyAccentState()
    {
        OnPropertyChanged(nameof(HasCustomAccent));
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(EffectiveAccentHex));
        OnPropertyChanged(nameof(AccentStatusText));
    }

    private void PersistConfig()
    {
        if (_isInitializing)
        {
            return;
        }

        _config.LauncherName = LauncherName;
        _config.Theme = Theme;
        _config.AccentColor = AccentColor;
        _config.ThemeVersion = LauncherConfig.CurrentThemeVersion;
        _configService.Save(_config);
    }
}
