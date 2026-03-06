namespace SkiaSharp.Extended;

/// <summary>
/// Manages three <see cref="SpringAnimator"/> instances (X, Y, width) for animating
/// deep-zoom-style viewport transitions with spring physics.
/// </summary>
/// <remarks>
/// This is a general-purpose animated viewport helper, usable by any content that pans/zooms
/// and wants spring-physics animation. It is decoupled from tile loading and rendering — those
/// concerns belong in the consuming layer.
/// </remarks>
public class ViewportSpring
{
/// <summary>Initializes a new <see cref="ViewportSpring"/> at the default (full-image-fit) state.</summary>
public ViewportSpring()
{
OriginX = new SpringAnimator(0.0);
OriginY = new SpringAnimator(0.0);
Width = new SpringAnimator(1.0);
}

/// <summary>The spring for the viewport origin X.</summary>
public SpringAnimator OriginX { get; }

/// <summary>The spring for the viewport origin Y.</summary>
public SpringAnimator OriginY { get; }

/// <summary>The spring for the viewport width (zoom level).</summary>
public SpringAnimator Width { get; }

/// <summary>
/// Spring stiffness applied to all three axes. Higher = faster snap, lower = slower/smoother.
/// Default is <c>100.0</c>.
/// </summary>
public double Stiffness
{
get => Width.Stiffness;
set
{
OriginX.Stiffness = value;
OriginY.Stiffness = value;
Width.Stiffness = value;
}
}

/// <summary>
/// Damping ratio applied to all three axes.
/// <c>1.0</c> = critically damped (no overshoot), <c>&lt;1.0</c> = underdamped (bouncy),
/// <c>&gt;1.0</c> = overdamped (sluggish). Default is <c>1.0</c>.
/// </summary>
public double DampingRatio
{
get => Width.DampingRatio;
set
{
OriginX.DampingRatio = value;
OriginY.DampingRatio = value;
Width.DampingRatio = value;
}
}

/// <summary>Gets a value indicating whether all three springs have settled.</summary>
public bool IsSettled => OriginX.IsSettled && OriginY.IsSettled && Width.IsSettled;

/// <summary>Updates all three springs by the given time step.</summary>
/// <param name="deltaTime">Time step in seconds.</param>
public void Update(double deltaTime)
{
OriginX.Update(deltaTime);
OriginY.Update(deltaTime);
Width.Update(deltaTime);
}

/// <summary>Immediately snaps all springs to their targets, stopping animation.</summary>
public void SnapToTarget()
{
OriginX.SnapToTarget();
OriginY.SnapToTarget();
Width.SnapToTarget();
}

/// <summary>Sets the target viewport state that the springs will animate toward.</summary>
/// <param name="originX">Target viewport origin X.</param>
/// <param name="originY">Target viewport origin Y.</param>
/// <param name="width">Target viewport width (zoom level).</param>
public void SetTarget(double originX, double originY, double width)
{
OriginX.Target = originX;
OriginY.Target = originY;
Width.Target = width;
}

/// <summary>Resets all springs to a new state instantly, with no animation.</summary>
/// <param name="originX">New origin X (both current and target).</param>
/// <param name="originY">New origin Y (both current and target).</param>
/// <param name="width">New width (both current and target).</param>
public void Reset(double originX, double originY, double width)
{
OriginX.Reset(originX);
OriginY.Reset(originY);
Width.Reset(width);
}

/// <summary>
/// Gets the current animated viewport state as a value tuple.
/// </summary>
/// <returns>A <c>(OriginX, OriginY, Width)</c> tuple of the current spring positions.</returns>
public (double OriginX, double OriginY, double Width) GetCurrentState()
=> (OriginX.Current, OriginY.Current, Width.Current);
}
