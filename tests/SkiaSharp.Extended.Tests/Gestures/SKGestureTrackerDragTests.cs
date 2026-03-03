using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for drag lifecycle in <see cref="SKGestureTracker"/>.</summary>
public class SKGestureTrackerDragTests
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
	public void FirstPan_FiresDragStarted()
	{
		var tracker = CreateTracker();
		var dragStarted = false;
		tracker.DragStarted += (s, e) => dragStarted = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(dragStarted);
	}

	[Fact]
	public void SubsequentPan_FiresDragUpdated()
	{
		var tracker = CreateTracker();
		var dragUpdatedCount = 0;
		tracker.DragUpdated += (s, e) => dragUpdatedCount++;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(140, 100));

		Assert.True(dragUpdatedCount > 0);
	}

	[Fact]
	public void GestureEnd_FiresDragEnded()
	{
		var tracker = CreateTracker();
		var dragEnded = false;
		tracker.DragEnded += (s, e) => dragEnded = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(500); // Long pause to avoid fling
		tracker.ProcessTouchUp(1, new SKPoint(120, 100));

		Assert.True(dragEnded);
	}

	[Fact]
	public void DragStarted_HasCorrectStartLocation()
	{
		var tracker = CreateTracker();
		SKPoint? startLocation = null;
		tracker.DragStarted += (s, e) => startLocation = e.Location;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotNull(startLocation);
		Assert.Equal(120, startLocation.Value.X, 1);
		Assert.Equal(100, startLocation.Value.Y, 1);
	}

	[Fact]
	public void DragLifecycle_CorrectOrder()
	{
		var tracker = CreateTracker();
		var events = new List<string>();
		tracker.DragStarted += (s, e) => events.Add("started");
		tracker.DragUpdated += (s, e) => events.Add("updated");
		tracker.DragEnded += (s, e) => events.Add("ended");

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(140, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(160, 100));
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, new SKPoint(160, 100));

		Assert.Equal("started", events[0]);
		Assert.Equal("ended", events[^1]);
		Assert.True(events.Count >= 3);
	}



	[Fact]
	public async Task DragHandled_SuppressesFlingAnimation()
	{
		var tracker = CreateTracker();
		tracker.DragStarted += (s, e) => e.Handled = true;
		tracker.DragUpdated += (s, e) => e.Handled = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 100), new SKPoint(300, 100));
		var offsetAfterSwipe = tracker.Offset;

		// Wait for potential fling animation
		await Task.Delay(100);

		// Offset should not have changed — fling animation was suppressed
		Assert.Equal(offsetAfterSwipe, tracker.Offset);
	}


}
