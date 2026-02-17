# Attractor Physics Fix - Inverse Square Law Implementation

## Problem Identified

The attractor force calculation in `PhysicsWorld.shared.cs` was missing the inverse square law, making attractors have constant force regardless of distance. This is physically incorrect and resulted in unrealistic particle behavior.

## The Issue

### Before (Incorrect)
```csharp
// Apply attractors
foreach (var attractor in attractors)
{
    var direction = attractor.Position - particle.Position;
    var distanceSq = direction.LengthSquared();
    if (distanceSq > 0.1f)
    {
        var attractorForce = direction / (float)Math.Sqrt(distanceSq) * attractor.Strength;
        force += attractorForce;
    }
}
```

**Problem**: The force was just `normalized_direction * strength`, which means:
- Force magnitude = `strength` (constant)
- Distance had NO effect on force magnitude
- Only direction was affected by particle position

### After (Correct)
```csharp
// Apply attractors (inverse square law)
foreach (var attractor in attractors)
{
    var direction = attractor.Position - particle.Position;
    var distanceSq = direction.LengthSquared();
    if (distanceSq > 0.1f)
    {
        var distance = (float)Math.Sqrt(distanceSq);
        // Inverse square law: F = k * (1/r²), with minimum distance to prevent extreme forces
        var attractorForce = (direction / distance) * (attractor.Strength / Math.Max(distanceSq, 100f));
        force += attractorForce;
    }
}
```

**Fixed**: Now implements inverse square law:
- Force magnitude = `strength / distance²`
- Force decreases with square of distance (realistic physics)
- Particles accelerate as they get closer to attractor
- Minimum distance clamping prevents extreme forces

## Why This Matters

### Inverse Square Law in Physics

In real physics (gravity, electromagnetism), force follows the inverse square law:

```
F = k / r²
```

Where:
- `F` = force magnitude
- `k` = strength constant
- `r` = distance

This means:
- At distance `r`, force = `k/r²`
- At distance `2r`, force = `k/(2r)² = k/4r²` (1/4 the force)
- At distance `3r`, force = `k/(3r)² = k/9r²` (1/9 the force)

### What Was Happening (Bug)

Without inverse square law:
- Particle at 10 units away: force = `strength`
- Particle at 100 units away: force = `strength` (same!)
- Particle at 1000 units away: force = `strength` (still same!)

This meant attractors pulled equally hard on all particles regardless of distance, which is completely unrealistic.

### What Happens Now (Fixed)

With inverse square law:
- Particle at 10 units away: force = `strength / 100`
- Particle at 100 units away: force = `strength / 10,000` (100x weaker)
- Particle at 1000 units away: force = `strength / 1,000,000` (10,000x weaker)

This creates realistic "pull harder when closer" behavior.

## The GIF Generator Was Correct

Interestingly, the GIF generator code (`GifGeneratorImageSharp.cs`) had the correct implementation all along:

```csharp
var toAttractor = attractorPos - particle.Position;
var distanceSq = toAttractor.LengthSquared();
if (distanceSq > 1f)
{
    var distance = (float)Math.Sqrt(distanceSq);
    var force = toAttractor / distance * (attractorStrength / Math.Max(distanceSq, 100f));
    particle.Velocity += force * dt;
}
```

This is why the GIF looked somewhat correct (particles being pulled in), but when users tried to use the actual `PhysicsWorld` class in their apps, the attractors wouldn't work as expected.

## Unit Tests Added

To prevent regression and verify the fix, I added 3 new comprehensive tests:

### 1. Inverse Square Law Verification
```csharp
PhysicsWorld_Attractor_InverseSquareLaw_ForceDimishesWithDistance
```
- Tests particle at distance `d` vs. particle at distance `2d`
- Verifies force at `2d` is approximately 1/4 the force at `d`
- Confirms inverse square law is working

### 2. Closer is Stronger
```csharp
PhysicsWorld_Attractor_CloserParticleExperiencesStrongerForce
```
- Two particles at different distances
- Verifies closer particle gets stronger pull
- Confirms distance matters

### 3. Division by Zero Protection
```csharp
PhysicsWorld_Attractor_MinimumDistancePreventsDivisionByZero
```
- Particle very close to attractor center
- Verifies no exceptions from extreme forces
- Confirms `Math.Max(distanceSq, 100f)` works

All tests pass! ✅

## Visual Comparison

### Before Fix
Particles would:
- Move toward attractor at constant speed
- Not accelerate as they approach
- Behave unrealistically

### After Fix
Particles now:
- Accelerate as they get closer to attractor
- Move faster near the attractor
- Follow realistic physics
- Match the GIF behavior

## Impact

This fix affects:
1. **Sample Apps**: Morphysics Physics Playground demo now has realistic attractors
2. **Production Code**: Any apps using PhysicsWorld with attractors will see correct behavior
3. **Documentation**: GIFs and demos now match actual library behavior

## Minimum Distance Clamping

The `Math.Max(distanceSq, 100f)` prevents division by very small numbers:
- Prevents infinite force at distance = 0
- Prevents extreme forces at very close distances
- Makes simulation stable and realistic
- Value of 100 means minimum effective distance of 10 units

This is standard practice in physics simulations to prevent numerical instability.

---

**Status**: ✅ Fixed, tested, and verified

**Files Modified**:
- `source/SkiaSharp.Extended.UI.Maui/Controls/Morphysics/PhysicsWorld.shared.cs`
- `tests/SkiaSharp.Extended.UI.Maui.Tests/Controls/Morphysics/PhysicsWorldTest.cs`

**Files Regenerated**:
- `docs/images/morphysics/gifs/attractor-demo.gif` (now shows correct behavior)
