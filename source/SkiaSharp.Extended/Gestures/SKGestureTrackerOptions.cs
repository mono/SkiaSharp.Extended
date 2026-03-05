using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Configuration options for <see cref="SKGestureTracker"/>. Inherits gesture detection thresholds
/// from <see cref="SKGestureDetectorOptions"/> and adds tracker-specific settings for transform
/// limits, animation, and feature toggles.
/// </summary>
/// <remarks>
/// <seealso cref="SKGestureTracker"/>
/// <seealso cref="SKGestureDetectorOptions"/>
/// </remarks>
public class SKGestureTrackerOptions : SKGestureDetectorOptions
{
	private float _minScale = 0.1f;
	private float _maxScale = 10f;
	private float _doubleTapZoomFactor = 2f;
	private TimeSpan _zoomAnimationDuration = TimeSpan.FromMilliseconds(250);
	private float _scrollZoomFactor = 0.1f;
	private float _flingFriction = 0.08f;
	private float _flingMinVelocity = 5f;
	private TimeSpan _flingFrameInterval = TimeSpan.FromMilliseconds(16);
	private TimeSpan _zoomAnimationInterval = TimeSpan.FromMilliseconds(16);

	/// <summary>
	/// Gets or sets the minimum allowed zoom scale.
	/// </summary>
	/// <value>The minimum scale factor. The default is <c>0.1</c>. Must be positive.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
	public float MinScale
	{
		get => _minScale;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MinScale must be positive.");
			if (value > _maxScale)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MinScale must not be greater than MaxScale.");
			_minScale = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum allowed zoom scale.
	/// </summary>
	/// <value>The maximum scale factor. The default is <c>10</c>. Must be positive and greater than or equal to <see cref="MinScale"/>.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero, negative, or less than <see cref="MinScale"/>.</exception>
	public float MaxScale
	{
		get => _maxScale;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MaxScale must be positive.");
			if (value < _minScale)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MaxScale must not be less than MinScale.");
			_maxScale = value;
		}
	}

	/// <summary>
	/// Sets both <see cref="MinScale"/> and <see cref="MaxScale"/> atomically,
	/// avoiding ordering-dependent validation errors when the desired range lies
	/// entirely outside the current default range of [0.1, 10].
	/// </summary>
	/// <param name="minScale">The minimum scale value. Must be positive and less than <paramref name="maxScale"/>.</param>
	/// <param name="maxScale">The maximum scale value. Must be positive and greater than <paramref name="minScale"/>.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="minScale"/> is less than or equal to zero, <paramref name="maxScale"/> is less than
	/// or equal to zero, or <paramref name="minScale"/> is greater than or equal to <paramref name="maxScale"/>.
	/// </exception>
	public void SetScaleRange(float minScale, float maxScale)
	{
		if (minScale <= 0)
			throw new ArgumentOutOfRangeException(nameof(minScale), minScale, "MinScale must be positive.");
		if (maxScale <= 0)
			throw new ArgumentOutOfRangeException(nameof(maxScale), maxScale, "MaxScale must be positive.");
		if (minScale >= maxScale)
			throw new ArgumentOutOfRangeException(nameof(minScale), minScale, "MinScale must be less than MaxScale.");

		_minScale = minScale;
		_maxScale = maxScale;
	}

	/// <summary>
	/// Gets or sets the multiplicative zoom factor applied when a double-tap is detected.
	/// </summary>
	/// <value>The zoom multiplier per double-tap. The default is <c>2.0</c>. Must be positive.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
	/// <remarks>
	/// When the current scale is at or near <see cref="MaxScale"/>, a double-tap animates
	/// the scale back to <c>1.0</c> instead of zooming further.
	/// </remarks>
	public float DoubleTapZoomFactor
	{
		get => _doubleTapZoomFactor;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "DoubleTapZoomFactor must be positive.");
			_doubleTapZoomFactor = value;
		}
	}

	/// <summary>
	/// Gets or sets the duration of the double-tap zoom animation.
	/// </summary>
	/// <value>The animation duration. The default is <c>250 ms</c>. A value of <see cref="TimeSpan.Zero"/> applies the zoom instantly.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
	public TimeSpan ZoomAnimationDuration
	{
		get => _zoomAnimationDuration;
		set
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ZoomAnimationDuration must not be negative.");
			_zoomAnimationDuration = value;
		}
	}

	/// <summary>
	/// Gets or sets the scale sensitivity for mouse scroll-wheel zoom.
	/// </summary>
	/// <value>
	/// A multiplier applied to each scroll tick's <see cref="SKScrollGestureEventArgs.Delta"/>.Y
	/// to compute the scale change. The default is <c>0.1</c>. Must be positive.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
	public float ScrollZoomFactor
	{
		get => _scrollZoomFactor;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ScrollZoomFactor must be positive.");
			_scrollZoomFactor = value;
		}
	}

	/// <summary>
	/// Gets or sets the fling friction coefficient that controls deceleration speed.
	/// </summary>
	/// <value>
	/// A value between <c>0</c> (no friction, fling continues indefinitely) and <c>1</c> (full friction,
	/// fling stops immediately). The default is <c>0.08</c>.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <c>0</c> or greater than <c>1</c>.</exception>
	public float FlingFriction
	{
		get => _flingFriction;
		set
		{
			if (value < 0 || value > 1)
				throw new ArgumentOutOfRangeException(nameof(value), value, "FlingFriction must be between 0 and 1.");
			_flingFriction = value;
		}
	}

	/// <summary>
	/// Gets or sets the minimum velocity, in pixels per second, below which the fling animation stops.
	/// </summary>
	/// <value>The minimum fling velocity threshold in pixels per second. The default is <c>5</c>.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
	public float FlingMinVelocity
	{
		get => _flingMinVelocity;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "FlingMinVelocity must not be negative.");
			_flingMinVelocity = value;
		}
	}

	/// <summary>
	/// Gets or sets the fling animation frame interval.
	/// </summary>
	/// <value>
	/// The timer interval between fling animation frames.
	/// The default is <c>16 ms</c> (approximately 60 FPS). Must be positive.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
	public TimeSpan FlingFrameInterval
	{
		get => _flingFrameInterval;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "FlingFrameInterval must be positive.");
			_flingFrameInterval = value;
		}
	}

	/// <summary>
	/// Gets or sets the zoom animation frame interval.
	/// </summary>
	/// <value>
	/// The timer interval between zoom animation frames.
	/// The default is <c>16 ms</c> (approximately 60 FPS). Must be positive.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
	public TimeSpan ZoomAnimationInterval
	{
		get => _zoomAnimationInterval;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ZoomAnimationInterval must be positive.");
			_zoomAnimationInterval = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether tap detection is enabled.</summary>
	/// <value><see langword="true"/> to detect single taps; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsTapEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether double-tap detection is enabled.</summary>
	/// <value><see langword="true"/> to detect double taps; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsDoubleTapEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether long press detection is enabled.</summary>
	/// <value><see langword="true"/> to detect long presses; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsLongPressEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether pan gestures update the tracker's offset.</summary>
	/// <value><see langword="true"/> to apply pan deltas to the offset; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsPanEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether pinch-to-zoom gestures update the tracker's scale.</summary>
	/// <value><see langword="true"/> to apply pinch scale changes; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsPinchEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether rotation gestures update the tracker's rotation.</summary>
	/// <value><see langword="true"/> to apply rotation changes; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsRotateEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether fling (inertia) animation is enabled after pan gestures.</summary>
	/// <value><see langword="true"/> to run fling animations; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsFlingEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether double-tap triggers an animated zoom.</summary>
	/// <value><see langword="true"/> to enable double-tap zoom; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsDoubleTapZoomEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether scroll-wheel events trigger zoom.</summary>
	/// <value><see langword="true"/> to enable scroll-wheel zoom; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsScrollZoomEnabled { get; set; } = true;

	/// <summary>Gets or sets a value indicating whether hover (mouse move without contact) detection is enabled.</summary>
	/// <value><see langword="true"/> to detect hover events; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
	public bool IsHoverEnabled { get; set; } = true;
}
