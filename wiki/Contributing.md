# Contributing

Contributions are welcome!

## Before opening a PR

1. Fork the repo and create a topic branch.
2. Make your change.
3. Add/update xUnit tests under `tests/ColorExtractor.Net.Tests/` —
   aim for every public method to have coverage, and every PHP-parity
   behavior to have a test mirroring the original `*Test.php`.
4. Run the full suite across all TFMs:

   ```bash
   dotnet test ColorExtractor.Net.slnx
   ```

   All 22+ tests must pass on net8.0, net9.0, and net10.0.

## Code style

- `Nullable` enabled throughout — honor the annotations.
- Prefer **verbatim** ports over "cleaned up" math when touching the
  CIEDE2000 path — behavioral parity with the PHP reference is a feature.
  If you change the math, existing expected integer outputs will shift.
- Use `SixLabors.ImageSharp` for all pixel access (no `System.Drawing`).

## Releasing

Maintainers only:

1. Bump `<Version>` in `Directory.Build.props` on a PR.
2. Merge to `main`.
3. The `release.yml` workflow will pack, push to NuGet.org, and cut a
   tagged GitHub Release automatically (requires `NUGET_API_KEY` secret).

## Reporting bugs

Open an issue with:

- The smallest reproducing image (if applicable)
- Expected vs actual extracted colors (as ints or hex)
- The exact `Extract(n)` count you called with
- .NET version and OS

## Security

Please report security issues privately via GitHub's
[security advisory flow](https://github.com/totpero/ColorExtractor.Net/security/advisories/new)
rather than a public issue.
