# Image Pyramid for MAUI

Use `SKImagePyramidController` with a plain `SKCanvasView` in .NET MAUI to render Image Pyramid images. There is no custom control — you wire the services directly to the canvas, keeping the integration minimal and transparent.

## Quick Start

### XAML

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"
             x:Class="MyApp.ImagePyramidPage"
             Title="Image Pyramid">

    <skia:SKCanvasView x:Name="canvas" PaintSurface="OnPaintSurface" />

</ContentPage>
```

### Code-Behind

```csharp
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Maui;

public partial class ImagePyramidPage : ContentPage
{
    private readonly SKImagePyramidController _controller = new();
    private readonly SKImagePyramidRenderer _renderer = new();

    public ImagePyramidPage()
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
        using var stream = await FileSystem.OpenAppPackageFileAsync("image.dzi");
        using var reader = new StreamReader(stream);
        var xml = await reader.ReadToEndAsync();

        var source = SKImagePyramidDziSource.Parse(xml, "image_files/");
        _controller.Load(source, new AppPackageFetcher());
        canvas.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _renderer.Canvas = e.Surface.Canvas;
        _controller.Render(_renderer);
    }
}
```

## App-Package Tile Fetcher

When tiles are bundled as MAUI assets, implement `ISKImagePyramidTileFetcher` to read them via `FileSystem.OpenAppPackageFileAsync` and decode with `SKImage.FromEncodedData`:

```csharp
public sealed class AppPackageFetcher : ISKImagePyramidTileFetcher
{
    public async Task<SKImage?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(url);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            return SKImage.FromEncodedData(ms.ToArray());
        }
        catch
        {
            return null;
        }
    }

    public void Dispose() { }
}
```

Use it directly:

```csharp
_controller.Load(source, new AppPackageFetcher());
```

Include assets in the project file:

```xml
<ItemGroup>
    <MauiAsset Include="Assets\image.dzi"
               LogicalName="image.dzi" />
    <MauiAsset Include="Assets\image_files\**"
               LogicalName="image_files/%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

## HTTP Tile Fetcher

For remote images, use the built-in `SKImagePyramidHttpTileFetcher`:

```csharp
using var httpClient = new HttpClient();
var fetcher = new SKImagePyramidHttpTileFetcher(httpClient);

var xml    = await httpClient.GetStringAsync("https://example.com/image.dzi");
var source = SKImagePyramidDziSource.Parse(xml, "https://example.com/image_files/");
_controller.Load(source, fetcher);
```

## Pan and Zoom

Add gesture recognizers or pointer events to enable interactive navigation:

```csharp
public ImagePyramidPage()
{
    InitializeComponent();
    _controller.InvalidateRequired += (_, _) => canvas.InvalidateSurface();

    // Pan with finger/mouse drag
    var pan = new PanGestureRecognizer();
    pan.PanUpdated += OnPanUpdated;
    canvas.GestureRecognizers.Add(pan);

    // Pinch to zoom
    var pinch = new PinchGestureRecognizer();
    pinch.PinchUpdated += OnPinchUpdated;
    canvas.GestureRecognizers.Add(pinch);
}

// TotalX/Y is cumulative per gesture, so we must track the previous value
// to compute a per-frame delta.
private double _lastPanX, _lastPanY;

private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
{
    switch (e.StatusType)
    {
        case GestureStatus.Started:
            _lastPanX = 0;
            _lastPanY = 0;
            break;
        case GestureStatus.Running:
            var dx = e.TotalX - _lastPanX;
            var dy = e.TotalY - _lastPanY;
            _lastPanX = e.TotalX;
            _lastPanY = e.TotalY;
            _controller.Pan(dx, dy);
            canvas.InvalidateSurface();
            break;
    }
}

private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
{
    if (e.Status == GestureStatus.Running)
    {
        _controller.SetZoom(_controller.Viewport.Zoom * e.Scale);
        canvas.InvalidateSurface();
    }
}
```

## Loading a DZC Collection

```csharp
using var stream = await FileSystem.OpenAppPackageFileAsync("collection.dzc");
using var reader = new StreamReader(stream);
var xml = await reader.ReadToEndAsync();

var collection = SKImagePyramidDziCollectionSource.Parse(xml);
collection.TilesBaseUri = "collection_files/";
_controller.Load(collection, new AppPackageFetcher());
```

## Custom Cache

```csharp
// Smaller cache for memory-constrained devices
var cache = new SKImagePyramidMemoryTileCache(maxEntries: 256);
var controller = new SKImagePyramidController(cache: cache);
```

See the [Caching docs](caching.md) for custom disk-backed caches.

## Rendering Behaviour

- **Fit and center**: On load the controller fits the full image into the canvas with `ResetView()`. The image is centered; neither cropping nor distortion occurs.
- **LOD blending**: While high-resolution tiles load, lower-resolution tiles from parent levels are upscaled and composited as placeholders.
- **Idle detection**: `controller.IsIdle` is `true` when no tiles are loading.

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Tile Fetching](fetching.md)
- [Caching](caching.md)
- [Image Pyramid for Blazor](blazor.md)
