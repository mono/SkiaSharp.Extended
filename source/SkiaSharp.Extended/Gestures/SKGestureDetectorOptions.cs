namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Configuration options for <see cref="SKGestureDetector"/>.
/// </summary>
public class SKGestureDetectorOptions
{
	/// <summary>
	/// Gets or sets the touch slop (minimum movement distance to start a gesture).
	/// </summary>
	public float TouchSlop { get; set; } = 8f;

	/// <summary>
	/// Gets or sets the maximum distance between two taps for double-tap detection.
	/// </summary>
	public float DoubleTapSlop { get; set; } = 40f;

	/// <summary>
	/// Gets or sets the fling velocity threshold in pixels per second.
	/// </summary>
	public float FlingThreshold { get; set; } = 200f;

	/// <summary>
	/// Gets or sets the long press duration in milliseconds.
	/// </summary>
	public int LongPressDuration { get; set; } = 500;
}
