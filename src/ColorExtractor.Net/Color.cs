using System.Globalization;

namespace ColorExtractor.Net;

/// <summary>
/// Static helpers for converting between 24-bit RGB integer representations and common color formats.
/// Mirrors the API of <c>League\ColorExtractor\Color</c> from the PHP original.
/// </summary>
public static class Color
{
    /// <summary>
    /// Converts a 24-bit RGB integer (0xRRGGBB) to a hex string.
    /// </summary>
    public static string FromIntToHex(int color, bool prependHash = true)
        => (prependHash ? "#" : string.Empty) + color.ToString("X6", CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a hex color string (with or without leading '#') to a 24-bit RGB integer.
    /// </summary>
    public static int FromHexToInt(string color)
    {
        if (color is null) throw new ArgumentNullException(nameof(color));
        var trimmed = color.StartsWith('#') ? color[1..] : color;
        return int.Parse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a 24-bit RGB integer to an (R, G, B) component tuple.
    /// </summary>
    public static (int R, int G, int B) FromIntToRgb(int color)
        => ((color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF);

    /// <summary>
    /// Packs an (R, G, B) component tuple into a 24-bit RGB integer.
    /// </summary>
    public static int FromRgbToInt(int r, int g, int b)
        => (r * 65536) + (g * 256) + b;

    /// <summary>
    /// Packs an (R, G, B) component tuple into a 24-bit RGB integer.
    /// </summary>
    public static int FromRgbToInt((int R, int G, int B) rgb)
        => FromRgbToInt(rgb.R, rgb.G, rgb.B);
}
