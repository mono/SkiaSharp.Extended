# Migrating from SkiaSharp.Extended.Svg to Svg.Skia

## Overview

`SkiaSharp.Extended.Svg` has been deprecated and is no longer maintained. Due to breaking changes in SkiaSharp (specifically the removal of `SKMatrix.MakeTranslation` method), the old library is incompatible with .NET 9 and recent versions of SkiaSharp.

**The recommended migration path is to use the [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia) library**, which is actively maintained and fully compatible with modern SkiaSharp versions.

## Why Migrate?

- ⚠️ `SkiaSharp.Extended.Svg` is **no longer maintained**
- ❌ Incompatible with .NET 9 and recent SkiaSharp versions
- 🚫 Causes runtime errors: `MissingMethodException: Method not found: SkiaSharp.SKMatrix.MakeTranslation`
- ✅ `Svg.Skia` is actively maintained with regular updates
- ✅ Full SVG specification support
- ✅ Compatible with latest SkiaSharp releases

## Migration Steps

### 1. Update NuGet Packages

Remove the old package and install Svg.Skia:

```bash
# Remove the old package
dotnet remove package SkiaSharp.Extended.Svg

# Add Svg.Skia
dotnet add package Svg.Skia
```

### 2. Update Using Statements

Replace the old namespace with the new one:

```csharp
// OLD - Remove this
using SkiaSharp.Extended.Svg;

// NEW - Add this
using Svg.Skia;
```

### 3. Update Code Usage

The good news is that `Svg.Skia` provides a very similar API, so most code will work with minimal changes:

#### Loading SVG from Stream

```csharp
// Both old and new code look the same!
var svg = new SKSvg();

using (var stream = GetType().Assembly.GetManifestResourceStream(resourceId))
{
    if (stream != null)
    {
        svg.Load(stream);
    }
}
```

#### Drawing SVG on Canvas

```csharp
// Drawing the SVG picture
if (svg?.Picture != null)
{
    canvas.Clear(SKColors.White);
    
    // Calculate scaling to fit canvas
    var canvasMin = Math.Min(width, height);
    var svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
    var scale = canvasMin / svgMax;
    var matrix = SKMatrix.CreateScale(scale, scale);
    
    canvas.DrawPicture(svg.Picture, matrix);
}
```

## Common Issues and Solutions

### Issue: MissingMethodException for MakeTranslation

**Error:**
```
System.MissingMethodException: Method not found: 
SkiaSharp.SKMatrix SkiaSharp.SKMatrix.MakeTranslation(single,single)
```

**Solution:** This error occurs when using `SkiaSharp.Extended.Svg` with modern SkiaSharp versions. Migrate to `Svg.Skia` as described above.

### Issue: Package Version Conflicts

If you're experiencing package conflicts, ensure all SkiaSharp-related packages are aligned to compatible versions:

```xml
<PackageReference Include="SkiaSharp" Version="3.x.x" />
<PackageReference Include="SkiaSharp.Views.Maui.Controls" Version="3.x.x" />
<PackageReference Include="Svg.Skia" Version="2.x.x" />
```

## Example: Complete Migration

### Before (SkiaSharp.Extended.Svg)

```csharp
using SkiaSharp;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MyApp
{
    public partial class MyPage : ContentPage
    {
        private SKSvg? svg;

        private void LoadSvg()
        {
            svg = new SKSvg();
            using var stream = FileSystem.OpenAppPackageFileAsync("image.svg").Result;
            svg.Load(stream);
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);
            
            if (svg?.Picture != null)
            {
                canvas.DrawPicture(svg.Picture);
            }
        }
    }
}
```

### After (Svg.Skia)

```csharp
using SkiaSharp;
using Svg.Skia;  // Changed this line
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MyApp
{
    public partial class MyPage : ContentPage
    {
        private SKSvg? svg;

        private void LoadSvg()
        {
            svg = new SKSvg();
            using var stream = FileSystem.OpenAppPackageFileAsync("image.svg").Result;
            svg.Load(stream);  // Same API!
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);
            
            if (svg?.Picture != null)
            {
                canvas.DrawPicture(svg.Picture);  // Same API!
            }
        }
    }
}
```

## Additional Resources

- [Svg.Skia GitHub Repository](https://github.com/wieslawsoltes/Svg.Skia)
- [Svg.Skia NuGet Package](https://www.nuget.org/packages/Svg.Skia)
- [Microsoft MAUI SkiaSharp Migration Guide](https://learn.microsoft.com/en-us/dotnet/maui/migration/skiasharp)
- [SkiaSharp Documentation](https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)

## Need Help?

If you encounter issues during migration:

1. Check that all SkiaSharp packages are using compatible versions
2. Review the [Svg.Skia examples](https://github.com/wieslawsoltes/Svg.Skia/tree/main/samples)
3. Open an issue on the [Svg.Skia repository](https://github.com/wieslawsoltes/Svg.Skia/issues) for library-specific questions
4. Open an issue on the [SkiaSharp.Extended repository](https://github.com/mono/SkiaSharp.Extended/issues) for general guidance
