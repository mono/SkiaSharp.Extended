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
		tracker.Options.FlingFrameInterval = TimeSpan.FromMilliseconds(16);
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
		tracker.Options.FlingFrameInterval = TimeSpan.FromMilliseconds(16);
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
		var tracker = CreateTracker();
		tracker.Options.FlingFrameInterval = TimeSpan.FromMilliseconds(16);
		tracker.Options.FlingFriction = 0.5f;
		tracker.Options.FlingMinVelocity = 100f;

		// Advance fake time on each fling frame so HandleFlingFrame computes a valid
		// actualDtMs (matching the nominal frame interval). With friction=0.5 this halves
		// velocity each frame, so a high-velocity fling will drop below FlingMinVelocity
		// in ~7 frames regardless of real wall-clock scheduling.
		tracker.FlingUpdated += (s, e) => AdvanceTime(16);

		var tcs = new TaskCompletionSource<bool>();
		tracker.FlingCompleted += (s, e) => tcs.TrySetResult(true);

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		Assert.True(tracker.IsFlinging, "Fling should start after fast swipe");

		var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
		Assert.True(tcs.Task.IsCompletedSuccessfully, "Fling should complete within 5 seconds");
		Assert.False(tracker.IsFlinging);
		tracker.Dispose();
	}


}
