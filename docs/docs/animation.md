# Animation System

SkiaSharp.Extended ships a small, composable animation toolkit in the `SkiaSharp.Extended` assembly. It powers the built-in gesture animations in `SKGestureTracker` but is fully independent — you can use any part on its own.

## Architecture

| Class | Role |
| :---- | :--- |
| `SKTimerAnimation` | Runs an animation loop at ~60 fps using a platform timer. Cancels itself when done. |
| `SKEasingFunctions` | Static easing functions (e.g., `CubicOut`, `Linear`). |
| `SpringAnimator` | Single-axis spring physics (position + velocity). |
| `ViewportSpring` | Wraps three `SpringAnimator` axes (OriginX, OriginY, Width) for viewport animation. |

All classes live in the `SkiaSharp.Extended` namespace.

## SKTimerAnimation

An internal animation runner used by `SKGestureTracker`. On each tick it fires a callback with the elapsed fraction (0 → 1). When the callback returns `false` the animation stops.

```csharp
// SKTimerAnimation is internal — use it via SKGestureTracker, or via SKEasingFunctions for your own easing.
// To run your own animation, simply use a platform timer + SKEasingFunctions:

private async void AnimateSomething()
{
    var start = DateTimeOffset.UtcNow;
    var duration = TimeSpan.FromMilliseconds(300);

    while (true)
    {
        var t = (DateTimeOffset.UtcNow - start).TotalSeconds / duration.TotalSeconds;
        if (t >= 1.0) { t = 1.0; break; }

        var eased = SKEasingFunctions.CubicOut(t);
        ApplyValue(Lerp(fromValue, toValue, eased));

        await Task.Delay(16); // ~60 fps
    }
    ApplyValue(toValue);
}
```

## SKEasingFunctions

A static class providing common easing curves. All functions take a `double t` in `[0, 1]` and return an eased value in approximately `[0, 1]`.

```csharp
using SkiaSharp.Extended;

// Ease-out cubic: fast start, slows near end (the default for double-tap zoom)
double eased = SKEasingFunctions.CubicOut(t);

// Ease-in-out cubic: slow start, fast middle, slow end
double eased = SKEasingFunctions.CubicInOut(t);

// Linear (no easing)
double eased = SKEasingFunctions.Linear(t);
```

All easing functions are pure static methods — no allocation, no state.

## SpringAnimator

A single-axis spring simulation. Useful for smooth animated transitions where you want physical feel (configurable stiffness and damping).

```csharp
using SkiaSharp.Extended;

var spring = new SpringAnimator(0.0); // initialValue
spring.Stiffness = 100.0;
spring.DampingRatio = 1.0;

// Set target (current position stays where it is, velocity carries through)
spring.Target = 1.0;

// Advance simulation (call ~60 fps)
spring.Update(deltaTime: 0.016); // seconds

// Read current state
double position = spring.Current;
bool settled = spring.IsSettled;
```

**Properties:**
- `Stiffness` — how fast the spring snaps to target (higher = faster). Default `100.0`.
- `DampingRatio` — `1.0` = critically damped (no overshoot), `< 1.0` = bouncy, `> 1.0` = sluggish.
- `Target` — the target value the spring is animating toward.
- `Current` — the current interpolated value.
- `IsSettled` — `true` when position and velocity are both near zero.

## ViewportSpring

A convenience wrapper over three `SpringAnimator` axes — `OriginX`, `OriginY`, and `Width` — representing a deep zoom viewport.

```csharp
using SkiaSharp.Extended;

var spring = new ViewportSpring();

// Move to a new viewport
spring.SetTarget(originX: 0.25, originY: 0.0, width: 0.5);

// In your animation loop (~60 fps)
spring.Update(deltaSeconds);

// Get current state for rendering
var (ox, oy, w) = spring.GetCurrentState();

// Snap instantly to target (during direct touch manipulation)
spring.SnapToTarget();

// Reset to full-image view
spring.Reset(originX: 0, originY: 0, width: 1.0);

// Check if animation is complete
bool done = spring.IsSettled;
```

**Properties:**
- `spring.OriginX` / `spring.OriginY` / `spring.Width` — individual `SpringAnimator` axes for fine-grained access.
- `spring.Stiffness` / `spring.DampingRatio` — applied to all three axes atomically.

## Relationship to SKGestureTracker

`SKGestureTracker` uses `SKTimerAnimation` and `SKEasingFunctions` internally to animate fling deceleration and double-tap zoom. These animations fire `TransformChanged` events on each frame — you don't need to manage a timer yourself.

```
Touch events
    ↓
SKGestureTracker  ← owns SKTimerAnimation + SKEasingFunctions
    ↓ TransformChanged (every frame while animating)
Your code  →  update viewport / deep zoom controller / canvas
    ↓ InvalidateSurface
Canvas repaint
```

`ViewportSpring` is provided for cases where you want viewport animation that doesn't come from gesture events — for example, programmatically navigating to a bookmark, or animating a "fit to screen" button. In `SKDeepZoomView` (MAUI) and the Blazor deep zoom sample, all animation is gesture-tracker-driven; `ViewportSpring` is available as a standalone utility for other scenarios.

## Next Steps

- [Gestures](gestures.md) — SKGestureTracker and how animation integrates with gesture handling
- [Gesture Configuration](gesture-configuration.md) — Fine-tune zoom speed, fling behavior, and easing
- [Deep Zoom](deep-zoom.md) — How animation and gestures compose with tile loading
- [API Reference — SKEasingFunctions](xref:SkiaSharp.Extended.SKEasingFunctions)
- [API Reference — SpringAnimator](xref:SkiaSharp.Extended.SpringAnimator)
- [API Reference — ViewportSpring](xref:SkiaSharp.Extended.ViewportSpring)
