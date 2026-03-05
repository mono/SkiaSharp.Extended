using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Configuration options for the <see cref="SKGestureDetector"/> gesture recognition engine.
/// </summary>
/// <remarks>
/// <para>These options control the thresholds and timing used to classify touch input into discrete
/// gesture types. Adjust these values to fine-tune gesture sensitivity for your application.</para>
/// <seealso cref="SKGestureDetector"/>
/// <seealso cref="SKGestureTrackerOptions"/>
/// </remarks>
public class SKGestureDetectorOptions
{
	private float _touchSlop = 8f;
	private float _doubleTapSlop = 40f;
	private float _flingThreshold = 200f;
	private TimeSpan _longPressDuration = TimeSpan.FromMilliseconds(500);

	/// <summary>
	/// Gets or sets the minimum movement distance, in pixels, before a touch is considered a pan gesture.
	/// </summary>
	/// <value>The touch slop distance in pixels. The default is <c>8</c>.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
	/// <remarks>
	/// Touches that move less than this distance are classified as taps or long presses rather than pans.
	/// </remarks>
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
	/// Gets or sets the maximum distance, in pixels, between two taps for them to be recognized
	/// as a double-tap gesture.
	/// </summary>
	/// <value>The double-tap slop distance in pixels. The default is <c>40</c>.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
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
	/// Gets or sets the minimum velocity, in pixels per second, required for a pan gesture
	/// to be classified as a fling upon touch release.
	/// </summary>
	/// <value>The fling velocity threshold in pixels per second. The default is <c>200</c>.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
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
	/// Gets or sets the duration a touch must be held stationary before a long press gesture is recognized.
	/// </summary>
	/// <value>The long press duration. The default is <c>500 ms</c>. Must be positive.</value>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
	public TimeSpan LongPressDuration
	{
		get => _longPressDuration;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "LongPressDuration must be positive.");
			_longPressDuration = value;
		}
	}
}
