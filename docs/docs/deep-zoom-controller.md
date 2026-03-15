# Controller & Viewport

`SKDeepZoomController` is the central orchestrator of the Deep Zoom system. It manages viewport math, tile scheduling, cache population, and rendering. All platform-specific code (canvas, touch, gestures) lives in the caller — the controller knows nothing about UI.

## SKDeepZoomController

### Construction

```csharp
// Default: in-memory LRU cache with 1024 tile capacity
var controller = new SKDeepZoomController();

// Custom capacity
var controller = new SKDeepZoomController(defaultCacheCapacity: 512);

// Custom pluggable cache (see Caching docs)
ISKDeepZoomTileCache myCache = new MyCustomCache();
var controller = new SKDeepZoomController(cache: myCache);
```

### Loading Images

```csharp
// DZI — single image
var source = SKDeepZoomImageSource.Parse(xmlString, "https://example.com/image_files/");
controller.Load(source, new SKDeepZoomHttpTileFetcher());

// DZC — collection of images
var collection = SKDeepZoomCollectionSource.Parse(xmlString);
collection.TilesBaseUri = "https://example.com/";
controller.Load(collection, new SKDeepZoomHttpTileFetcher());
```

`Load()` resets the viewport to show the full image and starts fetching tiles in the background.

### Render Loop

Call these three methods from your canvas paint handler:

```csharp
void OnPaintSurface(SKPaintSurfaceEventArgs e)
{
    // 1. Update control dimensions (call on every paint; no-op when unchanged)
    controller.SetControlSize(e.Info.Width, e.Info.Height);

    // 2. Compute visible tiles and kick off background fetches
    controller.Update();

    // 3. Draw all cached tiles to the canvas
    controller.Render(e.Surface.Canvas);
}
```

> **Performance tip:** `controller.IsIdle` is `true` when no tiles are loading. If you have a continuous render loop, you can pause it when idle.

### Navigation

```csharp
// Pan by screen pixel deltas (from mouse drag or touch gesture)
controller.Pan(deltaScreenX, deltaScreenY);

// Zoom about a screen point (anchor stays fixed on screen)
controller.ZoomAboutScreenPoint(factor: 1.2, screenX, screenY);

// Zoom about a logical image point (0-1 normalized)
controller.ZoomAboutLogicalPoint(factor: 1.5, logicalX: 0.5, logicalY: 0.5);

// Set zoom level directly (1.0 = image fills control width)
controller.SetZoom(zoom: 2.0);

// Set viewport state directly
controller.SetViewport(viewportWidth, originX, originY);

// Fit image to canvas (centered, aspect-ratio preserved)
controller.ResetView();
```

### Properties

| Property | Type | Description |
| :------- | :--- | :---------- |
| `Viewport` | `SKDeepZoomViewport` | The current viewport (position and zoom). |
| `Cache` | `ISKDeepZoomTileCache` | The tile cache. |
| `Scheduler` | `SKDeepZoomTileScheduler` | The tile scheduler. |
| `Renderer` | `SKDeepZoomRenderer` | The tile renderer. |
| `TileSource` | `SKDeepZoomImageSource?` | The loaded source, or `null`. |
| `SubImages` | `IReadOnlyList<SKDeepZoomSubImage>` | Sub-images from a DZC; empty for DZI. |
| `AspectRatio` | `double` | Width/height of the loaded image; 0 if not loaded. |
| `IsIdle` | `bool` | `true` when no tiles are in-flight. |
| `PendingTileCount` | `int` | Number of tile fetches currently in progress. |
| `NativeZoom` | `double` | Zoom level where 1 image pixel = 1 screen pixel. |
| `ShowTileBorders` | `bool` | Debug: draw a border around each tile. |

### Events

| Event | Signature | Description |
| :---- | :-------- | :---------- |
| `ImageOpenSucceeded` | `EventHandler` | Image parsed and ready to render. |
| `ImageOpenFailed` | `EventHandler<Exception>` | Image failed to parse. |
| `InvalidateRequired` | `EventHandler` | A tile loaded — trigger a canvas repaint. |
| `TileFailed` | `EventHandler<SKDeepZoomTileFailedEventArgs>` | A tile download failed. |
| `ViewportChanged` | `EventHandler` | Viewport position or zoom changed. |

### Disposal

```csharp
// Cancels all in-flight tile requests, clears the cache, and releases resources.
controller.Dispose();
```

---

## SKDeepZoomViewport

`SKDeepZoomViewport` handles all coordinate math. You rarely need to access it directly — the controller exposes the most common operations — but it's available via `controller.Viewport` when you need raw position values.

### Coordinate System

The viewport uses a **normalized logical coordinate system** where the full image width = 1.0, regardless of actual pixel dimensions. This makes zoom and pan math resolution-independent.

```
ViewportWidth = 1.0   → full image fits the control width (zoom = 1.0)
ViewportWidth = 0.5   → zoomed in 2x (zoom = 2.0)
ViewportWidth = 0.25  → zoomed in 4x (zoom = 4.0)
```

The `ViewportOriginX` / `ViewportOriginY` are the logical coordinates of the top-left corner of the visible area.

### Key Properties

| Property | Description |
| :------- | :---------- |
| `ViewportWidth` | Logical width of visible area (1.0 = full image). |
| `ViewportOriginX` | Logical X of the top-left corner. |
| `ViewportOriginY` | Logical Y of the top-left corner. |
| `Zoom` | `= 1.0 / ViewportWidth`. 1.0 = fit, 2.0 = 2× zoomed in. |
| `ViewportHeight` | Derived: `ViewportWidth × (controlHeight / controlWidth)`. |
| `ControlWidth` | Screen width in pixels. |
| `ControlHeight` | Screen height in pixels. |
| `Scale` | Pixels per logical unit (`controlWidth / viewportWidth`). |
| `MaxViewportWidth` | Maximum zoom-out level (default: unconstrained). |

### Coordinate Conversion

```csharp
var vp = controller.Viewport;

// Screen → logical image coordinates (e.g., map a tap to the image)
var (lx, ly) = vp.ElementToLogicalPoint(screenX, screenY);

// Logical → screen (e.g., draw an overlay at a known image position)
var (sx, sy) = vp.LogicalToElementPoint(logicalX, logicalY);

// Visible logical rect at current zoom
var (x, y, w, h) = controller.GetZoomRect();
```

### NativeZoom

The **native zoom** is the zoom level at which one image pixel maps to exactly one screen pixel. At this level you see maximum detail with no upscaling:

```csharp
double native = controller.NativeZoom;

// Navigate to native zoom, centred on current view
var (cx, cy) = vp.ElementToLogicalPoint(vp.ControlWidth / 2, vp.ControlHeight / 2);
controller.ZoomAboutLogicalPoint(native / vp.Zoom, cx, cy);
```

---

## SKDeepZoomImageSource

`SKDeepZoomImageSource` describes a single DZI image and constructs tile URLs.

### Parsing

```csharp
// From XML string + base URI for tile requests
var source = SKDeepZoomImageSource.Parse(xmlString, "https://example.com/image_files/");

// From a stream
var source = SKDeepZoomImageSource.Parse(stream, "https://example.com/image_files/");
```

### Properties

| Property | Description |
| :------- | :---------- |
| `ImageWidth` | Full image width in pixels. |
| `ImageHeight` | Full image height in pixels. |
| `TileSize` | Tile edge length in pixels (typically 256 or 512). |
| `Overlap` | Pixel overlap between adjacent tiles (typically 1). |
| `Format` | Tile image format string (`"jpeg"` or `"png"`). |
| `MaxLevel` | Highest zoom level (log₂ of the larger dimension). |
| `AspectRatio` | `ImageWidth / ImageHeight`. |
| `TilesBaseUri` | Base URL for tile requests. |
| `TilesQueryString` | Optional query string appended to every tile URL. |

### Tile URL Construction

```csharp
// Relative tile path (Level/Col_Row.format)
string relUrl  = source.GetTileUrl(level: 12, col: 3, row: 5);

// Full absolute tile URL (includes TilesBaseUri)
string fullUrl = source.GetFullTileUrl(level: 12, col: 3, row: 5);

// Grid dimensions at a given level
int tilesX = source.GetTileCountX(level: 10);
int tilesY = source.GetTileCountY(level: 10);

// Optimal level to render at given viewport
int level = source.GetOptimalLevel(viewportWidth: vp.ViewportWidth, controlWidth: vp.ControlWidth);
```

---

## SKDeepZoomCollectionSource

`SKDeepZoomCollectionSource` describes a DZC collection — many DZI images composited into one tile pyramid using Morton (Z-order) indexing.

### Parsing

```csharp
var collection = SKDeepZoomCollectionSource.Parse(xmlString);
collection.TilesBaseUri = "https://example.com/";
controller.Load(collection, fetcher);
```

### Properties

| Property | Description |
| :------- | :---------- |
| `MaxLevel` | Pyramid level count. |
| `TileSize` | Tile edge length (pixels). |
| `Format` | Tile image format. |
| `ItemCount` | Number of sub-images. |
| `Items` | All sub-images as `SKDeepZoomCollectionSubImage`. |
| `TilesBaseUri` | Base URL for tile requests. |

### Sub-Images After Load

```csharp
controller.Load(collection, fetcher);

foreach (var sub in controller.SubImages)
{
    int id = sub.Id;
    int morton = sub.MortonIndex;
    double aspect = sub.AspectRatio;
    string? source = sub.Source;
}
```

---

## Related

- [Deep Zoom overview](deep-zoom.md)
- [Tile Fetching](deep-zoom-fetching.md)
- [Caching](deep-zoom-caching.md)
- [Blazor Integration](deep-zoom-blazor.md)
- [MAUI Integration](deep-zoom-maui.md)
- [API Reference — SKDeepZoomController](xref:SkiaSharp.Extended.DeepZoom.SKDeepZoomController)
- [API Reference — SKDeepZoomViewport](xref:SkiaSharp.Extended.DeepZoom.SKDeepZoomViewport)
