using SkiaSharp;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for double-tap zoom and scroll zoom in <see cref="SKGestureTracker"/>.</summary>
public class SKGestureTrackerZoomScrollTests
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

	#region Zoom Animation (Double-Tap) Tests

	[Fact]
	public void DoubleTap_StartsZoomAnimation()
	{
		var tracker = CreateTracker();

		SimulateDoubleTap(tracker, new SKPoint(200, 200));

		Assert.True(tracker.IsZoomAnimating);
		tracker.Dispose();
	}

	[Fact]
	public async Task DoubleTap_ScaleChangesToDoubleTapZoomFactor()
	{
		var tracker = CreateTracker();
		tracker.Options.DoubleTapZoomFactor = 2f;
		tracker.Options.ZoomAnimationDuration = 100;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));

		// Advance test time past animation duration so timer tick sees it as complete
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);

		Assert.Equal(2f, tracker.Scale, 0.1);
		tracker.Dispose();
	}

	[Fact]
	public async Task DoubleTap_AtMaxScale_ResetsToOne()
	{
		var tracker = CreateTracker();
		tracker.Options.DoubleTapZoomFactor = 2f;
		tracker.Options.MaxScale = 2f;
		tracker.Options.ZoomAnimationDuration = 100;

		// First double tap: zoom to 2x
		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);
		Assert.Equal(2f, tracker.Scale, 0.1);

		// Second double tap at max: should reset to 1.0
		AdvanceTime(500);
		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);
		Assert.Equal(1f, tracker.Scale, 0.1);

		tracker.Dispose();
	}

	[Fact]
	public async Task DoubleTap_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		tracker.Options.ZoomAnimationDuration = 100;
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);

		Assert.True(changeCount > 0, "TransformChanged should fire during zoom animation");
		tracker.Dispose();
	}

	#endregion

	#region Scroll Zoom Tests

	[Fact]
	public void ScrollUp_IncreasesScale()
	{
		var tracker = CreateTracker();

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(tracker.Scale > 1.0f, $"Scale should increase on scroll up, was {tracker.Scale}");
	}

	[Fact]
	public void ScrollDown_DecreasesScale()
	{
		var tracker = CreateTracker();

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, -1f);

		Assert.True(tracker.Scale < 1.0f, $"Scale should decrease on scroll down, was {tracker.Scale}");
	}

	[Fact]
	public void Scroll_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var transformChanged = false;
		tracker.TransformChanged += (s, e) => transformChanged = true;

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(transformChanged);
	}

	[Fact]
	public void Scroll_ScaleClampedToMinMax()
	{
		var tracker = CreateTracker();
		tracker.Options.MinScale = 0.5f;
		tracker.Options.MaxScale = 3f;

		for (int i = 0; i < 100; i++)
			tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, -1f);

		Assert.True(tracker.Scale >= 0.5f, "Scale should not go below MinScale");

		for (int i = 0; i < 200; i++)
			tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(tracker.Scale <= 3f, "Scale should not exceed MaxScale");
	}

	#endregion

}
