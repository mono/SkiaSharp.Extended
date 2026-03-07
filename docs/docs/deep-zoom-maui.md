# Deep Zoom for MAUI

`SKDeepZoomView` is a ready-to-use .NET MAUI control that brings together the [deep zoom engine](deep-zoom.md) and [gesture tracker](gestures.md) into a single view. Drop it into your page, point it at a `.dzi` source, and you get pan, pinch, double-tap zoom, scroll zoom, and fling out of the box.

## Quick Start

### XAML

```xml
<ContentPage xmlns:dz="clr-namespace:SkiaSharp.Extended.UI.Maui.DeepZoom;assembly=SkiaSharp.Extended.UI.Maui.DeepZoom">

    <dz:SKDeepZoomView x:Name="deepZoomView"
                       Source="https://example.com/image.dzi" />

</ContentPage>
```

### Code-Behind

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

## How It Works

`SKDeepZoomView` composes two independent layers:

| Layer | Role |
| :---- | :--- |
| **SKGestureTracker** | Detects and animates all gestures — pan, pinch, double-tap zoom, scroll zoom, fling. All animation lives here. |
| **DeepZoomController** | Loads tiles and renders the current viewport. No animation, no gesture awareness. |

The view listens to the tracker's `TransformChanged` event, translates the tracker's scale and pixel offset into the controller's logical viewport coordinates, applies viewport constraints, and syncs the constrained state back to the tracker. This round-trip ensures gestures feel natural while respecting image bounds.

```
Touch events → SKGestureTracker → TransformChanged → viewport sync → DeepZoomController → Render
```

## Properties

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

## Events

| Event | Args | Description |
| :---- | :--- | :---------- |
| `ImageOpenSucceeded` | `EventArgs` | Fired after the DZI source is parsed and the controller is ready. |
| `ImageOpenFailed` | `Exception` | Fired when the source URI fails to load or parse. |
| `ViewportChanged` | `EventArgs` | Fired on every viewport change (pan, zoom, or programmatic navigation). |

## Methods

```csharp
// Zoom to fit the whole image
deepZoomView.ResetView();

// Animated zoom around a logical point (0–1 normalised coordinates)
deepZoomView.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);

// Set the viewport directly (instant, no animation)
deepZoomView.SetViewport(viewportWidth: 0.5, originX: 0.25, originY: 0.1);

// Load a DZI tile source manually
deepZoomView.Load(tileSource, fetcher);

// Load a DZC collection
deepZoomView.Load(dzcTileSource, fetcher);
// Access sub-images via deepZoomView.SubImages

// Convert between screen and logical coordinate spaces
var (lx, ly) = deepZoomView.ElementToLogicalPoint(screenX, screenY);
var (sx, sy) = deepZoomView.LogicalToElementPoint(logicalX, logicalY);
```

## Keyboard Navigation

Call `HandleKeyPress` from your platform key event handler:

```csharp
deepZoomView.HandleKeyPress("ArrowLeft");  // Pan left
deepZoomView.HandleKeyPress("+");          // Zoom in
deepZoomView.HandleKeyPress("Home");       // Reset to full image
```

| Key | Action |
| :-- | :----- |
| `Left` / `ArrowLeft` | Pan left |
| `Right` / `ArrowRight` | Pan right |
| `Up` / `ArrowUp` | Pan up |
| `Down` / `ArrowDown` | Pan down |
| `+` / `=` / `OemPlus` / `Add` | Zoom in (animated) |
| `-` / `_` / `OemMinus` / `Subtract` | Zoom out (animated) |
| `Home` | Reset to full image |

## Customising Gestures

Access the built-in gesture tracker to fine-tune behaviour:

```csharp
var tracker = deepZoomView.GestureTracker;

// Adjust zoom limits (default: 1–32×)
tracker.Options.MinScale = 0.5f;
tracker.Options.MaxScale = 64f;

// Disable fling if you prefer instant-stop panning
tracker.Options.IsFlingEnabled = false;
```

See [Gesture Configuration](gesture-configuration.md) for all available options.

## Advanced: Direct Controller Access

For scenarios that go beyond the standard view — such as drawing overlays on top of the deep zoom image — you can access the controller directly:

```csharp
var controller = deepZoomView.Controller;

// Draw a marker at a logical position
deepZoomView.PaintSurface += (s, e) =>
{
    // The view already renders tiles; draw on top
    var (sx, sy) = controller.Viewport.LogicalToElementPoint(0.5, 0.5);
    e.Surface.Canvas.DrawCircle((float)sx, (float)sy, 10, markerPaint);
};
```

## Next Steps

- [Deep Zoom Core](deep-zoom.md) — Core classes shared by MAUI and Blazor
- [Deep Zoom for Blazor](deep-zoom-blazor.md) — Blazor integration guide
- [Gestures](gestures.md) — SKGestureTracker documentation
- [API Reference — SKDeepZoomView](xref:SkiaSharp.Extended.UI.Maui.DeepZoom.SKDeepZoomView)
- [API Reference — DeepZoomController](xref:SkiaSharp.Extended.DeepZoom.DeepZoomController)
- [MAUI Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo/Demos/DeepZoom)
