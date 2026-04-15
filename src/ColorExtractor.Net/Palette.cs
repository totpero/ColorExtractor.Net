using System.Collections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorExtractor.Net;

/// <summary>
/// A palette is an ordered map from 24-bit RGB colors (keys) to pixel counts (values),
/// built from an image. Iteration yields colors sorted from most-used to least-used.
/// Port of <c>League\ColorExtractor\Palette</c>.
/// </summary>
public sealed class Palette : IEnumerable<KeyValuePair<int, int>>, IReadOnlyCollection<KeyValuePair<int, int>>
{
    private List<KeyValuePair<int, int>> _colors;

    private Palette()
    {
        _colors = new List<KeyValuePair<int, int>>();
    }

    /// <summary>Total number of distinct colors in the palette.</summary>
    public int Count => _colors.Count;

    /// <summary>Returns how many pixels used the given 24-bit RGB color, or 0 if the color is absent.</summary>
    public int GetColorCount(int color)
    {
        foreach (var kvp in _colors)
        {
            if (kvp.Key == color) return kvp.Value;
        }
        return 0;
    }

    /// <summary>Returns the most-used colors (up to <paramref name="limit"/>), ordered by usage descending.</summary>
    public IReadOnlyList<KeyValuePair<int, int>> GetMostUsedColors(int? limit = null)
    {
        if (limit is null || limit.Value >= _colors.Count)
            return _colors.AsReadOnly();
        return _colors.Take(limit.Value).ToList().AsReadOnly();
    }

    public IEnumerator<KeyValuePair<int, int>> GetEnumerator() => _colors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Load a palette from a file path.</summary>
    public static Palette FromFilename(string filename, int? backgroundColor = null)
    {
        if (filename is null) throw new ArgumentNullException(nameof(filename));
        if (!File.Exists(filename))
            throw new ArgumentException("Filename must be a valid path and should be readable", nameof(filename));

        using var image = Image.Load<Rgba32>(filename);
        return FromImage(image, backgroundColor);
    }

    /// <summary>Load a palette from a byte array containing an encoded image.</summary>
    public static Palette FromContents(byte[] contents, int? backgroundColor = null)
    {
        if (contents is null) throw new ArgumentNullException(nameof(contents));
        using var image = Image.Load<Rgba32>(contents);
        return FromImage(image, backgroundColor);
    }

    /// <summary>Load a palette from a stream containing an encoded image.</summary>
    public static Palette FromStream(Stream stream, int? backgroundColor = null)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));
        using var image = Image.Load<Rgba32>(stream);
        return FromImage(image, backgroundColor);
    }

    /// <summary>
    /// Fetch an image from a URL and build a palette from it.
    /// </summary>
    public static async Task<Palette> FromUrlAsync(
        string url,
        int? backgroundColor = null,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        if (url is null) throw new ArgumentNullException(nameof(url));

        var client = httpClient ?? new HttpClient();
        try
        {
            var bytes = await client.GetByteArrayAsync(url, cancellationToken).ConfigureAwait(false);
            return FromContents(bytes, backgroundColor);
        }
        finally
        {
            if (httpClient is null) client.Dispose();
        }
    }

    /// <summary>
    /// Build a palette from an ImageSharp image. Pixels are iterated column-major (x outer, y inner)
    /// to match the original PHP implementation, which affects insertion order for tied counts.
    /// </summary>
    public static Palette FromImage(Image<Rgba32> image, int? backgroundColor = null)
    {
        if (image is null) throw new ArgumentNullException(nameof(image));
        if (backgroundColor.HasValue && (backgroundColor.Value < 0 || backgroundColor.Value > 0xFFFFFF))
            throw new ArgumentException(
                $"\"{backgroundColor.Value}\" does not represent a valid color",
                nameof(backgroundColor));

        var palette = new Palette();

        int width = image.Width;
        int height = image.Height;

        int bgR = (backgroundColor.GetValueOrDefault() >> 16) & 0xFF;
        int bgG = (backgroundColor.GetValueOrDefault() >> 8) & 0xFF;
        int bgB = backgroundColor.GetValueOrDefault() & 0xFF;

        // Copy pixel data out so we can index by (x, y) cheaply.
        var pixels = new Rgba32[width * height];
        image.CopyPixelDataTo(pixels);

        // Preserve insertion order (column-major like PHP) while counting occurrences.
        var counts = new Dictionary<int, int>();
        var order = new List<int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var px = pixels[y * width + x];

                // PHP GD uses a 0..127 alpha range where 0 = opaque and 127 = fully transparent.
                // Convert ImageSharp's 0..255 (255 = opaque) to match.
                int gdAlpha = 127 - (px.A * 127 + 127) / 255;

                int color;
                if (gdAlpha != 0)
                {
                    if (backgroundColor is null) continue;

                    double a = gdAlpha / 127.0;
                    int r = (int)(px.R * (1 - a) + bgR * a);
                    int g = (int)(px.G * (1 - a) + bgG * a);
                    int b = (int)(px.B * (1 - a) + bgB * a);
                    color = (r * 65536) + (g * 256) + b;
                }
                else
                {
                    color = (px.R << 16) | (px.G << 8) | px.B;
                }

                if (counts.TryGetValue(color, out var n))
                {
                    counts[color] = n + 1;
                }
                else
                {
                    counts[color] = 1;
                    order.Add(color);
                }
            }
        }

        // Sort descending by count, preserving first-seen order for ties (LINQ OrderBy is stable).
        palette._colors = order
            .Select(c => new KeyValuePair<int, int>(c, counts[c]))
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        return palette;
    }
}
