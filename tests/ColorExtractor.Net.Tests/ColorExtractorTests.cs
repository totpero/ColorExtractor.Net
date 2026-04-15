using Shouldly;

namespace ColorExtractor.Net.Tests;

public class ColorExtractorTests
{
    private static readonly string AssetsDir = Path.Combine(AppContext.BaseDirectory, "assets");

    public static IEnumerable<object[]> ExtractData()
    {
        var google = Path.Combine(AssetsDir, "google.png");
        var empty = Path.Combine(AssetsDir, "empty.png");

        yield return new object[] { google, 0, Array.Empty<int>() };
        yield return new object[] { google, 1, new[] { 18417 } };
        yield return new object[] { google, 2, new[] { 18417, 42259 } };
        yield return new object[] { google, 3, new[] { 18417, 15080241, 42259 } };
        yield return new object[] { google, 4, new[] { 18417, 15080241, 42259, 16360960 } };
        yield return new object[] { google, 5, new[] { 18417, 15080241, 42259, 16360960, 4753405 } };
        yield return new object[] { empty, 0, Array.Empty<int>() };
        yield return new object[] { empty, 1, Array.Empty<int>() };
    }

    [Theory]
    [MemberData(nameof(ExtractData))]
    public void Extract(string imagePath, int colorCount, int[] expected)
    {
        var palette = Palette.FromFilename(imagePath);
        var extractor = new ColorExtractor(palette);
        var colors = extractor.Extract(colorCount);

        colors.ShouldBe(expected);
    }
}
