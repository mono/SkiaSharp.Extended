# Confetti Effects

Add celebratory confetti explosions to your .NET MAUI apps. `SKConfettiView` provides a full-featured particle system with customizable shapes, colors, physics, and emission patterns.

| Top Stream | Center Burst | Side Spray |
| :--------: | :----------: | :--------: |
| ![][top-stream] | ![][center-burst] | ![][sides-spray] |

## Quick Start

### Add to your XAML

```xml
<ContentPage xmlns:skia="clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI.Maui">
    
    <skia:SKConfettiView x:Name="confettiView" />
    
</ContentPage>
```

### Trigger a confetti burst

```csharp
confettiView.Systems.Add(new SKConfettiSystem
{
    Emitter = SKConfettiEmitter.Burst(100),
    EmitterBounds = SKConfettiEmitterBounds.Center
});
```

## Emission Patterns

The emitter controls *how many* particles and *how fast* they appear:

```csharp
// Instant explosion - all particles at once
Emitter = SKConfettiEmitter.Burst(100);

// Continuous stream - 50 particles per second, for 3 seconds
Emitter = SKConfettiEmitter.Stream(50, 3);

// Infinite - 30 particles per second, forever
Emitter = SKConfettiEmitter.Infinite(30);
```

The emitter bounds controls *where* particles appear:

```csharp
// From the center of the view
EmitterBounds = SKConfettiEmitterBounds.Center;

// From the top edge (falling down)
EmitterBounds = SKConfettiEmitterBounds.Top;

// From a specific point
EmitterBounds = SKConfettiEmitterBounds.Point(100, 200);

// From both sides
confettiView.Systems.Add(new SKConfettiSystem { EmitterBounds = SKConfettiEmitterBounds.Left });
confettiView.Systems.Add(new SKConfettiSystem { EmitterBounds = SKConfettiEmitterBounds.Right });
```

## Customizing Appearance

### Colors

```csharp
// Custom color palette
var system = new SKConfettiSystem
{
    Colors = { Colors.Red, Colors.Gold, Colors.Blue, Colors.Green }
};
```

Or in XAML:

```xml
<skia:SKConfettiSystem Colors="Red, Gold, Blue, Green" />
```

### Shapes

Built-in shapes:

```csharp
Shapes = 
{
    new SKConfettiCircleShape(),
    new SKConfettiSquareShape(),
    new SKConfettiOvalShape { HeightRatio = 0.5 },
    new SKConfettiRectShape { HeightRatio = 0.3 }
}
```

Custom shapes using paths:

```csharp
Shapes = { new SKConfettiPathShape(SKGeometry.CreateRegularStarPath(20, 10, 5)) }
```

Or create a fully custom shape:

```csharp
public class HeartShape : SKConfettiShape
{
    protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
    {
        using var path = new SKPath();
        // Draw heart shape...
        canvas.DrawPath(path, paint);
    }
}
```

### Physics

Control particle size and how they respond to gravity:

```csharp
Physics = 
{
    new SKConfettiPhysics { Size = 10, Mass = 0.5 },  // Small, light
    new SKConfettiPhysics { Size = 20, Mass = 1.0 },  // Medium
    new SKConfettiPhysics { Size = 30, Mass = 2.0 }   // Large, heavy (falls faster)
}
```

## Motion Control

### Velocity

```csharp
var system = new SKConfettiSystem
{
    // Initial launch speed range
    MinimumInitialVelocity = 100,
    MaximumInitialVelocity = 300,
    
    // Speed limit
    MaximumVelocity = 500,
    
    // Spin rate
    MinimumRotationVelocity = -90,
    MaximumRotationVelocity = 90
};
```

### Gravity

```csharp
// Standard gravity (particles fall down)
Gravity = new Point(0, 100);

// Upward drift
Gravity = new Point(0, -50);

// Diagonal wind effect
Gravity = new Point(50, 100);
```

### Launch direction

Control the angle range particles are emitted:

```csharp
// Full 360° explosion (default)
StartAngle = 0;
EndAngle = 360;

// Upward cone from top emitter
EmitterBounds = SKConfettiEmitterBounds.Top;
StartAngle = 45;
EndAngle = 135;
```

## Particle Lifecycle

```csharp
var system = new SKConfettiSystem
{
    Lifetime = 3.0,   // Particles live for 3 seconds
    FadeOut = true    // Gradually fade before disappearing
};

// Handle completion
confettiView.IsComplete  // true when all particles are gone
```

## Complete Example

```csharp
void CelebrateAchievement()
{
    confettiView.Systems.Add(new SKConfettiSystem
    {
        Emitter = SKConfettiEmitter.Burst(150),
        EmitterBounds = SKConfettiEmitterBounds.Center,
        Colors = { Colors.Gold, Colors.Orange, Colors.Yellow },
        Shapes = 
        {
            new SKConfettiSquareShape(),
            new SKConfettiCircleShape()
        },
        MinimumInitialVelocity = 150,
        MaximumInitialVelocity = 350,
        Lifetime = 4.0,
        FadeOut = true
    });
}
```

## Learn More

- [API Reference](xref:SkiaSharp.Extended.UI.Controls.SKConfettiView) — Full method documentation
- [Geometry Helpers](geometry.md) — Create custom shapes with `SKGeometry`

[top-stream]: ../images/ui/controls/skconfettiview/top-stream.gif
[center-burst]: ../images/ui/controls/skconfettiview/center-burst.gif
[sides-spray]: ../images/ui/controls/skconfettiview/sides-spray.gif
