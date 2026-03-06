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
        IsDoubleTapZoomEnabled = false,  // deep zoom handles zoom animation
        IsScrollZoomEnabled = false,
        IsFlingEnabled = true,
    });
    private float _displayScale = 1f;

    protected override void OnInitialized()
    {
        _tracker.PanDetected    += (_, e) => { e.Handled = true; _controller?.Pan(e.Delta.X, e.Delta.Y); _controller?.SnapSpringToTarget(); _canvas?.Invalidate(); };
        _tracker.PinchDetected  += (_, e) => { _controller?.ZoomAboutScreenPoint(e.ScaleDelta, e.FocalPoint.X, e.FocalPoint.Y); _canvas?.Invalidate(); };
        _tracker.ScrollDetected += (_, e) => { _controller?.ZoomAboutScreenPoint(e.Delta.Y > 0 ? 1.2 : 1/1.2, e.Location.X, e.Location.Y); _canvas?.Invalidate(); };
        _tracker.FlingUpdated   += (_, e) => { if (e.Delta != SKPoint.Empty) { _controller?.Pan(e.Delta.X, e.Delta.Y); _canvas?.Invalidate(); } };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _controller = new DeepZoomController();
        _controller.InvalidateRequired += (_, _) => _canvas?.Invalidate();

        var xml = await Http.GetStringAsync("deepzoom/image.dzi");
        var baseUrl = new Uri(Http.BaseAddress!, "deepzoom/image_files/").ToString();
        var tileSource = DziTileSource.Parse(xml, baseUrl);
        _controller.Load(tileSource, new HttpTileFetcher(new HttpClient()));
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        _displayScale = e.Info.Height / 600f; // CSS height
        _controller?.SetControlSize(e.Info.Width, e.Info.Height);
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
        _tracker.Dispose();
        _controller?.Dispose();
    }
}
```

## Gestures

`SKDeepZoomView` (MAUI) and the Blazor sample both use [`SKGestureTracker`](xref:SkiaSharp.Extended.SKGestureTracker) for unified gesture handling:

| Gesture | Action |
| :------ | :----- |
| **Pan** (single finger / mouse drag) | Pan the image |
| **Pinch** (two fingers) | Zoom in or out around the pinch center |
| **Double-tap** | Toggle between fit-to-screen and 2× zoom |
| **Scroll** (mouse wheel) | Zoom in or out at the cursor position |
| **Fling** | Momentum scrolling after a fast pan |

The tracker's built-in pan/zoom animation is **disabled** for deep zoom — the controller uses its own spring physics for smooth transitions, so double-tap zoom and animated resets look and feel native.

## SKDeepZoomView Properties (MAUI)

| Property | Type | Default | Description |
| :------- | :--- | :------ | :---------- |
| `Source` | `string?` | `null` | URI to a `.dzi` file. Setting this fetches and loads the image automatically. |
| `UseSprings` | `bool` | `true` | Enable spring-physics transitions for smooth pan/zoom. |
| `SpringStiffness` | `double` | `100.0` | Spring stiffness — higher values snap faster, lower values feel smoother. |
| `SpringDampingRatio` | `double` | `1.0` | Damping ratio — 1.0 = no overshoot, &lt;1.0 = bouncy, &gt;1.0 = sluggish. |
| `ShowTileBorders` | `bool` | `false` | Draw a colored border around each tile (debug aid). |
| `ShowDebugStats` | `bool` | `false` | Overlay viewport, level, and tile cache statistics. |
| `ViewportWidth` | `double` | `1.0` | Current zoom level in normalized units (1.0 = full image visible). |
| `ViewportOriginX` | `double` | `0.0` | Normalized horizontal pan offset. |
| `ViewportOriginY` | `double` | `0.0` | Normalized vertical pan offset. |
| `AspectRatio` | `double` | `0` | Image width / height (read-only, 0 when not loaded). |
| `IsIdle` | `bool` | `true` | True when no animation is running and no tiles are loading. |
| `Controller` | `DeepZoomController` | — | Access the underlying controller for advanced use. |
| `GestureTracker` | `SKGestureTracker` | — | Access the gesture tracker to configure thresholds or feature toggles. |

## SKDeepZoomView Events (MAUI)

| Event | Args | Description |
| :---- | :--- | :---------- |
| `ImageOpenSucceeded` | `EventArgs` | Fired after the DZI source is parsed and the controller is ready. |
| `ImageOpenFailed` | `Exception` | Fired when the source URI fails to load or parse. |
| `MotionFinished` | `EventArgs` | Fired when spring animation settles (image is at rest). |
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

// Called each frame to advance spring animation
bool needsRepaint = controller.Update(TimeSpan.FromMilliseconds(16));

// Called in the paint handler
controller.SetControlSize(width, height);
controller.Render(canvas);

// Programmatic navigation
controller.Pan(dx, dy);                              // Pan by pixel delta
controller.ZoomAboutScreenPoint(factor, cx, cy);     // Zoom around a point
controller.ZoomAboutLogicalPoint(factor, lx, ly);    // Zoom around logical coords
controller.ResetView();                              // Fit to viewport
controller.SnapSpringToTarget();                     // Skip to spring target (during gestures)

// Configure spring animation feel
controller.SpringStiffness = 200;    // Faster snap (default 100)
controller.SpringDampingRatio = 0.8; // Slightly bouncy (default 1.0 = no overshoot)
```

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
- **Spring physics**: Disable `UseSprings = false` for simpler/faster rendering if you don't need animated transitions. Use `SpringStiffness` and `SpringDampingRatio` to tune the animation feel — higher stiffness snaps faster, damping ratio below 1.0 adds bounce.
- **Stop the timer when idle**: Check `controller.IsIdle` in your animation loop and stop the timer to avoid burning CPU between interactions.

## Next Steps

- [API Reference — DeepZoomController](xref:SkiaSharp.Extended.DeepZoom.DeepZoomController)
- [API Reference — SKDeepZoomView](xref:SkiaSharp.Extended.UI.Maui.DeepZoom.SKDeepZoomView)
- [API Reference — DziTileSource](xref:SkiaSharp.Extended.DeepZoom.DziTileSource)
- [MAUI Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo/Demos/DeepZoom)
- [Blazor Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo.Blazor/Pages/DeepZoom.razor)
- [Gestures](gestures.md) — SKGestureTracker used for deep zoom input handling
