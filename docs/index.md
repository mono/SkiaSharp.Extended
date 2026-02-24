# SkiaSharp.Extended

SkiaSharp.Extended brings powerful graphics utilities and .NET MAUI controls to your SkiaSharp projects—from blur hash placeholders to Lottie animations and confetti effects.

## Getting Started

Install via NuGet:

```bash
# Core utilities
dotnet add package SkiaSharp.Extended

# MAUI controls (includes core)
dotnet add package SkiaSharp.Extended.UI.Maui
```

For MAUI projects, register the handler in `MauiProgram.cs`:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseSkiaSharp();  // Add this line
    
    return builder.Build();
}
```

---

## SkiaSharp.Extended

Core utilities for image processing, geometry, and path manipulation.

### [Blur Hash](docs/blurhash.md)

Compact placeholder representations for images. Perfect for showing a beautiful blur while the full image loads.

| Original | Placeholder |
| :------: | :---------: |
| ![Original][blur-original] | ![Blur][blur-placeholder] |

### [Geometry Helpers](docs/geometry.md)

Create regular polygons, stars, and other shapes with simple method calls.

### [Path Interpolation](docs/path-interpolation.md)

Smoothly morph between two paths for shape transitions and animations.

![Path Interpolation][interpolation-img]

---

## SkiaSharp.Extended.UI.Maui

Ready-to-use .NET MAUI controls for rich visual effects.

### [Lottie Animations](docs/lottie.md)

Play designer-created After Effects animations exported as Lottie JSON files.

![Lottie Animation][lottie-img]

### [Confetti Effects](docs/confetti.md)

Celebrate achievements with customizable particle explosions.

![Confetti][confetti-img]

---

## Resources

- [GitHub Repository](https://github.com/mono/SkiaSharp.Extended)
- [NuGet Packages](https://www.nuget.org/packages?q=SkiaSharp.Extended)
- [SkiaSharp Documentation](https://docs.microsoft.com/en-us/dotnet/api/skiasharp)

[blur-original]: images/extended/skblurhash/logo.png
[blur-placeholder]: images/extended/skblurhash/blur.png
[interpolation-img]: images/extended/skpathinterpolation/interpolation.gif
[lottie-img]: images/ui/controls/sklottieview/lottie.gif
[confetti-img]: images/ui/controls/skconfettiview/top-stream.gif
