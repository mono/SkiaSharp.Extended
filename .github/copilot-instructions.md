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
dotnet build source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj -f net9.0-android35.0
```

## Architecture

This repository contains two main libraries:

- **SkiaSharp.Extended** (`source/SkiaSharp.Extended/`) - Core library with utility APIs
  - BlurHash encoding/decoding
  - Geometry helpers
  - Path interpolation
  - Image comparison utilities
  - Targets: `netstandard2.0`, `net9.0`

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
- `Microsoft.Maui.Controls` (9.x)

## SkiaSharp.Drawing

A drop-in replacement for `System.Drawing.Common` backed by SkiaSharp.

### Architecture
- **`source/SkiaSharp.Drawing/`** — Main library. `AssemblyName=System.Drawing.Common`, `RootNamespace=System.Drawing`
- **`tests/SkiaSharp.Drawing.Tests/`** — 346+ tests: unit tests + pixel comparison against GDI+ reference images
- **`tests/SkiaSharp.Drawing.Scenarios/`** — Shared drawing scenario source files (compiled by both backends)
- **`tests/SkiaSharp.Drawing.ReferenceGenerator/`** — Windows-only, generates golden PNGs using real System.Drawing/GDI+
- **`tests/SkiaSharp.Drawing.Tests/`** — Cross-platform, generates PNGs using our SkiaSharp.Drawing wrapper
- **`tools/api-baseline/`** — Official System.Drawing.Common reference assembly for API compatibility validation

### Implementation Rules
- The API surface is **100% ABI-compatible** with `System.Drawing.Common` (validated by `dotnet apicompat --strict-mode` in CI)
- Methods are implemented incrementally. Unimplemented methods throw `PlatformNotSupportedException`
- When implementing a method, search for `throw new System.PlatformNotSupportedException` in the source to find stubs
- Every public member MUST have XML doc comments matching the official Microsoft documentation
- All coordinate-based curve rendering (ellipses, arcs, pies) uses a +0.5 pixel offset via `GdiCurveRect()` for GDI+ compatibility
- Polygon vertex coordinates also use +0.5 offset via `GdiPolygonPath()`

### Test Rules
- **Every new drawing feature must have pixel comparison scenarios** — add `[Fact]` methods to the appropriate class in `tests/SkiaSharp.Drawing.Scenarios/`
- Each scenario class name = folder name for reference images (e.g., `Ellipses` class → `ReferenceImages/Ellipses/`)
- Each scenario method name = PNG filename (e.g., `Ellipse_Fill_Circle` → `Ellipse_Fill_Circle.skia.png`)
- **Non-AA scenarios** must achieve <0.5% pixel error vs GDI+ reference
- **AA scenarios** must achieve <5% pixel error vs GDI+ (different AA algorithms)
- **Solid fills and colors** must achieve <0.1% pixel error
- Run `dotnet test tests/SkiaSharp.Drawing.Tests/ --filter "FullyQualifiedName~Scenarios"` to generate scenario images after changes
- CI generates GDI+ reference images on Windows and compares — test failures fail the build

### Pixel Validation (3 steps)

Reference images are checked in with two variants per scenario:
- `{name}.gdi.png` — rendered by real System.Drawing/GDI+ on Windows CI
- `{name}.skia.png` — rendered by our SkiaSharp.Drawing wrapper

CI runs three validation steps:
1. **GDI stability** — fresh GDI+ output must match checked-in `.gdi.png` (ensures scenarios unchanged)
2. **Skia regression** — fresh Skia output must match checked-in `.skia.png` (ensures no rendering regression)
3. **Skia == GDI** — fresh Skia output compared to fresh GDI+ output (pixel compatibility with tolerance)

When a step fails:
- Step 1 fails: scenario changed — download new `.gdi.png` from CI artifacts, review, check in
- Step 2 fails: rendering changed — download new `.skia.png` from CI artifacts, review, check in
- Step 3 fails: Skia doesn't match GDI+ — fix the rendering implementation

### Adding a New Drawing Scenario
1. Find the appropriate category file in `tests/SkiaSharp.Drawing.Scenarios/` (e.g., `Ellipses.cs`)
2. Add a `[Fact]` method:
   ```csharp
   [Fact] public void Ellipse_Fill_Large() => Render(200, 200, g => {
       g.SmoothingMode = SmoothingMode.None;
       g.Clear(Color.White);
       using var brush = new SolidBrush(Color.Blue);
       g.FillEllipse(brush, 10, 10, 180, 180);
   });
   ```
3. Run the scenarios to generate baselines:
   `dotnet test tests/SkiaSharp.Drawing.Tests/ --filter "FullyQualifiedName~Scenarios"`
4. Copy the generated `.skia.png` image to reference images:
   The output goes to `ScenarioOutput/` in the test output directory
5. Run comparison tests: `dotnet test tests/SkiaSharp.Drawing.Tests/ --filter "FullyQualifiedName~SkiaRegression"`
6. CI generates GDI+ reference images automatically and compares

### Build & Test Commands
```bash
# Build SkiaSharp.Drawing
dotnet build source/SkiaSharp.Drawing/SkiaSharp.Drawing.csproj -c Release

# Run all SkiaSharp.Drawing tests
dotnet test tests/SkiaSharp.Drawing.Tests/

# Run only scenario tests (generates images)
dotnet test tests/SkiaSharp.Drawing.Tests/ --filter "FullyQualifiedName~Scenarios"

# Run only pixel comparison tests (Skia regression)
dotnet test tests/SkiaSharp.Drawing.Tests/ --filter "FullyQualifiedName~SkiaRegression"

# Validate API compatibility
dotnet tool restore
dotnet apicompat --left tools/api-baseline/netstandard2.0/System.Drawing.Common.dll --right source/SkiaSharp.Drawing/bin/Release/netstandard2.0/System.Drawing.Common.dll --strict-mode --suppression-file tools/api-baseline/api-compat-suppressions.xml
```
