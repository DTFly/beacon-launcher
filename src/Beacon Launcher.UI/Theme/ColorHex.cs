using System.Globalization;
using Avalonia.Media;

namespace BeaconLaunch.Theme;

/// <summary>Small helpers for the #RRGGBB colors stored in config and theme palettes.</summary>
public static class ColorHex
{
    public static bool TryParse(string? value, out Color color)
    {
        color = default;
        if (value is not { Length: 7 } || value[0] != '#')
        {
            return false;
        }

        if (!uint.TryParse(
                value.AsSpan(1),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var rgb))
        {
            return false;
        }

        color = Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        return true;
    }

    public static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    public static Color WithAlpha(Color color, byte alpha) =>
        Color.FromArgb(alpha, color.R, color.G, color.B);

    /// <summary>Mixes <paramref name="from"/> towards <paramref name="to"/> by 0..1.</summary>
    public static Color Blend(Color from, Color to, double amount)
    {
        amount = Math.Clamp(amount, 0d, 1d);
        return Color.FromRgb(
            Mix(from.R, to.R, amount),
            Mix(from.G, to.G, amount),
            Mix(from.B, to.B, amount));
    }

    private static byte Mix(byte from, byte to, double amount) =>
        (byte)Math.Round(from + (to - from) * amount);
}
