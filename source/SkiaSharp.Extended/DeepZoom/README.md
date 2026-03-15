# SkiaSharp.Extended DeepZoom

A static rendering system for Deep Zoom images (DZI) and collections (DZC). Loads tiled pyramid images at the optimal resolution for the current canvas size, rendering them centered-fit onto any `SKCanvas`.

**Full documentation:** [docs/docs/deep-zoom.md](../../../../docs/docs/deep-zoom.md)

---

## Class Reference

### Image Sources

| Class | Purpose |
| :---- | :------ |
| `SKDeepZoomImageSource` | Parses a `.dzi` XML descriptor; constructs tile URLs |
| `SKDeepZoomCollectionSource` | Parses a `.dzc` XML collection; Morton-indexed sub-images |
| `SKDeepZoomSubImage` | One sub-image within a DZC collection |
| `SKDeepZoomDisplayRect` | Sparse-image display region with min/max pyramid levels |

### Controller & Viewport

| Class | Purpose |
| :---- | :------ |
| `SKDeepZoomController` | Main orchestrator: viewport, scheduling, caching, rendering |
| `SKDeepZoomViewport` | Logical coordinate math (normalized 0–1 image space) |
| `SKDeepZoomViewportState` | `readonly record struct` snapshot of viewport position + zoom |

### Rendering & Scheduling

| Class | Purpose |
| :---- | :------ |
| `SKDeepZoomRenderer` | Draws visible tiles onto an `SKCanvas` with LOD fallback |
| `SKDeepZoomTileScheduler` | Determines visible tiles and their fetch priority |
| `SKDeepZoomTileId` | `readonly record struct` identifying a tile: `(Level, Col, Row)` |
| `SKDeepZoomTileRequest` | A tile + priority for the scheduler queue |
| `SKDeepZoomTileFailedEventArgs` | Event args for tile fetch failures |

### Caching

| Class / Interface | Purpose |
| :---------------- | :------ |
| `ISKDeepZoomTileCache` | Pluggable cache interface (sync + async read/write) |
| `SKDeepZoomMemoryTileCache` | Thread-safe LRU in-memory cache of decoded bitmaps |

### Fetching

| Class / Interface | Purpose |
| :---------------- | :------ |
| `ISKDeepZoomTileFetcher` | Pluggable fetcher interface |
| `SKDeepZoomHttpTileFetcher` | HTTP fetcher using `HttpClient` |
| `SKDeepZoomFileTileFetcher` | File system fetcher (plain paths or `file://` URIs) |

---

## Minimal Usage

```csharp
using SkiaSharp.Extended.DeepZoom;

// 1. Controller
var controller = new SKDeepZoomController();
controller.InvalidateRequired += (_, _) => myCanvas.InvalidateSurface();

// 2. Load
var xml    = await httpClient.GetStringAsync("https://example.com/image.dzi");
var source = SKDeepZoomImageSource.Parse(xml, "https://example.com/image_files/");
controller.Load(source, new SKDeepZoomHttpTileFetcher());

// 3. Render loop (from canvas paint handler)
void OnPaintSurface(SKPaintSurfaceEventArgs e)
{
    controller.SetControlSize(e.Info.Width, e.Info.Height);
    controller.Update();
    controller.Render(e.Surface.Canvas);
}

// 4. Dispose
controller.Dispose();
```

---

## Architecture Notes

- **No custom control, no gestures, no animations.** The controller is a pure service; layout and interaction belong in the UI layer.
- **Pluggable cache.** Pass any `ISKDeepZoomTileCache` to `SKDeepZoomController`. Use tiered caches (memory → disk → browser storage) via `TryGetAsync`.
- **Pluggable fetcher.** Pass any `ISKDeepZoomTileFetcher` at load time — HTTP, file, app-package, or custom.
- **LOD fallback blending.** While the correct tile loads, a lower-resolution parent tile is scaled up and rendered as a placeholder. The view is never blank.
- **Centered fit.** `FitToView()` is called automatically on load. The image fills the canvas while preserving aspect ratio — no cropping, no stretching.

---

## Coordinate System

The viewport uses **normalized logical coordinates** where the full image width = 1.0:

```
Viewport.ViewportWidth = 1.0  → entire image fits the control (Zoom = 1.0)
Viewport.ViewportWidth = 0.5  → zoomed in 2× (Zoom = 2.0)
Viewport.Zoom          = 1.0 / ViewportWidth
controller.NativeZoom  = ImageWidth / ControlWidth  (1 px → 1 px)
```
