# Image Pyramid for Blazor

Use `SKImagePyramidController` with a plain `SKCanvasView` in Blazor WebAssembly to render Image Pyramid images. There is no custom component — the page wires the services directly to the canvas, giving you full control over layout, interaction, and lifecycle.

## Quick Start

```razor
@page "/deepzoom"
@implements IAsyncDisposable
@inject HttpClient Http
@using SkiaSharp.Extended

<SKCanvasView @ref="_canvas"
              OnPaintSurface="OnPaintSurface"
              style="width: 100%; height: 600px; touch-action: none;" />

@code {
    private SKCanvasView? _canvas;
    private SKImagePyramidController? _controller;
    private readonly SKImagePyramidRenderer _renderer = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _controller = new SKImagePyramidController();
        _controller.InvalidateRequired += OnInvalidateRequired;

        var xml     = await Http.GetStringAsync("deepzoom/image.dzi");
        var baseUrl = new Uri(Http.BaseAddress!, "deepzoom/image_files/").ToString();
        var source  = SKImagePyramidDziSource.Parse(xml, baseUrl);

        _controller.Load(source, new SKTieredTileProvider(new SKHttpTileFetcher()));
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        if (_controller == null) return;
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _renderer.Canvas = e.Surface.Canvas;
        _controller.Render(_renderer);
    }

    private void OnInvalidateRequired(object? sender, EventArgs e)
        => InvokeAsync(() => _canvas?.Invalidate());

    public async ValueTask DisposeAsync()
    {
        if (_controller != null)
        {
            _controller.InvalidateRequired -= OnInvalidateRequired;
            _controller.Dispose();
        }
    }
}
```

## Serving Tile Assets

Place `.dzi` and tile folder under `wwwroot`. In the project file, mark them as content:

```xml
<ItemGroup>
    <Content Include="wwwroot\deepzoom\image.dzi" />
    <Content Include="wwwroot\deepzoom\image_files\**" />
</ItemGroup>
```

The `SKTieredTileProvider` with `SKHttpTileFetcher` fetches each tile via `HttpClient`; tile URLs are constructed automatically from the base URL you pass to `SKImagePyramidDziSource.Parse`.

## Pan and Zoom

Wire mouse and touch events to the controller's navigation methods:

```razor
<SKCanvasView @ref="_canvas"
              OnPaintSurface="OnPaintSurface"
              @onmousedown="OnMouseDown"
              @onmousemove="OnMouseMove"
              @onmouseup="OnMouseUp"
              @onwheel="OnWheel"
              style="width: 100%; height: 600px; touch-action: none;" />

@code {
    private bool _dragging;
    private double _lastX, _lastY;

    private void OnMouseDown(MouseEventArgs e)
    {
        _dragging = true;
        _lastX = e.ClientX;
        _lastY = e.ClientY;
    }

    private void OnMouseMove(MouseEventArgs e)
    {
        if (!_dragging || _controller == null) return;
        _controller.Pan(e.ClientX - _lastX, e.ClientY - _lastY);
        _lastX = e.ClientX;
        _lastY = e.ClientY;
        _canvas?.Invalidate();
    }

    private void OnMouseUp(MouseEventArgs e) => _dragging = false;

    private void OnWheel(WheelEventArgs e)
    {
        if (_controller == null) return;
        double factor = e.DeltaY < 0 ? 1.15 : 1.0 / 1.15;
        _controller.ZoomAboutScreenPoint(factor, e.OffsetX, e.OffsetY);
        _canvas?.Invalidate();
    }
}
```

## Loading a DZC Collection

```csharp
var xml = await Http.GetStringAsync("collection.dzc");
var collection = SKImagePyramidDziCollectionSource.Parse(xml);
collection.TilesBaseUri = Http.BaseAddress!.ToString();
_controller!.Load(collection, new SKTieredTileProvider(new SKHttpTileFetcher()));
```

## Custom Provider

Pass a custom `ISKImagePyramidTileProvider` to the controller's `Load()` method for advanced scenarios:

```csharp
// With disk cache (persists across Blazor restarts via OPFS or service worker)
controller.Load(source, new SKTieredTileProvider(
    new SKHttpTileFetcher(),
    new SKDiskTileCacheStore("/cache/tiles", expiry: TimeSpan.FromDays(7))));
```

See the [Tile Providers](fetching.md) docs for implementing browser storage tiers, delay wrappers, and other custom strategies.

## Canvas Resize

When the Blazor page resizes, `SetControlSize` automatically picks up the new dimensions on the next paint. If you want to trigger a reset to fit the image after a resize:

```csharp
// In a JS interop resize callback:
[JSInvokable]
public void OnCanvasResized()
{
    _controller?.ResetView();
    InvokeAsync(() => _canvas?.Invalidate());
}
```

## Rendering Behaviour

- **Fit and center**: On load the controller calls `ResetView()` — the full image is visible, centered horizontally and vertically, with aspect ratio preserved. No cropping or distortion.
- **LOD blending**: While high-resolution tiles are in-flight, lower-resolution parent tiles are upscaled and composited as placeholders.
- **Idle detection**: `controller.IsIdle` is `true` when no tiles are loading. You can pause periodic repaints when the view is idle.

## Related

- [Image Pyramid overview](index.md)
- [Controller & Viewport](controller.md)
- [Tile Fetching](fetching.md)
- [Caching](caching.md)
- [Image Pyramid for MAUI](maui.md)
