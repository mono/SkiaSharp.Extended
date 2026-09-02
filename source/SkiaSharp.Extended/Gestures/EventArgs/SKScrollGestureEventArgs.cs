using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Provides data for a mouse scroll (wheel) gesture event.
/// </summary>
/// <remarks>
/// <para>Scroll events are raised when the mouse wheel is rotated or a trackpad scroll gesture
/// is performed. The <see cref="SKGestureTracker"/> uses <see cref="Delta"/>.Y for scroll-wheel zoom
/// when <see cref="SKGestureTrackerOptions.IsScrollZoomEnabled"/> is <see langword="true"/>.</para>
/// <para>Scroll deltas use the v120 convention: <c>120</c> represents one discrete wheel notch,
/// positive Y scrolls up, and continuous trackpad input may produce proportional sub-120 values.</para>
/// <seealso cref="SKGestureTracker.ScrollDetected"/>
/// <seealso cref="SKGestureTracker.ProcessMouseWheel"/>
/// <seealso cref="SKGestureTracker.ScrollDetected"/>
/// </remarks>
public class SKScrollGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKScrollGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The position of the mouse cursor when the scroll occurred, in view coordinates.</param>
	/// <param name="delta">The scroll delta.</param>
	public SKScrollGestureEventArgs(SKPoint location, SKPoint delta)
	{
		Location = location;
		Delta = delta;
	}

	/// <summary>
	/// Gets the position of the mouse cursor when the scroll occurred, in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the mouse position at the time of the scroll.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the scroll delta.
	/// </summary>
	/// <value>
	/// An <see cref="SKPoint"/> where <c>X</c> is the horizontal scroll amount and <c>Y</c> is the
	/// vertical scroll amount in v120 units. Positive <c>Y</c> indicates scrolling up or zooming in.
	/// A value of <c>120</c> represents one discrete wheel notch; continuous input can be smaller.
	/// When <see cref="SKGestureTrackerOptions.IsScrollZoomEnabled"/> is <see langword="true"/>, <c>Y</c>
	/// is converted to wheel notches and multiplied by
	/// <see cref="SKGestureTrackerOptions.ScrollZoomFactor"/> to determine the zoom change.
	/// </value>
	public SKPoint Delta { get; }
}
