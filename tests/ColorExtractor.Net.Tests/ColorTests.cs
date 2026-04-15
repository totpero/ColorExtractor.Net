using Shouldly;

namespace ColorExtractor.Net.Tests;

public class ColorTests
{
    [Fact]
    public void FromIntToHex_PrependsHashByDefault()
    {
        Color.FromIntToHex(0xFF8080).ShouldBe("#FF8080");
    }

    [Fact]
    public void FromIntToHex_OmitsHashWhenRequested()
    {
        Color.FromIntToHex(0xFF8080, prependHash: false).ShouldBe("FF8080");
    }

    [Fact]
    public void FromHexToInt_HandlesLeadingHash()
    {
        Color.FromHexToInt("#FF8080").ShouldBe(0xFF8080);
    }

    [Fact]
    public void FromHexToInt_WithoutHash()
    {
        Color.FromHexToInt("FF8080").ShouldBe(0xFF8080);
    }

    [Fact]
    public void FromIntToRgb_SplitsComponents()
    {
        var rgb = Color.FromIntToRgb(0xFF8040);
        rgb.R.ShouldBe(0xFF);
        rgb.G.ShouldBe(0x80);
        rgb.B.ShouldBe(0x40);
    }

    [Fact]
    public void FromRgbToInt_PacksComponents()
    {
        Color.FromRgbToInt(0xFF, 0x80, 0x40).ShouldBe(0xFF8040);
    }
}
