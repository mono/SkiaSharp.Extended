using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for a pan (single-finger drag) gesture event.
/// </summary>
/// <remarks>
/// <para>Pan events are raised continuously as a single touch moves beyond the
/// <see cref="SKGestureDetectorOptions.TouchSlop"/> threshold. Each event provides both
/// the incremental <see cref="Delta"/> and the instantaneous <see cref="Velocity"/>.</para>
/// <seealso cref="SKGestureDetector.PanDetected"/>
/// <seealso cref="SKGestureTracker.PanDetected"/>
/// </remarks>
public class SKPanGestureEventArgs : SKGestureEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKPanGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The current touch location in view coordinates.</param>
	/// <param name="previousLocation">The touch location from the previous pan event.</param>
	/// <param name="delta">The displacement from <paramref name="previousLocation"/> to <paramref name="location"/>.</param>
	/// <param name="velocity">The current velocity of the touch in pixels per second.</param>
	public SKPanGestureEventArgs(SKPoint location, SKPoint previousLocation, SKPoint delta, SKPoint velocity)
	{
		Location = location;
		PreviousLocation = previousLocation;
		Delta = delta;
		Velocity = velocity;
	}

	/// <summary>
	/// Gets the current touch location in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the current position of the touch.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the touch location from the previous pan event.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the previous position of the touch.</value>
	public SKPoint PreviousLocation { get; }

	/// <summary>
	/// Gets the displacement from <see cref="PreviousLocation"/> to <see cref="Location"/>.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> represent the change in position, in pixels.</value>
	public SKPoint Delta { get; }

	/// <summary>
	/// Gets the current velocity of the touch movement.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> represent the velocity components
	/// in pixels per second. Positive X is rightward; positive Y is downward.
	/// </value>
	/// <remarks>
	/// The velocity is computed from a time-weighted average of recent touch events by the
	/// internal <c>SKFlingTracker</c>. This value is also used to determine whether a fling
	/// gesture should be triggered when the touch is released.
	/// </remarks>
	public SKPoint Velocity { get; }

}
