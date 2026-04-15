# Algorithm

ColorExtractor.Net uses the same two-stage algorithm as the PHP original.

## Stage 1 — Count distinct colors (`Palette`)

1. Decode the image to RGBA with `SixLabors.ImageSharp`.
2. Iterate pixels **column-major** (`x` outer, `y` inner) — matches the PHP
   reference's insertion order for tied counts.
3. For each pixel:
   - If it's opaque, key by 24-bit RGB.
   - If partially transparent and no background color was given, skip it.
   - If partially transparent and a background was given, blend with the
     background (see [[PHP Compatibility]] for the exact formula).
4. Increment the count for the resulting color in a dictionary.
5. Stable-sort descending by count. Ties keep first-seen order.

The palette is effectively a sorted `(color → pixelCount)` map.

## Stage 2 — Rank & merge (`ColorExtractor.Extract`)

### Ranking

Each distinct color gets a **priority score** combining chroma, lightness,
and usage:

```
priority = (√(a² + b²) or 1)       // chroma in Lab space; at least 1 so
                                   // pure greys aren't zeroed
         × (1 − L / 200)           // gentle darkness bias
         × √count                  // how often it appears
```

where `L`, `a`, `b` are the sRGB-linear → XYZ → CIE Lab conversion of the
color. This is inserted into a **priority queue** so the most "interesting"
colors come out first.

### Merging

Given a target `n` colors, the merge threshold is `maxDelta = 100 / n`.

Walking the sorted list, we accept a color only if its CIEDE2000 delta-E
distance to every already-accepted color is **≥ `maxDelta`**. This prevents
the output from being five shades of the same red.

Smaller `n` → larger threshold → more visually distinct results.

## Why Lab + CIEDE2000?

RGB distance is a poor proxy for perceptual difference — two colors with the
same Euclidean RGB distance can look very different to the eye. The
[CIE Lab color space](https://en.wikipedia.org/wiki/CIELAB_color_space) is
designed so that distances correlate better with perception, and
[CIEDE2000](https://en.wikipedia.org/wiki/Color_difference#CIEDE2000) is the
current standard refinement for that distance.

> **Note:** the CIEDE2000 implementation is ported **verbatim** from the PHP
> library, including places where it mixes radian outputs from `atan2` with
> literal degree constants. That's technically a quirk of the reference, but
> faithfully preserving it is what lets our results match the PHP output
> byte-for-byte. See [[PHP Compatibility]] for details.

## Complexity

- Palette build: **O(width × height)** pixels read, **O(unique colors)**
  dictionary ops.
- Extraction: ranking is **O(k log k)** where `k` is distinct colors; merging
  is **O(k × n)** in the worst case (small `n`, so this is fast in practice).

## Next

- [[PHP Compatibility]]
- [[Library Usage]]
