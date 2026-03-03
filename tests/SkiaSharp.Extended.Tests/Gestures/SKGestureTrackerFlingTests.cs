using SkiaSharp;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for fling animation in <see cref="SKGestureTracker"/>.</summary>
public class SKGestureTrackerFlingTests
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


	[Fact]
	public void FastSwipe_FiresFlingDetected()
	{
		var tracker = CreateTracker();
		var flingDetected = false;
		tracker.FlingDetected += (s, e) => flingDetected = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.True(flingDetected);
		tracker.Dispose();
	}

	[Fact]
	public async Task Fling_FiresFlingUpdatedEvents()
	{
		var tracker = CreateTracker();
		tracker.Options.FlingFrameInterval = 16;
		var flingUpdatedCount = 0;
		tracker.FlingUpdated += (s, e) => flingUpdatedCount++;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		await Task.Delay(200);

		Assert.True(flingUpdatedCount > 0, $"FlingUpdated should have fired, count was {flingUpdatedCount}");
		tracker.Dispose();
	}

	[Fact]
	public async Task Fling_UpdatesOffset()
	{
		var tracker = CreateTracker();
		tracker.Options.FlingFrameInterval = 16;
		var flingUpdatedFired = false;
		tracker.FlingUpdated += (s, e) => flingUpdatedFired = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		var offsetAfterSwipe = tracker.Offset;

		await Task.Delay(200);

		Assert.True(flingUpdatedFired, "FlingUpdated event should have fired");
		Assert.True(tracker.Offset.X > offsetAfterSwipe.X || !tracker.IsFlinging,
			$"Offset should move during fling or fling already completed");
		tracker.Dispose();
	}

	[Fact]
	public void StopFling_StopsAnimation()
	{
		var tracker = CreateTracker();

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		Assert.True(tracker.IsFlinging);

		tracker.StopFling();

		Assert.False(tracker.IsFlinging);
		tracker.Dispose();
	}

	[Fact]
	public void StopFling_FiresFlingCompleted()
	{
		var tracker = CreateTracker();
		var flingCompleted = false;
		tracker.FlingCompleted += (s, e) => flingCompleted = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		tracker.StopFling();

		Assert.True(flingCompleted);
		tracker.Dispose();
	}

	[Fact]
	public async Task Fling_EventuallyCompletes()
	{
		// Use real TimeProvider so fling frame timing advances with wall-clock time
		var tracker = new SKGestureTracker();
		tracker.Options.FlingFrameInterval = 16;
		tracker.Options.FlingFriction = 0.5f;
		tracker.Options.FlingMinVelocity = 100f;
		var flingCompleted = false;
		tracker.FlingCompleted += (s, e) => flingCompleted = true;

		// Use the tracker's own TimeProvider for touch timestamps so velocity is computed correctly
		var start = new SKPoint(100, 200);
		var mid = new SKPoint(300, 200);
		var end = new SKPoint(500, 200);
		tracker.ProcessTouchDown(1, start);
		await Task.Delay(20);
		tracker.ProcessTouchMove(1, mid);
		await Task.Delay(20);
		tracker.ProcessTouchMove(1, end);
		await Task.Delay(20);
		tracker.ProcessTouchUp(1, end);

		await Task.Delay(2000);

		Assert.True(flingCompleted, "Fling should eventually complete");
		Assert.False(tracker.IsFlinging);
		tracker.Dispose();
	}


}
