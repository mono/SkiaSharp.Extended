using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a rotation gesture.
/// </summary>
public class SKRotateEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKRotateEventArgs(SKPoint center, SKPoint previousCenter, float rotationDelta)
	{
		Center = center;
		PreviousCenter = previousCenter;
		RotationDelta = rotationDelta;
	}

	/// <summary>
	/// Gets the center point of rotation.
	/// </summary>
	public SKPoint Center { get; }

	/// <summary>
	/// Gets the previous center point of rotation.
	/// </summary>
	public SKPoint PreviousCenter { get; }

	/// <summary>
	/// Gets the rotation delta in degrees.
	/// </summary>
	public float RotationDelta { get; }

	/// <summary>
	/// Gets or sets whether the event was handled.
	/// </summary>
	public bool Handled { get; set; }
}
