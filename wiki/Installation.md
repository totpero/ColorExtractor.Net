# Installation

## Prerequisites

- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0), [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0), or [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) SDK
- Supported platforms: Windows, Linux, macOS

## From NuGet

```bash
dotnet add package ColorExtractor.Net
```

Or via the NuGet UI in Visual Studio / Rider: search for **ColorExtractor.Net**.

## Verify

```csharp
using ColorExtractor.Net;

Console.WriteLine(Color.FromIntToHex(0xFF8080)); // → "#FF8080"
```

## Dependencies

`ColorExtractor.Net` has a single transitive dependency:

- [**SixLabors.ImageSharp** `3.x`](https://www.nuget.org/packages/SixLabors.ImageSharp) — managed image decoding. No native `libgd`/`libjpeg` required.

## Next

- [[Quick Start]]
- [[Library Usage]]
