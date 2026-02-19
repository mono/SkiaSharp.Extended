# Deep Zoom

Deep Zoom is a technology for viewing and navigating ultra-high-resolution images smoothly, originally developed by Microsoft for Silverlight. SkiaSharp.Extended brings this capability to modern .NET with a platform-agnostic core and optional MAUI integration.

## What is Deep Zoom?

Deep Zoom converts large images into multi-resolution tile pyramids. At low zoom, you see a small overview. As you zoom in, higher-resolution tiles are loaded progressively — you can explore a 100-megapixel photo as smoothly as scrolling a web page.

## Quick Start

### Core Library (no MAUI required)

```csharp
using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;

// Parse a DZI file
string dziXml = File.ReadAllText("photo.dzi");
var tileSource = DziTileSource.Parse(dziXml, "https://example.com/photo");

// Create a controller with tile fetcher
using var controller = new DeepZoomController();
controller.SetControlSize(800, 600);
controller.Load(tileSource, new HttpTileFetcher());

// In your render loop:
controller.Update(deltaTime);
controller.Render(canvas);

// Zoom in 2x about center:
controller.ZoomAboutScreenPoint(2.0, 400, 300);
```

### MAUI View

```xml
<deepzoom:SKDeepZoomView x:Name="dzView" UseSprings="True" />
```

```csharp
var tileSource = DziTileSource.Parse(dziXml, baseUrl);
dzView.Load(tileSource, new HttpTileFetcher());
```

## DZI File Format

Deep Zoom Image (`.dzi`) files describe a tiled image pyramid:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Image xmlns="http://schemas.microsoft.com/deepzoom/2008"
       Format="jpg" Overlap="1" TileSize="256">
  <Size Width="7026" Height="9221"/>
</Image>
```

Tiles are stored at: `{name}_files/{level}/{col}_{row}.{format}`

### Tile Pyramid Math

| Property | Formula |
|---|---|
| Max level | `ceil(log2(max(width, height)))` |
| Level width | `ceil(imageWidth / 2^(maxLevel - level))` |
| Tile count X | `ceil(levelWidth / tileSize)` |
| Tile bounds | Includes overlap pixels on shared edges |

Level 0 is a 1×1 pixel thumbnail. The max level is full resolution.

## DZC Collections

Deep Zoom Collections (`.dzc`) composite multiple images into a single tiled mosaic using Morton (Z-order) curve placement:

```xml
<Collection xmlns="http://schemas.microsoft.com/deepzoom/2009"
            MaxLevel="8" TileSize="256" Format="jpg">
  <Items>
    <I Id="0" N="0" Source="items/0.dzi">
      <Size Width="800" Height="600"/>
      <Viewport Width="2.5" X="-0.5" Y="-0.3"/>
    </I>
  </Items>
</Collection>
```

## Coordinate Systems

| Space | Description |
|---|---|
| **Logical** | 0–1 normalized. (0,0) = top-left. (1, 1/aspectRatio) = bottom-right |
| **Screen** | Pixels in the control |
| **Level pixel** | Pixel coords within a pyramid level |
| **Tile** | (level, col, row) triplet |

### Viewport

`ViewportWidth = 1.0` means the full image width fits in the control. Smaller values = zoomed in.

```csharp
// Convert between coordinate spaces
var (lx, ly) = viewport.ElementToLogicalPoint(screenX, screenY);
var (sx, sy) = viewport.LogicalToElementPoint(logicalX, logicalY);
```

## Spring Animation

All viewport transitions use critically-damped spring physics for smooth, natural movement. Disable with `UseSprings = false` for immediate jumps.

## Architecture

```
SkiaSharp.Extended.DeepZoom (core, netstandard2.0 + net9.0)
├── DziTileSource       — DZI XML parser + tile pyramid math
├── DzcTileSource       — DZC XML parser + Morton Z-order math
├── Viewport            — Coordinate system + ZoomAboutLogicalPoint
├── SpringAnimator      — Critically-damped spring physics
├── TileCache           — LRU bitmap cache with IDisposable
├── TileScheduler       — Visible tile computation + fallback
├── DeepZoomRenderer    — SKCanvas tile rendering with LOD blend
├── DeepZoomController  — Orchestrator: viewport + spring + scheduler + cache + renderer
└── DeepZoomSubImage    — DZC item with inverted viewport coords

SkiaSharp.Extended.UI.Maui.DeepZoom (MAUI)
└── SKDeepZoomView      — ContentView with gestures + BindableProperties
```

## ITileFetcher

Implement `ITileFetcher` for custom tile loading:

```csharp
public interface ITileFetcher : IDisposable
{
    Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default);
}
```

Built-in implementations: `HttpTileFetcher` (HTTP), `FileTileFetcher` (local files), `MemoryTileFetcher` (testing).

## Events

| Event | Fired When |
|---|---|
| `ImageOpenSucceeded` | DZI/DZC source loaded successfully |
| `ImageOpenFailed` | Source load failed (args: `Exception`) |
| `MotionFinished` | Spring animation settled — all tiles at rest |
| `TileFailed` | Individual tile download failed (args: `TileFailedEventArgs` with TileId + Exception) |
| `InvalidateRequired` | New tiles loaded — view needs repaint |

## Headless Testing

The core library works without any UI framework:

```csharp
var dzi = DziTileSource.Parse(File.ReadAllText("photo.dzi"), baseUrl);
using var controller = new DeepZoomController();
controller.SetControlSize(800, 600);
controller.Load(dzi, new MemoryTileFetcher());

// Pre-populate cache with test tiles
var scheduler = controller.Scheduler;
var tiles = scheduler.GetVisibleTiles(dzi, controller.Viewport);
foreach (var req in tiles)
    controller.Cache.Put(req.TileId, new SKBitmap(256, 256));

// Render to bitmap
using var surface = SKSurface.Create(new SKImageInfo(800, 600));
controller.Render(surface.Canvas);
```

## Thread Safety

The `TileCache` is fully thread-safe with internal locking. Multiple async tile loads can safely access the cache concurrently. The `DeepZoomController.Dispose()` cancels all pending tile loads.

## Learn More

- [DZI Format Specification](https://learn.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/cc645077) — Microsoft Learn
- [OpenSeadragon](https://openseadragon.github.io/) — Modern open-source DZI viewer
- [Deep Zoom Composer](https://legacyupdate.net/download-center/download/24819/deep-zoom-composer) — Original Microsoft tool for creating DZI files
