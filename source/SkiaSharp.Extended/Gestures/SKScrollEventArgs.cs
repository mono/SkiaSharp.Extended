using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a mouse scroll (wheel) event.
/// </summary>
public class SKScrollEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKScrollEventArgs(SKPoint location, float deltaX, float deltaY)
	{
		Location = location;
		DeltaX = deltaX;
		DeltaY = deltaY;
	}

	/// <summary>
	/// Gets the location of the mouse when the scroll occurred.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the horizontal scroll delta.
	/// </summary>
	public float DeltaX { get; }

	/// <summary>
	/// Gets the vertical scroll delta (positive = scroll up/zoom in).
	/// </summary>
	public float DeltaY { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}
