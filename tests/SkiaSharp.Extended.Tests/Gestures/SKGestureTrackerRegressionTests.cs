using SkiaSharp;
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

public class SKGestureTrackerRegressionTests
{
	private readonly FakeGestureClock _clock = new(1000000);

	private SKGestureTracker CreateTracker(SKGestureTrackerOptions? options = null)
	{
		var tracker = new SKGestureTracker(options ?? new SKGestureTrackerOptions())
		{
			Clock = _clock
		};
		return tracker;
	}

	private void AdvanceTime(long milliseconds) =>
		_clock.Advance(TimeSpan.FromMilliseconds(milliseconds));

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

	private void SimulateFastSwipe(SKGestureTracker tracker)
	{
		tracker.ProcessTouchDown(1, new SKPoint(0, 0));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 0));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(200, 0));
		AdvanceTime(10);
		tracker.ProcessTouchUp(1, new SKPoint(200, 0));
	}

	[Fact]
	public void DoubleTapHandler_DisposesTracker_DoesNotStartZoom()
	{
		var tracker = CreateTracker();
		tracker.DoubleTapDetected += (_, _) => tracker.Dispose();

		SimulateDoubleTap(tracker, new SKPoint(100, 100));

		Assert.False(tracker.IsZoomAnimating);
		AdvanceTime(500);
	}

	[Fact]
	public void FlingHandler_DisposesTracker_DoesNotStartAnimation()
	{
		var tracker = CreateTracker();
		tracker.FlingDetected += (_, _) => tracker.Dispose();

		SimulateFastSwipe(tracker);

		Assert.False(tracker.IsFlinging);
		AdvanceTime(500);
	}

	[Fact]
	public void DisablingTracker_AbandonsContactsAndAnimationsWithoutChangingTransform()
	{
		var tracker = CreateTracker();
		tracker.SetTransform(2f, 15f, new SKPoint(20, 30));
		var longPressCount = 0;
		tracker.LongPressDetected += (_, _) => longPressCount++;
		tracker.ProcessTouchDown(1, new SKPoint(50, 50));
		tracker.ZoomTo(1.5f, new SKPoint(100, 100));

		tracker.IsEnabled = false;
		AdvanceTime(1000);

		Assert.False(tracker.IsGestureActive);
		Assert.False(tracker.IsZoomAnimating);
		Assert.Equal(0, longPressCount);
		Assert.Equal(2f, tracker.Scale);
		Assert.Equal(15f, tracker.Rotation);
		Assert.Equal(new SKPoint(20, 30), tracker.Offset);
		Assert.False(tracker.ProcessTouchUp(1, new SKPoint(50, 50)));
	}

	[Fact]
	public void FlingUpdatedHandler_ResetIsNotOverwrittenByStaleFrame()
	{
		var tracker = CreateTracker();
		tracker.FlingUpdated += (_, _) => tracker.Reset();
		SimulateFastSwipe(tracker);

		AdvanceTime(16);

		Assert.False(tracker.IsFlinging);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
		Assert.Equal(1f, tracker.Scale);
	}

	[Fact]
	public void CompletedZoomFrame_DoesNotCancelReplacementAnimation()
	{
		var tracker = CreateTracker();
		tracker.Options.ZoomAnimationDuration = TimeSpan.FromMilliseconds(100);
		var replacementStarted = false;
		tracker.TransformChanged += (_, _) =>
		{
			if (!replacementStarted && tracker.Scale >= 1.99f)
			{
				replacementStarted = true;
				tracker.ZoomTo(0.5f, new SKPoint(100, 100));
			}
		};

		tracker.ZoomTo(2f, new SKPoint(100, 100));
		AdvanceTime(120);

		Assert.True(replacementStarted);
		Assert.True(tracker.IsZoomAnimating);

		AdvanceTime(200);
		Assert.False(tracker.IsZoomAnimating);
		Assert.Equal(1f, tracker.Scale, 2);
	}

	[Fact]
	public void PanDisabled_DoesNotStartReleaseFling()
	{
		var tracker = CreateTracker();
		tracker.IsPanEnabled = false;
		var flingDetected = false;
		tracker.FlingDetected += (_, _) => flingDetected = true;

		SimulateFastSwipe(tracker);

		Assert.False(flingDetected);
		Assert.False(tracker.IsFlinging);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void PanDisabledDuringFling_StopsBeforeAnotherFrame()
	{
		var tracker = CreateTracker();
		SimulateFastSwipe(tracker);
		var offset = tracker.Offset;

		tracker.Options.IsPanEnabled = false;
		AdvanceTime(16);

		Assert.False(tracker.IsFlinging);
		Assert.Equal(offset, tracker.Offset);
	}

	[Fact]
	public void NumericOptions_RejectNonFiniteValues()
	{
		var values = new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity };

		foreach (var value in values)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().TouchSlop = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().DoubleTapSlop = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().FlingThreshold = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().MinScale = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().MaxScale = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().DoubleTapZoomFactor = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().ScrollZoomFactor = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().FlingFriction = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().FlingMinVelocity = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().SetScaleRange(value, 2f));
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKGestureTrackerOptions().SetScaleRange(1f, value));
		}
	}

	[Fact]
	public void FlingMinVelocity_Zero_IsAcceptedAndCompletesAtRest()
	{
		var tracker = CreateTracker();
		tracker.Options.FlingMinVelocity = 0f;
		tracker.Options.FlingFriction = 1f;

		SimulateFastSwipe(tracker);
		AdvanceTime(16);

		Assert.False(tracker.IsFlinging);
	}

	[Fact]
	public void InitialAndResetScale_ClampToConfiguredRange()
	{
		var options = new SKGestureTrackerOptions();
		options.SetScaleRange(2f, 4f);
		var tracker = CreateTracker(options);

		Assert.Equal(2f, tracker.Scale);

		tracker.SetScale(3f);
		tracker.Reset();

		Assert.Equal(2f, tracker.Scale);
		Assert.Equal(0f, tracker.Rotation);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void Reset_ClearsLockedGesturePivot()
	{
		var tracker = CreateTracker();
		tracker.IsPanEnabled = false;
		ApplyPinch(tracker, 100);
		tracker.Reset();
		ApplyPinch(tracker, 400);

		var freshTracker = CreateTracker();
		freshTracker.IsPanEnabled = false;
		ApplyPinch(freshTracker, 400);

		Assert.Equal(freshTracker.Scale, tracker.Scale, 3);
		Assert.Equal(freshTracker.Offset.X, tracker.Offset.X, 3);
		Assert.Equal(freshTracker.Offset.Y, tracker.Offset.Y, 3);
	}

	[Fact]
	public void ZeroDurationZoom_AppliesSynchronously()
	{
		var tracker = CreateTracker();
		tracker.Options.ZoomAnimationDuration = TimeSpan.Zero;
		var transformChanges = 0;
		tracker.TransformChanged += (_, _) => transformChanges++;

		tracker.ZoomTo(2f, new SKPoint(100, 100));

		Assert.Equal(2f, tracker.Scale);
		Assert.False(tracker.IsZoomAnimating);
		Assert.Equal(1, transformChanges);
	}

	private static void ApplyPinch(SKGestureTracker tracker, float centerX)
	{
		tracker.ProcessTouchDown(1, new SKPoint(centerX - 50, 100));
		tracker.ProcessTouchDown(2, new SKPoint(centerX + 50, 100));
		tracker.ProcessTouchMove(1, new SKPoint(centerX - 100, 100));
		tracker.ProcessTouchMove(2, new SKPoint(centerX + 100, 100));
		tracker.ProcessTouchUp(2, new SKPoint(centerX + 100, 100));
		tracker.ProcessTouchUp(1, new SKPoint(centerX - 100, 100));
	}
}
