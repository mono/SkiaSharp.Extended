namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// The current state of a gesture.
/// </summary>
internal enum SKGestureState
{
	/// <summary>
	/// No gesture is active.
	/// </summary>
	None,

	/// <summary>
	/// A gesture is being detected.
	/// </summary>
	Detecting,

	/// <summary>
	/// A pan gesture is in progress.
	/// </summary>
	Panning,

	/// <summary>
	/// A pinch/zoom gesture is in progress.
	/// </summary>
	Pinching
}
