namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Configuration options for <see cref="SKGestureTracker"/>.
/// Inherits engine-level options and adds tracker-specific settings.
/// </summary>
public class SKGestureTrackerOptions : SKGestureEngineOptions
{
	/// <summary>
	/// Gets or sets the minimum allowed scale.
	/// </summary>
	public float MinScale { get; set; } = 0.1f;

	/// <summary>
	/// Gets or sets the maximum allowed scale.
	/// </summary>
	public float MaxScale { get; set; } = 10f;

	/// <summary>
	/// Gets or sets the zoom factor applied per double-tap.
	/// </summary>
	public float DoubleTapZoomFactor { get; set; } = 2f;

	/// <summary>
	/// Gets or sets the zoom animation duration in milliseconds.
	/// </summary>
	public int ZoomAnimationDuration { get; set; } = 250;

	/// <summary>
	/// Gets or sets how much each scroll tick changes scale.
	/// </summary>
	public float ScrollZoomFactor { get; set; } = 0.1f;

	/// <summary>
	/// Gets or sets the fling friction (0 = no friction / infinite fling, 1 = full friction / no fling).
	/// </summary>
	public float FlingFriction { get; set; } = 0.08f;

	/// <summary>
	/// Gets or sets the minimum fling velocity before the animation stops.
	/// </summary>
	public float FlingMinVelocity { get; set; } = 5f;

	/// <summary>
	/// Gets or sets the fling animation frame interval in milliseconds.
	/// </summary>
	public int FlingFrameInterval { get; set; } = 16;
}
