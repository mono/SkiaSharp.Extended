using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Configuration options for <see cref="SKGestureTracker"/>.
/// Inherits engine-level options and adds tracker-specific settings.
/// </summary>
public class SKGestureTrackerOptions : SKGestureDetectorOptions
{
	private float _minScale = 0.1f;
	private float _maxScale = 10f;
	private float _doubleTapZoomFactor = 2f;
	private int _zoomAnimationDuration = 250;
	private float _scrollZoomFactor = 0.1f;
	private float _flingFriction = 0.08f;
	private float _flingMinVelocity = 5f;
	private int _flingFrameInterval = 16;

	/// <summary>
	/// Gets or sets the minimum allowed scale.
	/// </summary>
	public float MinScale
	{
		get => _minScale;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MinScale must be positive.");
			_minScale = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum allowed scale.
	/// </summary>
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
	/// Gets or sets the zoom factor applied per double-tap.
	/// </summary>
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
	/// Gets or sets the zoom animation duration in milliseconds.
	/// </summary>
	public int ZoomAnimationDuration
	{
		get => _zoomAnimationDuration;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ZoomAnimationDuration must not be negative.");
			_zoomAnimationDuration = value;
		}
	}

	/// <summary>
	/// Gets or sets how much each scroll tick changes scale.
	/// </summary>
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
	/// Gets or sets the fling friction (0 = no friction / infinite fling, 1 = full friction / no fling).
	/// </summary>
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
	/// Gets or sets the minimum fling velocity before the animation stops.
	/// </summary>
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
	/// Gets or sets the fling animation frame interval in milliseconds.
	/// </summary>
	public int FlingFrameInterval
	{
		get => _flingFrameInterval;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "FlingFrameInterval must be positive.");
			_flingFrameInterval = value;
		}
	}
}
