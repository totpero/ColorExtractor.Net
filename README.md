<p align="center">
  <b>ColorExtractor.Net</b>
  <br>
  <i>Extract colors from an image the way a human would, in .NET</i>
  <br>
  <i>A .NET port of <a href="https://github.com/thephpleague/color-extractor">thephpleague/color-extractor</a></i>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/download"><img src="https://img.shields.io/badge/.NET-8.0%20|%209.0%20|%2010.0-512BD4?logo=dotnet" alt=".NET 8.0 | 9.0 | 10.0"></a>
  <a href="https://github.com/totpero/ColorExtractor.Net/blob/main/LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue" alt="License: Apache 2.0"></a>
  <a href="https://www.nuget.org/packages/ColorExtractor.Net"><img src="https://img.shields.io/nuget/v/ColorExtractor.Net?label=ColorExtractor.Net&logo=nuget" alt="NuGet ColorExtractor.Net"></a>
  <a href="https://www.nuget.org/packages/ColorExtractor.Net"><img src="https://img.shields.io/nuget/dt/ColorExtractor.Net?label=downloads&logo=nuget" alt="NuGet Downloads"></a>
</p>

---

## About

**ColorExtractor.Net** is a cross-platform .NET rewrite of the popular [league/color-extractor](https://github.com/thephpleague/color-extractor) PHP library. It selects the most visually significant colors from an image by mapping pixels to the CIE Lab color space and merging near-duplicates via CIEDE2000 delta-E — the same approach as the reference library.

### Why .NET?

- Cross-platform (Windows, Linux, macOS) — no native `libgd` dependency
- Pure managed image decoding via [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- Supports PNG, JPEG, GIF, WebP, BMP, TGA
- Multi-targets .NET 8.0, 9.0, and 10.0
- Faithful port: transparency blending and CIEDE2000 results match the PHP reference byte-for-byte on lossless formats

---

## Installation

### Prerequisites

- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0), [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0), or [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) SDK
- Supported platforms: Windows, Linux, macOS

### Install from NuGet

```bash
dotnet add package ColorExtractor.Net
```

### Build from source

```bash
git clone https://github.com/totpero/ColorExtractor.Net.git
cd ColorExtractor.Net
dotnet build
dotnet test
```

---

## Quick Start

```csharp
using ColorExtractor.Net;

var palette = Palette.FromFilename("photo.jpg");
var extractor = new ColorExtractor(palette);

int[] colors = extractor.Extract(5);

foreach (var color in colors)
    Console.WriteLine(Color.FromIntToHex(color));
// #F3ED98
// #E6614A
// #A4C6D8
// ...
```

---

## Usage

### Loading a palette

```csharp
// From a file path
var p1 = Palette.FromFilename("photo.png");

// From an in-memory byte array
var p2 = Palette.FromContents(File.ReadAllBytes("photo.png"));

// From a stream
using var fs = File.OpenRead("photo.png");
var p3 = Palette.FromStream(fs);

// From a URL (optionally pass your own HttpClient)
var p4 = await Palette.FromUrlAsync("https://example.com/photo.png");

// From an already-loaded ImageSharp image
using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>("photo.png");
var p5 = Palette.FromImage(img);
```

### Extracting colors

```csharp
var extractor = new ColorExtractor(palette);

int[] top1  = extractor.Extract(1);   // single dominant color
int[] top5  = extractor.Extract(5);   // five visually distinct colors
int[] top10 = extractor.Extract(10);
```

`Extract(n)` returns up to `n` distinct colors. Colors that are visually too similar (under CIEDE2000 delta-E threshold `100 / n`) are merged, matching the reference behavior.

### Inspecting the palette

```csharp
var palette = Palette.FromFilename("photo.png");

Console.WriteLine(palette.Count);                    // distinct colors
Console.WriteLine(palette.GetColorCount(0xFF0000));  // pixel count for red, or 0

// Already sorted most-used first
foreach (var (color, count) in palette)
    Console.WriteLine($"{Color.FromIntToHex(color)}: {count} px");

// Top-10 most used (pre-merge)
var top = palette.GetMostUsedColors(10);
```

### Handling transparency

By default, pixels with any transparency are skipped. Pass a 24-bit background color to blend them in:

```csharp
// Blend transparent pixels against white
var onWhite = Palette.FromFilename("icon.png", Color.FromHexToInt("#FFFFFF"));

// Blend transparent pixels against black
var onBlack = Palette.FromFilename("icon.png", Color.FromHexToInt("#000000"));
```

### Color conversion helpers

```csharp
Color.FromIntToHex(0xFF8080);            // "#FF8080"
Color.FromIntToHex(0xFF8080, false);     // "FF8080"
Color.FromHexToInt("#FF8080");           // 16744576
Color.FromIntToRgb(0xFF8040);            // (R: 255, G: 128, B: 64)
Color.FromRgbToInt(255, 128, 64);        // 16744512
```

---

## Project Layout

```
ColorExtractor.Net/
├── Directory.Build.props              # shared metadata, multi-target net8/9/10
├── ColorExtractor.Net.slnx            # solution
├── src/
│   └── ColorExtractor.Net/
│       ├── Color.cs                   # hex / int / rgb conversions
│       ├── Palette.cs                 # image -> color histogram
│       └── ColorExtractor.cs          # Lab + CIEDE2000 extraction
├── tests/
│   └── ColorExtractor.Net.Tests/      # xUnit + Shouldly
│       ├── ColorTests.cs
│       ├── PaletteTests.cs
│       ├── ColorExtractorTests.cs
│       └── assets/                    # PNG / JPEG / GIF / WebP fixtures
└── color-extractor/                   # original PHP source (git submodule, reference)
```

## Compatibility with the PHP reference

- **PNG / GIF / WebP (lossless)** — integer color values match the PHP library exactly.
- **Transparency blending** — ImageSharp's 0–255 alpha is quantized to PHP GD's 0–127 range (`gdAlpha = 127 − (a·127 + 127) / 255`) so blended pixel integers match byte-for-byte.
- **CIEDE2000** — ported verbatim, including the PHP original's mix of radian inputs and literal degree constants, to preserve delta-E values.
- **JPEG** — differs by up to one bit per channel because ImageSharp and libjpeg (PHP GD) round differently during lossy decode. The ported test uses a ±2 per-channel tolerance.

## Credits

- Original PHP library: [thephpleague/color-extractor](https://github.com/thephpleague/color-extractor) by Matthieu Moquet and contributors.
- Image decoding: [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp).

## License

Apache 2.0 — see [LICENSE](LICENSE).
