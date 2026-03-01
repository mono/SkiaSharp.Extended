# Gestures

Add pan, pinch, rotate, fling, tap, and more to any SkiaSharp canvas — on any platform — with a single, unified API. `SKGestureTracker` handles all the math so you can focus on what your app does with the gestures.

## Quick Start

### 1. Create a tracker and subscribe to events

```csharp
using SkiaSharp.Extended.Gestures;

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

### Tap, Double Tap, Long Press

Single finger gestures detected after the finger lifts (or after a timeout for long press).

```csharp
tracker.TapDetected += (s, e) =>
{
    // e.Location — where the tap occurred
    // e.TapCount — always 1 for single tap
};

tracker.DoubleTapDetected += (s, e) =>
{
    // Two taps within DoubleTapSlop distance and timing
    // By default, also triggers a zoom animation (see Double Tap Zoom below)
    // Set e.Handled = true to prevent the zoom
};

tracker.LongPressDetected += (s, e) =>
{
    // Finger held down without moving for LongPressDuration (default 500ms)
    // e.Location — where the press occurred
    // e.Duration — how long the finger was held
};
```

### Pan

Single finger drag. The tracker automatically updates its internal offset.

```csharp
tracker.PanDetected += (s, e) =>
{
    // e.Location — current position
    // e.PreviousLocation — previous position
    // e.Delta — movement since last event
    // e.Velocity — current velocity in pixels/second
};
```

### Pinch (Scale)

Two finger pinch gesture. The tracker automatically updates its internal scale, clamped to `MinScale`/`MaxScale`.

```csharp
tracker.PinchDetected += (s, e) =>
{
    // e.ScaleDelta — relative scale change (>1 = spread, <1 = pinch)
    // e.FocalPoint — midpoint between the two fingers
    // e.PreviousFocalPoint — previous midpoint
};
```

### Rotate

Two finger rotation. The tracker automatically updates its internal rotation.

```csharp
tracker.RotateDetected += (s, e) =>
{
    // e.RotationDelta — change in degrees
    // e.FocalPoint — center of rotation
};
```

### Fling

Momentum-based animation after a fast pan. The tracker runs a fling animation that decays over time.

```csharp
tracker.FlingDetected += (s, e) =>
{
    // Fling started — e.VelocityX, e.VelocityY in px/s
};

tracker.FlingUpdated += (s, e) =>
{
    // Called each frame during fling animation
};

tracker.FlingCompleted += (s, e) =>
{
    // Fling animation finished
};
```

### Drag (App-Level Object Dragging)

The tracker provides a drag lifecycle derived from pan events. Use this to move objects within your canvas (e.g., stickers, nodes).

```csharp
tracker.DragStarted += (s, e) =>
{
    if (HitTest(e.StartLocation) is { } item)
    {
        selectedItem = item;
        e.Handled = true; // Prevents pan from updating the transform
    }
};

tracker.DragUpdated += (s, e) =>
{
    if (selectedItem != null)
    {
        // Convert screen delta to content coordinates
        var inverse = tracker.Matrix; inverse.TryInvert(out inverse);
        var contentDelta = inverse.MapVector(e.Delta.X, e.Delta.Y);
        selectedItem.Position += contentDelta;
        e.Handled = true;
    }
};

tracker.DragEnded += (s, e) =>
{
    selectedItem = null;
};
```

When `DragStarted` or `DragUpdated` sets `Handled = true`, the tracker skips its normal pan offset update **and** suppresses fling after release.

### Scroll (Mouse Wheel)

Mouse wheel zoom. Call `ProcessMouseWheel` to feed wheel events.

```csharp
tracker.ScrollDetected += (s, e) =>
{
    // e.Location — mouse position
    // e.DeltaX, e.DeltaY — scroll amounts
};
```

### Hover

Mouse movement without any buttons pressed. Useful for cursor-based UI feedback.

```csharp
tracker.HoverDetected += (s, e) =>
{
    // e.Location — current mouse position
};
```

## Customization

### Options

Configure thresholds and behavior through `SKGestureTrackerOptions`:

```csharp
var options = new SKGestureTrackerOptions
{
    // Detection thresholds (inherited from SKGestureDetectorOptions)
    TouchSlop = 8f,           // Pixels to move before pan starts (default: 8)
    DoubleTapSlop = 40f,      // Max distance between double-tap taps (default: 40)
    FlingThreshold = 200f,    // Min velocity (px/s) to trigger fling (default: 200)
    LongPressDuration = 500,  // Milliseconds to hold for long press (default: 500)

    // Scale limits
    MinScale = 0.1f,          // Minimum zoom level (default: 0.1)
    MaxScale = 10f,           // Maximum zoom level (default: 10)

    // Double-tap zoom
    DoubleTapZoomFactor = 2f, // Scale multiplier on double tap (default: 2)
    ZoomAnimationDuration = 250, // Animation duration in ms (default: 250)

    // Scroll zoom
    ScrollZoomFactor = 0.1f,  // Zoom per scroll unit (default: 0.1)

    // Fling animation
    FlingFriction = 0.08f,    // Velocity decay per frame (default: 0.08)
    FlingMinVelocity = 5f,    // Stop threshold in px/s (default: 5)
    FlingFrameInterval = 16,  // Frame interval in ms (~60fps) (default: 16)
};

var tracker = new SKGestureTracker(options);
```

Options can also be modified at runtime through the tracker's Options property:

```csharp
tracker.Options.MinScale = 0.5f;
tracker.Options.MaxScale = 20f;
tracker.Options.DoubleTapZoomFactor = 3f;
```

### Feature Toggles

Enable or disable individual gesture types at runtime. Feature toggles live on the Options and can be set at construction time or modified later:

```csharp
// Configure at construction time via options
var options = new SKGestureTrackerOptions
{
    IsTapEnabled = true,
    IsPanEnabled = true,
    IsPinchEnabled = false,
    IsRotateEnabled = false,
};
var tracker = new SKGestureTracker(options);

// Or toggle at runtime
tracker.IsTapEnabled = false;
tracker.IsDoubleTapEnabled = false;
tracker.IsLongPressEnabled = false;
tracker.IsPanEnabled = false;
tracker.IsPinchEnabled = false;
tracker.IsRotateEnabled = false;
tracker.IsFlingEnabled = false;
tracker.IsDoubleTapZoomEnabled = false;
tracker.IsScrollZoomEnabled = false;
tracker.IsHoverEnabled = false;
```

When a gesture is disabled, the tracker suppresses its events. The underlying detector still recognizes the gesture (enabling toggling at runtime without losing state).

### Reading Transform State

```csharp
float scale = tracker.Scale;       // Current zoom level
float rotation = tracker.Rotation; // Current rotation in degrees
SKPoint offset = tracker.Offset;   // Current pan offset
SKMatrix matrix = tracker.Matrix;  // Combined transform matrix

// Reset everything back to identity
tracker.Reset();

// Programmatically set the transform
tracker.SetTransform(scale: 2f, rotation: 45f, offset: new SKPoint(100, 50));
tracker.SetScale(1.5f);
tracker.SetRotation(0f);
tracker.SetOffset(SKPoint.Empty);
```

### Lifecycle Events

```csharp
// Fired when any finger touches down
tracker.GestureStarted += (s, e) => { /* gesture began */ };

// Fired when all fingers lift
tracker.GestureEnded += (s, e) => { /* gesture ended */ };

// Fired whenever the transform matrix changes (pan, zoom, rotate, fling frame)
tracker.TransformChanged += (s, e) => canvas.Invalidate();
```

## Double Tap Zoom

By default, double-tapping zooms in by `DoubleTapZoomFactor` (2x). Double-tapping again at max scale resets to 1x. The zoom animates smoothly over `ZoomAnimationDuration` milliseconds.

To use double tap for your own logic instead, set `e.Handled = true` in your `DoubleTapDetected` handler, or disable it entirely:

```csharp
tracker.IsDoubleTapZoomEnabled = false;
```

## Learn More

- [API Reference — SKGestureTracker](xref:SkiaSharp.Extended.Gestures.SKGestureTracker) — Full property and event documentation
- [API Reference — SKGestureDetector](xref:SkiaSharp.Extended.Gestures.SKGestureDetector) — Low-level gesture detection
- [MAUI Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo/Demos/Gestures) — Full MAUI demo with stickers
- [Blazor Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo.Blazor/Pages/Gestures.razor) — Full Blazor demo
