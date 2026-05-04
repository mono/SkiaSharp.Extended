# Copilot Instructions for SkiaSharp.Extended

## Build, Test, and Lint

```bash
# Restore local tools first (required for Cake)
dotnet tool restore

# Full build, pack, and test (CI equivalent)
dotnet cake

# Build only
dotnet cake --target=build

# Run all tests
dotnet cake --target=test

# Run a single test by name
dotnet test tests/SkiaSharp.Extended.Tests --filter "FullyQualifiedName~CanEncodeAndDecode"

# Build specific project (useful for MAUI on Linux - only Android supported)
dotnet build source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj -f net10.0-android36.0
```

## Architecture

This repository contains two main libraries:

- **SkiaSharp.Extended** (`source/SkiaSharp.Extended/`) - Core library with utility APIs
  - BlurHash encoding/decoding
  - Geometry helpers
  - Path interpolation
  - Image comparison utilities
  - Targets: `netstandard2.0`, `net9.0`, `net10.0`

- **SkiaSharp.Extended.UI.Maui** (`source/SkiaSharp.Extended.UI.Maui/`) - .NET MAUI controls
  - Lottie animation support (`SKLottieView`)
  - Confetti effects (`SKConfettiView`)
  - Animated surface views
  - Multi-platform: iOS, Android, macOS Catalyst, Windows

## Key Conventions

### Naming
- All control classes use `SK` prefix (e.g., `SKConfettiView`, `SKLottieView`)
- Event args classes end with `EventArgs` suffix

### File Naming for Cross-Platform Code
Use `.shared.cs` suffix for platform-agnostic source files in the MAUI project. Platform-specific files use `.android.cs`, `.ios.cs`, `.macos.cs`, `.windows.cs` suffixes and are conditionally compiled.

### MAUI Control Pattern
All MAUI controls inherit from `TemplatedView` and use `ResourceLoader<T>` to register XAML resources in the constructor:

```csharp
public class MyControl : TemplatedView
{
    public MyControl()
    {
        ResourceLoader<Themes.MyControlResources>.EnsureRegistered(this);
    }
}
```

Each control has a corresponding XAML resources file (e.g., `SKMyControlResources.shared.xaml`) with a `ControlTemplate` using `PART_DrawingSurface` as the canvas name.

### Animation Pattern
For animated controls, inherit from `SKAnimatedSurfaceView`:

```csharp
public class MyAnimatedControl : SKAnimatedSurfaceView
{
    protected override void Update(TimeSpan deltaTime)
    {
        // Update animation state
    }

    protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
    {
        // Draw current frame
    }
}
```

The base class handles Window lifecycle and timer management internally.

### Test Structure
Tests use xUnit v3 and mirror the source structure. Test files are named `*Test.cs` (e.g., `SKBlurHashTest.cs`).

## Blazor Sample

There is a Blazor WebAssembly sample app at `samples/SkiaSharpDemo.Blazor/` that demonstrates core `SkiaSharp.Extended` features (Shapes, Path Interpolation, BlurHash). It is deployed to GitHub Pages at `mono.github.io/SkiaSharp.Extended/sample/` via the `builds-docs.yml` workflow.

## PR Screenshot Requirement

**Whenever a PR touches the Blazor sample (`samples/SkiaSharpDemo.Blazor/`) or updates a feature or control in the libraries (`source/SkiaSharp.Extended/` or `source/SkiaSharp.Extended.UI.Maui/`), you MUST add screenshots of all affected and related Blazor sample pages as a new comment on the PR.** This is required for every such PR without exception, so that reviewers can visually verify changes and track change history.

To capture screenshots:
1. Run the Blazor sample locally (`dotnet run --project samples/SkiaSharpDemo.Blazor`)
2. Navigate to each affected page in the browser
3. Verify there are no errors on the page or in the browser console logs
4. Take a screenshot of each page
5. Post the screenshots as a comment on the PR

## Dependencies

- `SkiaSharp` (3.119.1+)
- `SkiaSharp.Skottie` (3.119.1+) - For Lottie animations
- `SkiaSharp.Views.Maui.Controls` (3.119.1+)
- `SkiaSharp.Views.Blazor` (3.119.1+) - For Blazor WebAssembly
- `Microsoft.Maui.Controls` (10.x)

## SkiaSharp.Extended.Drawing.Common

A drop-in replacement for `System.Drawing.Common` backed by SkiaSharp.

### Architecture
- **`source/SkiaSharp.Extended.Drawing.Common/`** — Main library. `AssemblyName=SkiaSharp.Extended.Drawing.Common`, `RootNamespace=System.Drawing`
- **`tests/SkiaSharp.Extended.Drawing.Common.Tests/`** — Unit tests + pixel comparison against reference images
- **`tests/SkiaSharp.Extended.Drawing.Common.Scenarios/`** — Shared drawing scenario source files (compiled by both generator projects)
- **`tests/SkiaSharp.Extended.Drawing.Common.ReferenceGenerator/`** — Windows-only xUnit project, generates `.gdi.png` using real System.Drawing/GDI+
- **`tests/SkiaSharp.Extended.Drawing.Common.SkiaGenerator/`** — Cross-platform xUnit project, generates `.skia.png` using our SkiaSharp.Extended.Drawing.Common wrapper
- **`tools/api-baseline/`** — Official System.Drawing.Common reference assembly for API compatibility validation

### Implementation Rules
- The API surface is **compatible** with `System.Drawing.Common` (validated by `dotnet apicompat` in CI)
- Methods are implemented incrementally. Unimplemented methods throw `PlatformNotSupportedException`
- When implementing a method, search for `throw new System.PlatformNotSupportedException` in the source to find stubs
- Every public member MUST have XML doc comments matching the official Microsoft documentation
- All coordinate-based curve rendering (ellipses, arcs, pies) uses a +0.5 pixel offset via `GdiCurveRect()` for GDI+ compatibility
- Polygon vertex coordinates also use +0.5 offset via `GdiPolygonPath()`

### Partial Implementations (known gaps)
Some features are implemented but have known limitations:
- **ImageAttributes** — `SetColorMatrix()` applied via `SKColorFilter`, but `SetGamma()`/`SetThreshold()`/`SetColorKey()` values are stored only, not applied during drawing
- **HatchBrush** — all 53 patterns rendered but may not match GDI+ pixel-for-pixel
- **PathGradientBrush** — approximated with `SKShader.CreateRadialGradient()`
- **LinearGradientBrush** — basic blends work; complex multi-stop `InterpolationColors` may differ
- **DrawString** — word wrapping implemented; character-level trimming (`EllipsisCharacter` etc.) not fully implemented
- **MeasureString** — padding matches GDI+ ~1/6 em, but exact metrics may differ across platforms
- **LockBits** — works for `Format32bppArgb`; sub-byte formats (`1bpp`/`4bpp`) not supported
- **Printing** — PDF output only, no physical printer spooler integration
- **Font** — all constructors work; some metrics may differ from GDI+ due to different font rasterizers

See `source/SkiaSharp.Extended.Drawing.Common/KNOWN-LIMITATIONS.md` for the complete list of all 130 unimplemented API stubs and detailed partial implementation notes.

### Test Rules
- **Every new drawing feature must have pixel comparison scenarios** — add `[Fact]` methods to the appropriate class in `tests/SkiaSharp.Extended.Drawing.Common.Scenarios/`
- Each scenario class name = folder name for reference images (e.g., `Ellipses` class → `ReferenceImages/Ellipses/`)
- Each scenario method name = PNG filename (e.g., `Ellipse_Fill_Circle` → `Ellipse_Fill_Circle.skia.png`)
- **Non-AA scenarios** must achieve <0.5% pixel error vs GDI+ reference
- **AA scenarios** must achieve <5% pixel error vs GDI+ (different AA algorithms)
- **Solid fills and colors** must achieve <0.1% pixel error
- Run `dotnet test tests/SkiaSharp.Extended.Drawing.Common.SkiaGenerator/` to generate `.skia.png` scenario images locally
- CI generates both GDI+ and Skia images in parallel, then compares — test failures fail the build

### Pixel Validation (CI)

Reference images are checked in with two variants per scenario:
- `{name}.gdi.png` — rendered by real System.Drawing/GDI+ on Windows CI
- `{name}.skia.png` — rendered by our SkiaSharp.Extended.Drawing.Common wrapper

CI runs three jobs:
1. **gdi_generate** (Windows) — runs `ReferenceGenerator` scenarios, uploads `.gdi.png` artifacts
2. **skia_generate** (parallel) — runs `SkiaGenerator` scenarios, uploads `.skia.png` artifacts
3. **pixel_validation** (depends on 1+2) — downloads both artifacts, replaces checked-in refs, runs `PixelComparisonTests`

Each generator also compares against checked-in baselines (via `REFERENCE_IMAGES_PATH`). If a scenario changed, the generator test fails, signaling that fresh images need to be checked in.

### Adding a New Drawing Scenario
1. Find the appropriate category file in `tests/SkiaSharp.Extended.Drawing.Common.Scenarios/` (e.g., `Ellipses.cs`)
2. Add a `[Fact]` method:
   ```csharp
   [Fact] public void Ellipse_Fill_Large() => Render(200, 200, g => {
       g.SmoothingMode = SmoothingMode.None;
       g.Clear(Color.White);
       using var brush = new SolidBrush(Color.Blue);
       g.FillEllipse(brush, 10, 10, 180, 180);
   });
   ```
3. Run `dotnet test tests/SkiaSharp.Extended.Drawing.Common.SkiaGenerator/` to generate `.skia.png` baselines locally
4. Copy the generated `.skia.png` from `ScenarioOutput/` to `tests/SkiaSharp.Extended.Drawing.Common.Tests/ReferenceImages/{Category}/`
5. CI generates `.gdi.png` on Windows and compares — download from CI artifacts and check in
6. Run `dotnet test tests/SkiaSharp.Extended.Drawing.Common.Tests/ --filter "FullyQualifiedName~PixelComparison"` to validate locally

### Build & Test Commands
```bash
# Build SkiaSharp.Extended.Drawing.Common
dotnet build source/SkiaSharp.Extended.Drawing.Common/SkiaSharp.Extended.Drawing.Common.csproj -c Release

# Run all SkiaSharp.Extended.Drawing.Common unit tests
dotnet test tests/SkiaSharp.Extended.Drawing.Common.Tests/

# Generate Skia scenario images locally
dotnet test tests/SkiaSharp.Extended.Drawing.Common.SkiaGenerator/

# Run pixel comparison tests (Skia vs GDI checked-in refs)
dotnet test tests/SkiaSharp.Extended.Drawing.Common.Tests/ --filter "FullyQualifiedName~PixelComparison"

# Validate API compatibility (build with original assembly name for matching)
dotnet tool restore
dotnet build source/SkiaSharp.Extended.Drawing.Common/SkiaSharp.Extended.Drawing.Common.csproj -c Release -p:AssemblyName=System.Drawing.Common
dotnet apicompat --left tools/api-baseline/netstandard2.0/System.Drawing.Common.dll --right source/SkiaSharp.Extended.Drawing.Common/bin/Release/netstandard2.0/System.Drawing.Common.dll --strict-mode --suppression-file tools/api-baseline/api-compat-suppressions.xml
```

### Benchmarks
```bash
# Run all benchmarks (Windows only — needs both GDI+ and Skia)
dotnet run -c Release --project benchmarks/SkiaSharp.Extended.Drawing.Common.Benchmarks/

# Filter specific benchmark class
dotnet run -c Release --project benchmarks/SkiaSharp.Extended.Drawing.Common.Benchmarks/ -- --filter "*Fill*"

# Quick run for testing
dotnet run -c Release --project benchmarks/SkiaSharp.Extended.Drawing.Common.Benchmarks/ -- --filter "*Clear*" --job short
```
