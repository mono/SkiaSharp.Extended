# Configuration & Customization

This page covers how to configure gesture thresholds, enable/disable features, read transform state, and programmatically control `SKGestureTracker`. For the quick-start guide, see [Gestures](gestures.md).

## Options

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
    ZoomAnimationInterval = 16,  // Frame interval for zoom animation in ms (~60fps) (default: 16)

    // Scroll zoom
    ScrollZoomFactor = 0.1f,  // Zoom per scroll unit (default: 0.1)

    // Fling animation
    FlingFriction = 0.08f,    // Velocity decay per frame (default: 0.08)
    FlingMinVelocity = 5f,    // Stop threshold in px/s (default: 5)
    FlingFrameInterval = 16,  // Frame interval in ms (~60fps) (default: 16)
};

var tracker = new SKGestureTracker(options);
```

You can also modify options at runtime:

```csharp
tracker.Options.MinScale = 0.5f;
tracker.Options.MaxScale = 20f;
tracker.Options.DoubleTapZoomFactor = 3f;
```

## Feature Toggles

Enable or disable individual gesture types at runtime. Feature toggles can be set at construction time or modified later:

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

When a gesture is disabled, the tracker suppresses its events. The underlying detector still recognizes the gesture, so you can re-enable it at runtime without losing state.

## Reading Transform State

```csharp
float scale = tracker.Scale;       // Current zoom level
float rotation = tracker.Rotation; // Current rotation in degrees
SKPoint offset = tracker.Offset;   // Current pan offset
SKMatrix matrix = tracker.Matrix;  // Combined transform matrix
```

## Programmatic Transform Control

You can set the transform directly without any touch input:

```csharp
// Reset everything back to identity
tracker.Reset();

// Set all values at once
tracker.SetTransform(scale: 2f, rotation: 45f, offset: new SKPoint(100, 50));

// Set individual components
tracker.SetScale(1.5f);
tracker.SetScale(2f, pivot: new SKPoint(400, 300));  // Scale around a specific point
tracker.SetRotation(0f);
tracker.SetRotation(45f, pivot: new SKPoint(400, 300));  // Rotate around a specific point
tracker.SetOffset(SKPoint.Empty);
```

### Animated Zoom

Use `ZoomTo` to animate to a target scale level with a smooth ease-out curve:

```csharp
// Zoom to 3x at the center of the view
tracker.ZoomTo(targetScale: 3f, pivot: new SKPoint(400, 300));

// Check animation state
bool animating = tracker.IsZoomAnimating;
```

The animation duration and frame interval are controlled by `ZoomAnimationDuration` and `ZoomAnimationInterval` in the options.

## See Also

- [Gestures — Quick Start](gestures.md)
- [Gesture Events](gesture-events.md)
- [API Reference — SKGestureTrackerOptions](xref:SkiaSharp.Extended.SKGestureTrackerOptions)
