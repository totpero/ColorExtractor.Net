using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

const int size = 512;
string outputPath = args.Length > 0 ? args[0] : "icon.png";

// Five palette colors "extracted" from the frame — evoking a sunset landscape.
var swatches = new[]
{
    Color.ParseHex("#1E3A5F"), // deep blue
    Color.ParseHex("#E8A530"), // amber
    Color.ParseHex("#D14B3D"), // terracotta red
    Color.ParseHex("#6EBE8F"), // sage green
    Color.ParseHex("#F4E4C1"), // cream
};

using var image = new Image<Rgba32>(size, size, Color.Transparent);

image.Mutate(ctx =>
{
    // --- Rounded "image" card on the left -------------------------------------
    const float cardW = 288f;
    const float cardH = 288f;
    const float cardX = 48f;
    const float cardY = (size - cardH) / 2f;
    const float cardRadius = 40f;

    // Soft drop shadow (offset rounded rect at reduced opacity)
    ctx.Fill(
        new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = 0.22f } },
        Color.Black,
        BuildRoundedRect(cardX + 8, cardY + 12, cardW, cardH, cardRadius));

    // Gradient representing a colorful photo (top-left blue → bottom-right amber)
    var gradient = new LinearGradientBrush(
        new PointF(cardX, cardY),
        new PointF(cardX + cardW, cardY + cardH),
        GradientRepetitionMode.None,
        new ColorStop(0.00f, swatches[0]),
        new ColorStop(0.35f, swatches[2]),
        new ColorStop(0.70f, swatches[1]),
        new ColorStop(1.00f, swatches[4]));

    var cardPath = BuildRoundedRect(cardX, cardY, cardW, cardH, cardRadius);
    ctx.Fill(gradient, cardPath);

    // Sun motif
    float sunCx = cardX + cardW * 0.72f;
    float sunCy = cardY + cardH * 0.30f;
    ctx.Fill(
        new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = 0.92f } },
        swatches[4],
        new EllipsePolygon(sunCx, sunCy, 28f));

    // Mountain silhouette clipped to the card
    var mountains = new Polygon(new LinearLineSegment(new[]
    {
        new PointF(cardX, cardY + cardH),
        new PointF(cardX, cardY + cardH * 0.72f),
        new PointF(cardX + cardW * 0.28f, cardY + cardH * 0.48f),
        new PointF(cardX + cardW * 0.48f, cardY + cardH * 0.68f),
        new PointF(cardX + cardW * 0.66f, cardY + cardH * 0.55f),
        new PointF(cardX + cardW, cardY + cardH * 0.78f),
        new PointF(cardX + cardW, cardY + cardH),
        new PointF(cardX, cardY + cardH),
    }));
    var clipped = mountains.Clip(cardPath);
    ctx.Fill(
        new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = 0.60f } },
        swatches[0],
        clipped);

    // Card border
    ctx.Draw(new SolidPen(Color.ParseHex("#1C1C1C"), 3f), cardPath);

    // --- Swatches being "extracted" to the right ------------------------------
    float swatchCx = cardX + cardW + 78f;
    float swatchTopY = cardY + 20f;
    float swatchDia = 56f;
    float gapY = 52f;

    for (int i = 0; i < swatches.Length; i++)
    {
        float cy = swatchTopY + i * gapY;
        float cx = swatchCx + (i % 2 == 0 ? 0f : 28f);

        // Connector line from card edge to swatch
        var line = new Polygon(new LinearLineSegment(new[]
        {
            new PointF(cardX + cardW + 6f, cy),
            new PointF(cx - swatchDia / 2f - 2f, cy),
        }));
        ctx.Draw(new SolidPen(Color.ParseHex("#1C1C1C"), 2.5f), line);

        var circle = new EllipsePolygon(cx, cy, swatchDia / 2f);
        ctx.Fill(swatches[i], circle);
        ctx.Draw(new SolidPen(Color.ParseHex("#1C1C1C"), 3f), circle);
    }
});

// Downscale the rendered 512x512 artwork to a smaller final icon. This keeps the
// image crisp (anti-aliased from a larger source) but produces a file whose
// natural size looks good inline in GitHub README / NuGet markdown — neither of
// which accept HTML-based sizing. Sherlock.Net uses the same trick.
const int finalSize = 216;
using (var final = image.Clone(c => c.Resize(finalSize, finalSize)))
{
    final.Save(outputPath);
}
Console.WriteLine($"Wrote {outputPath} ({finalSize}x{finalSize}, downscaled from {size}x{size})");

static IPath BuildRoundedRect(float x, float y, float w, float h, float r)
{
    var rect = new RectangleF(x, y, w, h);
    var outer = new RectangularPolygon(rect);
    // Round corners by intersecting with big circles at each corner — simpler:
    // use SixLabors convex corner cutter via Polygon boolean ops.
    // Simplest correct approach: build the rounded rect from arcs via IPathCollection.
    var pb = new PathBuilder();
    pb.StartFigure();
    pb.AddLine(new PointF(x + r, y), new PointF(x + w - r, y));
    pb.AddArc(new RectangleF(x + w - 2 * r, y, 2 * r, 2 * r), 0f, -90f, 90f);
    pb.AddLine(new PointF(x + w, y + r), new PointF(x + w, y + h - r));
    pb.AddArc(new RectangleF(x + w - 2 * r, y + h - 2 * r, 2 * r, 2 * r), 0f, 0f, 90f);
    pb.AddLine(new PointF(x + w - r, y + h), new PointF(x + r, y + h));
    pb.AddArc(new RectangleF(x, y + h - 2 * r, 2 * r, 2 * r), 0f, 90f, 90f);
    pb.AddLine(new PointF(x, y + h - r), new PointF(x, y + r));
    pb.AddArc(new RectangleF(x, y, 2 * r, 2 * r), 0f, 180f, 90f);
    pb.CloseFigure();
    return pb.Build();
}
