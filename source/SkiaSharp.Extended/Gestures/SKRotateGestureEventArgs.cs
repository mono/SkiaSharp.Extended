using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Event arguments for a rotation gesture.
/// </summary>
public class SKRotateGestureEventArgs : SKGestureEventArgs
{
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public SKRotateGestureEventArgs(SKPoint focalPoint, SKPoint previousFocalPoint, float rotationDelta)
	{
		FocalPoint = focalPoint;
		PreviousFocalPoint = previousFocalPoint;
		RotationDelta = rotationDelta;
	}

	/// <summary>
	/// Gets the focal point (center of the rotation fingers).
	/// </summary>
	public SKPoint FocalPoint { get; }

	/// <summary>
	/// Gets the previous focal point.
	/// </summary>
	public SKPoint PreviousFocalPoint { get; }

	/// <summary>
	/// Gets the rotation delta in degrees.
	/// </summary>
	public float RotationDelta { get; }

}
