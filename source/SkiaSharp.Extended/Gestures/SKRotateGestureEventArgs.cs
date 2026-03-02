using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for a rotation gesture event detected from two simultaneous touches.
/// </summary>
/// <remarks>
/// <para>Rotation events are raised simultaneously with <see cref="SKPinchGestureEventArgs"/> when
/// two or more touches are active. The <see cref="RotationDelta"/> is a per-event incremental
/// angle change in degrees. To compute cumulative rotation, sum successive deltas or use the
/// <see cref="SKGestureTracker.Rotation"/> property.</para>
/// <seealso cref="SKGestureDetector.RotateDetected"/>
/// <seealso cref="SKGestureTracker.RotateDetected"/>
/// <seealso cref="SKPinchGestureEventArgs"/>
/// </remarks>
public class SKRotateGestureEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKRotateGestureEventArgs"/> class.
	/// </summary>
	/// <param name="focalPoint">The current center point between the rotation fingers, in view coordinates.</param>
	/// <param name="previousFocalPoint">The center point between the rotation fingers from the previous event.</param>
	/// <param name="rotationDelta">The incremental rotation angle in degrees since the previous event.</param>
	public SKRotateGestureEventArgs(SKPoint focalPoint, SKPoint previousFocalPoint, float rotationDelta)
	{
		FocalPoint = focalPoint;
		PreviousFocalPoint = previousFocalPoint;
		RotationDelta = rotationDelta;
	}

	/// <summary>
	/// Gets the current center point between the rotation fingers in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the midpoint of the two touches.</value>
	public SKPoint FocalPoint { get; }

	/// <summary>
	/// Gets the center point between the rotation fingers from the previous event.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the previous midpoint of the two touches.</value>
	public SKPoint PreviousFocalPoint { get; }

	/// <summary>
	/// Gets the incremental rotation angle change since the previous event, in degrees.
	/// </summary>
	/// <value>
	/// A positive value indicates clockwise rotation; a negative value indicates counter-clockwise
	/// rotation. The value is normalized to the range <c>(-180, 180]</c>.
	/// </value>
	public float RotationDelta { get; }

}
