using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for fling gesture events, including the initial fling detection and
/// per-frame animation updates.
/// </summary>
/// <remarks>
/// <para>This class is used by two distinct events:</para>
/// <list type="bullet">
/// <item><description><see cref="SKGestureDetector.FlingDetected"/> / <see cref="SKGestureTracker.FlingDetected"/>:
/// Fired once when a fling is initiated. <see cref="VelocityX"/> and <see cref="VelocityY"/>
/// contain the initial velocity. <see cref="DeltaX"/> and <see cref="DeltaY"/> are <c>0</c>.</description></item>
/// <item><description><see cref="SKGestureTracker.FlingUpdated"/>: Fired each animation frame during
/// the fling deceleration. <see cref="VelocityX"/> and <see cref="VelocityY"/> contain the
/// current (decaying) velocity, and <see cref="DeltaX"/> and <see cref="DeltaY"/> contain
/// the per-frame displacement in pixels.</description></item>
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
	/// <param name="velocityX">The horizontal velocity in pixels per second.</param>
	/// <param name="velocityY">The vertical velocity in pixels per second.</param>
	public SKFlingGestureEventArgs(float velocityX, float velocityY)
		: this(velocityX, velocityY, 0f, 0f)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKFlingGestureEventArgs"/> class with
	/// velocity and per-frame displacement. Used for <see cref="SKGestureTracker.FlingUpdated"/> events.
	/// </summary>
	/// <param name="velocityX">The current horizontal velocity in pixels per second.</param>
	/// <param name="velocityY">The current vertical velocity in pixels per second.</param>
	/// <param name="deltaX">The horizontal displacement for this animation frame, in pixels.</param>
	/// <param name="deltaY">The vertical displacement for this animation frame, in pixels.</param>
	public SKFlingGestureEventArgs(float velocityX, float velocityY, float deltaX, float deltaY)
	{
		VelocityX = velocityX;
		VelocityY = velocityY;
		DeltaX = deltaX;
		DeltaY = deltaY;
	}

	/// <summary>
	/// Gets the horizontal velocity component.
	/// </summary>
	/// <value>The horizontal velocity in pixels per second. Positive values indicate rightward movement.</value>
	public float VelocityX { get; }

	/// <summary>
	/// Gets the vertical velocity component.
	/// </summary>
	/// <value>The vertical velocity in pixels per second. Positive values indicate downward movement.</value>
	public float VelocityY { get; }

	/// <summary>
	/// Gets the horizontal displacement for this animation frame.
	/// </summary>
	/// <value>
	/// The per-frame horizontal displacement in pixels. This is <c>0</c> for
	/// <see cref="SKGestureDetector.FlingDetected"/> events and contains the actual frame
	/// displacement for <see cref="SKGestureTracker.FlingUpdated"/> events.
	/// </value>
	public float DeltaX { get; }

	/// <summary>
	/// Gets the vertical displacement for this animation frame.
	/// </summary>
	/// <value>
	/// The per-frame vertical displacement in pixels. This is <c>0</c> for
	/// <see cref="SKGestureDetector.FlingDetected"/> events and contains the actual frame
	/// displacement for <see cref="SKGestureTracker.FlingUpdated"/> events.
	/// </value>
	public float DeltaY { get; }

	/// <summary>
	/// Gets the current speed (magnitude of the velocity vector).
	/// </summary>
	/// <value>The speed in pixels per second, computed as <c>sqrt(VelocityX² + VelocityY²)</c>.</value>
	public float Speed => (float)Math.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);

}
