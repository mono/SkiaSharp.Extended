using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Provides data for fling gesture events, including the initial fling detection and
/// per-frame animation updates.
/// </summary>
/// <remarks>
/// <para>This class is used by two distinct events:</para>
/// <list type="bullet">
/// <item><description><see cref="SKGestureDetector.FlingDetected"/> / <see cref="SKGestureTracker.FlingDetected"/>:
/// Fired once when a fling is initiated. <see cref="Velocity"/> contains the initial velocity.
/// <see cref="Delta"/> is <see cref="SKPoint.Empty"/>.</description></item>
/// <item><description><see cref="SKGestureTracker.FlingUpdated"/>: Fired each animation frame during
/// the fling deceleration. <see cref="Velocity"/> contains the current (decaying) velocity, and
/// <see cref="Delta"/> contains the per-frame displacement in pixels.</description></item>
/// </list>
/// <seealso cref="SKGestureDetector.FlingDetected"/>
/// <seealso cref="SKGestureTracker.FlingUpdated"/>
/// <seealso cref="SKGestureTracker.FlingCompleted"/>
/// </remarks>
public class SKFlingGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKFlingGestureEventArgs"/> class with
	/// velocity only. Used for the initial <see cref="SKGestureDetector.FlingDetected"/> event.
	/// </summary>
	/// <param name="velocity">The initial velocity in pixels per second.</param>
	public SKFlingGestureEventArgs(SKPoint velocity)
		: this(velocity, SKPoint.Empty)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKFlingGestureEventArgs"/> class with
	/// velocity and per-frame displacement. Used for <see cref="SKGestureTracker.FlingUpdated"/> events.
	/// </summary>
	/// <param name="velocity">The current velocity in pixels per second.</param>
	/// <param name="delta">The displacement for this animation frame, in pixels.</param>
	public SKFlingGestureEventArgs(SKPoint velocity, SKPoint delta)
	{
		Velocity = velocity;
		Delta = delta;
	}

	/// <summary>
	/// Gets the velocity of the fling.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> are the velocity components in pixels
	/// per second. Positive X is rightward; positive Y is downward.
	/// </value>
	public SKPoint Velocity { get; }

	/// <summary>
	/// Gets the displacement for this animation frame.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> with the per-frame displacement in pixels. This is
	/// <see cref="SKPoint.Empty"/> for <see cref="SKGestureDetector.FlingDetected"/> events.
	/// </value>
	public SKPoint Delta { get; }

	/// <summary>
	/// Gets the current speed (magnitude of the velocity vector).
	/// </summary>
	/// <value>The speed in pixels per second, computed as <c>sqrt(Velocity.X² + Velocity.Y²)</c>.</value>
	public float Speed => (float)Math.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y);
}
