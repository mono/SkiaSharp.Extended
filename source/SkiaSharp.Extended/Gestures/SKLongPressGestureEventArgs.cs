using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a long press gesture.
/// </summary>
public class SKLongPressGestureEventArgs : SKGestureEventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKLongPressGestureEventArgs(SKPoint location, TimeSpan duration)
	{
		Location = location;
		Duration = duration;
	}

	/// <summary>
	/// Gets the location of the long press.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the duration the touch was held before the long press was detected.
	/// </summary>
	public TimeSpan Duration { get; }
}
