using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Provides data for a pinch (scale) gesture event detected from two or more simultaneous touches.
/// </summary>
/// <remarks>
/// <para>Pinch events are raised continuously as two or more touches move relative to each other.
/// The <see cref="ScaleDelta"/> is a relative (per-event) multiplier, not an absolute scale. To
/// compute the cumulative scale, multiply successive <see cref="ScaleDelta"/> values together, or
/// use the <see cref="SKGestureTracker.Scale"/> property which maintains the absolute value.</para>
/// <seealso cref="SKGestureDetector.PinchDetected"/>
/// <seealso cref="SKGestureTracker.PinchDetected"/>
/// <seealso cref="SKRotateGestureEventArgs"/>
/// </remarks>
public class SKPinchGestureEventArgs : SKGestureEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKPinchGestureEventArgs"/> class.
	/// </summary>
	/// <param name="focalPoint">The current center point between the pinch fingers, in view coordinates.</param>
	/// <param name="previousFocalPoint">The center point between the pinch fingers from the previous event.</param>
	/// <param name="scaleDelta">The relative scale change factor since the previous event.</param>
	public SKPinchGestureEventArgs(SKPoint focalPoint, SKPoint previousFocalPoint, float scaleDelta)
	{
		FocalPoint = focalPoint;
		PreviousFocalPoint = previousFocalPoint;
		ScaleDelta = scaleDelta;
	}

	/// <summary>
	/// Gets the current center point between the pinch fingers in view coordinates.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the midpoint of all active touches.</value>
	public SKPoint FocalPoint { get; }

	/// <summary>
	/// Gets the center point between the pinch fingers from the previous event.
	/// </summary>
	/// <value>An <see cref="SKPoint"/> representing the previous midpoint of all active touches.</value>
	public SKPoint PreviousFocalPoint { get; }

	/// <summary>
	/// Gets the relative scale change factor since the previous pinch event.
	/// </summary>
	/// <value>
	/// A value of <c>1.0</c> means no change. Values greater than <c>1.0</c> indicate zooming in
	/// (fingers spreading apart), and values less than <c>1.0</c> indicate zooming out
	/// (fingers pinching together). This is a per-event delta, not a cumulative scale.
	/// </value>
	public float ScaleDelta { get; }

}
