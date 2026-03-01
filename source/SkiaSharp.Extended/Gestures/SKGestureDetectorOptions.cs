using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Configuration options for <see cref="SKGestureDetector"/>.
/// </summary>
public class SKGestureDetectorOptions
{
	private float _touchSlop = 8f;
	private float _doubleTapSlop = 40f;
	private float _flingThreshold = 200f;
	private int _longPressDuration = 500;

	/// <summary>
	/// Gets or sets the touch slop (minimum movement distance to start a gesture).
	/// </summary>
	public float TouchSlop
	{
		get => _touchSlop;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "TouchSlop must not be negative.");
			_touchSlop = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum distance between two taps for double-tap detection.
	/// </summary>
	public float DoubleTapSlop
	{
		get => _doubleTapSlop;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "DoubleTapSlop must not be negative.");
			_doubleTapSlop = value;
		}
	}

	/// <summary>
	/// Gets or sets the fling velocity threshold in pixels per second.
	/// </summary>
	public float FlingThreshold
	{
		get => _flingThreshold;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "FlingThreshold must not be negative.");
			_flingThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the long press duration in milliseconds.
	/// </summary>
	public int LongPressDuration
	{
		get => _longPressDuration;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "LongPressDuration must be positive.");
			_longPressDuration = value;
		}
	}
}
