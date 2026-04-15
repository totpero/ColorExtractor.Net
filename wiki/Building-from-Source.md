# Building from Source

## Clone

```bash
git clone --recurse-submodules https://github.com/totpero/ColorExtractor.Net.git
cd ColorExtractor.Net
```

The `color-extractor/` folder is a git submodule containing the original PHP
source. It's included **for reference only** (so you can diff algorithms
side-by-side) and is not part of the build.

## Build

```bash
dotnet build ColorExtractor.Net.slnx
```

Multi-targets `net8.0`, `net9.0`, and `net10.0`. All three are built
simultaneously.

## Test

```bash
dotnet test ColorExtractor.Net.slnx
```

Expected: **22 tests pass on each of net8.0 / net9.0 / net10.0**.

To run just one TFM:

```bash
dotnet test ColorExtractor.Net.slnx -f net9.0
```

## Pack NuGet locally

```bash
dotnet pack ColorExtractor.Net.slnx -c Release -o ./nupkgs
```

Produces `ColorExtractor.Net.{Version}.nupkg` (and the matching `.snupkg`
symbol package) containing `lib/net8.0/`, `lib/net9.0/`, `lib/net10.0/`, the
README, and the icon. The version comes from `<Version>` in
`Directory.Build.props`.

## Regenerate the icon

```bash
cd tools/GenerateIcon
dotnet run -c Release -- ../../icon.png
```

## Repo layout

```
ColorExtractor.Net/
├── Directory.Build.props              # shared multi-TFM metadata + package props
├── ColorExtractor.Net.slnx            # solution
├── icon.png                           # NuGet package icon
├── src/
│   └── ColorExtractor.Net/
│       ├── Color.cs                   # hex / int / rgb conversions
│       ├── Palette.cs                 # image → color histogram
│       └── ColorExtractor.cs          # Lab + CIEDE2000 extraction
├── tests/
│   └── ColorExtractor.Net.Tests/      # xUnit + Shouldly
├── tools/
│   └── GenerateIcon/                  # icon generator (not packed)
├── wiki/                              # source of GitHub wiki pages
└── color-extractor/                   # original PHP source (submodule, reference)
```

## CI

- **`.github/workflows/build.yml`** — matrix build+test on
  ubuntu/windows/macos × net8/9/10 on push and PR.
- **`.github/workflows/release.yml`** — on bump of `<Version>` in
  `Directory.Build.props` (on `main`): pack, push `.nupkg` + `.snupkg` to
  NuGet.org, and cut a tagged GitHub Release.
- **`.github/workflows/wiki.yml`** — syncs `wiki/` + README-derived `Home.md`
  to the GitHub wiki on push.

## Next

- [[Contributing]]
