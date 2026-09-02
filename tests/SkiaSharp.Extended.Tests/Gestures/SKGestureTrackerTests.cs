using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>
/// Tests for <see cref="SKGestureTracker"/>.
/// </summary>
public class SKGestureTrackerTests
{
	private long _testTicks = 1000000;

	private SKGestureTracker CreateTracker()
	{
		var tracker = new SKGestureTracker
		{
			TimeProvider = () => _testTicks
		};
		return tracker;
	}

	private void AdvanceTime(long milliseconds)
	{
		_testTicks += milliseconds * TimeSpan.TicksPerMillisecond;
	}

	private void SimulateFastSwipe(SKGestureTracker tracker, SKPoint start, SKPoint end)
	{
		var mid = new SKPoint((start.X + end.X) / 2, (start.Y + end.Y) / 2);
		tracker.ProcessTouchDown(1, start);
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, mid);
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, end);
		AdvanceTime(10);
		tracker.ProcessTouchUp(1, end);
	}

	private void SimulateDoubleTap(SKGestureTracker tracker, SKPoint location)
	{
		tracker.ProcessTouchDown(1, location);
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, location);
		AdvanceTime(100);
		tracker.ProcessTouchDown(1, location);
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, location);
	}


	[Fact]
	public void Pan_UpdatesOffset()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotEqual(SKPoint.Empty, tracker.Offset);
		Assert.True(tracker.Offset.X > 0, $"Offset.X should be > 0, was {tracker.Offset.X}");
	}

	[Fact]
	public void Pan_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var transformChanged = false;
		tracker.TransformChanged += (s, e) => transformChanged = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(transformChanged);
	}

	[Fact]
	public void Pan_OffsetAccumulates()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		var offset1 = tracker.Offset;
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(140, 100));
		var offset2 = tracker.Offset;

		Assert.True(offset2.X > offset1.X, "Offset should accumulate with continued panning");
	}



	[Fact]
	public void Pinch_UpdatesScale()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.True(tracker.Scale > 1.0f, $"Scale should be > 1 after spreading fingers, was {tracker.Scale}");
	}

	[Fact]
	public void Pinch_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.True(changeCount > 0);
	}

	[Fact]
	public void Pinch_ScaleClampedToMinMax()
	{
		var tracker = CreateTracker();
		tracker.Options.MinScale = 0.5f;
		tracker.Options.MaxScale = 3f;

		// Pinch fingers very close together
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(149, 200));
		tracker.ProcessTouchMove(2, new SKPoint(151, 200));

		Assert.True(tracker.Scale >= 0.5f, "Scale should not go below MinScale");
		Assert.True(tracker.Scale <= 3f, "Scale should not exceed MaxScale");
	}



	[Fact]
	public void Rotate_UpdatesRotation()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 250));
		tracker.ProcessTouchMove(2, new SKPoint(200, 150));

		Assert.NotEqual(0f, tracker.Rotation);
	}

	[Fact]
	public void Rotate_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 250));
		tracker.ProcessTouchMove(2, new SKPoint(200, 150));

		Assert.True(changeCount > 0);
	}



	[Fact]
	public void IsPanEnabled_False_DoesNotUpdateOffset()
	{
		var tracker = CreateTracker();
		tracker.IsPanEnabled = false;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));

		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void IsDoubleTapZoomEnabled_False_DoesNotZoom()
	{
		var tracker = CreateTracker();
		tracker.IsDoubleTapZoomEnabled = false;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));

		Assert.Equal(1f, tracker.Scale);
		Assert.False(tracker.IsZoomAnimating);
	}

	[Fact]
	public void IsPinchEnabled_False_DoesNotScale()
	{
		var tracker = CreateTracker();
		tracker.IsPinchEnabled = false;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.Equal(1f, tracker.Scale);
	}

	[Fact]
	public void IsRotateEnabled_False_DoesNotRotate()
	{
		var tracker = CreateTracker();
		tracker.IsRotateEnabled = false;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 250));
		tracker.ProcessTouchMove(2, new SKPoint(200, 150));

		Assert.Equal(0f, tracker.Rotation);
	}

	[Fact]
	public void IsFlingEnabled_False_DoesNotFling()
	{
		var tracker = CreateTracker();
		tracker.IsFlingEnabled = false;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.False(tracker.IsFlinging);
	}

	[Fact]
	public void IsScrollZoomEnabled_False_DoesNotZoomOnScroll()
	{
		var tracker = CreateTracker();
		tracker.IsScrollZoomEnabled = false;

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.Equal(1f, tracker.Scale);
	}



	[Fact]
	public void Reset_RestoresDefaultTransform()
	{
		var tracker = CreateTracker();

		// Apply pan
		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, new SKPoint(150, 100));

		// Apply scroll zoom
		AdvanceTime(500);
		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 2f);

		Assert.NotEqual(1f, tracker.Scale);
		Assert.NotEqual(SKPoint.Empty, tracker.Offset);

		tracker.Reset();

		Assert.Equal(1f, tracker.Scale);
		Assert.Equal(0f, tracker.Rotation);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void Reset_MatrixIsIdentity()
	{
		var tracker = CreateTracker();

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 2f);

		tracker.Reset();

		var m = tracker.Matrix;
		Assert.Equal(1f, m.ScaleX, 0.01);
		Assert.Equal(1f, m.ScaleY, 0.01);
		Assert.Equal(0f, m.TransX, 0.01);
		Assert.Equal(0f, m.TransY, 0.01);
	}

	[Fact]
	public void Reset_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 2f);

		var transformChanged = false;
		tracker.TransformChanged += (s, e) => transformChanged = true;

		tracker.Reset();

		Assert.True(transformChanged);
	}



	[Fact]
	public void TouchSlop_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.TouchSlop = 50;

		var panRaised = false;
		tracker.PanDetected += (s, e) => panRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(130, 100)); // 30px < 50px slop

		Assert.False(panRaised, "Pan should not fire when within custom touch slop");
	}

	[Fact]
	public void DoubleTapSlop_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.DoubleTapSlop = 10;

		var doubleTapRaised = false;
		tracker.DoubleTapDetected += (s, e) => doubleTapRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(100);
		tracker.ProcessTouchDown(1, new SKPoint(120, 120));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(120, 120));

		Assert.False(doubleTapRaised, "Double tap should not fire outside slop distance");
	}

	[Fact]
	public void FlingThreshold_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.FlingThreshold = 50000;

		var flingRaised = false;
		tracker.FlingDetected += (s, e) => flingRaised = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.False(flingRaised, "Fling should not fire with very high threshold");
	}

	[Fact]
	public void IsEnabled_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.IsEnabled = false;

		var result = tracker.ProcessTouchDown(1, new SKPoint(100, 100));

		Assert.False(result);
	}

	[Fact]
	public void LongPressDuration_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.LongPressDuration = TimeSpan.FromMilliseconds(200);
		Assert.Equal(TimeSpan.FromMilliseconds(200), tracker.Options.LongPressDuration);
	}



	[Fact]
	public void Dispose_StopsFlingAnimation()
	{
		var tracker = CreateTracker();

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		Assert.True(tracker.IsFlinging);

		tracker.Dispose();

		Assert.False(tracker.IsFlinging);
	}

	[Fact]
	public void Dispose_StopsZoomAnimation()
	{
		var tracker = CreateTracker();

		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		Assert.True(tracker.IsZoomAnimating);

		tracker.Dispose();

		Assert.False(tracker.IsZoomAnimating);
	}



	[Fact]
	public void TapDetected_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var tapRaised = false;
		tracker.TapDetected += (s, e) => tapRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.True(tapRaised);
	}

	[Fact]
	public void PanDetected_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var panRaised = false;
		tracker.PanDetected += (s, e) => panRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(panRaised);
	}

	[Fact]
	public void ScrollDetected_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var scrollRaised = false;
		tracker.ScrollDetected += (s, e) => scrollRaised = true;

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(scrollRaised);
	}

	[Fact]
	public void GestureStarted_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var gestureStarted = false;
		tracker.GestureStarted += (s, e) => gestureStarted = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));

		Assert.True(gestureStarted);
	}

	[Fact]
	public void GestureEnded_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var gestureEnded = false;
		tracker.GestureEnded += (s, e) => gestureEnded = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchUp(1, new SKPoint(120, 100));

		Assert.True(gestureEnded);
	}



	[Fact]
	public void IsTapEnabled_False_SuppressesTap()
	{
		var tracker = CreateTracker();
		tracker.IsTapEnabled = false;
		var tapFired = false;
		tracker.TapDetected += (s, e) => tapFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(350);

		Assert.False(tapFired);
	}

	[Fact]
	public void IsDoubleTapEnabled_False_SuppressesDoubleTap()
	{
		var tracker = CreateTracker();
		tracker.IsDoubleTapEnabled = false;
		var doubleTapFired = false;
		tracker.DoubleTapDetected += (s, e) => doubleTapFired = true;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		AdvanceTime(350);

		Assert.False(doubleTapFired);
	}

	[Fact]
	public async Task IsLongPressEnabled_False_SuppressesLongPress()
	{
		var tracker = CreateTracker();
		tracker.IsTapEnabled = false;
		tracker.IsLongPressEnabled = false;
		var longPressFired = false;
		tracker.LongPressDetected += (s, e) => longPressFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		await Task.Delay(600);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.False(longPressFired);
	}

	[Fact]
	public void IsHoverEnabled_False_SuppressesHover()
	{
		var tracker = CreateTracker();
		tracker.IsHoverEnabled = false;
		var hoverFired = false;
		tracker.HoverDetected += (s, e) => hoverFired = true;

		tracker.ProcessTouchMove(1, new SKPoint(100, 100), false);

		Assert.False(hoverFired);
	}

	[Fact]
	public void IsTapEnabled_True_AllowsTap()
	{
		var tracker = CreateTracker();
		tracker.IsTapEnabled = true;
		var tapFired = false;
		tracker.TapDetected += (s, e) => tapFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(350);

		Assert.True(tapFired);
	}

	[Fact]
	public void IsHoverEnabled_True_AllowsHover()
	{
		var tracker = CreateTracker();
		tracker.IsHoverEnabled = true;
		var hoverFired = false;
		tracker.HoverDetected += (s, e) => hoverFired = true;

		tracker.ProcessTouchMove(1, new SKPoint(100, 100), false);

		Assert.True(hoverFired);
	}



	[Fact]
	public void PanDetected_HasVelocity()
	{
		var tracker = CreateTracker();
		SKPoint? velocity = null;
		tracker.PanDetected += (s, e) => velocity = e.Velocity;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));

		Assert.NotNull(velocity);
	}



	[Fact]
	public void ConstructorWithOptions_AppliesValues()
	{
		var options = new SKGestureTrackerOptions
		{
			MinScale = 0.5f,
			MaxScale = 5f,
			DoubleTapZoomFactor = 3f,
			ScrollZoomFactor = 0.2f,
			TouchSlop = 16f,
			DoubleTapSlop = 80f,
		};
		var tracker = new SKGestureTracker(options)
		{
			TimeProvider = () => _testTicks
		};

		Assert.Equal(0.5f, tracker.Options.MinScale);
		Assert.Equal(5f, tracker.Options.MaxScale);
		Assert.Equal(3f, tracker.Options.DoubleTapZoomFactor);
		Assert.Equal(0.2f, tracker.Options.ScrollZoomFactor);
		Assert.Equal(16f, tracker.Options.TouchSlop);
		Assert.Equal(80f, tracker.Options.DoubleTapSlop);
	}

	[Fact]
	public void DefaultOptions_HaveExpectedValues()
	{
		var tracker = CreateTracker();

		Assert.Equal(0.1f, tracker.Options.MinScale);
		Assert.Equal(10f, tracker.Options.MaxScale);
		Assert.Equal(2f, tracker.Options.DoubleTapZoomFactor);
		Assert.Equal(0.1f, tracker.Options.ScrollZoomFactor);
		Assert.Equal(8f, tracker.Options.TouchSlop);
		Assert.Equal(40f, tracker.Options.DoubleTapSlop);
	}



	[Fact]
	public void Options_MinScale_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MinScale = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MinScale = -1f);
	}

	[Fact]
	public void Options_MaxScale_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxScale = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxScale = -1f);
	}

	[Fact]
	public void Options_MaxScale_LessThanMinScale_Throws()
	{
		var options = new SKGestureTrackerOptions { MinScale = 1f, MaxScale = 5f };
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxScale = 0.5f);
	}

	[Fact]
	public void Options_MinScale_GreaterThanMaxScale_Throws()
	{
		var options = new SKGestureTrackerOptions { MinScale = 1f, MaxScale = 5f };
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MinScale = 6f);
	}

	[Fact]
	public void Options_DoubleTapZoomFactor_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.DoubleTapZoomFactor = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.DoubleTapZoomFactor = -1f);
	}

	[Fact]
	public void Options_ScrollZoomFactor_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.ScrollZoomFactor = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.ScrollZoomFactor = -1f);
	}

	[Theory]
	[InlineData(-0.1f)]
	[InlineData(1.1f)]
	public void Options_FlingFriction_OutOfRange_Throws(float value)
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingFriction = value);
	}

	[Fact]
	public void Options_FlingMinVelocity_Negative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingMinVelocity = -1f);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Options_FlingFrameInterval_ZeroOrNegative_Throws(int value)
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingFrameInterval = TimeSpan.FromMilliseconds(value));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Options_ZoomAnimationInterval_ZeroOrNegative_Throws(int value)
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.ZoomAnimationInterval = TimeSpan.FromMilliseconds(value));
	}

	[Fact]
	public void Options_ZoomAnimationInterval_DefaultIs16()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Equal(TimeSpan.FromMilliseconds(16), options.ZoomAnimationInterval);
	}

	[Fact]
	public void Options_ZoomAnimationInterval_AcceptsPositiveValue()
	{
		var options = new SKGestureTrackerOptions();
		options.ZoomAnimationInterval = TimeSpan.FromMilliseconds(33);
		Assert.Equal(TimeSpan.FromMilliseconds(33), options.ZoomAnimationInterval);
	}

	[Fact]
	public void Constructor_NullOptions_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new SKGestureTracker(null!));
	}



	[Fact]
	public void Pinch_ScaleDelta_MatchesExpectedRatio()
	{
		var tracker = CreateTracker();

		// Fingers start 100px apart, spread to 200px → expected scale ≈ 2.0
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.Equal(2.0f, tracker.Scale, 2);
	}

	[Fact]
	public void Pinch_PinchIn_HalvesScale()
	{
		var tracker = CreateTracker();

		// Fingers start 200px apart, pinch to 100px → expected scale ≈ 0.5
		tracker.ProcessTouchDown(1, new SKPoint(50, 200));
		tracker.ProcessTouchDown(2, new SKPoint(250, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 200));
		tracker.ProcessTouchMove(2, new SKPoint(200, 200));

		Assert.Equal(0.5f, tracker.Scale, 2);
	}



	[Fact]
	public void PanEventArgs_PreviousLocation_IsCorrect()
	{
		var tracker = CreateTracker();
		SKPanGestureEventArgs? captured = null;
		tracker.PanDetected += (s, e) => captured = e;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotNull(captured);
		Assert.Equal(100, captured.PreviousLocation.X, 1);
		Assert.Equal(100, captured.PreviousLocation.Y, 1);
	}

	[Fact]
	public void PinchEventArgs_ScaleDelta_ProductMatchesCumulativeScale()
	{
		var tracker = CreateTracker();
		var scaleDeltas = new List<float>();
		tracker.PinchDetected += (s, e) => scaleDeltas.Add(e.ScaleDelta);

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.NotEmpty(scaleDeltas);
		var cumulativeScale = 1f;
		foreach (var delta in scaleDeltas)
			cumulativeScale *= delta;
		Assert.Equal(2.0f, cumulativeScale, 2);
	}

	[Fact]
	public void FlingEventArgs_SpeedMatchesVelocityMagnitude()
	{
		var tracker = CreateTracker();
		SKFlingGestureEventArgs? captured = null;
		tracker.FlingDetected += (s, e) => captured = e;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.NotNull(captured);
		var expectedSpeed = (float)Math.Sqrt(captured.Velocity.X * captured.Velocity.X + captured.Velocity.Y * captured.Velocity.Y);
		Assert.Equal(expectedSpeed, captured.Speed, 1);
		tracker.Dispose();
	}



	[Fact]
	public void Dispose_CalledTwice_DoesNotThrow()
	{
		var tracker = CreateTracker();
		tracker.Dispose();
		tracker.Dispose(); // should not throw
	}



	[Theory]
	[InlineData(0f)]
	[InlineData(-1f)]
	[InlineData(-0.001f)]
	public void ZoomTo_WithNonPositiveFactor_ThrowsArgumentOutOfRangeException(float factor)
	{
		var tracker = CreateTracker();
		Assert.Throws<ArgumentOutOfRangeException>(() => tracker.ZoomTo(factor, SKPoint.Empty));
	}

	[Theory]
	[InlineData(float.NaN)]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	public void ZoomTo_WithNonFiniteFactor_ThrowsArgumentOutOfRangeException(float factor)
	{
		var tracker = CreateTracker();
		Assert.Throws<ArgumentOutOfRangeException>(() => tracker.ZoomTo(factor, SKPoint.Empty));
	}

	[Fact]
	public void PinchAndRotate_FiresOnlyOneTransformChangedPerFrame()
	{
		// Each two-finger move should fire exactly one TransformChanged, not two
		// (one from pinch handler + one from rotate handler).
		var tracker = CreateTracker();
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		// Set up two fingers in a position that generates both pinch and rotate
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(300, 200));
		AdvanceTime(10);

		changeCount = 0; // Reset after setup touches
		tracker.ProcessTouchMove(1, new SKPoint(50, 180));
		tracker.ProcessTouchMove(2, new SKPoint(350, 220));

		// Two moves → 2 frames, but each frame (triggered by move 1 AND move 2) should fire once
		// Each ProcessTouchMove call may or may not trigger a frame depending on which finger moves.
		// The important thing is that for each frame where pinch fires, rotate does NOT fire an extra event.
		// With the fix: rotate fires TransformChanged (1 per frame), pinch does not.
		// We can verify by checking the count doesn't double what a pinch-only gesture would produce.
		Assert.True(changeCount <= 2, $"Expected at most 2 TransformChanged (one per move), got {changeCount}");
	}

	[Fact]
	public void ProcessTouchCancel_DuringPinch_WithOneFingerRemaining_TransitionsToPan()
	{
		var tracker = CreateTracker();
		var panDetected = false;
		tracker.PanDetected += (s, e) => panDetected = true;

		// Start a two-finger pinch
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(300, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(80, 200));
		tracker.ProcessTouchMove(2, new SKPoint(320, 200));

		// Cancel one finger
		tracker.ProcessTouchCancel(2);

		// Subsequent move with remaining finger should produce pan
		panDetected = false;
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(60, 200));

		Assert.True(panDetected, "Pan should be detected after one finger cancelled during pinch");
	}



	[Fact]
	public void DragEnded_ReportsActualEndLocation_NotStartLocation()
	{
		// Regression: DragEnded was passing _dragStartLocation as both start and end,
		// so CurrentLocation and Delta were always wrong.
		var tracker = CreateTracker();
		SKDragGestureEventArgs? dragEndedArgs = null;
		tracker.DragEnded += (s, e) => dragEndedArgs = e;

		var startPoint = new SKPoint(100, 100);
		var midPoint = new SKPoint(150, 100);
		var endPoint = new SKPoint(200, 100);

		tracker.ProcessTouchDown(1, startPoint);
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, midPoint);
		AdvanceTime(500); // Long pause to avoid fling
		tracker.ProcessTouchMove(1, endPoint);
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, endPoint);

		Assert.NotNull(dragEndedArgs);
		// Location must reflect the final touch position, not the previous
		Assert.NotEqual(dragEndedArgs!.PreviousLocation, dragEndedArgs.Location);
		Assert.Equal(endPoint.X, dragEndedArgs.Location.X, 1f);
		Assert.Equal(endPoint.Y, dragEndedArgs.Location.Y, 1f);
	}

	[Fact]
	public void FlingCompleted_DoesNotFire_WhenFlingInterruptedByNewGesture()
	{
		// Regression: StopFling() unconditionally raised FlingCompleted, so starting a new
		// gesture while a fling was in progress incorrectly fired FlingCompleted.
		var tracker = CreateTracker();
		tracker.Options.FlingFriction = 0.001f; // Near-zero friction so fling persists
		tracker.Options.FlingMinVelocity = 1f;
		tracker.Options.FlingFrameInterval = TimeSpan.FromSeconds(1); // Slow timer — won't fire during test

		var flingCompletedCount = 0;
		tracker.FlingCompleted += (s, e) => flingCompletedCount++;

		// Start a fling
		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		Assert.True(tracker.IsFlinging, "Fling should be active after fast swipe");

		// Interrupt with a new touch — should NOT fire FlingCompleted
		tracker.ProcessTouchDown(1, new SKPoint(300, 300));

		Assert.Equal(0, flingCompletedCount);
	}

	[Fact]
	public void TimeProvider_SetNull_ThrowsArgumentNullException()
	{
		var tracker = CreateTracker();
		Assert.Throws<ArgumentNullException>(() => tracker.TimeProvider = null!);
	}

	[Fact]
	public void ZoomTo_AfterDispose_ThrowsObjectDisposedException()
	{
		var tracker = CreateTracker();
		tracker.Dispose();
		Assert.Throws<ObjectDisposedException>(() => tracker.ZoomTo(2f, new SKPoint(100, 100)));
	}

	[Fact]
	public void ScrollZoom_LargeNegativeDelta_ClampsScaleDeltaPositive()
	{
		var tracker = CreateTracker();
		tracker.IsScrollZoomEnabled = true;
		var transformFired = false;
		tracker.TransformChanged += (s, e) => transformFired = true;

		// A large negative delta that would make scaleDelta <= 0 without clamping
		tracker.ProcessMouseWheel(new SKPoint(100, 100), 0, -100f);

		Assert.True(transformFired);
		Assert.True(tracker.Scale > 0, "Scale must remain positive");
	}

	[Fact]
	public void ProcessTouchDown_WithNaNCoordinates_DoesNotCorruptState()
	{
		var tracker = CreateTracker();
		tracker.ProcessTouchDown(1, new SKPoint(float.NaN, float.NaN));
		tracker.ProcessTouchUp(1, new SKPoint(float.NaN, float.NaN));

		// State should remain valid
		Assert.False(float.IsNaN(tracker.Scale));
		Assert.False(float.IsNaN(tracker.Offset.X));
		Assert.False(float.IsNaN(tracker.Offset.Y));
	}

	[Fact]
	public void ProcessTouchDown_WithInfinityCoordinates_DoesNotCorruptState()
	{
		var tracker = CreateTracker();
		tracker.ProcessTouchDown(1, new SKPoint(float.PositiveInfinity, float.NegativeInfinity));
		tracker.ProcessTouchUp(1, new SKPoint(float.PositiveInfinity, float.NegativeInfinity));

		Assert.False(float.IsInfinity(tracker.Scale));
		Assert.False(float.IsInfinity(tracker.Offset.X));
	}

	[Fact]
	public void Detector_ProcessTouchDown_AfterDispose_ReturnsFalse()
	{
		var detector = new SKGestureDetector();
		detector.Dispose();
		var result = detector.ProcessTouchDown(1, new SKPoint(100, 100));
		Assert.False(result);
	}

	[Fact]
	public void Detector_ProcessTouchMove_AfterDispose_ReturnsFalse()
	{
		var detector = new SKGestureDetector();
		detector.Dispose();
		var result = detector.ProcessTouchMove(1, new SKPoint(100, 100));
		Assert.False(result);
	}

	[Fact]
	public void Detector_ProcessTouchUp_AfterDispose_ReturnsFalse()
	{
		var detector = new SKGestureDetector();
		detector.Dispose();
		var result = detector.ProcessTouchUp(1, new SKPoint(100, 100));
		Assert.False(result);
	}

	[Fact]
	public void ZeroDistanceTouch_DoesNotTriggerPan()
	{
		var tracker = CreateTracker();
		var panFired = false;
		tracker.PanDetected += (s, e) => panFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		tracker.ProcessTouchMove(1, new SKPoint(100, 100)); // same point
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.False(panFired);
	}

	[Fact]
	public void Reset_ClearsAllTransformState()
	{
		var tracker = CreateTracker();
		tracker.SetTransform(2f, 45f, new SKPoint(50, 50));

		Assert.NotEqual(1f, tracker.Scale);
		Assert.NotEqual(0f, tracker.Rotation);

		tracker.Reset();

		Assert.Equal(1f, tracker.Scale);
		Assert.Equal(0f, tracker.Rotation);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void SetScale_ClampsToMinMax()
	{
		var tracker = new SKGestureTracker
		{
			TimeProvider = () => _testTicks
		};
		tracker.Options.MinScale = 0.5f;
		tracker.Options.MaxScale = 3f;

		tracker.SetScale(10f);
		Assert.Equal(3f, tracker.Scale);

		tracker.SetScale(0.1f);
		Assert.Equal(0.5f, tracker.Scale);
	}

	[Fact]
	public void PinchAndPan_Simultaneously_BothApply()
	{
		var tracker = CreateTracker();
		var panFired = false;
		var pinchFired = false;
		tracker.PanDetected += (s, e) => panFired = true;
		tracker.PinchDetected += (s, e) => pinchFired = true;

		// Two finger down
		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		tracker.ProcessTouchDown(2, new SKPoint(200, 100));

		// Move both fingers apart and to the right (pinch + pan)
		tracker.ProcessTouchMove(1, new SKPoint(80, 100));
		tracker.ProcessTouchMove(2, new SKPoint(250, 100));

		Assert.True(pinchFired);
		// Pan during pinch should also update offset
		Assert.NotEqual(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void IsPinchEnabled_False_PanEnabled_True_PinchDetected_NotRaised()
	{
		// Bug fix: PinchDetected was incorrectly raised when only IsPanEnabled=true
		var tracker = CreateTracker();
		tracker.IsPinchEnabled = false;
		tracker.IsPanEnabled = true;
		var pinchFired = false;
		tracker.PinchDetected += (s, e) => pinchFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		tracker.ProcessTouchDown(2, new SKPoint(200, 100));
		tracker.ProcessTouchMove(1, new SKPoint(50, 100));
		tracker.ProcessTouchMove(2, new SKPoint(250, 100));

		Assert.False(pinchFired, "PinchDetected must not fire when IsPinchEnabled is false");
	}

	[Fact]
	public void SetScaleRange_SetsMinAndMaxAtomically()
	{
		var options = new SKGestureTrackerOptions();
		options.SetScaleRange(15f, 20f);

		Assert.Equal(15f, options.MinScale);
		Assert.Equal(20f, options.MaxScale);
	}

	[Fact]
	public void SetScaleRange_InvalidOrder_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.SetScaleRange(20f, 15f));
	}

	[Fact]
	public void SetScaleRange_EqualValues_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.SetScaleRange(5f, 5f));
	}

	[Fact]
	public void SetScaleRange_NegativeMin_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.SetScaleRange(-1f, 10f));
	}
	[Fact]
	public void PinchWithPanDisabled_FingersTranslate_OffsetDoesNotChange()
	{
		// Bug fix: when IsPanEnabled=false, moving the finger midpoint during a pinch
		// (translation, no scale change) should not cause the content to pan.
		var tracker = CreateTracker();
		tracker.IsPanEnabled = false;
		tracker.IsPinchEnabled = true;
		tracker.IsRotateEnabled = false; // isolate pinch-only behavior

		var initialOffset = tracker.Offset;

		// Both fingers go down 100px apart
		tracker.ProcessTouchDown(1, new SKPoint(150, 200));
		tracker.ProcessTouchDown(2, new SKPoint(250, 200));

		// Both fingers translate upward together (same distance = scale delta 1.0)
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));
		tracker.ProcessTouchMove(2, new SKPoint(250, 100));

		// Translate further
		tracker.ProcessTouchMove(1, new SKPoint(150, 50));
		tracker.ProcessTouchMove(2, new SKPoint(250, 50));

		// Offset must not change -- pure translation with pan disabled should be ignored
		Assert.Equal(initialOffset.X, tracker.Offset.X, 1f);
		Assert.Equal(initialOffset.Y, tracker.Offset.Y, 1f);
	}

	[Fact]
	public void RotateWithPanDisabled_FingersTranslate_OffsetDoesNotChange()
	{
		// Bug fix: when IsPanEnabled=false, moving the finger midpoint during rotation
		// should not cause content drift -- the pivot must be locked via GetEffectiveGesturePivot.
		var tracker = CreateTracker();
		tracker.IsPanEnabled = false;
		tracker.IsPinchEnabled = false; // isolate rotation-only behavior
		tracker.IsRotateEnabled = true;

		var initialOffset = tracker.Offset;

		// Both fingers down, equidistant from center
		tracker.ProcessTouchDown(1, new SKPoint(150, 200));
		tracker.ProcessTouchDown(2, new SKPoint(250, 200));

		// Both fingers translate upward together (no scale change, some rotation)
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));
		tracker.ProcessTouchMove(2, new SKPoint(250, 100));

		// Translate further
		tracker.ProcessTouchMove(1, new SKPoint(150, 50));
		tracker.ProcessTouchMove(2, new SKPoint(250, 50));

		// Offset must not change -- translation with pan disabled should be ignored
		Assert.Equal(initialOffset.X, tracker.Offset.X, 1f);
		Assert.Equal(initialOffset.Y, tracker.Offset.Y, 1f);
	}

}
