# Gesture Events

This page covers all gesture events raised by `SKGestureTracker`, with code examples for each. For the quick-start guide and architecture overview, see [Gestures](gestures.md).

## Tap, Double Tap, Long Press

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

> **Note:** A double tap always raises `TapDetected` for the first tap, then `DoubleTapDetected` for the
> second (the second tap raises only `DoubleTapDetected`, not another `TapDetected`). Taps fire immediately
> on finger-up — there is no delay to disambiguate a possible second tap — so the sequence is
> `TapDetected` → `DoubleTapDetected`. If you need tap and double-tap to be mutually exclusive, handle the
> first tap optimistically and undo it when the `DoubleTapDetected` arrives.

## Pan

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

## Pinch (Scale)

Two finger pinch gesture. The tracker automatically updates its internal scale, clamped to `MinScale`/`MaxScale`.

```csharp
tracker.PinchDetected += (s, e) =>
{
    // e.ScaleDelta — relative scale change (>1 = spread, <1 = pinch)
    // e.FocalPoint — midpoint between the two fingers
    // e.PreviousFocalPoint — previous midpoint
};
```

## Rotate

Two finger rotation. The tracker automatically updates its internal rotation.

```csharp
tracker.RotateDetected += (s, e) =>
{
    // e.RotationDelta — change in degrees
    // e.FocalPoint — center of rotation
};
```

## Fling

Momentum-based animation after a fast pan. The tracker runs a fling animation that decays over time.

```csharp
tracker.FlingDetected += (s, e) =>
{
    // Fling started — e.Velocity.X, e.Velocity.Y in px/s
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

## Drag (App-Level Object Dragging)

The tracker provides a drag lifecycle derived from pan events. Use this to move objects within your canvas (e.g., stickers, nodes in a graph editor).

```csharp
tracker.DragStarted += (s, e) =>
{
    if (HitTest(e.Location) is { } item)
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

## Scroll (Mouse Wheel)

Mouse wheel zoom. Call `ProcessMouseWheel` to feed wheel events.

```csharp
tracker.ScrollDetected += (s, e) =>
{
    // e.Location — mouse position
    // e.Delta.X, e.Delta.Y — scroll amounts
};
```

## Hover

Mouse movement without any buttons pressed. Useful for cursor-based UI feedback.

```csharp
tracker.HoverDetected += (s, e) =>
{
    // e.Location — current mouse position
};
```

## Double Tap Zoom

By default, double-tapping zooms in by `DoubleTapZoomFactor` (2×). Double-tapping again at max scale resets to 1×. The zoom animates smoothly over `ZoomAnimationDuration` milliseconds.

To use double tap for your own logic instead, set `e.Handled = true` in your `DoubleTapDetected` handler, or disable it entirely:

```csharp
tracker.IsDoubleTapZoomEnabled = false;
```

## Lifecycle Events

```csharp
// Fired when the first finger touches down (once per gesture sequence)
tracker.GestureStarted += (s, e) => { /* gesture began */ };

// Fired when all fingers lift
tracker.GestureEnded += (s, e) => { /* gesture ended */ };

// Fired whenever the transform matrix changes (pan, zoom, rotate, fling frame)
tracker.TransformChanged += (s, e) => canvas.Invalidate();
```

## See Also

- [Gestures — Quick Start](gestures.md)
- [Configuration & Customization](gesture-configuration.md)
- [API Reference — SKGestureTracker](xref:SkiaSharp.Extended.SKGestureTracker)
