using SkiaSharp;
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for custom pan handling in <see cref="SKGestureTracker"/>.</summary>
public class SKGestureTrackerPanHandlingTests
{
	private readonly FakeGestureClock _clock = new(1000000);

	private SKGestureTracker CreateTracker()
	{
		return new SKGestureTracker
		{
			Clock = _clock
		};
	}

	private void AdvanceTime(long milliseconds)
	{
		_clock.Advance(TimeSpan.FromMilliseconds(milliseconds));
	}

	[Fact]
	public void HandledPan_DoesNotUpdateOffset()
	{
		var tracker = CreateTracker();
		tracker.PanDetected += (s, e) => e.Handled = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void HandledPan_SuppressesViewportMovementForRestOfGesture()
	{
		var tracker = CreateTracker();
		var first = true;
		tracker.PanDetected += (s, e) =>
		{
			if (first)
			{
				e.Handled = true;
				first = false;
			}
		};

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(160, 100));

		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void HandledPan_SuppressesFlingAnimation()
	{
		var tracker = CreateTracker();
		tracker.PanDetected += (s, e) => e.Handled = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(200, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(300, 100));
		AdvanceTime(10);
		tracker.ProcessTouchUp(1, new SKPoint(300, 100));

		Assert.False(tracker.IsFlinging);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void NewGesture_ClearsHandledPanState()
	{
		var tracker = CreateTracker();
		EventHandler<SKPanGestureEventArgs> handler = (s, e) => e.Handled = true;
		tracker.PanDetected += handler;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, new SKPoint(120, 100));

		tracker.PanDetected -= handler;
		tracker.ProcessTouchDown(2, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(2, new SKPoint(140, 100));

		Assert.NotEqual(SKPoint.Empty, tracker.Offset);
	}
}
