using SkiaSharp;
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for fling detection and animation in <see cref="SKGestureDetector"/>.</summary>
public class SKGestureDetectorFlingTests
{
	private long _testTicks = 1000000;

	private SKGestureDetector CreateEngine()
	{
		var engine = new SKGestureDetector
		{
			TimeProvider = () => _testTicks
		};
		return engine;
	}

	private void AdvanceTime(long milliseconds)
	{
		_testTicks += milliseconds * TimeSpan.TicksPerMillisecond;
	}


	[Fact]
	public void FastSwipe_RaisesFlingDetected()
	{
		var engine = CreateEngine();
		var flingRaised = false;
		engine.FlingDetected += (s, e) => flingRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(500, 100)); // Fast movement
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(500, 100));

		Assert.True(flingRaised);
	}

	[Fact]
	public void FlingDetected_VelocityIsCorrect()
	{
		var engine = CreateEngine();
		float? velocityX = null;
		engine.FlingDetected += (s, e) => velocityX = e.Velocity.X;

		// Start and immediately make fast movements
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(200, 100)); // Move 100 px
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(600, 100)); // Move 400 px in 10ms = fast
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(600, 100));

		Assert.NotNull(velocityX);
		// Movement should be fast enough to trigger fling
		Assert.True(velocityX.Value > 200, $"VelocityX should be > 200, was {velocityX.Value}");
	}

	[Fact]
	public void SlowSwipe_DoesNotRaiseFling()
	{
		var engine = CreateEngine();
		var flingRaised = false;
		engine.FlingDetected += (s, e) => flingRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(500);
		engine.ProcessTouchMove(1, new SKPoint(110, 100));
		AdvanceTime(500);
		engine.ProcessTouchUp(1, new SKPoint(110, 100));

		Assert.False(flingRaised);
	}



	[Fact]
	public void Fling_PauseBeforeRelease_NoFling()
	{
		var engine = CreateEngine();
		var flingRaised = false;
		engine.FlingDetected += (s, e) => flingRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(500, 100)); // Fast move
		AdvanceTime(500); // Long pause — velocity decays past threshold window
		engine.ProcessTouchUp(1, new SKPoint(500, 100));

		Assert.False(flingRaised, "Fling should not fire after a long pause");
	}

	[Fact]
	public void Fling_VerticalDirection_CorrectVelocity()
	{
		var engine = CreateEngine();
		float? velocityX = null, velocityY = null;
		engine.FlingDetected += (s, e) => { velocityX = e.Velocity.X; velocityY = e.Velocity.Y; };

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 150));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 400));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 700));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(100, 700));

		Assert.NotNull(velocityY);
		Assert.True(velocityY.Value > 200, $"VelocityY should be > 200, was {velocityY.Value}");
		Assert.True(Math.Abs(velocityX!.Value) < Math.Abs(velocityY.Value),
			"Horizontal velocity should be less than vertical for vertical fling");
	}

	[Fact]
	public void Fling_DiagonalDirection_BothAxesHaveVelocity()
	{
		var engine = CreateEngine();
		float? velocityX = null, velocityY = null;
		engine.FlingDetected += (s, e) => { velocityX = e.Velocity.X; velocityY = e.Velocity.Y; };

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(200, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(400, 400));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(700, 700));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(700, 700));

		Assert.NotNull(velocityX);
		Assert.NotNull(velocityY);
		Assert.True(velocityX.Value > 200);
		Assert.True(velocityY.Value > 200);
	}



	[Fact]
	public void FlingDetected_StillFiresOnceAtStart()
	{
		var engine = CreateEngine();
		var flingDetectedCount = 0;
		engine.FlingDetected += (s, e) => flingDetectedCount++;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(500, 100));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(500, 100));

		Assert.Equal(1, flingDetectedCount);
	}


}
