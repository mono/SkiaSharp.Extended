# Animation System

SkiaSharp.Extended ships a small, composable animation toolkit in the `SkiaSharp.Extended` assembly. It powers the built-in gesture animations in `SKGestureTracker` but is fully independent — you can use any part on its own.

## Architecture

| Class | Role |
| :---- | :--- |
| `SKAnimationTimer` | Runs an animation loop at ~60 fps using a platform timer. Cancels itself when done. |
| `SKAnimationEasing` | Static easing functions (e.g., `CubicOut`, `Linear`). |
| `SKAnimationSpring` | Single-axis spring physics (position + velocity). |

All classes live in the `SkiaSharp.Extended` namespace.

## SKAnimationTimer

An internal animation runner used by `SKGestureTracker`. On each tick it fires a callback with the elapsed fraction (0 → 1). When the callback returns `false` the animation stops.

```csharp
// SKAnimationTimer is internal — use it via SKGestureTracker, or via SKAnimationEasing for your own easing.
// To run your own animation, simply use a platform timer + SKAnimationEasing:

private async void AnimateSomething()
{
    var start = DateTimeOffset.UtcNow;
    var duration = TimeSpan.FromMilliseconds(300);

    while (true)
    {
        var t = (DateTimeOffset.UtcNow - start).TotalSeconds / duration.TotalSeconds;
        if (t >= 1.0) { t = 1.0; break; }

        var eased = SKAnimationEasing.CubicOut(t);
        ApplyValue(Lerp(fromValue, toValue, eased));

        await Task.Delay(16); // ~60 fps
    }
    ApplyValue(toValue);
}
```

## SKAnimationEasing

A static class providing common easing curves. All functions take a `double t` in `[0, 1]` and return an eased value in approximately `[0, 1]`.

```csharp
using SkiaSharp.Extended;

// Ease-out cubic: fast start, slows near end (the default for double-tap zoom)
double eased = SKAnimationEasing.CubicOut(t);

// Ease-in-out cubic: slow start, fast middle, slow end
double eased = SKAnimationEasing.CubicInOut(t);

// Linear (no easing)
double eased = SKAnimationEasing.Linear(t);
```

All easing functions are pure static methods — no allocation, no state.

## SKAnimationSpring

A single-axis spring simulation. Useful for smooth animated transitions where you want physical feel (configurable stiffness and damping).

```csharp
using SkiaSharp.Extended;

var spring = new SKAnimationSpring(0.0); // initialValue
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

## Relationship to SKGestureTracker

`SKGestureTracker` uses `SKAnimationTimer` and `SKAnimationEasing` internally to animate fling deceleration and double-tap zoom. These animations fire `TransformChanged` events on each frame — you don't need to manage a timer yourself.

```
Touch events
    ↓
SKGestureTracker  ← owns SKAnimationTimer + SKAnimationEasing
    ↓ TransformChanged (every frame while animating)
Your code  →  update viewport / deep zoom controller / canvas
    ↓ InvalidateSurface
Canvas repaint
```

## Next Steps

- [Gestures](gestures.md) — SKGestureTracker and how animation integrates with gesture handling
- [Gesture Configuration](gesture-configuration.md) — Fine-tune zoom speed, fling behavior, and easing
- [Deep Zoom](deep-zoom.md) — How animation and gestures compose with tile loading
- [API Reference — SKAnimationEasing](xref:SkiaSharp.Extended.SKAnimationEasing)
- [API Reference — SKAnimationSpring](xref:SkiaSharp.Extended.SKAnimationSpring)
