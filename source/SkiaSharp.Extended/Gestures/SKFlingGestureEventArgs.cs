using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a fling gesture.
/// </summary>
public class SKFlingGestureEventArgs : SKGestureEventArgs
{
	/// <summary>
	/// Creates a new instance with velocity only (used for FlingDetected).
	/// </summary>
	public SKFlingGestureEventArgs(float velocityX, float velocityY)
		: this(velocityX, velocityY, 0f, 0f)
	{
	}

	/// <summary>
	/// Creates a new instance with velocity and per-frame delta (used for Flinging).
	/// </summary>
	public SKFlingGestureEventArgs(float velocityX, float velocityY, float deltaX, float deltaY)
	{
		VelocityX = velocityX;
		VelocityY = velocityY;
		DeltaX = deltaX;
		DeltaY = deltaY;
	}

	/// <summary>
	/// Gets the X velocity in pixels per second.
	/// </summary>
	public float VelocityX { get; }

	/// <summary>
	/// Gets the Y velocity in pixels per second.
	/// </summary>
	public float VelocityY { get; }

	/// <summary>
	/// Gets the per-frame X displacement in pixels.
	/// </summary>
	public float DeltaX { get; }

	/// <summary>
	/// Gets the per-frame Y displacement in pixels.
	/// </summary>
	public float DeltaY { get; }

	/// <summary>
	/// Gets the current speed (magnitude of velocity) in pixels per second.
	/// </summary>
	public float Speed => (float)Math.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);

}
