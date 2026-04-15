namespace ColorExtractor.Net;

/// <summary>
/// Extracts the most visually significant colors from a <see cref="Palette"/>
/// using the CIE Lab color space and CIEDE2000 delta-E to merge near-duplicates.
/// Port of <c>League\ColorExtractor\ColorExtractor</c>.
/// </summary>
public sealed class ColorExtractor
{
    private readonly Palette _palette;
    private int[]? _sortedColors;

    public ColorExtractor(Palette palette)
    {
        _palette = palette ?? throw new ArgumentNullException(nameof(palette));
    }

    /// <summary>
    /// Returns up to <paramref name="colorCount"/> distinct colors, ordered from most to
    /// least representative. Colors too similar (under CIEDE2000) are merged.
    /// </summary>
    public int[] Extract(int colorCount = 1)
    {
        if (colorCount == 0) return Array.Empty<int>();

        if (_sortedColors is null) Initialize();

        return MergeColors(_sortedColors!, colorCount, 100.0 / colorCount);
    }

    private void Initialize()
    {
        // PriorityQueue is a min-heap; negate priority so we pop the highest-priority color first.
        var queue = new PriorityQueue<int, double>();

        foreach (var kvp in _palette)
        {
            int color = kvp.Key;
            int count = kvp.Value;

            var lab = IntColorToLab(color);
            double chroma = Math.Sqrt(lab.A * lab.A + lab.B * lab.B);
            double priority = (chroma != 0 ? chroma : 1.0)
                              * (1 - lab.L / 200.0)
                              * Math.Sqrt(count);

            queue.Enqueue(color, -priority);
        }

        var sorted = new int[_palette.Count];
        int i = 0;
        while (queue.TryDequeue(out var color, out _))
        {
            sorted[i++] = color;
        }
        _sortedColors = sorted;
    }

    private static int[] MergeColors(int[] colors, int limit, double maxDelta)
    {
        limit = Math.Min(colors.Length, limit);
        if (limit == 0) return Array.Empty<int>();
        if (limit == 1) return new[] { colors[0] };

        var labCache = new LabColor[limit - 1];
        var merged = new List<int>(limit);

        foreach (var color in colors)
        {
            var colorLab = IntColorToLab(color);
            bool mergedAlready = false;

            for (int i = 0; i < merged.Count; i++)
            {
                if (Ciede2000DeltaE(colorLab, labCache[i]) < maxDelta)
                {
                    mergedAlready = true;
                    break;
                }
            }

            if (mergedAlready) continue;

            int mergedColorCount = merged.Count;
            merged.Add(color);

            if (mergedColorCount + 1 == limit) break;

            labCache[mergedColorCount] = colorLab;
        }

        return merged.ToArray();
    }

    // --- Color science helpers (verbatim port of the PHP original, including its quirks) -------

    private readonly struct LabColor
    {
        public readonly double L;
        public readonly double A;
        public readonly double B;
        public LabColor(double l, double a, double b) { L = l; A = a; B = b; }
    }

    private static LabColor IntColorToLab(int color)
    {
        double r = (color >> 16) & 0xFF;
        double g = (color >> 8) & 0xFF;
        double b = color & 0xFF;

        double sr = RgbToSrgbStep(r);
        double sg = RgbToSrgbStep(g);
        double sb = RgbToSrgbStep(b);

        double x = (.4124564 * sr) + (.3575761 * sg) + (.1804375 * sb);
        double y = (.2126729 * sr) + (.7151522 * sg) + (.0721750 * sb);
        double z = (.0193339 * sr) + (.1191920 * sg) + (.9503041 * sb);

        // D65 reference white
        const double Xn = .95047;
        const double Yn = 1.0;
        const double Zn = 1.08883;

        double fx = XyzToLabStep(x / Xn);
        double fy = XyzToLabStep(y / Yn);
        double fz = XyzToLabStep(z / Zn);

        return new LabColor(
            116 * fy - 16,
            500 * (fx - fy),
            200 * (fy - fz));
    }

    private static double RgbToSrgbStep(double value)
    {
        value /= 255.0;
        return value <= .03928
            ? value / 12.92
            : Math.Pow((value + .055) / 1.055, 2.4);
    }

    private static double XyzToLabStep(double value)
        => value > 216.0 / 24389.0
            ? Math.Pow(value, 1.0 / 3.0)
            : 841.0 * value / 108.0 + 4.0 / 29.0;

    private static double Ciede2000DeltaE(LabColor first, LabColor second)
    {
        // NOTE: this is a direct port of the PHP library's implementation, which mixes
        // radian outputs from atan2 with literal degree constants. Preserved verbatim so
        // deltas match the reference library byte-for-byte.

        double C1 = Math.Sqrt(first.A * first.A + first.B * first.B);
        double C2 = Math.Sqrt(second.A * second.A + second.B * second.B);
        double Cb = (C1 + C2) / 2.0;

        double Cb7 = Math.Pow(Cb, 7);
        double twentyFive7 = Math.Pow(25, 7);

        double G = .5 * (1 - Math.Sqrt(Cb7 / (Cb7 + twentyFive7)));

        double a1p = (1 + G) * first.A;
        double a2p = (1 + G) * second.A;

        double C1p = Math.Sqrt(a1p * a1p + first.B * first.B);
        double C2p = Math.Sqrt(a2p * a2p + second.B * second.B);

        double h1p = (a1p == 0 && first.B == 0) ? 0 : Math.Atan2(first.B, a1p);
        double h2p = (a2p == 0 && second.B == 0) ? 0 : Math.Atan2(second.B, a2p);

        double LpDelta = second.L - first.L;
        double CpDelta = C2p - C1p;

        double hpDelta;
        if (C1p * C2p == 0)
        {
            hpDelta = 0;
        }
        else if (Math.Abs(h2p - h1p) <= 180)
        {
            hpDelta = h2p - h1p;
        }
        else if (h2p - h1p > 180)
        {
            hpDelta = h2p - h1p - 360;
        }
        else
        {
            hpDelta = h2p - h1p + 360;
        }

        double HpDelta = 2 * Math.Sqrt(C1p * C2p) * Math.Sin(hpDelta / 2);

        double Lbp = (first.L + second.L) / 2.0;
        double Cbp = (C1p + C2p) / 2.0;

        double hbp;
        if (C1p * C2p == 0)
        {
            hbp = h1p + h2p;
        }
        else if (Math.Abs(h1p - h2p) <= 180)
        {
            hbp = (h1p + h2p) / 2.0;
        }
        else if (h1p + h2p < 360)
        {
            hbp = (h1p + h2p + 360) / 2.0;
        }
        else
        {
            hbp = (h1p + h2p - 360) / 2.0;
        }

        double T = 1
                   - .17 * Math.Cos(hbp - 30)
                   + .24 * Math.Cos(2 * hbp)
                   + .32 * Math.Cos(3 * hbp + 6)
                   - .20 * Math.Cos(4 * hbp - 63);

        double sigmaDelta = 30 * Math.Exp(-Math.Pow((hbp - 275) / 25, 2));

        double Cbp7 = Math.Pow(Cbp, 7);
        double Rc = 2 * Math.Sqrt(Cbp7 / (Cbp7 + twentyFive7));

        double Sl = 1 + ((.015 * Math.Pow(Lbp - 50, 2)) / Math.Sqrt(20 + Math.Pow(Lbp - 50, 2)));
        double Sc = 1 + .045 * Cbp;
        double Sh = 1 + .015 * Cbp * T;

        double Rt = -Math.Sin(2 * sigmaDelta) * Rc;

        return Math.Sqrt(
            Math.Pow(LpDelta / Sl, 2)
            + Math.Pow(CpDelta / Sc, 2)
            + Math.Pow(HpDelta / Sh, 2)
            + Rt * (CpDelta / Sc) * (HpDelta / Sh));
    }
}
