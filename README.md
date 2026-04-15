# ColorExtractor.Net

[![.NET](https://img.shields.io/badge/.NET-8.0%20|%209.0%20|%2010.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

A cross-platform .NET port of [thephpleague/color-extractor](https://github.com/thephpleague/color-extractor).

Extracts the most visually significant colors from an image, the way a human would — by mapping pixels to the CIE Lab color space and merging near-duplicates via CIEDE2000 delta-E.

Supports PNG, JPEG, GIF, WebP, BMP, and TGA via [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp).

---

## Install

```bash
dotnet add package ColorExtractor.Net
```

## Usage

```csharp
using ColorExtractor.Net;

// Load a palette from a file
var palette = Palette.FromFilename("photo.jpg");

// Or from bytes / a stream / a URL
// var palette = Palette.FromContents(bytes);
// var palette = Palette.FromStream(stream);
// var palette = await Palette.FromUrlAsync("https://…/image.png");

// Extract the top N visually distinct colors
var extractor = new ColorExtractor(palette);
int[] colors = extractor.Extract(5);

foreach (var color in colors)
    Console.WriteLine(Color.FromIntToHex(color)); // e.g. #F3ED98
```

### Handling transparency

By default, transparent pixels are skipped. Pass a 24-bit background color to blend them in:

```csharp
var onWhite = Palette.FromFilename("icon.png", Color.FromHexToInt("#FFFFFF"));
var onBlack = Palette.FromFilename("icon.png", Color.FromHexToInt("#000000"));
```

### Color helpers

```csharp
Color.FromIntToHex(0xFF8080);          // "#FF8080"
Color.FromHexToInt("#FF8080");          // 0xFF8080
Color.FromIntToRgb(0xFF8040);           // (R: 255, G: 128, B: 64)
Color.FromRgbToInt(255, 128, 64);       // 0xFF8040
```

---

## Project layout

```
ColorExtractor.Net/
├── Directory.Build.props              # shared metadata, multi-target net8/9/10
├── ColorExtractor.Net.slnx
├── src/
│   └── ColorExtractor.Net/
│       ├── Color.cs                   # hex / int / rgb conversions
│       ├── Palette.cs                 # image → color histogram
│       └── ColorExtractor.cs          # Lab + CIEDE2000 extraction
├── tests/
│   └── ColorExtractor.Net.Tests/      # xUnit + Shouldly, ported from PHPUnit suite
└── color-extractor/                   # original PHP source (git submodule, reference only)
```

## Credits

- Original PHP library: [thephpleague/color-extractor](https://github.com/thephpleague/color-extractor) by Matthieu Moquet and contributors.
- Image decoding: [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp).

## License

MIT — see [LICENSE](LICENSE).
