namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for gesture start and end events.
/// </summary>
public class SKGestureEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance with the specified touch locations.
	/// </summary>
	/// <param name="locations">The locations of all active touch points.</param>
	public SKGestureEventArgs(IEnumerable<SKPoint> locations)
	{
		ArgumentNullException.ThrowIfNull(locations);
		Locations = locations.ToArray();
	}

	/// <summary>
	/// Gets the locations of all active touch points.
	/// </summary>
	public IReadOnlyList<SKPoint> Locations { get; }

	/// <summary>
	/// Gets or sets whether the event has been handled.
	/// </summary>
	public bool Handled { get; set; }
}
