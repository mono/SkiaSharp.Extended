namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a tap gesture is detected (single, double, or long press).
/// </summary>
public class SKTapDetectedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance for a single tap.
	/// </summary>
	/// <param name="location">The location of the tap.</param>
	public SKTapDetectedEventArgs(SKPoint location)
		: this(location, 1)
	{
	}

	/// <summary>
	/// Creates a new instance with the specified tap count.
	/// </summary>
	/// <param name="location">The location of the tap.</param>
	/// <param name="tapCount">The number of taps (1 for single, 2 for double, etc.).</param>
	public SKTapDetectedEventArgs(SKPoint location, int tapCount)
	{
		Location = location;
		TapCount = tapCount;
	}

	/// <summary>
	/// Gets the location of the tap.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the number of taps (1 for single, 2 for double, etc.).
	/// </summary>
	public int TapCount { get; }

	/// <summary>
	/// Gets or sets whether the event has been handled.
	/// </summary>
	public bool Handled { get; set; }
}
