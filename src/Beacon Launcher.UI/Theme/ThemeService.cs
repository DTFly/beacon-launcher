using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace BeaconLaunch.Theme;

/// <summary>
/// Projects an <see cref="AppTheme"/> onto the application-wide brush resources.
/// A custom accent overrides the accent tokens; all other tokens stay theme-owned.
/// </summary>
public static class ThemeService
{
    public static Color ResolveAccent(AppTheme theme, string? accentOverride) =>
        ColorHex.TryParse(accentOverride, out var customAccent)
            ? customAccent
            : theme.AccentText;

    public static string ResolveAccentHex(AppTheme theme, string? accentOverride) =>
        ColorHex.ToHex(ResolveAccent(theme, accentOverride));

    public static void Apply(AppTheme theme, string? accentOverride)
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        application.RequestedThemeVariant = theme.IsDark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

        var accentText = theme.AccentText;
        var accentButton = theme.AccentButton;
        var accentHover = theme.AccentHover;
        var accentSoft = theme.AccentSoft;
        var focusRing = theme.FocusRing;
        var navHover = theme.IsDark ? theme.BorderStrong : theme.Border;
        var surfaceHover = theme.IsDark
            ? ColorHex.Blend(theme.SurfaceSecondary, theme.BorderStrong, 0.45)
            : ColorHex.Blend(theme.SurfaceSecondary, theme.Border, 0.55);

        if (ColorHex.TryParse(accentOverride, out var customAccent))
        {
            accentText = customAccent;
            accentButton = customAccent;
            accentHover = theme.IsDark
                ? ColorHex.Blend(customAccent, Colors.White, 0.12)
                : ColorHex.Blend(customAccent, Colors.Black, 0.12);
            accentSoft = theme.IsDark
                ? ColorHex.Blend(theme.Background, customAccent, 0.22)
                : ColorHex.Blend(Colors.White, customAccent, 0.08);
            focusRing = theme.IsDark
                ? customAccent
                : ColorHex.Blend(Colors.White, customAccent, 0.55);
        }

        Set(application, "AppBackgroundBrush", theme.Background);
        Set(application, "TopBarBackgroundBrush", theme.Surface);
        Set(application, "TopBarBorderBrush", theme.Border);
        Set(application, "TextPrimaryBrush", theme.TextPrimary);
        Set(application, "TextSecondaryBrush", theme.TextSecondary);
        Set(application, "TextMutedBrush", theme.TextMuted);
        Set(application, "NavHoverBrush", navHover);
        Set(application, "NavSelectedBrush", accentSoft);
        Set(application, "SurfaceBrush", theme.Surface);
        Set(application, "SurfaceSecondaryBrush", theme.SurfaceSecondary);
        Set(application, "SurfaceBorderBrush", theme.Border);
        Set(application, "SurfaceBorderStrongBrush", theme.BorderStrong);
        Set(application, "SurfaceHoverBrush", surfaceHover);
        Set(application, "SettingsRailBrush", theme.SurfaceSecondary);
        Set(application, "SelectionBrush", ColorHex.WithAlpha(focusRing, 0x66));
        Set(application, "CaretBrush", theme.TextPrimary);

        Set(application, "AccentBrush", accentText);
        Set(application, "AccentButtonBrush", accentButton);
        Set(application, "AccentHoverBrush", accentHover);
        Set(application, "AccentSoftBrush", accentSoft);
        Set(application, "AccentOnBrush", ContrastingText(accentButton));
        Set(application, "FocusRingBrush", focusRing);

        Set(application, "SuccessBrush", theme.Success);
        Set(application, "WarningBrush", theme.Warning);
        Set(application, "DangerBrush", theme.Danger);
    }

    private static void Set(Application application, string key, Color color) =>
        application.Resources[key] = new SolidColorBrush(color);

    private static Color ContrastingText(Color background)
    {
        var luminance =
            0.2126 * Linear(background.R) +
            0.7152 * Linear(background.G) +
            0.0722 * Linear(background.B);

        // The crossover where black and white have equal contrast is ~0.179.
        return luminance > 0.179 ? Color.FromRgb(0x09, 0x09, 0x0B) : Colors.White;
    }

    private static double Linear(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
