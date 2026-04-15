# Quick Start

```csharp
using ColorExtractor.Net;

// 1. Load a palette from an image
var palette = Palette.FromFilename("photo.jpg");

// 2. Extract the top 5 visually distinct colors
var extractor = new ColorExtractor(palette);
int[] colors = extractor.Extract(5);

// 3. Print them
foreach (var color in colors)
    Console.WriteLine(Color.FromIntToHex(color));
```

Sample output:

```
#F3ED98
#E6614A
#A4C6D8
#3A3E4D
#CBA37A
```

## What just happened?

1. `Palette.FromFilename` decodes the image and builds a histogram of distinct
   24-bit RGB colors.
2. `ColorExtractor.Extract(n)` ranks them in CIE Lab space, weighted by chroma,
   lightness, and frequency, then merges near-duplicates using CIEDE2000
   delta-E.
3. The result is up to `n` visually distinct, representative colors.

## Next

- [[Library Usage]] — every supported loader and API
- [[Algorithm]] — how extraction works under the hood
