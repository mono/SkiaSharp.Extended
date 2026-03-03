using System;

namespace SkiaSharp.Extended;

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
public class SKPanGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKPanGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The current touch location in view coordinates.</param>
	/// <param name="prevLocation">The touch location from the previous pan event.</param>
	/// <param name="velocity">The current velocity of the touch in pixels per second.</param>
	public SKPanGestureEventArgs(SKPoint location, SKPoint prevLocation, SKPoint velocity)
	{
		Location = location;
		PrevLocation = prevLocation;
		Velocity = velocity;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the event has been handled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the event has been handled by a consumer and default processing
	/// should be skipped; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
	/// </value>
	/// <remarks>
	/// Set this to <see langword="true"/> in a <see cref="SKGestureTracker.PanDetected"/> handler
	/// to prevent the <see cref="SKGestureTracker"/> from updating <see cref="SKGestureTracker.Offset"/>.
	/// </remarks>
	public bool Handled { get; set; }

	/// <summary>
	/// Gets the current touch location in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the current position of the touch.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the touch location from the previous pan event.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the previous position of the touch.</value>
	public SKPoint PrevLocation { get; }

	/// <summary>
	/// Gets the displacement from <see cref="PrevLocation"/> to <see cref="Location"/>.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> represent the change in position, in pixels.</value>
	/// <remarks>Calculated as <c>Location - PrevLocation</c>.</remarks>
	public SKPoint Delta => new SKPoint(Location.X - PrevLocation.X, Location.Y - PrevLocation.Y);

	/// <summary>
	/// Gets the current velocity of the touch movement.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> where <c>X</c> and <c>Y</c> represent the velocity components
	/// in pixels per second. Positive X is rightward; positive Y is downward.
	/// </value>
	public SKPoint Velocity { get; }
}
