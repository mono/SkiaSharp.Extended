# Deep Zoom

Pan, pinch, and explore gigapixel images in your .NET MAUI and Blazor apps using the Deep Zoom Image (DZI) format. The deep zoom system downloads only the tiles visible at the current zoom level, so even multi-gigapixel images load instantly.

## What is Deep Zoom?

[Deep Zoom](https://docs.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/cc645050(v=vs.95)) is a tile-based image format developed by Microsoft. An image is pre-sliced into a pyramid of tiles at multiple resolutions. At any zoom level, only the small set of tiles visible in the viewport is loaded — making it practical to explore images with billions of pixels.

**When to use Deep Zoom:**
- 🗺️ High-resolution maps, satellite imagery, or floor plans
- 🎨 Gigapixel art, museum collection viewers
- 🔬 Medical imaging, microscopy slides
- 📸 Any image too large to load in full at once

## Quick Start

### MAUI — Add SKDeepZoomView

Add the view to your XAML:

```xml
<ContentPage xmlns:dz="clr-namespace:SkiaSharp.Extended.UI.Maui.DeepZoom;assembly=SkiaSharp.Extended.UI.Maui.DeepZoom">

    <dz:SKDeepZoomView x:Name="deepZoomView"
                       Source="https://example.com/image.dzi" />

</ContentPage>
```

Or load programmatically from app resources:

```csharp
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

// Load from a bundled .dzi file
using var stream = await FileSystem.OpenAppPackageFileAsync("image.dzi");
using var reader = new StreamReader(stream);
var xml = await reader.ReadToEndAsync();

var tileSource = DziTileSource.Parse(xml, "asset://image_files/");
deepZoomView.Load(tileSource, new AppPackageTileFetcher());
```

### Blazor — Use DeepZoomController with SKGestureTracker

```razor
@page "/deepzoom"
@implements IDisposable
@inject HttpClient Http
@using SkiaSharp.Extended
@using SkiaSharp.Extended.DeepZoom

<div @onpointerdown="OnPointerDown"
     @onpointermove="OnPointerMove"
     @onpointerup="OnPointerUp"
     @onpointercancel="OnPointerCancel"
     @onwheel="OnWheel"
     @onwheel:preventDefault
     style="touch-action: none; cursor: grab;">
    <SKCanvasView @ref="_canvas" OnPaintSurface="OnPaintSurface"
                  style="width: 100%; height: 600px;" />
</div>

@code {
    private SKCanvasView? _canvas;
    private DeepZoomController? _controller;
    private readonly SKGestureTracker _tracker = new(new SKGestureTrackerOptions
    {
        IsRotateEnabled = false,
        IsDoubleTapZoomEnabled = true,   // Tracker animates double-tap zoom
        IsScrollZoomEnabled = true,      // Tracker handles scroll/wheel zoom
        IsFlingEnabled = true,           // Tracker animates fling deceleration
        MinScale = 1f,                   // Can't zoom out past the full image
        MaxScale = 32f,
    });
    private float _displayScale = 1f;
    private bool _suppressSync;

    protected override void OnInitialized()
    {
        _tracker.TransformChanged += OnTrackerTransformChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _controller = new DeepZoomController();
        _controller.ImageOpenSucceeded += (_, _) => _tracker.Reset();
        _controller.InvalidateRequired += (_, _) => InvokeAsync(() => _canvas?.Invalidate());

        var xml = await Http.GetStringAsync("deepzoom/image.dzi");
        var baseUrl = new Uri(Http.BaseAddress!, "deepzoom/image_files/").ToString();
        var tileSource = DziTileSource.Parse(xml, baseUrl);
        _controller.Load(tileSource, new HttpTileFetcher(new HttpClient()));
    }

    private void OnTrackerTransformChanged(object? sender, EventArgs e)
    {
        if (_suppressSync || _controller == null) return;
        var w = _controller.Viewport.ControlWidth;
        if (w <= 0) { InvokeAsync(() => _canvas?.Invalidate()); return; }

        double scale = _tracker.Scale;
        _controller.Viewport.ViewportWidth = 1.0 / scale;
        _controller.Viewport.ViewportOriginX = -_tracker.Offset.X / (w * scale);
        _controller.Viewport.ViewportOriginY = -_tracker.Offset.Y / (w * scale);
        _controller.Viewport.Constrain();

        var vp = _controller.Viewport;
        float ns = (float)(1.0 / vp.ViewportWidth);
        _suppressSync = true;
        _tracker.SetTransform(ns, 0f, new SKPoint(
            (float)(-vp.ViewportOriginX * w * ns),
            (float)(-vp.ViewportOriginY * w * ns)));
        _suppressSync = false;

        InvokeAsync(() => _canvas?.Invalidate());
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        _displayScale = e.Info.Height / 600f; // CSS height
        _controller?.SetControlSize(e.Info.Width, e.Info.Height);
        _controller?.Update();
        e.Surface.Canvas.Clear(SKColors.White);
        _controller?.Render(e.Surface.Canvas);
    }

    private SKPoint Scale(PointerEventArgs e) =>
        new((float)e.OffsetX * _displayScale, (float)e.OffsetY * _displayScale);

    private void OnPointerDown(PointerEventArgs e) =>
        _tracker.ProcessTouchDown(e.PointerId, Scale(e), e.PointerType == "mouse");
    private void OnPointerMove(PointerEventArgs e) =>
        _tracker.ProcessTouchMove(e.PointerId, Scale(e), e.Buttons > 0 || e.PointerType != "mouse");
    private void OnPointerUp(PointerEventArgs e) =>
        _tracker.ProcessTouchUp(e.PointerId, Scale(e), e.PointerType == "mouse");
    private void OnPointerCancel(PointerEventArgs e) =>
        _tracker.ProcessTouchCancel(e.PointerId);
    private void OnWheel(WheelEventArgs e) =>
        _tracker.ProcessMouseWheel(new((float)e.OffsetX * _displayScale, (float)e.OffsetY * _displayScale), 0, e.DeltaY < 0 ? 1f : -1f);

    public void Dispose()
    {
        _tracker.TransformChanged -= OnTrackerTransformChanged;
        _tracker.Dispose();
        _controller?.Dispose();
    }
}
```

## Gestures

`SKDeepZoomView` (MAUI) and the Blazor sample both use [`SKGestureTracker`](xref:SkiaSharp.Extended.SKGestureTracker) for unified gesture handling. The tracker owns all gesture animation — fling deceleration, double-tap zoom easing, and scroll zoom — via the built-in `SKTimerAnimation` and `SKEasingFunctions` animation system.

| Gesture | Action |
| :------ | :----- |
| **Pan** (single finger / mouse drag) | Pan the image |
| **Pinch** (two fingers) | Zoom in or out around the pinch center |
| **Double-tap** | Toggle between fit-to-screen and 2× zoom (animated via the tracker) |
| **Scroll** (mouse wheel) | Zoom in or out at the cursor position (animated via the tracker) |
| **Fling** | Momentum scrolling after a fast pan (decelerated via the tracker) |

## SKDeepZoomView Properties (MAUI)

| Property | Type | Default | Description |
| :------- | :--- | :------ | :---------- |
| `Source` | `string?` | `null` | URI to a `.dzi` file. Setting this fetches and loads the image automatically. |
| `ShowTileBorders` | `bool` | `false` | Draw a colored border around each tile (debug aid). |
| `ShowDebugStats` | `bool` | `false` | Overlay viewport, level, and tile cache statistics. |
| `ViewportWidth` | `double` | `1.0` | Current zoom level in normalized units (1.0 = full image visible). |
| `ViewportOriginX` | `double` | `0.0` | Normalized horizontal pan offset. |
| `ViewportOriginY` | `double` | `0.0` | Normalized vertical pan offset. |
| `AspectRatio` | `double` | `0` | Image width / height (read-only, 0 when not loaded). |
| `IsIdle` | `bool` | `true` | True when no gesture animation is running and no tiles are loading. |
| `Controller` | `DeepZoomController` | — | Access the underlying controller for advanced use. |
| `GestureTracker` | `SKGestureTracker` | — | Access the gesture tracker to configure thresholds, scale limits, or feature toggles. |

## SKDeepZoomView Events (MAUI)

| Event | Args | Description |
| :---- | :--- | :---------- |
| `ImageOpenSucceeded` | `EventArgs` | Fired after the DZI source is parsed and the controller is ready. |
| `ImageOpenFailed` | `Exception` | Fired when the source URI fails to load or parse. |
| `ViewportChanged` | `EventArgs` | Fired on every viewport change (pan or zoom). |

## Methods

```csharp
// Zoom to fit the whole image in the viewport
deepZoomView.ResetView();

// Zoom 2× around the center of the image in logical coordinates
deepZoomView.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);

// Load a DZI tile source manually (e.g., from a file or stream)
deepZoomView.Load(tileSource, fetcher);

// Convert between screen and logical coordinate spaces
var (lx, ly) = deepZoomView.ElementToLogicalPoint(screenX, screenY);
var (sx, sy) = deepZoomView.LogicalToElementPoint(logicalX, logicalY);
```

## Core Classes

### DeepZoomController

The platform-agnostic rendering engine. Used directly in Blazor and internally by `SKDeepZoomView`.

```csharp
var controller = new DeepZoomController(cacheCapacity: 1024);

// Load a DZI image
controller.Load(tileSource, fetcher);

// Called each frame to schedule tile loading (returns true while tiles are loading)
bool needsRepaint = controller.Update();

// Called in the paint handler
controller.SetControlSize(width, height);
controller.Render(canvas);

// Programmatic navigation (instant; no animation)
controller.Pan(dx, dy);                              // Pan by pixel delta
controller.ZoomAboutScreenPoint(factor, cx, cy);     // Zoom around a point
controller.ZoomAboutLogicalPoint(factor, lx, ly);    // Zoom around logical coords
controller.ResetView();                              // Fit to viewport

// Read current viewport state
double vw = controller.Viewport.ViewportWidth;   // 1.0 = full image
double ox = controller.Viewport.ViewportOriginX;
double oy = controller.Viewport.ViewportOriginY;
```

> **Note**: `DeepZoomController` is animation-free by design — it just manages tiles and provides a viewport. All animation (fling, double-tap zoom, scroll zoom) is handled by `SKGestureTracker`.

### DziTileSource

Parses `.dzi` (Deep Zoom Image) XML descriptors:

```csharp
// From a URI string (sets TilesBaseUri automatically)
var tileSource = DziTileSource.Parse(xmlString, "https://example.com/image_files/");

// From a stream
using var stream = File.OpenRead("image.dzi");
var tileSource = DziTileSource.Parse(stream);
tileSource.TilesBaseUri = "https://example.com/image_files/";

// Key properties
int width = tileSource.ImageWidth;
int height = tileSource.ImageHeight;
int tileSize = tileSource.TileSize;
int maxLevel = tileSource.MaxLevel;
double aspectRatio = tileSource.AspectRatio;
```

### DzcTileSource

Parses `.cxml` (Deep Zoom Collection) descriptors for multi-image mosaics:

```csharp
var dzcSource = DzcTileSource.Parse(xmlString, baseUri);
controller.Load(dzcSource, fetcher);

// Access sub-images after loading
foreach (var sub in controller.SubImages)
{
    Console.WriteLine($"Sub-image at ({sub.X}, {sub.Y}), size {sub.Width}×{sub.Height}");
}
```

### Tile Fetchers

Two built-in fetchers are provided:

```csharp
// HTTP — fetches tiles from a URL (requires HttpClient)
var fetcher = new HttpTileFetcher();           // Creates its own HttpClient
var fetcher = new HttpTileFetcher(httpClient); // Reuse an existing client

// File system (for testing or offline use)
var fetcher = new FileTileFetcher(baseDirectory);
```

Custom fetchers implement `ITileFetcher`:

```csharp
public class AppPackageTileFetcher : ITileFetcher
{
    public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
    {
        var path = url.Replace("asset://", "");
        using var stream = await FileSystem.OpenAppPackageFileAsync(path);
        return SKBitmap.Decode(stream);
    }

    public void Dispose() { }
}
```

### Viewport

Handles the coordinate transforms between screen pixels, logical (0–1) image coordinates, and tile pixel coordinates:

```csharp
var viewport = controller.Viewport;

// Convert a screen tap to logical image coordinates
var (lx, ly) = viewport.ElementToLogicalPoint(tapX, tapY);

// Current zoom/pan state
double viewportWidth = viewport.ViewportWidth;   // 1.0 = full image visible
double originX = viewport.ViewportOriginX;
double originY = viewport.ViewportOriginY;
```

### TileCache

LRU cache for decoded tile bitmaps. Deferred disposal prevents GPU stalls:

```csharp
// Inspect cache state (via controller)
int count = controller.Cache.Count;
int capacity = controller.Cache.Capacity;
controller.Cache.Clear();
```

## Performance Tips

- **Cache capacity**: The default of 1024 tiles is generous for most use cases. Reduce for memory-constrained devices: `new DeepZoomController(cacheCapacity: 256)`.
- **Stop rendering when idle**: Check `controller.IsIdle` and stop your animation loop when there are no pending tile loads and no active gesture.
- **Gesture tracker limits**: Configure `MinScale`/`MaxScale` on `SKGestureTrackerOptions` to bound zoom range and prevent users from zooming too far out or in.

## Next Steps

- [API Reference — DeepZoomController](xref:SkiaSharp.Extended.DeepZoom.DeepZoomController)
- [API Reference — SKDeepZoomView](xref:SkiaSharp.Extended.UI.Maui.DeepZoom.SKDeepZoomView)
- [API Reference — DziTileSource](xref:SkiaSharp.Extended.DeepZoom.DziTileSource)
- [MAUI Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo/Demos/DeepZoom)
- [Blazor Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo.Blazor/Pages/DeepZoom.razor)
- [Gestures](gestures.md) — SKGestureTracker used for deep zoom input handling
