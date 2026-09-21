# Copilot Instructions for SkiaSharp.Extended

## Build, Test, and Lint

```bash
# Install workloads for the SDK/workload set pinned in global.json.
# On Linux use maui-android instead of maui; on Windows use eng\common\dotnet.cmd.
./eng/common/dotnet.sh workload install maui wasm-tools --source https://api.nuget.org/v3/index.json

# Full build, pack, and test (CI equivalent)
./build.sh -configuration Release -pack -test

# Build only
./build.sh -configuration Release

# Run all tests
./build.sh -configuration Release -test

# Run a single test by name
dotnet test tests/SkiaSharp.Extended.Tests --filter "FullyQualifiedName~CanEncodeAndDecode"

# Build specific project (useful for MAUI on Linux - only Android supported)
dotnet build source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj -f net10.0-android36.0
```

Use `build.cmd` on Windows. Arcade owns the real project graph, versioning,
packaging, symbols, signing, and publishing. Build outputs belong in `artifacts/`;
do not add Cake adapters or custom package/version staging scripts. The generated
`eng/common/` snapshot must remain identical to its pinned upstream revision.

## Architecture

This repository contains two main libraries:

- **SkiaSharp.Extended** (`source/SkiaSharp.Extended/`) - Core library with utility APIs
  - BlurHash encoding/decoding
  - Geometry helpers
  - Path interpolation
  - Image comparison utilities
  - Targets: `netstandard2.0`, `net10.0+`

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

There is a Blazor WebAssembly sample app at `samples/SkiaSharpDemo.Blazor/` that demonstrates core `SkiaSharp.Extended` features (Shapes, Path Interpolation, BlurHash). It is deployed to GitHub Pages at `mono.github.io/SkiaSharp.Extended/sample/` via the `docs-deploy.yml` workflow.

## PR Screenshot Requirement

**Whenever a PR touches the Blazor sample (`samples/SkiaSharpDemo.Blazor/`) or updates a feature or control in the libraries (`source/SkiaSharp.Extended/` or `source/SkiaSharp.Extended.UI.Maui/`), you MUST add screenshots of all affected and related Blazor sample pages as a new comment on the PR.** This is required for every such PR without exception, so that reviewers can visually verify changes and track change history.

To capture screenshots:
1. Run the Blazor sample locally (`dotnet run --project samples/SkiaSharpDemo.Blazor`)
2. Navigate to each affected page in the browser
3. Verify there are no errors on the page or in the browser console logs
4. Take a screenshot of each page
5. Post the screenshots as a comment on the PR

## Dependencies

- `SkiaSharp` (4.150.1+)
- `SkiaSharp.Skottie` (4.150.1+) - For Lottie animations
- `SkiaSharp.Views.Maui.Controls` (4.150.1+)
- `SkiaSharp.Views.Blazor` (4.150.1+) - For Blazor WebAssembly
- `Microsoft.Maui.Controls` (10.x)
