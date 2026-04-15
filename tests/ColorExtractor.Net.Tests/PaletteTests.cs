using Shouldly;

namespace ColorExtractor.Net.Tests;

public class PaletteTests
{
    private static readonly string AssetsDir = Path.Combine(AppContext.BaseDirectory, "assets");

    private static string JpegPath => Path.Combine(AssetsDir, "test.jpeg");
    private static string GifPath => Path.Combine(AssetsDir, "test.gif");
    private static string PngPath => Path.Combine(AssetsDir, "test.png");
    private static string WebpPath => Path.Combine(AssetsDir, "test.webp");
    private static string TransparentPngPath => Path.Combine(AssetsDir, "red-transparent-50.png");

    [Fact]
    public void JpegExtractSingleColor()
    {
        var extractor = new ColorExtractor(Palette.FromFilename(JpegPath));
        var colors = extractor.Extract(1);

        colors.Length.ShouldBe(1);
        // JPEG decoding is lossy and differs slightly between decoders (ImageSharp vs libjpeg/GD),
        // so we compare components with a small tolerance instead of the PHP-reference exact value.
        var (r, g, b) = Color.FromIntToRgb(colors[0]);
        var (er, eg, eb) = Color.FromIntToRgb(15985688);
        Math.Abs(r - er).ShouldBeLessThanOrEqualTo(2);
        Math.Abs(g - eg).ShouldBeLessThanOrEqualTo(2);
        Math.Abs(b - eb).ShouldBeLessThanOrEqualTo(2);
    }

    [Fact]
    public void GifExtractSingleColor()
    {
        var extractor = new ColorExtractor(Palette.FromFilename(GifPath));
        var colors = extractor.Extract(1);

        colors.Length.ShouldBe(1);
        colors[0].ShouldBe(12022491);
    }

    [Fact]
    public void PngExtractSingleColor()
    {
        var extractor = new ColorExtractor(Palette.FromFilename(PngPath));
        var colors = extractor.Extract(1);

        colors.Length.ShouldBe(1);
        colors[0].ShouldBe(14024704);
    }

    [Fact]
    public void WebpExtractSingleColor()
    {
        var extractor = new ColorExtractor(Palette.FromFilename(WebpPath));
        var colors = extractor.Extract(1);

        colors.Length.ShouldBe(1);
        colors[0].ShouldBe(15008271);
    }

    [Fact]
    public void PngExtractMultipleColors()
    {
        var extractor = new ColorExtractor(Palette.FromFilename(PngPath));
        var colors = extractor.Extract(3);

        colors.ShouldBe(new[] { 14024704, 3407872, 7111569 });
    }

    [Fact]
    public void TransparencyHandling_SkippedWithoutBackground()
    {
        var palette = Palette.FromFilename(TransparentPngPath);
        palette.Count.ShouldBe(0);
    }

    [Fact]
    public void TransparencyHandling_WhiteBackground()
    {
        var palette = Palette.FromFilename(TransparentPngPath, Color.FromHexToInt("#FFFFFF"));
        palette.Count.ShouldBe(1);
        palette.GetColorCount(Color.FromHexToInt("#FF8080")).ShouldBe(1);
    }

    [Fact]
    public void TransparencyHandling_BlackBackground()
    {
        var palette = Palette.FromFilename(TransparentPngPath, Color.FromHexToInt("#000000"));
        palette.Count.ShouldBe(1);
        palette.GetColorCount(Color.FromHexToInt("#7E0000")).ShouldBe(1);
    }
}
