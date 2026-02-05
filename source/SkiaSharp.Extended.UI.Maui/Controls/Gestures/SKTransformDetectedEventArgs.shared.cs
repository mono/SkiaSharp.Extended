namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a transform gesture is detected (pan, zoom, or rotate).
/// </summary>
public class SKTransformDetectedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance for a pan gesture.
	/// </summary>
	/// <param name="center">The current center position.</param>
	/// <param name="previousCenter">The previous center position.</param>
	public SKTransformDetectedEventArgs(SKPoint center, SKPoint previousCenter)
		: this(center, previousCenter, 1f, 0f)
	{
	}

	/// <summary>
	/// Creates a new instance with all transform properties.
	/// </summary>
	/// <param name="center">The current center position.</param>
	/// <param name="previousCenter">The previous center position.</param>
	/// <param name="scaleDelta">The scale change ratio (1.0 = no change).</param>
	/// <param name="rotationDelta">The rotation change in degrees.</param>
	public SKTransformDetectedEventArgs(SKPoint center, SKPoint previousCenter, float scaleDelta, float rotationDelta)
	{
		Center = center;
		PreviousCenter = previousCenter;
		ScaleDelta = scaleDelta;
		RotationDelta = rotationDelta;
	}

	/// <summary>
	/// Gets the current center position of the gesture.
	/// </summary>
	public SKPoint Center { get; }

	/// <summary>
	/// Gets the previous center position of the gesture.
	/// </summary>
	public SKPoint PreviousCenter { get; }

	/// <summary>
	/// Gets the scale change ratio (1.0 = no change, > 1.0 = zoom in, &lt; 1.0 = zoom out).
	/// </summary>
	public float ScaleDelta { get; }

	/// <summary>
	/// Gets the rotation change in degrees.
	/// </summary>
	public float RotationDelta { get; }

	/// <summary>
	/// Gets or sets whether the event has been handled.
	/// </summary>
	public bool Handled { get; set; }
}
