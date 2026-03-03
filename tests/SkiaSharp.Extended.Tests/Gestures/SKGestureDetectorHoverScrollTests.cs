using SkiaSharp;
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for hover and mouse wheel detection in <see cref="SKGestureDetector"/>.</summary>
public class SKGestureDetectorHoverScrollTests
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

	#region Hover Detection Tests

	[Fact]
	public void MoveWithoutContact_RaisesHoverDetected()
	{
		var engine = CreateEngine();
		var hoverRaised = false;
		engine.HoverDetected += (s, e) => hoverRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 150), inContact: false);

		Assert.True(hoverRaised);
	}

	[Fact]
	public void HoverDetected_LocationIsCorrect()
	{
		var engine = CreateEngine();
		SKPoint? location = null;
		engine.HoverDetected += (s, e) => location = e.Location;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(175, 225), inContact: false);

		Assert.NotNull(location);
		Assert.Equal(175, location.Value.X);
		Assert.Equal(225, location.Value.Y);
	}

	[Fact]
	public void HoverWithoutPriorTouchDown_StillRaisesEvent()
	{
		var engine = CreateEngine();
		var hoverRaised = false;
		engine.HoverDetected += (s, e) => hoverRaised = true;

		// Mouse hover without any prior click/touch
		engine.ProcessTouchMove(99, new SKPoint(200, 200), inContact: false);

		Assert.True(hoverRaised, "Hover should work without a prior touch down");
	}

	[Fact]
	public void HoverWithoutPriorTouchDown_HasCorrectLocation()
	{
		var engine = CreateEngine();
		SKPoint? location = null;
		engine.HoverDetected += (s, e) => location = e.Location;

		engine.ProcessTouchMove(99, new SKPoint(300, 400), inContact: false);

		Assert.NotNull(location);
		Assert.Equal(300, location.Value.X);
		Assert.Equal(400, location.Value.Y);
	}

	#endregion

	#region Scroll (Mouse Wheel) Tests

	[Fact]
	public void ProcessMouseWheel_RaisesScrollDetected()
	{
		var engine = CreateEngine();
		var scrollRaised = false;
		engine.ScrollDetected += (s, e) => scrollRaised = true;

		engine.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(scrollRaised);
	}

	[Fact]
	public void ProcessMouseWheel_HasCorrectData()
	{
		var engine = CreateEngine();
		SKScrollGestureEventArgs? args = null;
		engine.ScrollDetected += (s, e) => args = e;

		engine.ProcessMouseWheel(new SKPoint(150, 250), 0, -3f);

		Assert.NotNull(args);
		Assert.Equal(150, args.Location.X);
		Assert.Equal(250, args.Location.Y);
		Assert.Equal(0, args.Delta.X);
		Assert.Equal(-3f, args.Delta.Y);
	}

	[Fact]
	public void ProcessMouseWheel_WhenDisabled_ReturnsFalse()
	{
		var engine = CreateEngine();
		engine.IsEnabled = false;

		var result = engine.ProcessMouseWheel(new SKPoint(100, 100), 0, 1f);

		Assert.False(result);
	}

	[Fact]
	public void ProcessMouseWheel_WhenDisposed_ReturnsFalse()
	{
		var engine = CreateEngine();
		engine.Dispose();

		var result = engine.ProcessMouseWheel(new SKPoint(100, 100), 0, 1f);

		Assert.False(result);
	}

	#endregion

}
