using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for a mouse scroll (wheel) gesture event.
/// </summary>
/// <remarks>
/// <para>Scroll events are raised when the mouse wheel is rotated or a trackpad scroll gesture
/// is performed. The <see cref="SKGestureTracker"/> uses <see cref="DeltaY"/> for scroll-wheel zoom
/// when <see cref="SKGestureTrackerOptions.IsScrollZoomEnabled"/> is <see langword="true"/>.</para>
/// <para><strong>Platform note:</strong> The sign convention for scroll deltas may vary by platform
/// and input device. Typically, positive <see cref="DeltaY"/> indicates scrolling up (or zooming in),
/// but this depends on the platform's scroll event normalization. Consumers should test on their
/// target platforms to confirm the expected behavior.</para>
/// <seealso cref="SKGestureDetector.ScrollDetected"/>
/// <seealso cref="SKGestureDetector.ProcessMouseWheel"/>
/// <seealso cref="SKGestureTracker.ScrollDetected"/>
/// </remarks>
public class SKScrollGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKScrollGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The position of the mouse cursor when the scroll occurred, in view coordinates.</param>
	/// <param name="deltaX">The horizontal scroll delta.</param>
	/// <param name="deltaY">The vertical scroll delta.</param>
	public SKScrollGestureEventArgs(SKPoint location, float deltaX, float deltaY)
	{
		Location = location;
		DeltaX = deltaX;
		DeltaY = deltaY;
	}

	/// <summary>
	/// Gets the position of the mouse cursor when the scroll occurred, in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the mouse position at the time of the scroll.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the horizontal scroll delta.
	/// </summary>
	/// <value>The horizontal scroll amount. Positive values typically indicate scrolling to the right.</value>
	public float DeltaX { get; }

	/// <summary>
	/// Gets the vertical scroll delta.
	/// </summary>
	/// <value>
	/// The vertical scroll amount. Positive values typically indicate scrolling up or zooming in.
	/// When <see cref="SKGestureTrackerOptions.IsScrollZoomEnabled"/> is <see langword="true"/>, this value
	/// is multiplied by <see cref="SKGestureTrackerOptions.ScrollZoomFactor"/> to determine the zoom change.
	/// </value>
	public float DeltaY { get; }

}
