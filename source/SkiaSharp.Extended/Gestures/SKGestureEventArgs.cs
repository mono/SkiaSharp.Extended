using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Base class for all gesture event arguments.
/// </summary>
public class SKGestureEventArgs : EventArgs
{
	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}
