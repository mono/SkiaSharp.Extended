# Deep Zoom for MAUI

Use `SKDeepZoomController` with a plain `SKCanvasView` to render Deep Zoom images in .NET MAUI. There is no custom control — you wire the services directly to the canvas, which keeps the integration minimal and transparent.

## Quick Start

### XAML

Add a `SKCanvasView` to your page:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"
             x:Class="MyApp.DeepZoomPage"
             Title="Deep Zoom">

    <skia:SKCanvasView x:Name="canvas" PaintSurface="OnPaintSurface" />

</ContentPage>
```

### Code-Behind

```csharp
using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;

public partial class DeepZoomPage : ContentPage
{
    private readonly SKDeepZoomController _controller = new();

    public DeepZoomPage()
    {
        InitializeComponent();
        _controller.InvalidateRequired += (_, _) => canvas.InvalidateSurface();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _controller.Dispose();
    }

    private async void LoadAsync()
    {
        // Load a .dzi bundled as a MAUI app-package asset
        using var stream = await FileSystem.OpenAppPackageFileAsync("image.dzi");
        using var reader = new StreamReader(stream);
        var xml = await reader.ReadToEndAsync();

        var tileSource = SKDeepZoomImageSource.Parse(xml, "image_files/");
        _controller.Load(tileSource, new AppPackageFetcher());
        canvas.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _controller.Render(e.Surface.Canvas);
    }
}
```

## App-Package Tile Fetcher

When tiles are bundled as MAUI assets, implement `ISKDeepZoomTileFetcher` to read them via `FileSystem.OpenAppPackageFileAsync`:

```csharp
public sealed class AppPackageFetcher : ISKDeepZoomTileFetcher
{
    public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(url);
            return SKBitmap.Decode(stream);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose() { }
}
```

Include the `.dzi` and tile folder in the project file:

```xml
<ItemGroup>
    <MauiAsset Include="Assets\image.dzi"               LogicalName="image.dzi" />
    <MauiAsset Include="Assets\image_files\**"          LogicalName="image_files/%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

## HTTP Tile Fetcher

For remote images, use the built-in `SKDeepZoomHttpTileFetcher`:

```csharp
var httpClient = new HttpClient();
var fetcher    = new SKDeepZoomHttpTileFetcher(httpClient);

var xml        = await httpClient.GetStringAsync("https://example.com/image.dzi");
var tileSource = SKDeepZoomImageSource.Parse(xml, "https://example.com/image_files/");

_controller.Load(tileSource, fetcher);
```

## Rendering Behaviour

- **Fit and center**: On load the controller fits the full image into the canvas with `FitToView()`. The image is centered horizontally and vertically; neither cropping nor distortion occurs.
- **Tile resolution**: `Update()` selects the pyramid level that best matches the physical pixel size of the canvas. Only tiles visible in the current viewport are fetched.
- **Tile blending**: While high-resolution tiles load, lower-resolution tiles from the parent level are upscaled and composited as a placeholder (LOD blending).

## Related

- [Deep Zoom overview](deep-zoom.md) — architecture, services, and API reference
- [Deep Zoom for Blazor](deep-zoom-blazor.md) — Blazor WebAssembly integration
