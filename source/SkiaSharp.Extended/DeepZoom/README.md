# SkiaSharp.Extended DeepZoom

A static rendering system for Deep Zoom images (DZI) and collections (DZC). Loads tiled pyramid images at the optimal resolution for the current canvas size, rendering them centered-fit onto any `SKCanvas`.

---

## What is Deep Zoom?

Deep Zoom is a tiled image pyramid protocol originally from Silverlight's `MultiScaleImage`. An image at any resolution is sliced into tiles at multiple zoom levels, so viewers download only the tiles visible at the current zoom — not the full gigapixel image.

Two file formats are supported:

| Format | Description |
|--------|-------------|
| `.dzi` | **Deep Zoom Image** — a single image pyramid |
| `.dzc` | **Deep Zoom Collection** — a composite of many images |

---

## Architecture Overview

```mermaid
flowchart TD
    App["App / Sample Page"]
    Controller["SKDeepZoomController\n(orchestrator)"]
    Viewport["SKDeepZoomViewport\n(geometry)"]
    Scheduler["SKDeepZoomTileScheduler\n(tile selection)"]
    Cache["SKDeepZoomMemoryTileCache\n(LRU bitmap store)"]
    Renderer["SKDeepZoomRenderer\n(draws tiles)"]
    Fetcher["ISKDeepZoomTileFetcher\n(HTTP or file)"]
    TileSource["SKDeepZoomImageSource\n(DZI metadata)"]
    Canvas["SKCanvas"]

    App -->|"Load(tileSource, fetcher)"| Controller
    App -->|"SetControlSize(w, h)"| Controller
    App -->|"Update() + Render(canvas)"| Controller

    Controller --> Viewport
    Controller --> Scheduler
    Controller --> Cache
    Controller --> Renderer
    Controller -->|"async FetchTileAsync(url)"| Fetcher

    Scheduler -->|"GetVisibleTiles()"| TileSource
    Scheduler -->|"queries"| Viewport

    Fetcher -->|"SKBitmap"| Cache
    Cache -->|"cached bitmaps"| Renderer
    Renderer -->|"DrawBitmap()"| Canvas
    Viewport -->|"LogicalToElementPoint()"| Renderer
```

### Typical call sequence (per frame)

```mermaid
sequenceDiagram
    participant App
    participant Controller
    participant Scheduler
    participant Cache
    participant Fetcher
    participant Renderer

    App->>Controller: SetControlSize(w, h)
    App->>Controller: Update()
    Controller->>Scheduler: GetVisibleTiles(tileSource, viewport)
    Scheduler-->>Controller: [TileRequest list]
    loop For each missing tile
        Controller->>Fetcher: FetchTileAsync(url) [async]
        Fetcher-->>Cache: Put(tileId, bitmap)
        Cache-->>App: InvalidateRequired event → repaint
    end
    App->>Controller: Render(canvas)
    Controller->>Renderer: Render(canvas, tileSource, viewport, cache, scheduler)
    Renderer->>Cache: TryGet(tileId)
    Renderer->>Canvas: DrawBitmap(tile, srcRect, destRect)
```

---

## Subsystem Breakdown

### 1 — Source Parsing (DZI / DZC)

Parses the XML descriptor and computes tile URLs, level dimensions, and tile bounds.

```mermaid
classDiagram
    class SKDeepZoomImageSource {
        +int ImageWidth
        +int ImageHeight
        +int TileSize
        +int Overlap
        +int MaxLevel
        +double AspectRatio
        +string Format
        +Parse(xml, baseUrl)$
        +GetTileUrl(level, col, row)
        +GetLevelWidth(level)
        +GetLevelHeight(level)
        +GetTileBounds(level, col, row)
        +GetOptimalLevel(vpWidth, ctrlWidth)
    }

    class SKDeepZoomCollectionSource {
        +int MaxLevel
        +int TileSize
        +IReadOnlyList~SKDeepZoomCollectionSubImage~ Items
        +Parse(xml)$
    }

    class SKDeepZoomCollectionSubImage {
        +int Id
        +int MortonIndex
        +double AspectRatio
        +string? Source
        +double ViewportWidth
        +double ViewportOriginX
        +double ViewportOriginY
    }

    SKDeepZoomCollectionSource "1" --> "*" SKDeepZoomCollectionSubImage
```

### 2 — Tile Infrastructure

The tile pipeline handles fetching, deduplication, caching, and scheduling.

```mermaid
flowchart LR
    subgraph "Fetching"
        F1["SKDeepZoomHttpTileFetcher\n(HTTP URLs)"]
        F2["SKDeepZoomFileTileFetcher\n(local file:// paths)"]
        IF["ISKDeepZoomTileFetcher"]
        F1 -.implements.-> IF
        F2 -.implements.-> IF
    end

    subgraph "Scheduling"
        S["SKDeepZoomTileScheduler"]
        TR["SKDeepZoomTileRequest\n{TileId, Priority}"]
        TI["SKDeepZoomTileId\n{Level, Col, Row}"]
        S --> TR
        TR --> TI
    end

    subgraph "Caching"
        C["SKDeepZoomMemoryTileCache\n(LRU, configurable capacity)"]
    end

    IF --> C
    S --> C
```

**`SKDeepZoomTileId`** — uniquely identifies a tile as `(level, col, row)`.  
**`SKDeepZoomTileRequest`** — adds fallback context: if the exact tile isn't cached, a parent tile at a lower level is used to fill the space while the tile loads.  
**`SKDeepZoomMemoryTileCache`** — thread-safe LRU cache of `SKBitmap` objects. Evicts least-recently-used tiles when capacity is reached.

### 3 — Viewport & Geometry

Translates between screen space and the normalized logical space (`0..1` horizontally) used by DeepZoom.

```mermaid
classDiagram
    class SKDeepZoomViewport {
        +double ControlWidth
        +double ControlHeight
        +double AspectRatio
        +double ViewportWidth
        +double ViewportOriginX
        +double ViewportOriginY
        +double Scale
        +double ViewportHeight
        +FitToView()
        +Constrain()
        +ZoomAboutLogicalPoint(factor, lx, ly)
        +PanByScreenDelta(dx, dy)
        +ElementToLogicalPoint(sx, sy)
        +LogicalToElementPoint(lx, ly)
        +GetLogicalBounds()
        +GetState() SKDeepZoomViewportState
        +SetState(state)
    }

    class SKDeepZoomViewportState {
        +double ViewportWidth
        +double OriginX
        +double OriginY
    }

    class SKDeepZoomDisplayRect {
        +double X, Y, Width, Height
    }

    SKDeepZoomViewport --> SKDeepZoomViewportState : snapshot
```

**Logical space**: `X=0` is the left edge of the image, `X=1` is the right edge. `Y` is scaled proportionally (height = `1/AspectRatio`).

**`FitToView()`** handles any combination of view and image aspect ratios:
- Wide image + tall view → fits width, centers vertically
- Tall image + wide view → fits height, centers horizontally
- `SetControlSize(w, h)` automatically refits when the canvas is resized.

### 4 — Rendering

```mermaid
flowchart TD
    Renderer["SKDeepZoomRenderer"]
    Cache["SKDeepZoomMemoryTileCache"]
    Viewport["SKDeepZoomViewport"]
    TileSource["SKDeepZoomImageSource"]
    Scheduler["SKDeepZoomTileScheduler"]
    Canvas["SKCanvas"]

    Renderer -->|"GetVisibleTiles()"| Scheduler
    Renderer -->|"TryGet(tileId)"| Cache
    Renderer -->|"LogicalToElementPoint()"| Viewport
    Renderer -->|"GetTileBounds()"| TileSource
    Renderer -->|"DrawBitmap(tile, src, dest)"| Canvas

    note1["If tile missing:\nDraw parent tile cropped\nto fill the gap (fallback)"]
    Cache --> note1
```

The renderer always falls back to a lower-level tile while the correct tile is loading, so the display is never blank. Tile borders and a debug overlay can be enabled via `ShowTileBorders`.

---

## Component Relationships (full picture)

```mermaid
classDiagram
    class SKDeepZoomController {
        +SKDeepZoomViewport Viewport
        +SKDeepZoomMemoryTileCache Cache
        +SKDeepZoomTileScheduler Scheduler
        +SKDeepZoomRenderer Renderer
        +SKDeepZoomImageSource? TileSource
        +IReadOnlyList~SKDeepZoomSubImage~ SubImages
        +Load(tileSource, fetcher)
        +Load(collectionSource, fetcher)
        +SetControlSize(width, height)
        +Update() bool
        +Render(canvas)
        +ResetView()
        +ImageOpenSucceeded event
        +ImageOpenFailed event
        +InvalidateRequired event
        +TileFailed event
    }

    SKDeepZoomController --> SKDeepZoomViewport
    SKDeepZoomController --> SKDeepZoomTileCache
    SKDeepZoomController --> SKDeepZoomTileScheduler
    SKDeepZoomController --> SKDeepZoomRenderer
    SKDeepZoomController --> SKDeepZoomImageSource
    SKDeepZoomController --> ISKDeepZoomTileFetcher
```

---

## Quick Start

### Blazor (WebAssembly)

```razor
@page "/viewer"
@using SkiaSharp.Extended.DeepZoom
@implements IDisposable

<SKGLView @ref="_canvas" OnPaintSurface="OnPaint"
          style="width: 100%; height: 600px;" />

@code {
    private SKGLView? _canvas;
    private readonly SKDeepZoomController _controller = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _controller.InvalidateRequired += (_, _) => InvokeAsync(() => _canvas?.Invalidate());

        var xml = await Http.GetStringAsync("https://example.com/image.dzi");
        var src = SKDeepZoomImageSource.Parse(xml, "https://example.com/image_files/");
        _controller.Load(src, new SKDeepZoomHttpTileFetcher(new HttpClient()));
    }

    private void OnPaint(SKPaintGLSurfaceEventArgs e)
    {
        _controller.SetControlSize(e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        _controller.Update();
        _controller.Render(e.Surface.Canvas);
    }

    public void Dispose() => _controller.Dispose();
}
```

### MAUI

```csharp
public partial class MyPage : ContentPage
{
    private readonly SKDeepZoomController _controller = new();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _controller.InvalidateRequired += (_, _) => canvas.InvalidateSurface();

        using var stream = await FileSystem.OpenAppPackageFileAsync("image.dzi");
        var xml = await new StreamReader(stream).ReadToEndAsync();
        var src = SKDeepZoomImageSource.Parse(xml, "image_files/");
        _controller.Load(src, new AppPackageFetcher());
    }

    private void OnPaintSurface(object? s, SKPaintSurfaceEventArgs e)
    {
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _controller.Render(e.Surface.Canvas);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _controller.Dispose();
    }
}
```

---

## File Reference

| File | Purpose |
|------|---------|
| `SKDeepZoomController.cs` | Top-level orchestrator — load, resize, update, render |
| `SKDeepZoomViewport.cs` | Logical↔screen coordinate transforms, centered-fit math |
| `SKDeepZoomViewportState.cs` | Snapshot struct for viewport state |
| `SKDeepZoomRenderer.cs` | Draws tiles onto `SKCanvas` with fallback support |
| `SKDeepZoomTileScheduler.cs` | Selects visible tiles at the optimal pyramid level |
| `SKDeepZoomMemoryTileCache.cs` | Thread-safe LRU cache of decoded tile bitmaps |
| `SKDeepZoomTileId.cs` | Identifies a tile: `(level, col, row)` |
| `SKDeepZoomTileRequest.cs` | Tile + optional fallback parent tile |
| `SKDeepZoomTileFailedEventArgs.cs` | Event args for tile load failures |
| `SKDeepZoomImageSource.cs` | Parses `.dzi` XML; computes tile URLs and level dims |
| `SKDeepZoomCollectionSource.cs` | Parses `.dzc` XML; contains sub-image metadata |
| `SKDeepZoomCollectionSubImage.cs` | Sub-image entry in a DZC collection |
| `SKDeepZoomSubImage.cs` | Runtime sub-image view with viewport positioning |
| `SKDeepZoomDisplayRect.cs` | Logical display rectangle helper |
| `ISKDeepZoomTileFetcher.cs` | Interface for tile fetching |
| `SKDeepZoomHttpTileFetcher.cs` | HTTP tile fetcher (`HttpClient`-based) |
| `SKDeepZoomFileTileFetcher.cs` | Local file tile fetcher |
