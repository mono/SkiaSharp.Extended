# SkiaSharp.Extended.UI.Blazor

This package adds SkiaSharp canvas controls with touch and gesture support to Blazor applications.

## Controls

The controls are designed with a layered architecture:

```
SKGestureSurfaceView
    └── SKTouchCanvasView
            └── SkiaSharp.Views.Blazor.SKCanvasView
```

### `SKCanvasView`

A thin wrapper around `SkiaSharp.Views.Blazor.SKCanvasView` that exposes a consistent surface for painting.

```razor
<SKCanvasView OnPaintSurface="OnPaint" Style="width: 400px; height: 300px;" />

@code {
    void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
        canvas.DrawCircle(200, 150, 100, paint);
    }
}
```

### `SKTouchCanvasView`

Extends the canvas view with touch/pointer event support using the same `SKTouchEventArgs` API as the MAUI version. This enables source sharing between Blazor and MAUI applications.

```razor
<SKTouchCanvasView
    OnPaintSurface="OnPaint"
    Touch="OnTouch"
    Style="width: 400px; height: 300px;" />

@code {
    void OnTouch(SKTouchEventArgs e)
    {
        e.Handled = true;
        if (e.ActionType == SKTouchAction.Pressed)
        {
            // e.Location contains CSS-pixel coordinates
            // e.DeviceType is Touch, Mouse, or Stylus
        }
    }
}
```

Touch events use browser pointer events (PointerEvent API) for cross-device support:
- Mouse (left/right/middle button)
- Touch (finger)
- Stylus/pen

### `SKGestureSurfaceView`

The top-level gesture-aware view. Uses the platform-agnostic `SKGestureEngine` from `SkiaSharp.Extended` to recognize complex gestures.

```razor
<SKGestureSurfaceView
    OnPaintSurface="OnPaint"
    TapDetected="OnTap"
    DoubleTapDetected="OnDoubleTap"
    PanDetected="OnPan"
    PinchDetected="OnPinch"
    RotateDetected="OnRotate"
    FlingDetected="OnFling"
    Flinging="OnFlinging"
    ScrollDetected="OnScroll"
    Style="width: 100%; height: 100%;" />
```

#### Supported Gestures

| Gesture | Event | Description |
|---------|-------|-------------|
| Tap | `TapDetected` | Single finger tap |
| Double Tap | `DoubleTapDetected` | Two taps in quick succession |
| Long Press | `LongPressDetected` | Extended touch (default 500ms) |
| Pan | `PanDetected` | Single finger drag |
| Pinch | `PinchDetected` | Two finger zoom |
| Rotate | `RotateDetected` | Two finger rotation |
| Fling | `FlingDetected` / `Flinging` / `FlingCompleted` | Quick swipe with inertia |
| Hover | `HoverDetected` | Mouse/stylus hover (no contact) |
| Scroll | `ScrollDetected` | Mouse wheel |
| Drag | `DragStarted` / `DragUpdated` / `DragEnded` | Drag operation lifecycle |

## Setup

Add the package to your Blazor project:

```xml
<PackageReference Include="SkiaSharp.Extended.UI.Blazor" Version="*" />
```

For Blazor WASM, also add the native SkiaSharp WASM assets:

```xml
<PackageReference Include="SkiaSharp.NativeAssets.WebAssembly" Version="3.119.2" />
```

## MAUI API Compatibility

The touch types in this library match the MAUI touch API:

| Type | MAUI | Blazor |
|------|------|--------|
| `SKTouchAction` | `SkiaSharp.Views.Maui.SKTouchAction` | `SkiaSharp.Extended.UI.Blazor.SKTouchAction` |
| `SKTouchDeviceType` | `SkiaSharp.Views.Maui.SKTouchDeviceType` | `SkiaSharp.Extended.UI.Blazor.SKTouchDeviceType` |
| `SKTouchEventArgs` | `SkiaSharp.Views.Maui.SKTouchEventArgs` | `SkiaSharp.Extended.UI.Blazor.SKTouchEventArgs` |

This allows touch handling code to be shared between MAUI and Blazor applications with minimal changes.
