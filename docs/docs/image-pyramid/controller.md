# Controller & Viewport

`SKImagePyramidController` is the central orchestrator of the Image Pyramid system. It manages viewport math, tile scheduling, cache population, and rendering. All platform-specific code (canvas, touch, gestures) lives in the caller — the controller knows nothing about UI.

## SKImagePyramidController

### Construction

```csharp
// Parameterless constructor — internal render buffer is managed automatically
var controller = new SKImagePyramidController();
```

### Loading Images

```csharp
// DZI — single image (HTTP)
var source = SKImagePyramidDziSource.Parse(xmlString, "https://example.com/image_files/");
controller.Load(source, new SKImagePyramidHttpTileProvider());

// DZC — collection of images
var collection = SKImagePyramidDziCollectionSource.Parse(xmlString);
collection.TilesBaseUri = "https://example.com/";
controller.Load(collection, new SKImagePyramidHttpTileProvider());

// DZI with disk cache (persists across app restarts)
controller.Load(source, new SKImagePyramidHttpTileProvider(
    diskCachePath: Path.Combine(FileSystem.CacheDirectory, "tiles"),
    expiry: TimeSpan.FromDays(30)));

// Local files
controller.Load(source, new SKImagePyramidFileTileProvider());
```

`Load()` resets the viewport to show the full image and starts fetching tiles in the background.

### Render Loop

Call these methods from your canvas paint handler:

```csharp
private readonly SKImagePyramidRenderer _renderer = new();

void OnPaintSurface(SKPaintSurfaceEventArgs e)
{
    // 1. Update control dimensions (call on every paint; no-op when unchanged)
    controller.SetControlSize(e.Info.Width, e.Info.Height);

    // 2. Compute visible tiles and kick off background fetches
    controller.Update();

    // 3. Draw all cached tiles to the canvas
    _renderer.Canvas = e.Surface.Canvas;
    controller.Render(_renderer);
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
| `Viewport` | `SKImagePyramidViewport` | The current viewport (position and zoom). |
| `Cache` | `SKImagePyramidMemoryTileCache` | The internal render buffer (in-memory LRU). |
| `TileLayout` | `SKImagePyramidTileLayout` | The tile geometry and visibility calculator. |
| `TileSource` | `ISKImagePyramidSource?` | The loaded source, or `null`. |
| `SubImages` | `IReadOnlyList<SKImagePyramidSubImage>` | Sub-images from a DZC; empty for DZI. |
| `AspectRatio` | `double` | Width/height of the loaded image; 0 if not loaded. |
| `IsIdle` | `bool` | `true` when no tiles are in-flight. |
| `PendingTileCount` | `int` | Number of tile fetches currently in progress. |
| `NativeZoom` | `double` | Zoom level where 1 image pixel = 1 screen pixel. |
| `EnableLodBlending` | `bool` | Enable LOD blending (blurry placeholders while tiles load). Default `true`. |

### LOD Blending

The controller has an `EnableLodBlending` property that controls whether lower-resolution tiles are used as blurry placeholders while high-resolution tiles stream in:

```csharp
controller.EnableLodBlending = false; // show blank areas instead of blurry placeholders
```

#### How two-pass rendering works

`SKImagePyramidRenderer` uses a two-pass strategy each frame:

1. **Pass 1 — fallback tiles** *(only when `EnableLodBlending = true`)*: For each visible tile that is not yet cached, the renderer walks up the tile pyramid to find the nearest available parent tile and draws a magnified crop of it as a placeholder.
2. **Pass 2 — exact tiles**: For each visible tile that is cached at the correct resolution, it is drawn on top. Missing tiles leave any placeholder from Pass 1 visible, or show blank if blending is off.

#### `EnableLodBlending` — when to use each mode

**`EnableLodBlending = true` (default):** The view always shows *something* — lower-resolution tiles are stretched up as blurry placeholders while high-res tiles stream in. As tiles arrive, they replace the placeholders and the image progressively sharpens.

> **Best for:** interactive image exploration, presentations, consumer apps — any situation where a continuously filled viewport is more comfortable than flickering blanks.

**`EnableLodBlending = false`:** Pass 1 is skipped entirely. Only fully loaded tiles at the exact requested zoom level are drawn; everything else is transparent/white until the tile arrives, then pops in.

> **Best for:**
> - **Scientific or medical imaging** — blurry placeholders could be confused with real image data; showing "not yet loaded" (blank) is more honest.
> - **Pixel-accurate rendering** — when the consumer must see only confirmed, full-resolution data.
> - **Load-performance testing** — blank tiles make it immediately obvious which tiles are still in-flight.
> - **Bandwidth-constrained apps** — some UX designs prefer showing nothing over showing misleadingly blurry content.

| Value | Pass 1 (fallbacks) | Pass 2 (exact tiles) | Missing tile appearance |
| :---- | :----------------- | :------------------- | :---------------------- |
| `true` (default) | Draws scaled parent tiles | Draws loaded tiles on top | Blurry placeholder from lower level |
| `false` | Skipped | Draws loaded tiles only | Blank (white) |

```csharp
// Disable LOD blending on the controller
controller.EnableLodBlending = false;
```

> **Tip:** To see the difference interactively, load an image, add a simulated tile delay (e.g. 1–2 seconds), zoom in so new tiles must load, then toggle `EnableLodBlending`. With blending on you get a smooth blurry → sharp transition; with blending off you see white rectangles pop in.

**Decorator pattern for debug overlays:**

Instead of building debug features into the core renderer, wrap it with a decorator:

```csharp
public sealed class TileBorderRenderer : ISKImagePyramidRenderer
{
    private readonly SKImagePyramidRenderer _inner;
    private readonly SKPaint _borderPaint = new() { IsStroke = true, StrokeWidth = 1, Color = SKColors.Red.WithAlpha(180) };
    public bool ShowBorders { get; set; }

    public TileBorderRenderer(SKImagePyramidRenderer inner) => _inner = inner;

    // Expose Canvas so the caller sets it once and both renderers see it
    public SKCanvas? Canvas
    {
        get => _inner.Canvas;
        set => _inner.Canvas = value;
    }

    public void BeginRender() => _inner.BeginRender();
    public void EndRender()   => _inner.EndRender();

    public void DrawTile(SKRect destRect, SKImagePyramidTile tile)
    {
        _inner.DrawTile(destRect, tile);
        if (ShowBorders && _inner.Canvas != null)
            _inner.Canvas.DrawRect(destRect, _borderPaint);
    }

    public void DrawFallbackTile(SKRect destRect, SKRect sourceRect, SKImagePyramidTile tile)
        => _inner.DrawFallbackTile(destRect, sourceRect, tile);

    public void Dispose()
    {
        _borderPaint.Dispose();
        _inner.Dispose();
    }
}

// Wire it up — set Canvas once, the decorator forwards it to the inner renderer:
var coreRenderer = new SKImagePyramidRenderer();
var debugRenderer = new TileBorderRenderer(coreRenderer) { ShowBorders = true };

// Each frame:
debugRenderer.Canvas = e.Surface.Canvas;
controller.Render(debugRenderer);
```

### Events

| Event | Signature | Description |
| :---- | :-------- | :---------- |
| `ImageOpenSucceeded` | `EventHandler` | DZI parsed and ready to render. |
| `CollectionOpenSucceeded` | `EventHandler` | DZC collection parsed; `SubImages` is populated. |
| `ImageOpenFailed` | `EventHandler<Exception>` | Image failed to parse. |
| `InvalidateRequired` | `EventHandler` | A tile loaded — trigger a canvas repaint. |
| `TileFailed` | `EventHandler<SKImagePyramidTileFailedEventArgs>` | A tile download failed. |
| `ViewportChanged` | `EventHandler` | Viewport position or zoom changed. |

### Disposal

```csharp
// Cancels all in-flight tile requests, clears the cache, and releases resources.
controller.Dispose();
```

---

## SKImagePyramidViewport

`SKImagePyramidViewport` handles all coordinate math. You rarely need to access it directly — the controller exposes the most common operations — but it's available via `controller.Viewport` when you need raw position values.

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

## SKImagePyramidViewportState

An immutable `readonly record struct` snapshot of the current viewport position and zoom. Useful for saving/restoring viewport state (e.g., for undo/redo, navigation history, or persisting the last position across sessions).

```csharp
// Capture current state
var state = new SKImagePyramidViewportState(
    controller.Viewport.ViewportWidth,
    controller.Viewport.ViewportOriginX,
    controller.Viewport.ViewportOriginY);

// Store it (e.g., in a stack for undo)
_history.Push(state);

// Restore it later
controller.SetViewport(state.ViewportWidth, state.OriginX, state.OriginY);
canvas.InvalidateSurface();
```

| Property | Type | Description |
| :------- | :--- | :---------- |
| `ViewportWidth` | `double` | Logical width of the visible area (1.0 = full image). |
| `OriginX` | `double` | Logical X coordinate of the top-left corner. |
| `OriginY` | `double` | Logical Y coordinate of the top-left corner. |

Because it's a `record struct`, it has value equality, works as a dictionary key, and supports `with` expressions:

```csharp
var zoomed = state with { ViewportWidth = state.ViewportWidth * 0.5 };
```

---

## SKImagePyramidDziSource

`SKImagePyramidDziSource` describes a single DZI image and constructs tile URLs.

### Parsing

```csharp
// From XML string + base URI for tile requests
var source = SKImagePyramidDziSource.Parse(xmlString, "https://example.com/image_files/");

// From a stream
var source = SKImagePyramidDziSource.Parse(stream, "https://example.com/image_files/");
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

## SKImagePyramidDziCollectionSource

`SKImagePyramidDziCollectionSource` describes a DZC collection — many DZI images composited into one tile pyramid using Morton (Z-order) indexing.

### Parsing

```csharp
var collection = SKImagePyramidDziCollectionSource.Parse(xmlString);
collection.TilesBaseUri = "https://example.com/";
controller.Load(collection, provider);
```

### Properties

| Property | Description |
| :------- | :---------- |
| `MaxLevel` | Pyramid level count. |
| `TileSize` | Tile edge length (pixels). |
| `Format` | Tile image format. |
| `ItemCount` | Number of sub-images. |
| `Items` | All sub-images as `SKImagePyramidDziCollectionSubImage`. |
| `TilesBaseUri` | Base URL for tile requests. |

### Sub-Images After Load

```csharp
controller.Load(collection, provider);

foreach (var sub in controller.SubImages)
{
    int id = sub.Id;
    int morton = sub.MortonIndex;
    double aspect = sub.AspectRatio;
    string? source = sub.Source;
}
```

---

## SKImagePyramidDziCollectionSubImage

Represents a single image within a DZC collection, including its position in the Morton-indexed mosaic grid.

| Property | Type | Description |
| :------- | :--- | :---------- |
| `Id` | `int` | Item ID from the DZC XML. |
| `MortonIndex` | `int` | Z-order (Morton) index in the mosaic grid. |
| `Width` | `int` | Full image width in pixels. |
| `Height` | `int` | Full image height in pixels. |
| `AspectRatio` | `double` | `Width / Height`. |
| `Source` | `string?` | Optional path to the individual `.dzi` file. |
| `ViewportWidth` | `double` | Position in the DZC mosaic coordinate system. |
| `ViewportX` | `double` | X origin in the DZC mosaic coordinate system. |
| `ViewportY` | `double` | Y origin in the DZC mosaic coordinate system. |

---

## SKImagePyramidDisplayRect

Used in sparse Image Pyramid Images to describe a region of available pixels and the pyramid level range at which it appears. Typically set on `SKImagePyramidDziSource.DisplayRects` when parsing a sparse DZI.

| Property | Type | Description |
| :------- | :--- | :---------- |
| `X` | `int` | X coordinate in full-image pixels. |
| `Y` | `int` | Y coordinate in full-image pixels. |
| `Width` | `int` | Width in full-image pixels. |
| `Height` | `int` | Height in full-image pixels. |
| `MinLevel` | `int` | Lowest pyramid level where this rect is visible. |
| `MaxLevel` | `int` | Highest pyramid level where this rect is visible. |

```csharp
bool visible = rect.IsVisibleAtLevel(level: 12);  // true if MinLevel ≤ 12 ≤ MaxLevel
```

---

## SKImagePyramidTileRequest

Returned by `SKImagePyramidTileLayout.GetVisibleTiles()` — represents a single tile that should be fetched, with a priority that controls fetch order.

| Property | Type | Description |
| :------- | :--- | :---------- |
| `TileId` | `SKImagePyramidTileId` | The tile to fetch (`Level`, `Col`, `Row`). |
| `Priority` | `double` | Fetch order: lower value = fetched first (higher visual importance). |

Equality is based on `TileId` only, so a `SKImagePyramidTileRequest` can be de-duplicated by tile regardless of priority:

```csharp
// Inspect the tile layout's current view
var tiles = controller.TileLayout.GetVisibleTiles(controller.TileSource, controller.Viewport);
foreach (var req in tiles)
    Console.WriteLine($"{req.TileId} priority={req.Priority:F2}");
```

---

## Related

- [Image Pyramid overview](index.md)
- [Tile Fetching](fetching.md)
- [Caching](caching.md)
- [Blazor Integration](blazor.md)
- [MAUI Integration](maui.md)
- [API Reference — SKImagePyramidController](xref:SkiaSharp.Extended.SKImagePyramidController)
- [API Reference — SKImagePyramidViewport](xref:SkiaSharp.Extended.SKImagePyramidViewport)
