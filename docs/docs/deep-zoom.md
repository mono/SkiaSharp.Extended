# Deep Zoom

Explore gigapixel images in your .NET MAUI and Blazor apps using the Deep Zoom Image (DZI) format. The deep zoom system downloads only the tiles visible at the current zoom level, so even multi-gigapixel images load instantly without gestures or animation infrastructure.

## What is Deep Zoom?

[Deep Zoom](https://docs.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/cc645050(v=vs.95)) is a tile-based image format developed by Microsoft. An image is pre-sliced into a pyramid of tiles at multiple resolutions. At any zoom level, only the small set of tiles visible in the viewport is loaded — making it practical to explore images with billions of pixels.

**When to use Deep Zoom:**
- 🗺️ High-resolution maps, satellite imagery, or floor plans
- 🎨 Gigapixel art, museum collection viewers
- 🔬 Medical imaging, microscopy slides
- 📸 Any image too large to load in full at once

## Architecture

The deep zoom system is intentionally minimal. There is **no custom control** and **no gesture system** — you wire the services directly to a plain `SKCanvasView`.

```
SKDeepZoomImageSource / SKDeepZoomCollectionSource
           ↓  (parsed from .dzi / .dzc XML)
    SKDeepZoomController
           ↓  (SetControlSize → Update → Render)
       SKCanvasView
```

| Class | Responsibility |
| :---- | :------------- |
| `SKDeepZoomImageSource` | Parses a `.dzi` descriptor into image metadata (size, tile size, level count). |
| `SKDeepZoomCollectionSource` | Parses a `.dzc` collection descriptor into a list of sub-images. |
| `SKDeepZoomController` | Orchestrates tile scheduling, caching, and rendering. Accepts a canvas size; returns rendered output. |
| `SKDeepZoomRenderer` | Draws visible tiles onto an `SKCanvas`, with LOD fallback blending. |
| `ISKDeepZoomTileFetcher` | Supplies tile bitmaps from any source (HTTP, file system, app package). |
| `SKDeepZoomTileCache` | LRU cache for decoded tile bitmaps. |
| `SKDeepZoomTileScheduler` | Determines which tiles are visible at the current viewport. |
| `SKDeepZoomViewport` | Coordinate math between screen pixels and logical (0–1) image space. |

## Quick Start

### 1. Create a controller

```csharp
using SkiaSharp.Extended.DeepZoom;

var controller = new SKDeepZoomController(cacheCapacity: 512);
```

### 2. Load an image source

```csharp
// DZI (single image)
var xml = await httpClient.GetStringAsync("https://example.com/image.dzi");
var tileSource = SKDeepZoomImageSource.Parse(xml, "https://example.com/image_files/");
controller.Load(tileSource, new SKDeepZoomHttpTileFetcher(httpClient));

// DZC (collection of images)
var collXml = await httpClient.GetStringAsync("https://example.com/collection.dzc");
var collection = SKDeepZoomCollectionSource.Parse(collXml, "https://example.com/");
controller.Load(collection, new SKDeepZoomHttpTileFetcher(httpClient));
```

### 3. Wire the canvas

Call `SetControlSize`, `Update`, and `Render` from your paint handler. The controller fits the image to the canvas automatically (centered, scale-to-fit).

```csharp
void OnPaintSurface(SKPaintSurfaceEventArgs e)
{
    controller.SetControlSize(e.Info.Width, e.Info.Height);
    controller.Update();   // schedules tile loads for the current viewport
    controller.Render(e.Surface.Canvas);
}
```

### 4. Trigger repaints when tiles arrive

```csharp
controller.InvalidateRequired += (_, _) => myCanvasView.InvalidateSurface();
```

### 5. Dispose when done

```csharp
controller.Dispose();
```

## SKDeepZoomController

The platform-agnostic rendering engine. It manages the viewport, tile scheduling, tile cache, and rendering.

```csharp
var controller = new SKDeepZoomController(cacheCapacity: 1024);

// Load
controller.Load(tileSource, fetcher);

// Per-frame (from paint handler)
controller.SetControlSize(width, height);  // call when canvas size changes
controller.Update();                       // returns true while tiles are loading
controller.Render(canvas);

// Optional: programmatic viewport control
controller.ResetView();                                      // fit to canvas
controller.SetViewport(viewportWidth, originX, originY);     // set directly
controller.Pan(deltaScreenX, deltaScreenY);                  // pan by pixels
controller.ZoomAboutScreenPoint(factor, screenX, screenY);  // zoom about screen point
controller.ZoomAboutLogicalPoint(factor, logicalX, logicalY); // zoom about logical coords
```

### Events

| Event | Args | Description |
| :---- | :--- | :---------- |
| `ImageOpenSucceeded` | `EventArgs` | Image source loaded and controller is ready to render. |
| `ImageOpenFailed` | `Exception` | Image source failed to load. |
| `InvalidateRequired` | `EventArgs` | A tile finished loading; trigger a repaint. |
| `TileFailed` | `SKDeepZoomTileFailedEventArgs` | A tile failed to download. |
| `ViewportChanged` | `EventArgs` | Viewport was moved or zoomed programmatically. |

## SKDeepZoomImageSource

Parses `.dzi` (Deep Zoom Image) XML:

```csharp
// From a string with a base URL for tile requests
var tileSource = SKDeepZoomImageSource.Parse(xmlString, "https://example.com/image_files/");

// Key properties
int width        = tileSource.ImageWidth;
int height       = tileSource.ImageHeight;
int tileSize     = tileSource.TileSize;
int overlap      = tileSource.Overlap;
int maxLevel     = tileSource.MaxLevel;
double aspect    = tileSource.AspectRatio;
```

## SKDeepZoomCollectionSource

Parses `.dzc` (Deep Zoom Collection) XML — a mosaic of many DZI images:

```csharp
var collection = SKDeepZoomCollectionSource.Parse(xmlString, baseUri);
controller.Load(collection, fetcher);

// After loading, sub-images are accessible via the controller
foreach (var sub in controller.SubImages)
{
    Console.WriteLine($"#{sub.Id} — aspect ratio {sub.AspectRatio:F2}");
}
```

## Tile Fetchers

Two built-in fetchers are provided:

```csharp
// HTTP — fetches tiles from a URL
var fetcher = new SKDeepZoomHttpTileFetcher(httpClient);

// File system — useful for testing or local files
var fetcher = new SKDeepZoomFileTileFetcher(baseDirectory);
```

Implement `ISKDeepZoomTileFetcher` to fetch from any source:

```csharp
public class AppPackageFetcher : ISKDeepZoomTileFetcher
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
            return null;   // returning null skips the tile silently
        }
    }

    public void Dispose() { }
}
```

## Viewport

The `SKDeepZoomViewport` handles coordinate transforms between screen pixels and the logical (0–1) image space:

```csharp
var vp = controller.Viewport;

// Convert a tap to logical image coordinates
var (lx, ly) = vp.ElementToLogicalPoint(tapX, tapY);

// Current viewport state
double viewportWidth = vp.ViewportWidth;   // 1.0 = full image visible; < 1.0 = zoomed in
double originX       = vp.ViewportOriginX;
double originY       = vp.ViewportOriginY;
double zoom          = vp.Zoom;            // 1.0 / ViewportWidth
```

## Rendering — Fit and Center

The controller automatically fits the image to the canvas on load (`FitToView`). The image is scaled to fit the canvas while preserving the aspect ratio — the full image is always visible, centered both horizontally and vertically.

If you resize the canvas, call `SetControlSize` again and the viewport will refit:

```csharp
// In a resize handler:
controller.SetControlSize(newWidth, newHeight);
controller.ResetView();
myCanvasView.InvalidateSurface();
```

## Performance Tips

- **Cache capacity**: Default 1 024 tiles. Reduce for memory-constrained devices: `new SKDeepZoomController(256)`.
- **Stop rendering when idle**: `controller.IsIdle` is `true` when no tiles are pending. Avoid continuous repaints when nothing is loading.
- **Dispose promptly**: `SKDeepZoomController.Dispose()` cancels all in-flight tile requests and clears the cache.

## Platform Integration

- [Deep Zoom for MAUI](deep-zoom-maui.md) — Using `SKDeepZoomController` with `SKCanvasView` in .NET MAUI
- [Deep Zoom for Blazor](deep-zoom-blazor.md) — Using `SKDeepZoomController` with `SKCanvasView` in Blazor WebAssembly
- [API Reference — SKDeepZoomController](xref:SkiaSharp.Extended.DeepZoom.SKDeepZoomController)
- [API Reference — SKDeepZoomImageSource](xref:SkiaSharp.Extended.DeepZoom.SKDeepZoomImageSource)

