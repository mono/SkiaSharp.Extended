namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a hover is detected (mouse or stylus without contact).
/// </summary>
public class SKHoverDetectedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance with the specified location.
	/// </summary>
	/// <param name="location">The location of the hover.</param>
	public SKHoverDetectedEventArgs(SKPoint location)
	{
		Location = location;
	}

	/// <summary>
	/// Gets the location of the hover.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets or sets whether the event has been handled.
	/// </summary>
	public bool Handled { get; set; }
}
