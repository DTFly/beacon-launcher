using Avalonia.Media;

namespace BeaconLaunch.Theme;

/// <summary>The semantic color tokens used by the launcher chrome.</summary>
public sealed record AppTheme(
    string Id,
    bool IsDark,
    Color Background,
    Color Surface,
    Color SurfaceSecondary,
    Color Border,
    Color BorderStrong,
    Color TextPrimary,
    Color TextSecondary,
    Color TextMuted,
    Color AccentText,
    Color AccentButton,
    Color AccentHover,
    Color AccentSoft,
    Color FocusRing,
    Color Success,
    Color Warning,
    Color Danger);

public static class AppThemes
{
    public static AppTheme Light { get; } = new(
        "Light",
        false,
        Parse("#FAFAFA"),
        Parse("#FFFFFF"),
        Parse("#F4F4F5"),
        Parse("#E4E4E7"),
        Parse("#D4D4D8"),
        Parse("#18181B"),
        Parse("#52525B"),
        Parse("#A1A1AA"),
        Parse("#2563EB"),
        Parse("#2563EB"),
        Parse("#1D4ED8"),
        Parse("#EFF6FF"),
        Parse("#93C5FD"),
        Parse("#16A34A"),
        Parse("#D97706"),
        Parse("#DC2626"));

    public static AppTheme Dark { get; } = new(
        "Dark",
        true,
        Parse("#09090B"),
        Parse("#121214"),
        Parse("#18181B"),
        Parse("#27272A"),
        Parse("#3F3F46"),
        Parse("#FAFAFA"),
        Parse("#A1A1AA"),
        Parse("#71717A"),
        Parse("#60A5FA"),
        Parse("#2563EB"),
        Parse("#3B82F6"),
        Parse("#172554"),
        Parse("#3B82F6"),
        Parse("#4ADE80"),
        Parse("#FBBF24"),
        Parse("#F87171"));

    public static AppTheme Get(string? id) =>
        string.Equals(id, "Dark", StringComparison.OrdinalIgnoreCase) ? Dark : Light;

    private static Color Parse(string value) =>
        ColorHex.TryParse(value, out var color) ? color : Colors.Transparent;
}
