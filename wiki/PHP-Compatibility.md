# PHP Compatibility

ColorExtractor.Net is a faithful port of
[thephpleague/color-extractor](https://github.com/thephpleague/color-extractor).
Every algorithmic step is ported verbatim so that results match the PHP library
**byte-for-byte** on lossless formats.

## Matrix

| Area | Behavior |
|---|---|
| PNG / GIF / WebP (lossless) | Integer color values match PHP exactly |
| Transparency blending | Matches PHP GD byte-for-byte (formula below) |
| CIEDE2000 delta-E | Verbatim port, including radian/degree quirks |
| JPEG (lossy) | May differ by ±1 per channel due to decoder rounding |
| Pixel iteration order | Column-major (`x` outer, `y` inner), like PHP |

## Transparency formula

PHP GD uses a **7-bit alpha channel** (range `0..127`, where `0 = opaque` and
`127 = fully transparent`) — the opposite convention from most modern image
libraries. ImageSharp delivers RGBA with `A ∈ [0, 255]`, `255 = opaque`.

ColorExtractor.Net quantizes ImageSharp's byte alpha to GD's 7-bit range the
same way `libgd` does internally when it ingests a PNG:

```
gdAlpha = 127 − (a × 127 + 127) / 255     // integer division
```

If `gdAlpha != 0` and a background color was given, the pixel is blended:

```
a_f = gdAlpha / 127
R   = (int)(pixel.R × (1 − a_f) + bg.R × a_f)
G   = (int)(pixel.G × (1 − a_f) + bg.G × a_f)
B   = (int)(pixel.B × (1 − a_f) + bg.B × a_f)
```

Casting to `int` **truncates toward zero** (same as PHP's `(int)` cast).

### Worked example

The `red-transparent-50.png` test fixture stores a single pixel `(255, 0, 0,
127)` in its PNG IDAT chunk. With `backgroundColor = #FFFFFF` (white):

```
gdAlpha = 127 − (127·127 + 127) / 255 = 127 − 63 = 64
a_f     = 64 / 127 ≈ 0.5039
R       = (int)(255·(1−0.5039) + 255·0.5039) = 255
G       = (int)(  0·(1−0.5039) + 255·0.5039) = 128 = 0x80
B       = (int)(  0·(1−0.5039) + 255·0.5039) = 128 = 0x80
→ #FF8080   ✅ matches PHP
```

On a black background the same pixel yields `#7E0000` (126 in red),
also matching.

## CIEDE2000: preserved quirks

The PHP implementation mixes `atan2` outputs (radians) with literal degree
constants. A purely mathematical port would have the form:

```
T = 1 − 0.17·cos(hbp − 30°) + 0.24·cos(2·hbp) + …
```

but the PHP code writes:

```php
$T = 1 - .17 * cos($hbp - 30) + .24 * cos(2 * $hbp) + …
```

where `$hbp` is already in radians. **We replicate this verbatim.** If you
"correct" the units, the produced delta-E numbers change and extraction
results diverge from the reference. The fact that this still produces
reasonable output is part of why the reference library works the way it does.

## JPEG notes

JPEG decoding is lossy. Our tests port the PHP JPEG integer assertion with a
**±2 per-channel tolerance** because `libjpeg` (PHP GD) and ImageSharp
round slightly differently during Huffman decode. The algorithm itself is
identical.

## Test parity

All PHPUnit tests from `thephpleague/color-extractor` are ported to xUnit under
`tests/ColorExtractor.Net.Tests/`. **22 / 22 pass** on .NET 8, 9, and 10,
including the exact-integer assertions from the original suite (except the
JPEG tolerance above).
