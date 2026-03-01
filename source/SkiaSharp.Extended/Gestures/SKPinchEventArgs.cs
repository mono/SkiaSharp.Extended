using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a pinch (scale) gesture.
/// </summary>
public class SKPinchEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKPinchEventArgs(SKPoint focalPoint, SKPoint previousFocalPoint, float scale)
	{
		FocalPoint = focalPoint;
		PreviousFocalPoint = previousFocalPoint;
		Scale = scale;
	}

	/// <summary>
	/// Gets the focal point (center of the pinch fingers).
	/// </summary>
	public SKPoint FocalPoint { get; }

	/// <summary>
	/// Gets the previous focal point.
	/// </summary>
	public SKPoint PreviousFocalPoint { get; }

	/// <summary>
	/// Gets the scale factor (1.0 = no change).
	/// </summary>
	public float Scale { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}
