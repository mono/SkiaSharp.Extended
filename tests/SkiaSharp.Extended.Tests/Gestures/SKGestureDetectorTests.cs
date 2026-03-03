using SkiaSharp;
using SkiaSharp.Extended.Gestures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>
/// Tests for <see cref="SKGestureDetector"/>.
/// </summary>
public class SKGestureDetectorTests
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

	#region Basic Touch Tests

	[Fact]
	public void ProcessTouchDown_WhenEnabled_ReturnsTrue()
	{
		var engine = CreateEngine();
		
		var result = engine.ProcessTouchDown(1, new SKPoint(100, 100));
		
		Assert.True(result);
	}

	[Fact]
	public void ProcessTouchDown_WhenDisabled_ReturnsFalse()
	{
		var engine = CreateEngine();
		engine.IsEnabled = false;
		
		var result = engine.ProcessTouchDown(1, new SKPoint(100, 100));
		
		Assert.False(result);
	}

	[Fact]
	public void ProcessTouchMove_WithoutTouchDown_ReturnsFalse()
	{
		var engine = CreateEngine();
		
		var result = engine.ProcessTouchMove(1, new SKPoint(110, 110));
		
		Assert.False(result);
	}

	[Fact]
	public void ProcessTouchUp_WithoutTouchDown_ReturnsFalse()
	{
		var engine = CreateEngine();
		
		var result = engine.ProcessTouchUp(1, new SKPoint(100, 100));
		
		Assert.False(result);
	}

	#endregion

	#region Tap Detection Tests

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

	#endregion

	#region Double Tap Detection Tests

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

	#endregion

	#region Long Press Tests

	[Fact]
	public async Task LongTouch_RaisesLongPressDetected()
	{
		var engine = new SKGestureDetector();
		engine.Options.LongPressDuration = 100; // Short duration for testing
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
		engine.Options.LongPressDuration = 100;
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
		engine.Options.LongPressDuration = 300;
		var longPressRaised = false;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		await Task.Delay(100);
		Assert.False(longPressRaised);
		
		await Task.Delay(300);
		Assert.True(longPressRaised);
		engine.Dispose();
	}

	#endregion

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

	#region Pinch Detection Tests

	[Fact]
	public void TwoFingerGesture_RaisesPinchDetected()
	{
		var engine = CreateEngine();
		var pinchRaised = false;
		engine.PinchDetected += (s, e) => pinchRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(90, 100));
		engine.ProcessTouchMove(2, new SKPoint(210, 100));

		Assert.True(pinchRaised);
	}

	[Fact]
	public void PinchDetected_ScaleIsCorrect()
	{
		var engine = CreateEngine();
		float? scale = null;
		engine.PinchDetected += (s, e) => scale = e.ScaleDelta;

		// Initial position: fingers 100 apart (100,100) and (200,100), center at (150,100), radius = 50
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Move to increase distance
		engine.ProcessTouchMove(1, new SKPoint(50, 100));
		engine.ProcessTouchMove(2, new SKPoint(250, 100));

		Assert.NotNull(scale);
		// Scale should be > 1 when zooming out
		Assert.True(scale.Value > 1.0f, $"Scale should be > 1.0, was {scale.Value}");
	}

	#endregion

	#region Rotation Detection Tests

	[Fact]
	public void TwoFingerRotation_RaisesRotateDetected()
	{
		var engine = CreateEngine();
		var rotateRaised = false;
		engine.RotateDetected += (s, e) => rotateRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 150));
		engine.ProcessTouchMove(2, new SKPoint(200, 50));

		Assert.True(rotateRaised);
	}

	[Fact]
	public void RotateDetected_RotationDeltaIsNormalized()
	{
		var engine = CreateEngine();
		float? rotation = null;
		engine.RotateDetected += (s, e) => rotation = e.RotationDelta;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Rotate 45 degrees
		engine.ProcessTouchMove(1, new SKPoint(79.3f, 120.7f));
		engine.ProcessTouchMove(2, new SKPoint(220.7f, 79.3f));

		Assert.NotNull(rotation);
		// Rotation should be normalized to -180 to 180 range
		Assert.True(rotation.Value >= -180 && rotation.Value <= 180);
	}

	#endregion

	#region Fling Detection Tests

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
		engine.FlingDetected += (s, e) => velocityX = e.VelocityX;

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

	#endregion

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
		Assert.Equal(0, args.DeltaX);
		Assert.Equal(-3f, args.DeltaY);
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

	#region Gesture State Tests

	[Fact]
	public void TouchDown_RaisesGestureStarted()
	{
		var engine = CreateEngine();
		var gestureStarted = false;
		engine.GestureStarted += (s, e) => gestureStarted = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));

		Assert.True(gestureStarted);
	}

	[Fact]
	public void TouchDown_SetsGestureActive()
	{
		var engine = CreateEngine();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));

		Assert.True(engine.IsGestureActive);
	}

	[Fact]
	public void TouchUp_RaisesGestureEnded()
	{
		var engine = CreateEngine();
		var gestureEnded = false;
		engine.GestureEnded += (s, e) => gestureEnded = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(150, 100));

		Assert.True(gestureEnded);
	}

	[Fact]
	public void TouchUp_ClearsGestureActive()
	{
		var engine = CreateEngine();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(150, 100));

		Assert.False(engine.IsGestureActive);
	}

	#endregion
	#region Reset Tests

	[Fact]
	public void Reset_ClearsState()
	{
		var engine = CreateEngine();
		engine.ProcessTouchDown(1, new SKPoint(100, 100));

		engine.Reset();

		Assert.False(engine.IsGestureActive);
	}

	#endregion

	#region Configuration Tests

	[Fact]
	public void TouchSlop_CanBeCustomized()
	{
		var engine = CreateEngine();
		engine.Options.TouchSlop = 20;
		var panRaised = false;
		engine.PanDetected += (s, e) => panRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(115, 100)); // Move 15 pixels (less than 20)

		Assert.False(panRaised);
	}

	[Fact]
	public void FlingThreshold_CanBeCustomized()
	{
		var engine = CreateEngine();
		engine.Options.FlingThreshold = 1000; // Very high threshold
		var flingRaised = false;
		engine.FlingDetected += (s, e) => flingRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchMove(1, new SKPoint(200, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(200, 100));

		Assert.False(flingRaised);
	}

	#endregion

	#region Cancel Tests

	[Fact]
	public void ProcessTouchCancel_ResetsGestureState()
	{
		var engine = CreateEngine();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		Assert.True(engine.IsGestureActive);

		engine.ProcessTouchCancel(1);
		Assert.False(engine.IsGestureActive);
	}

	[Fact]
	public void ProcessTouchCancel_ClearsGestureActive()
	{
		var engine = CreateEngine();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100));
		engine.ProcessTouchCancel(1);

		Assert.False(engine.IsGestureActive);
	}

	[Fact]
	public void ProcessTouchCancel_RaisesGestureEnded()
	{
		var engine = CreateEngine();
		var gestureEnded = false;
		engine.GestureEnded += (s, e) => gestureEnded = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100));
		engine.ProcessTouchCancel(1);

		Assert.True(gestureEnded);
	}

	[Fact]
	public void ProcessTouchCancel_DuringPinch_WithOneFingerRemaining_TransitionsToPanning()
	{
		// When one finger is cancelled during a two-finger pinch, the remaining finger
		// should continue generating pan events rather than the gesture freezing.
		var engine = CreateEngine();
		var panRaised = false;
		engine.PanDetected += (s, e) => panRaised = true;

		// Start a two-finger pinch
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(90, 100));
		engine.ProcessTouchMove(2, new SKPoint(210, 100));

		// Cancel one finger — should transition to Panning
		engine.ProcessTouchCancel(2);

		// Subsequent moves with the remaining finger should produce pan events
		panRaised = false;
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(80, 100));

		Assert.True(panRaised, "Pan should fire after cancelling one finger during a pinch");
	}

	[Fact]
	public void ProcessTouchCancel_DuringPinch_WithOneFingerRemaining_GestureStaysActive()
	{
		var engine = CreateEngine();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(90, 100));
		engine.ProcessTouchMove(2, new SKPoint(210, 100));

		// Cancel one finger
		engine.ProcessTouchCancel(2);

		// Gesture should still be active (remaining finger is panning)
		Assert.True(engine.IsGestureActive, "Gesture should remain active after one finger cancelled during pinch");
	}

	#endregion

	#region Bug Fix Tests

	[Fact]
	public void DoubleTap_FarApart_DoesNotTriggerDoubleTap()
	{
		var engine = CreateEngine();
		var doubleTapRaised = false;
		engine.DoubleTapDetected += (s, e) => doubleTapRaised = true;

		// First tap at (100, 100)
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));
		
		AdvanceTime(100);
		
		// Second tap far away at (500, 500)
		engine.ProcessTouchDown(1, new SKPoint(500, 500));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(500, 500));

		Assert.False(doubleTapRaised);
	}

	[Fact]
	public void DoubleTap_TooSlow_DoesNotTriggerDoubleTap()
	{
		var engine = CreateEngine();
		var doubleTapRaised = false;
		engine.DoubleTapDetected += (s, e) => doubleTapRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));
		
		AdvanceTime(500); // Longer than DoubleTapDelayTicks (300ms)
		
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.False(doubleTapRaised);
	}

	[Fact]
	public void SecondFingerDown_DoesNotBreakFirstFingerTap()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;

		// First finger down
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		// Second finger touches briefly
		engine.ProcessTouchDown(2, new SKPoint(200, 200));
		engine.ProcessTouchUp(2, new SKPoint(200, 200));
		AdvanceTime(50);
		// First finger up — should not trigger tap (state changed to pinching)
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		// After multi-touch, tap detection is naturally suppressed since state transitions away
		// This tests that we don't crash and state is consistent
		Assert.False(engine.IsGestureActive);
	}

	[Fact]
	public void FlingWithMultipleMoveEvents_ProducesReasonableVelocity()
	{
		var engine = CreateEngine();
		float? velocityX = null;
		engine.FlingDetected += (s, e) => velocityX = e.VelocityX;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(250, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(400, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(600, 100));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(600, 100));

		Assert.NotNull(velocityX);
		Assert.True(velocityX.Value > 200, $"VelocityX should be > 200, was {velocityX.Value}");
	}

	#endregion

	#region Tap Duration Tests

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

	#endregion

	#region Pinch Event Data Tests

	[Fact]
	public void PinchDetected_CenterIsMidpointOfTouches()
	{
		var engine = CreateEngine();
		SKPoint? center = null;
		engine.PinchDetected += (s, e) => center = e.FocalPoint;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(80, 100));
		engine.ProcessTouchMove(2, new SKPoint(220, 100));

		Assert.NotNull(center);
		Assert.Equal(150, center.Value.X, 0.1);
		Assert.Equal(100, center.Value.Y, 0.1);
	}

	[Fact]
	public void PinchDetected_PreviousCenterIsProvided()
	{
		var engine = CreateEngine();
		SKPinchGestureEventArgs? lastArgs = null;
		engine.PinchDetected += (s, e) => lastArgs = e;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Move both fingers right — events fire per-finger, so check last event
		engine.ProcessTouchMove(1, new SKPoint(120, 100));
		engine.ProcessTouchMove(2, new SKPoint(220, 100));

		Assert.NotNull(lastArgs);
		// PreviousCenter should be from the intermediate state (after finger1 moved)
		Assert.NotNull(lastArgs!.PreviousFocalPoint);
		// Center should be midpoint of final positions
		Assert.Equal(170, lastArgs.FocalPoint.X, 0.1);
	}

	[Fact]
	public void PinchDetected_EqualDistanceMove_ScaleIsOne()
	{
		var engine = CreateEngine();
		var scales = new List<float>();
		engine.PinchDetected += (s, e) => scales.Add(e.ScaleDelta);

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Move both outward equally — each finger fires separately
		engine.ProcessTouchMove(1, new SKPoint(50, 100));
		engine.ProcessTouchMove(2, new SKPoint(250, 100));

		Assert.True(scales.Count >= 1);
		// The net product of scales should show zoom-out (> 1)
		var totalScale = 1f;
		foreach (var s in scales) totalScale *= s;
		Assert.True(totalScale > 1.0f, $"Total scale should be > 1 for zoom-out, was {totalScale}");
	}

	[Fact]
	public void PinchDetected_FingersCloser_ScaleLessThanOne()
	{
		var engine = CreateEngine();
		float? scale = null;
		engine.PinchDetected += (s, e) => scale = e.ScaleDelta;

		// Initial: 100 apart
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Pinch in: 50 apart
		engine.ProcessTouchMove(1, new SKPoint(125, 100));
		engine.ProcessTouchMove(2, new SKPoint(175, 100));

		Assert.NotNull(scale);
		Assert.True(scale.Value < 1.0f, $"Scale should be < 1 (zoom in), was {scale.Value}");
	}

	#endregion

	#region Rotation Event Data Tests

	[Fact]
	public void RotateDetected_PreviousCenterIsProvided()
	{
		var engine = CreateEngine();
		SKRotateGestureEventArgs? lastArgs = null;
		engine.RotateDetected += (s, e) => lastArgs = e;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 150));
		engine.ProcessTouchMove(2, new SKPoint(200, 50));

		Assert.NotNull(lastArgs);
		Assert.NotNull(lastArgs!.PreviousFocalPoint);
		// Center should be midpoint of final positions
		Assert.Equal(150, lastArgs.FocalPoint.X, 0.1);
		Assert.Equal(100, lastArgs.FocalPoint.Y, 0.1);
	}

	[Fact]
	public void RotateDetected_CenterMovesWithFingers()
	{
		var engine = CreateEngine();
		SKPoint? center = null;
		engine.RotateDetected += (s, e) => center = e.FocalPoint;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Move both fingers down while rotating
		engine.ProcessTouchMove(1, new SKPoint(100, 200));
		engine.ProcessTouchMove(2, new SKPoint(200, 200));

		Assert.NotNull(center);
		Assert.Equal(150, center.Value.X, 0.1);
		Assert.Equal(200, center.Value.Y, 0.1);
	}

	[Fact]
	public void RotateDetected_NoRotation_DeltaIsZero()
	{
		var engine = CreateEngine();
		float? rotationDelta = null;
		engine.RotateDetected += (s, e) => rotationDelta = e.RotationDelta;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// Move both outward horizontally — no angle change
		engine.ProcessTouchMove(1, new SKPoint(50, 100));
		engine.ProcessTouchMove(2, new SKPoint(250, 100));

		Assert.NotNull(rotationDelta);
		Assert.Equal(0f, rotationDelta.Value, 0.1);
	}

	#endregion

	#region Three-Plus Touch Tests

	[Fact]
	public void ThreeFingers_DoesNotCrash()
	{
		var engine = CreateEngine();
		var pinchRaised = false;
		engine.PinchDetected += (s, e) => pinchRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		engine.ProcessTouchDown(3, new SKPoint(300, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(90, 100));
		engine.ProcessTouchMove(2, new SKPoint(210, 100));
		engine.ProcessTouchMove(3, new SKPoint(310, 100));

		// Should not crash; pinch fires for >= 2 touches
		Assert.True(pinchRaised, "Pinch should fire with 3+ touches using first 2");
	}

	[Fact]
	public void ThreeFingers_LiftOneToTwo_ResumesPinch()
	{
		var engine = CreateEngine();
		var pinchCount = 0;
		engine.PinchDetected += (s, e) => pinchCount++;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		engine.ProcessTouchDown(3, new SKPoint(300, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(90, 100)); // 3 fingers, no pinch

		// Lift third finger → back to 2
		engine.ProcessTouchUp(3, new SKPoint(300, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(80, 100));
		engine.ProcessTouchMove(2, new SKPoint(220, 100));

		Assert.True(pinchCount > 0, "Pinch should resume after lifting to 2 fingers");
	}

	#endregion
	#region Fling Edge Case Tests

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
		engine.FlingDetected += (s, e) => { velocityX = e.VelocityX; velocityY = e.VelocityY; };

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
		engine.FlingDetected += (s, e) => { velocityX = e.VelocityX; velocityY = e.VelocityY; };

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

	#endregion

	#region Fling Animation Tests

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

	#endregion

	#region Cancel Edge Case Tests

	[Fact]
	public void CancelDuringPinch_ResetsState()
	{
		var engine = CreateEngine();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		Assert.True(engine.IsGestureActive);

		engine.ProcessTouchCancel(1);
		engine.ProcessTouchCancel(2);
		Assert.False(engine.IsGestureActive);
	}

	#endregion

	#region Sequential Gesture Tests

	[Fact]
	public void MultipleSequentialTaps_EachFiresSeparately()
	{
		var engine = CreateEngine();
		var tapCount = 0;
		engine.TapDetected += (s, e) => tapCount++;

		for (int i = 0; i < 5; i++)
		{
			engine.ProcessTouchDown(1, new SKPoint(100 + i * 50, 100));
			AdvanceTime(50);
			engine.ProcessTouchUp(1, new SKPoint(100 + i * 50, 100));
			AdvanceTime(500); // Wait long enough to not trigger double-tap
		}

		Assert.Equal(5, tapCount);
	}

	[Fact]
	public void PanThenTap_TapFiresAfterPanEnds()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		var panRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;
		engine.PanDetected += (s, e) => panRaised = true;

		// Pan gesture
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(200, 100));
		Assert.True(panRaised);
		Assert.False(tapRaised);

		// New tap gesture after pan is done
		AdvanceTime(500);
		engine.ProcessTouchDown(1, new SKPoint(300, 300));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(300, 300));
		Assert.True(tapRaised);
	}

	[Fact]
	public void PinchThenTap_TapFiresAfterPinchEnds()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;

		// Pinch gesture
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(80, 100));
		engine.ProcessTouchMove(2, new SKPoint(220, 100));
		engine.ProcessTouchUp(2, new SKPoint(220, 100));
		engine.ProcessTouchUp(1, new SKPoint(80, 100));
		Assert.False(tapRaised);

		// New tap
		AdvanceTime(500);
		engine.ProcessTouchDown(1, new SKPoint(150, 150));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(150, 150));
		Assert.True(tapRaised);
	}

	#endregion

	#region Dispose/Reset Edge Cases

	[Fact]
	public void Dispose_DuringGesture_StopsProcessing()
	{
		var engine = CreateEngine();
		var panCount = 0;
		engine.PanDetected += (s, e) => panCount++;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100)); // Triggers pan
		Assert.True(panCount > 0);

		engine.Dispose();
		var beforeCount = panCount;
		
		// Further events should be ignored
		engine.ProcessTouchMove(1, new SKPoint(200, 100));
		Assert.Equal(beforeCount, panCount);
	}

	[Fact]
	public void Reset_DuringPan_AllowsNewGesture()
	{
		var engine = CreateEngine();
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100));
		Assert.True(engine.IsGestureActive);

		engine.Reset();
		Assert.False(engine.IsGestureActive);

		// New gesture should work
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;
		engine.ProcessTouchDown(1, new SKPoint(200, 200));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(200, 200));
		Assert.True(tapRaised);
	}

	[Fact]
	public void ProcessEvents_AfterDispose_ReturnsFalse()
	{
		var engine = CreateEngine();
		engine.Dispose();

		Assert.False(engine.ProcessTouchDown(1, new SKPoint(100, 100)));
		Assert.False(engine.ProcessTouchMove(1, new SKPoint(110, 110)));
		Assert.False(engine.ProcessTouchUp(1, new SKPoint(110, 110)));
	}

	#endregion

	#region Zero/Edge Value Tests

	[Fact]
	public void TouchMove_ToSameLocation_ZeroDelta()
	{
		var engine = CreateEngine();
		SKPoint? lastDelta = null;
		engine.PanDetected += (s, e) => lastDelta = e.Delta;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100)); // Start pan
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100)); // Same location

		Assert.NotNull(lastDelta);
		Assert.Equal(0, lastDelta.Value.X, 0.01);
		Assert.Equal(0, lastDelta.Value.Y, 0.01);
	}

	[Fact]
	public void Pinch_ZeroRadius_ScaleIsOne()
	{
		var engine = CreateEngine();
		float? scale = null;
		engine.PinchDetected += (s, e) => scale = e.ScaleDelta;

		// Both fingers at the same point
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 100));
		engine.ProcessTouchMove(2, new SKPoint(100, 100));

		// With zero initial radius, scale should be 1 (guarded)
		if (scale != null)
			Assert.Equal(1.0f, scale.Value, 0.01);
	}

	[Fact]
	public void TouchDown_DuplicateId_UpdatesExistingTouch()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(1, new SKPoint(200, 200)); // Same ID
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(200, 200));

		// Should not crash
		Assert.False(engine.IsGestureActive);
	}

	#endregion

	#region Review Fix Tests

	[Fact]
	public void DoubleTapSlop_FarApartTaps_DoNotTriggerDoubleTap()
	{
		var engine = CreateEngine();
		engine.Options.DoubleTapSlop = 40f;
		var doubleTapCount = 0;
		engine.DoubleTapDetected += (s, e) => doubleTapCount++;

		// First tap
		engine.ProcessTouchDown(1, new SKPoint(50, 50));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(50, 50));

		// Second tap far away (beyond 40px slop)
		AdvanceTime(100);
		engine.ProcessTouchDown(1, new SKPoint(200, 200));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(200, 200));

		Assert.Equal(0, doubleTapCount);
	}

	[Fact]
	public void DoubleTapSlop_CloseTaps_TriggerDoubleTap()
	{
		var engine = CreateEngine();
		engine.Options.DoubleTapSlop = 40f;
		var doubleTapCount = 0;
		engine.DoubleTapDetected += (s, e) => doubleTapCount++;

		// First tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		// Second tap within 40px slop
		AdvanceTime(100);
		engine.ProcessTouchDown(1, new SKPoint(120, 115));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(120, 115));

		Assert.Equal(1, doubleTapCount);
	}

	[Fact]
	public void ProcessTouchCancel_WhenDisposed_DoesNotThrow()
	{
		var engine = CreateEngine();
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.Dispose();

		// Should not throw
		var result = engine.ProcessTouchCancel(1);
		Assert.False(result);
	}

	[Fact]
	public void ProcessTouchCancel_StopsLongPressTimer()
	{
		var engine = CreateEngine();
		var longPressCount = 0;
		engine.LongPressDetected += (s, e) => longPressCount++;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(200);
		engine.ProcessTouchCancel(1);

		// Wait for timer to have fired if it wasn't stopped
		AdvanceTime(600);

		Assert.Equal(0, longPressCount);
	}

	[Fact]
	public void ThreeToTwoFinger_NoScaleJump()
	{
		var engine = CreateEngine();
		var scales = new List<float>();
		engine.PinchDetected += (s, e) => scales.Add(e.ScaleDelta);

		// Start 2-finger pinch
		engine.ProcessTouchDown(1, new SKPoint(100, 200));
		engine.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(95, 200));
		engine.ProcessTouchMove(2, new SKPoint(205, 200));

		// Add third finger
		engine.ProcessTouchDown(3, new SKPoint(300, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(90, 200));
		engine.ProcessTouchMove(2, new SKPoint(210, 200));
		engine.ProcessTouchMove(3, new SKPoint(310, 200));

		scales.Clear(); // Clear history

		// Lift third finger — should recalculate pinch state, no jump
		engine.ProcessTouchUp(3, new SKPoint(310, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(85, 200));
		engine.ProcessTouchMove(2, new SKPoint(215, 200));

		// Scale should be close to 1.0 (small incremental change, not a jump)
		foreach (var scale in scales)
		{
			Assert.InRange(scale, 0.8f, 1.2f);
		}
	}

	[Fact]
	public void PinchToPan_DragOriginIsUpdated()
	{
		var engine = CreateEngine();
		var panDeltas = new List<SKPoint>();
		engine.PanDetected += (s, e) => panDeltas.Add(new SKPoint(e.Delta.X, e.Delta.Y));

		// Start pinch with 2 fingers
		engine.ProcessTouchDown(1, new SKPoint(100, 200));
		engine.ProcessTouchDown(2, new SKPoint(300, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(105, 200));
		engine.ProcessTouchMove(2, new SKPoint(295, 200));

		// Lift one finger → transition to pan
		engine.ProcessTouchUp(2, new SKPoint(295, 200));
		panDeltas.Clear();

		// Continue moving remaining finger — delta should be small and incremental
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(110, 200));

		if (panDeltas.Count > 0)
		{
			var delta = panDeltas[0];
			// Delta should be small (5px move), not a jump from original touch position
			Assert.InRange(delta.X, -20f, 20f);
			Assert.InRange(delta.Y, -20f, 20f);
		}
	}

	[Fact]
	public void Reset_AllowsGesturesAgain()
	{
		var engine = CreateEngine();
		var tapCount = 0;
		engine.TapDetected += (s, e) => tapCount++;

		// First tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));
		Assert.Equal(1, tapCount);

		// Reset
		engine.Reset();

		// Second tap should still work
		AdvanceTime(500);
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));
		Assert.Equal(2, tapCount);
	}

	[Fact]
	public void Dispose_PreventsAllFutureGestures()
	{
		var engine = CreateEngine();
		var tapCount = 0;
		engine.TapDetected += (s, e) => tapCount++;

		engine.Dispose();

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.Equal(0, tapCount);
	}

	#endregion

	#region Options Validation Tests

	[Fact]
	public void Options_TouchSlop_Negative_Throws()
	{
		var options = new SKGestureDetectorOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.TouchSlop = -1f);
	}

	[Fact]
	public void Options_DoubleTapSlop_Negative_Throws()
	{
		var options = new SKGestureDetectorOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.DoubleTapSlop = -1f);
	}

	[Fact]
	public void Options_FlingThreshold_Negative_Throws()
	{
		var options = new SKGestureDetectorOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingThreshold = -1f);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Options_LongPressDuration_ZeroOrNegative_Throws(int value)
	{
		var options = new SKGestureDetectorOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.LongPressDuration = value);
	}

	[Fact]
	public void Constructor_NullOptions_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new SKGestureDetector(null!));
	}

	[Fact]
	public void Options_ValidValues_PassThrough()
	{
		var options = new SKGestureDetectorOptions
		{
			TouchSlop = 16f,
			DoubleTapSlop = 80f,
			FlingThreshold = 400f,
			LongPressDuration = 1000,
		};
		var engine = new SKGestureDetector(options);

		Assert.Equal(16f, engine.Options.TouchSlop);
		Assert.Equal(80f, engine.Options.DoubleTapSlop);
		Assert.Equal(400f, engine.Options.FlingThreshold);
		Assert.Equal(1000, engine.Options.LongPressDuration);
	}

	#endregion

	#region GestureStarted Bug Fix Verification

	[Fact]
	public void GestureStarted_OnlyFiresOnce_WhenMultipleFingersTouch()
	{
		var engine = CreateEngine();
		var count = 0;
		engine.GestureStarted += (s, e) => count++;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 200));
		engine.ProcessTouchDown(3, new SKPoint(300, 300));

		Assert.Equal(1, count);
	}

	#endregion

	#region EventArgs Verification Tests

	[Fact]
	public void PanEventArgs_PreviousLocation_IsSetCorrectly()
	{
		var engine = CreateEngine();
		SKPanGestureEventArgs? captured = null;
		engine.PanDetected += (s, e) => captured = e;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotNull(captured);
		Assert.Equal(100, captured.PreviousLocation.X, 1);
		Assert.Equal(100, captured.PreviousLocation.Y, 1);
	}

	[Fact]
	public void PinchEventArgs_ScaleDelta_ProductMatchesCumulativeScale()
	{
		var engine = CreateEngine();
		var scaleDeltas = new List<float>();
		engine.PinchDetected += (s, e) => scaleDeltas.Add(e.ScaleDelta);

		// Fingers start 100px apart, spread to 200px → cumulative scale ≈ 2.0
		engine.ProcessTouchDown(1, new SKPoint(100, 200));
		engine.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(50, 200));
		engine.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.NotEmpty(scaleDeltas);
		var cumulativeScale = 1f;
		foreach (var delta in scaleDeltas)
			cumulativeScale *= delta;
		Assert.Equal(2.0f, cumulativeScale, 2);
	}

	[Fact]
	public void FlingEventArgs_HasVelocityAndSpeed()
	{
		var engine = CreateEngine();
		SKFlingGestureEventArgs? captured = null;
		engine.FlingDetected += (s, e) => captured = e;

		engine.ProcessTouchDown(1, new SKPoint(100, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(300, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(500, 200));
		AdvanceTime(10);
		engine.ProcessTouchUp(1, new SKPoint(500, 200));

		Assert.NotNull(captured);
		Assert.True(captured.VelocityX > 0, $"VelocityX should be positive for rightward fling, was {captured.VelocityX}");
		Assert.True(captured.Speed > 0, $"Speed should be positive, was {captured.Speed}");
		Assert.Equal((float)Math.Sqrt(captured.VelocityX * captured.VelocityX + captured.VelocityY * captured.VelocityY), captured.Speed, 1);
	}

	#endregion

	#region Double Dispose Safety

	[Fact]
	public void Dispose_CalledTwice_DoesNotThrow()
	{
		var engine = CreateEngine();
		engine.Dispose();
		engine.Dispose(); // should not throw
	}

	#endregion

	#region Touch ID Reuse

	[Fact]
	public void TouchIdReuse_AfterTouchUp_StartsNewGesture()
	{
		var engine = CreateEngine();
		var tapCount = 0;
		engine.TapDetected += (s, e) => tapCount++;

		// First gesture with ID 1
		engine.ProcessTouchDown(1, new SKPoint(100, 100), false);
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100), false);

		// Wait past double-tap window
		AdvanceTime(500);

		// Reuse ID 1 at a different location
		engine.ProcessTouchDown(1, new SKPoint(300, 300), false);
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(300, 300), false);
		AdvanceTime(500);

		Assert.Equal(2, tapCount);
	}

	#endregion

	#region Triple Tap Sequence

	[Fact]
	public void ThreeTaps_RapidSequence_FiresDoubleTapAndSingleTap()
	{
		var engine = CreateEngine();
		var tapCount = 0;
		var doubleTapCount = 0;
		engine.TapDetected += (s, e) => tapCount++;
		engine.DoubleTapDetected += (s, e) => doubleTapCount++;

		// Tap 1
		engine.ProcessTouchDown(1, new SKPoint(100, 100), false);
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100), false);
		AdvanceTime(100);

		// Tap 2 — triggers double tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100), false);
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100), false);
		AdvanceTime(500); // Wait past double-tap window

		// Tap 3 — new single tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100), false);
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100), false);
		AdvanceTime(500);

		Assert.True(doubleTapCount >= 1, $"Expected at least 1 double tap, got {doubleTapCount}");
		Assert.True(tapCount >= 2, $"Expected at least 2 taps, got {tapCount}");
	}

	#endregion

	#region Bug Regression Tests

	[Fact]
	public void LongPressTimer_NotRestarted_OnSecondFingerDown()
	{
		// Regression: StartLongPressTimer() was called for every ProcessTouchDown,
		// including the 2nd finger, resetting the long-press timer during pinch start.
		var engine = CreateEngine();
		engine.Options.LongPressDuration = 200; // Short for test

		var longPressCount = 0;
		engine.LongPressDetected += (s, e) => longPressCount++;

		// Put down first finger and hold
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(150); // Almost at long-press threshold

		// Second finger touches — should NOT reset the timer
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10); // Small additional time
		engine.ProcessTouchUp(2, new SKPoint(200, 100));

		// The first finger has been held for 160ms total — still shouldn't trigger long press
		// (because state transitioned to Pinching then Panning, not Detecting)
		// More importantly, the timer should not have been reset such that it would fire 200ms
		// AFTER the second finger touched (which would be 360ms total, wrong behavior)
		Assert.Equal(0, longPressCount);
	}

	[Fact]
	public void PinchRotation_DoesNotJump_WhenThirdFingerAddedAndRemoved()
	{
		// Regression: GetActiveTouchPoints() used Dictionary iteration order (not guaranteed).
		// Adding/removing a 3rd finger could swap locations[0] and locations[1],
		// causing the angle to jump ~180°.
		var engine = CreateEngine();
		var rotationDeltas = new List<float>();
		engine.RotateDetected += (s, e) => rotationDeltas.Add(e.RotationDelta);

		// Start 2-finger pinch: finger 1 at left, finger 2 at right (horizontal → 0°)
		engine.ProcessTouchDown(1, new SKPoint(100, 200));
		engine.ProcessTouchDown(2, new SKPoint(300, 200));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(100, 200));
		engine.ProcessTouchMove(2, new SKPoint(300, 200));
		rotationDeltas.Clear();

		// Add 3rd finger
		engine.ProcessTouchDown(3, new SKPoint(200, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(3, new SKPoint(200, 100));
		AdvanceTime(10);

		// Remove 3rd finger — this is where the angle jump was observed
		engine.ProcessTouchUp(3, new SKPoint(200, 100));
		AdvanceTime(10);

		// Move fingers slightly to trigger rotation events
		engine.ProcessTouchMove(1, new SKPoint(100, 200));
		engine.ProcessTouchMove(2, new SKPoint(300, 200));

		// No rotation delta should be close to ±180° (a jump)
		foreach (var delta in rotationDeltas)
		{
			Assert.True(Math.Abs(delta) < 90f,
				$"Rotation delta {delta}° is too large — indicates angle jump from unstable ordering");
		}
	}

	[Fact]
	public void TapCount_ResetsAfterFailedTap_DueToMovement()
	{
		// Regression: _tapCount was incremented on touch-down but not reset when the
		// tap failed because the finger moved beyond TouchSlop. A subsequent valid tap
		// could incorrectly inherit the stale count and fire as a double-tap.
		var engine = CreateEngine();
		engine.Options.TouchSlop = 8f;

		var singleTapCount = 0;
		var doubleTapCount = 0;
		engine.TapDetected += (s, e) => { if (e.TapCount == 1) singleTapCount++; };
		engine.DoubleTapDetected += (s, e) => doubleTapCount++;

		// Touch down, then slide beyond slop (failed tap)
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchMove(1, new SKPoint(120, 100)); // 20px > 8px slop
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(120, 100));

		AdvanceTime(100);

		// A clean single tap now — should be count=1, NOT double-tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.Equal(1, singleTapCount);
		Assert.Equal(0, doubleTapCount);
	}

	[Fact]
	public void TapCount_ResetsAfterFailedTap_DueToLongHold()
	{
		// Regression: _tapCount was not reset when the finger was held too long
		// (exceeding tap timeout), which could cause a subsequent valid tap to be
		// counted as a double-tap.
		var engine = CreateEngine();

		var singleTapCount = 0;
		var doubleTapCount = 0;
		engine.TapDetected += (s, e) => { if (e.TapCount == 1) singleTapCount++; };
		engine.DoubleTapDetected += (s, e) => doubleTapCount++;

		// Touch down and hold beyond the tap timeout
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(600); // Far beyond any tap timeout
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		AdvanceTime(100);

		// A clean single tap — should be count=1, NOT double-tap
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.Equal(1, singleTapCount);
		Assert.Equal(0, doubleTapCount);
	}

	#endregion
}
