# Gestures

Add pan, pinch, rotate, fling, tap, and more to any SkiaSharp canvas — on any platform — with a single, unified API. `SKGestureTracker` handles all the math so you can focus on what your app does with the gestures.

## Quick Start

### 1. Create a tracker and subscribe to events

```csharp
using SkiaSharp.Extended;

var tracker = new SKGestureTracker();

// Transform events — the tracker manages the matrix for you
tracker.TransformChanged += (s, e) => canvas.Invalidate();

// Discrete gesture events
tracker.TapDetected += (s, e) => Console.WriteLine($"Tap at {e.Location}");
tracker.DoubleTapDetected += (s, e) => Console.WriteLine("Double tap!");
tracker.LongPressDetected += (s, e) => Console.WriteLine("Long press!");
```

### 2. Feed touch events from your platform

**MAUI** — forward `SKTouchEventArgs`:

```csharp
private void OnTouch(object? sender, SKTouchEventArgs e)
{
    e.Handled = e.ActionType switch
    {
        SKTouchAction.Pressed => tracker.ProcessTouchDown(e.Id, e.Location, e.DeviceType == SKTouchDeviceType.Mouse),
        SKTouchAction.Moved => tracker.ProcessTouchMove(e.Id, e.Location, e.InContact),
        SKTouchAction.Released => tracker.ProcessTouchUp(e.Id, e.Location, e.DeviceType == SKTouchDeviceType.Mouse),
        SKTouchAction.Cancelled => tracker.ProcessTouchCancel(e.Id),
        SKTouchAction.WheelChanged => tracker.ProcessMouseWheel(e.Location, 0, e.WheelDelta),
        _ => true,
    };

    if (e.Handled)
        canvasView.InvalidateSurface();
}
```

**Blazor** — forward `PointerEventArgs`:

```csharp
private void OnPointerDown(PointerEventArgs e)
{
    var location = new SKPoint((float)e.OffsetX * displayScale, (float)e.OffsetY * displayScale);
    tracker.ProcessTouchDown(e.PointerId, location, e.PointerType == "mouse");
}
```

### 3. Apply the transform when drawing

```csharp
private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);

    // Apply the tracked transform (pan + zoom + rotation)
    canvas.Save();
    canvas.Concat(tracker.Matrix);

    // Draw your content here — it will pan, zoom, and rotate
    DrawContent(canvas);

    canvas.Restore();
}
```

## How It Works

The gesture system has two layers:

| Layer | Class | Role |
| :---- | :---- | :--- |
| **Detection** | `SKGestureDetector` | Recognizes raw touch sequences as gestures (tap, pan, pinch, etc.) |
| **Tracking** | `SKGestureTracker` | Manages a transform matrix (pan/zoom/rotate), fling animation, and drag lifecycle |

Most apps only need `SKGestureTracker`. It wraps a detector internally and translates gesture events into transform updates. If you need raw gesture detection without transform management, you can use `SKGestureDetector` directly.

### Coordinate spaces

The tracker is coordinate-space-agnostic — it operates on whatever numbers you pass in. The important rule is: **touch input and canvas drawing must use the same coordinate space.**

- **MAUI**: `SKTouchEventArgs.Location` is already in device pixels (same as the canvas), so pass them through directly.
- **Blazor**: `PointerEventArgs.OffsetX/Y` are in CSS pixels, but the canvas renders in device pixels. Multiply by `devicePixelRatio` to match.

## Supported Gestures

| Gesture | Trigger | Key Event Args |
| :------ | :------ | :------------- |
| **Tap** | Single finger tap and release | `Location`, `TapCount` |
| **Double Tap** | Two taps in quick succession | `Location`, `TapCount` |
| **Long Press** | Finger held still for 500ms+ | `Location`, `Duration` |
| **Pan** | Single finger drag | `Delta`, `Velocity` |
| **Pinch** | Two finger spread/pinch | `ScaleDelta`, `FocalPoint` |
| **Rotate** | Two finger rotation | `RotationDelta`, `FocalPoint` |
| **Fling** | Fast pan with momentum | `VelocityX`, `VelocityY` |
| **Drag** | App-level object dragging | `StartLocation`, `Delta` |
| **Scroll** | Mouse wheel | `DeltaX`, `DeltaY` |
| **Hover** | Mouse move (no buttons) | `Location` |

For detailed code examples and event handler patterns for each gesture, see [Gesture Events](gesture-events.md).

## Next Steps

- **[Gesture Events](gesture-events.md)** — Detailed reference for every gesture event with code examples
- **[Configuration](gesture-configuration.md)** — Options, feature toggles, transform state, and programmatic control
- [API Reference — SKGestureTracker](xref:SkiaSharp.Extended.SKGestureTracker) — Full property and event documentation
- [API Reference — SKGestureDetector](xref:SkiaSharp.Extended.SKGestureDetector) — Low-level gesture detection
- [MAUI Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo/Demos/Gestures) — Full MAUI demo with stickers
- [Blazor Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo.Blazor/Pages/Gestures.razor) — Full Blazor demo
