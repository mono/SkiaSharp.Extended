using SkiaSharp;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for tap, double-tap, and long-press detection in <see cref="SKGestureDetector"/>.</summary>
public class SKGestureDetectorTapTests
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
	public void QuickTouchAndRelease_RaisesTapDetected()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50); // Quick tap
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.True(tapRaised);
	}

	[Fact]
	public void TapDetected_LocationIsCorrect()
	{
		var engine = CreateEngine();
		SKPoint? location = null;
		engine.TapDetected += (s, e) => location = e.Location;

		engine.ProcessTouchDown(1, new SKPoint(150, 250));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(150, 250));

		Assert.NotNull(location);
		Assert.Equal(150, location.Value.X);
		Assert.Equal(250, location.Value.Y);
	}

	[Fact]
	public void TapDetected_TapCountIsOne()
	{
		var engine = CreateEngine();
		int? tapCount = null;
		engine.TapDetected += (s, e) => tapCount = e.TapCount;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.Equal(1, tapCount);
	}



	[Fact]
	public void TwoQuickTaps_RaisesDoubleTapDetected()
	{
		var engine = CreateEngine();
		var doubleTapRaised = false;
		engine.DoubleTapDetected += (s, e) => doubleTapRaised = true;

		// First tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));
		
		// Wait a bit but not too long
		AdvanceTime(100);
		
		// Second tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.True(doubleTapRaised);
	}

	[Fact]
	public void DoubleTap_TapCountIsTwo()
	{
		var engine = CreateEngine();
		int? tapCount = null;
		engine.DoubleTapDetected += (s, e) => tapCount = e.TapCount;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(100);
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.Equal(2, tapCount);
	}



	[Fact]
	public async Task LongTouch_RaisesLongPressDetected()
	{
		var engine = new SKGestureDetector();
		engine.Options.LongPressDuration = TimeSpan.FromMilliseconds(100); // Short duration for testing
		var longPressRaised = false;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		await Task.Delay(200); // Wait for timer to fire

		Assert.True(longPressRaised);
		engine.Dispose();
	}

	[Fact]
	public async Task LongPress_DoesNotRaiseTapOnRelease()
	{
		var engine = new SKGestureDetector();
		engine.Options.LongPressDuration = TimeSpan.FromMilliseconds(100);
		var tapRaised = false;
		var longPressRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		await Task.Delay(200);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.True(longPressRaised);
		Assert.False(tapRaised);
		engine.Dispose();
	}

	[Fact]
	public async Task LongPressDuration_CanBeCustomized()
	{
		var engine = new SKGestureDetector();
		engine.Options.LongPressDuration = TimeSpan.FromMilliseconds(300);
		var longPressRaised = false;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		await Task.Delay(100);
		Assert.False(longPressRaised);
		
		await Task.Delay(300);
		Assert.True(longPressRaised);
		engine.Dispose();
	}



	[Fact]
	public void MouseClick_BeyondShortClickDuration_DoesNotFireTap()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100), isMouse: true);
		AdvanceTime(300); // Beyond ShortClickTicks (250ms)
		engine.ProcessTouchUp(1, new SKPoint(100, 100), isMouse: true);

		Assert.False(tapRaised, "Mouse click held too long should not fire tap");
	}

	[Fact]
	public void TouchHeld_WithSmallMoves_BeyondLongPressDuration_DoesNotFireTap()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		// Small moves within slop over a long time
		AdvanceTime(200);
		engine.ProcessTouchMove(1, new SKPoint(101, 101));
		AdvanceTime(200);
		engine.ProcessTouchMove(1, new SKPoint(100, 100));
		AdvanceTime(200); // Total 600ms > LongPressTicks (500ms)
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.False(tapRaised, "Touch held too long should not fire tap");
	}

	[Fact]
	public void TapThenPinchThenTap_DoesNotFireDoubleTap()
	{
		// Bug fix: _tapCount was not cleared when entering Pinching state, causing
		// a false DoubleTapDetected on the subsequent single tap.
		var engine = CreateEngine();
		var doubleTapRaised = false;
		engine.DoubleTapDetected += (s, e) => doubleTapRaised = true;

		// First tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		// Immediately start a pinch (within double-tap time window)
		AdvanceTime(100); // < 300ms double-tap delay
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		engine.ProcessTouchMove(1, new SKPoint(80, 100));
		engine.ProcessTouchMove(2, new SKPoint(220, 100));
		engine.ProcessTouchUp(2, new SKPoint(220, 100));
		engine.ProcessTouchUp(1, new SKPoint(80, 100));

		// Third tap shortly after pinch completes (still within original 300ms window from first tap)
		AdvanceTime(50);
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.False(doubleTapRaised, "DoubleTapDetected must not fire after tap → pinch → single tap sequence");
	}

}
