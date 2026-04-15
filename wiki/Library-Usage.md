# Library Usage

## Loading a palette

```csharp
using ColorExtractor.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// From a file path
var p1 = Palette.FromFilename("photo.png");

// From a byte array
var p2 = Palette.FromContents(File.ReadAllBytes("photo.png"));

// From a stream
using var fs = File.OpenRead("photo.png");
var p3 = Palette.FromStream(fs);

// From a URL (optionally pass your own HttpClient for retries/auth/proxies)
var p4 = await Palette.FromUrlAsync("https://example.com/photo.png");

// From an already-decoded ImageSharp image
using var img = Image.Load<Rgba32>("photo.png");
var p5 = Palette.FromImage(img);
```

Supported formats: **PNG, JPEG, GIF, WebP, BMP, TGA** (everything `SixLabors.ImageSharp` decodes).

## Extracting colors

```csharp
var extractor = new ColorExtractor(palette);

int[] one  = extractor.Extract(1);   // single dominant color
int[] five = extractor.Extract(5);   // five visually distinct colors
int[] ten  = extractor.Extract(10);
```

`Extract(n)` returns **up to** `n` colors. Candidates whose CIEDE2000 delta-E is
below `100 / n` are considered duplicates and merged — so small `n` gives you
more visually different swatches.

Returned integers are 24-bit RGB (`0xRRGGBB`). Convert them with the [[Color helpers|Library Usage#color-conversion-helpers]] below.

## Inspecting a palette

The palette itself is already a histogram, sorted most-used first:

```csharp
Console.WriteLine(palette.Count);                    // distinct colors
Console.WriteLine(palette.GetColorCount(0xFF0000));  // pixels that were pure red

foreach (var (color, count) in palette)
    Console.WriteLine($"{Color.FromIntToHex(color)}: {count} px");

// Top-10 (pre-merge, raw frequency)
IReadOnlyList<KeyValuePair<int, int>> top10 = palette.GetMostUsedColors(10);
```

## Handling transparency

By default, any pixel with partial or full transparency is **skipped**.
Pass a 24-bit background color to blend transparent pixels onto it instead:

```csharp
// Blend onto white
var onWhite = Palette.FromFilename("icon.png", Color.FromHexToInt("#FFFFFF"));

// Blend onto black
var onBlack = Palette.FromFilename("icon.png", Color.FromHexToInt("#000000"));
```

The blending formula matches PHP GD byte-for-byte (see [[PHP Compatibility]]).

## Color conversion helpers

```csharp
Color.FromIntToHex(0xFF8080);           // "#FF8080"
Color.FromIntToHex(0xFF8080, false);    // "FF8080"  (no leading '#')
Color.FromHexToInt("#FF8080");          // 16744576
Color.FromHexToInt("FF8080");           // 16744576  (optional '#')
Color.FromIntToRgb(0xFF8040);           // (R: 255, G: 128, B: 64)
Color.FromRgbToInt(255, 128, 64);       // 16744512
```

## Cancellation

`Palette.FromUrlAsync` accepts a `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var palette = await Palette.FromUrlAsync(url, cancellationToken: cts.Token);
```

## Thread safety

- `Palette` is immutable once built; reading it from multiple threads is safe.
- `ColorExtractor.Extract` lazily caches its internal sort on first call, so
  prefer creating a new `ColorExtractor` per thread, or call `Extract` once
  before sharing.

## Next

- [[Algorithm]] — how extraction ranks and merges colors
- [[PHP Compatibility]] — matching the reference library exactly
