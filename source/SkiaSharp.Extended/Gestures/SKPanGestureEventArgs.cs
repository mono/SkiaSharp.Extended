using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a pan gesture.
/// </summary>
public class SKPanGestureEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKPanGestureEventArgs(SKPoint location, SKPoint previousLocation, SKPoint delta, SKPoint velocity)
	{
		Location = location;
		PreviousLocation = previousLocation;
		Delta = delta;
		Velocity = velocity;
	}

	/// <summary>
	/// Gets the current location.
	/// </summary>
	public SKPoint Location { get; }

	/// <summary>
	/// Gets the previous location.
	/// </summary>
	public SKPoint PreviousLocation { get; }

	/// <summary>
	/// Gets the delta movement.
	/// </summary>
	public SKPoint Delta { get; }

	/// <summary>
	/// Gets the current velocity in pixels per second.
	/// </summary>
	public SKPoint Velocity { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}
