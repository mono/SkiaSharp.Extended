using SkiaSharp;
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for pan detection in <see cref="SKGestureDetector"/>.</summary>
public class SKGestureDetectorPanTests
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

	#region Pan Detection Tests

	[Fact]
	public void MoveBeyondTouchSlop_RaisesPanDetected()
	{
		var engine = CreateEngine();
		var panRaised = false;
		engine.PanDetected += (s, e) => panRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100)); // Move 20 pixels

		Assert.True(panRaised);
	}

	[Fact]
	public void PanDetected_DeltaIsCorrect()
	{
		var engine = CreateEngine();
		SKPoint? delta = null;
		engine.PanDetected += (s, e) => delta = e.Delta;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100)); // First move starts pan
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(130, 110)); // Second move has delta

		Assert.NotNull(delta);
		Assert.Equal(10, delta.Value.X, 0.1);
		Assert.Equal(10, delta.Value.Y, 0.1);
	}

	[Fact]
	public void MoveWithinTouchSlop_DoesNotRaisePan()
	{
		var engine = CreateEngine();
		var panRaised = false;
		engine.PanDetected += (s, e) => panRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(102, 101)); // Move 2 pixels

		Assert.False(panRaised);
	}

	#endregion

}
