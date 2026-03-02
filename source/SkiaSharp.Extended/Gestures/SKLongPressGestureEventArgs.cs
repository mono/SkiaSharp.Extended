using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for a long press gesture event.
/// </summary>
/// <remarks>
/// <para>A long press is detected when a touch is held stationary (within the
/// <see cref="SKGestureDetectorOptions.TouchSlop"/> threshold) for at least
/// <see cref="SKGestureDetectorOptions.LongPressDuration"/> milliseconds.</para>
/// <seealso cref="SKGestureDetector.LongPressDetected"/>
/// <seealso cref="SKGestureDetectorOptions.LongPressDuration"/>
/// </remarks>
public class SKLongPressGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKLongPressGestureEventArgs"/> class.
	/// </summary>
	/// <param name="location">The location of the long press in view coordinates.</param>
	/// <param name="duration">The duration the touch was held before the long press was recognized.</param>
	public SKLongPressGestureEventArgs(SKPoint location, TimeSpan duration)
	{
		Location = location;
		Duration = duration;
	}

	/// <summary>
	/// Gets the location of the long press in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the position where the long press occurred.</value>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the duration the touch was held before the long press was recognized.
	/// </summary>
	/// <value>A <see cref="TimeSpan"/> representing the elapsed time from touch-down to long press detection.</value>
	public TimeSpan Duration { get; }
}
