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
3. Take a screenshot of each page
4. Post the screenshots as a comment on the PR

## Dependencies

- `SkiaSharp` (3.119.1+)
- `SkiaSharp.Skottie` (3.119.1+) - For Lottie animations
- `SkiaSharp.Views.Maui.Controls` (3.119.1+)
- `SkiaSharp.Views.Blazor` (3.119.1+) - For Blazor WebAssembly
- `Microsoft.Maui.Controls` (9.x)
