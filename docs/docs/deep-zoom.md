# Deep Zoom

Pan, pinch, and explore gigapixel images in your .NET MAUI and Blazor apps using the Deep Zoom Image (DZI) format. The deep zoom system downloads only the tiles visible at the current zoom level, so even multi-gigapixel images load instantly.

## What is Deep Zoom?

[Deep Zoom](https://docs.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/cc645050(v=vs.95)) is a tile-based image format developed by Microsoft. An image is pre-sliced into a pyramid of tiles at multiple resolutions. At any zoom level, only the small set of tiles visible in the viewport is loaded — making it practical to explore images with billions of pixels.

**When to use Deep Zoom:**
- 🗺️ High-resolution maps, satellite imagery, or floor plans
- 🎨 Gigapixel art, museum collection viewers
- 🔬 Medical imaging, microscopy slides
- 📸 Any image too large to load in full at once

## Architecture

The deep zoom system is split into three layers with strict separation of concerns:

| Layer | Package | Responsibility |
| :---- | :------ | :------------- |
| **DeepZoomController** | `SkiaSharp.Extended.DeepZoom` | Tile loading, caching, viewport math, rendering. No animation, no gestures. |
| **SKGestureTracker** | `SkiaSharp.Extended` | Gesture detection and animation (fling, double-tap zoom, scroll zoom). No tile awareness. |
| **View** | Platform-specific | Composes the controller and gesture tracker. Translates tracker transforms into viewport coordinates. |

```
Touch events
    ↓
SKGestureTracker  ← owns all animation (fling, zoom easing)
    ↓ TransformChanged (every frame while animating)
View layer  →  translate scale/offset → controller.Viewport
    ↓
DeepZoomController  →  schedule tiles → render
    ↓
Canvas
```

For platform-specific integration, see:
- [Deep Zoom for MAUI](deep-zoom-maui.md) — `SKDeepZoomView` control
- [Deep Zoom for Blazor](deep-zoom-blazor.md) — Using `DeepZoomController` with `SKGestureTracker`

## DeepZoomController

The platform-agnostic rendering engine. It manages the viewport, tile scheduling, tile cache, and rendering — but has **no animation and no gesture handling**.

```csharp
using SkiaSharp.Extended.DeepZoom;

var controller = new DeepZoomController(cacheCapacity: 1024);

// Load a DZI image
controller.Load(tileSource, fetcher);

// Called each frame to schedule tile loading (returns true while tiles are loading)
bool hasPendingTiles = controller.Update();

// Called in the paint handler
controller.SetControlSize(width, height);
controller.Render(canvas);

// Programmatic navigation (instant; no animation)
controller.Pan(dx, dy);                              // Pan by pixel delta
controller.ZoomAboutScreenPoint(factor, cx, cy);     // Zoom around a screen point
controller.ZoomAboutLogicalPoint(factor, lx, ly);    // Zoom around logical coords
controller.SetViewport(viewportWidth, originX, originY);  // Set viewport directly
controller.ResetView();                              // Fit to viewport

// Read current viewport state
double vw = controller.Viewport.ViewportWidth;   // 1.0 = full image
double ox = controller.Viewport.ViewportOriginX;
double oy = controller.Viewport.ViewportOriginY;
```

### Events

| Event | Args | Description |
| :---- | :--- | :---------- |
| `ImageOpenSucceeded` | `EventArgs` | Fired after the source is parsed and the controller is ready. |
| `ImageOpenFailed` | `Exception` | Fired when the source fails to load or parse. |
| `ViewportChanged` | `EventArgs` | Fired on every viewport change (pan, zoom, or `SetViewport`). |
| `InvalidateRequired` | `EventArgs` | Fired when a tile finishes loading and the view should repaint. |
| `TileFailed` | `TileFailedEventArgs` | Fired when a tile fails to download. |

## DziTileSource

Parses `.dzi` (Deep Zoom Image) XML descriptors:

```csharp
// From a string with base URI
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

## DzcTileSource

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

## Tile Fetchers

Two built-in fetchers are provided:

```csharp
// HTTP — fetches tiles from a URL
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

## Viewport

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

## TileCache

LRU cache for decoded tile bitmaps. Deferred disposal prevents GPU stalls:

```csharp
// Inspect cache state (via controller)
int count = controller.Cache.Count;
int capacity = controller.Cache.Capacity;
controller.Cache.Clear();
```

## Gestures

Both the MAUI and Blazor integrations use [`SKGestureTracker`](gestures.md) for unified gesture handling. The tracker owns all gesture animation — fling deceleration, double-tap zoom easing, and scroll zoom — via the built-in [animation system](animation.md).

| Gesture | Action |
| :------ | :----- |
| **Pan** (single finger / mouse drag) | Pan the image |
| **Pinch** (two fingers) | Zoom in or out around the pinch center |
| **Double-tap** | Toggle between fit-to-screen and 2× zoom (animated) |
| **Scroll** (mouse wheel) | Zoom in or out at the cursor position (animated) |
| **Fling** | Momentum scrolling after a fast pan (decelerated) |

## Performance Tips

- **Cache capacity**: The default of 1024 tiles is generous for most use cases. Reduce for memory-constrained devices: `new DeepZoomController(cacheCapacity: 256)`.
- **Stop rendering when idle**: Check `controller.IsIdle` and stop your render loop when there are no pending tile loads.
- **Gesture tracker limits**: Configure `MinScale`/`MaxScale` on `SKGestureTrackerOptions` to bound the zoom range.

## Next Steps

- [Deep Zoom for MAUI](deep-zoom-maui.md) — `SKDeepZoomView` control
- [Deep Zoom for Blazor](deep-zoom-blazor.md) — Blazor integration guide
- [Gestures](gestures.md) — SKGestureTracker documentation
- [Animation](animation.md) — Animation utilities used by the gesture system
- [API Reference — DeepZoomController](xref:SkiaSharp.Extended.DeepZoom.DeepZoomController)
- [API Reference — DziTileSource](xref:SkiaSharp.Extended.DeepZoom.DziTileSource)
