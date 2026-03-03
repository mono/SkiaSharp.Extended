using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Provides data for a hover (mouse move without contact) gesture event.
/// </summary>
/// <remarks>
/// <para>Hover events are raised when a mouse cursor moves over the surface without any buttons
/// pressed (i.e., <c>inContact</c> is <see langword="false"/> in <see cref="SKGestureDetector.ProcessTouchMove"/>).
/// This is a mouse-only gesture that has no equivalent on touch devices.</para>
/// <seealso cref="SKGestureDetector.HoverDetected"/>
/// <seealso cref="SKGestureTracker.HoverDetected"/>
/// </remarks>
public class SKHoverGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKHoverGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The current position of the mouse cursor in view coordinates.</param>
	public SKHoverGestureEventArgs(SKPoint location)
	{
		Location = location;
	}

	/// <summary>
	/// Gets the current position of the mouse cursor in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the hover position.</value>
	public SKPoint Location { get; }

}
