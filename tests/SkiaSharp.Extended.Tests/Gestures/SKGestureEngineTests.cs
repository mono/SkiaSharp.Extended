using SkiaSharp;
using SkiaSharp.Extended.Gestures;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>
/// Tests for <see cref="SKGestureEngine"/>.
/// </summary>
public class SKGestureEngineTests
{
	private long _testTicks = 1000000;

	private SKGestureEngine CreateEngine()
	{
		var engine = new SKGestureEngine
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
	public void LongTouch_RaisesLongPressDetected()
	{
		var engine = CreateEngine();
		var longPressRaised = false;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(600); // Longer than long press duration
		engine.CheckLongPress();

		Assert.True(longPressRaised);
	}

	[Fact]
	public void LongPress_DoesNotRaiseTapOnRelease()
	{
		var engine = CreateEngine();
		var tapRaised = false;
		var longPressRaised = false;
		engine.TapDetected += (s, e) => tapRaised = true;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(600);
		engine.CheckLongPress();
		engine.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.True(longPressRaised);
		Assert.False(tapRaised);
	}

	[Fact]
	public void LongPressDuration_CanBeCustomized()
	{
		var engine = CreateEngine();
		engine.LongPressDuration = 1000;
		var longPressRaised = false;
		engine.LongPressDetected += (s, e) => longPressRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(800);
		engine.CheckLongPress();
		
		Assert.False(longPressRaised);
		
		AdvanceTime(300);
		engine.CheckLongPress();
		
		Assert.True(longPressRaised);
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
		engine.PinchDetected += (s, e) => scale = e.Scale;

		// Initial distance: 100 pixels
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		engine.ProcessTouchDown(2, new SKPoint(200, 100));
		AdvanceTime(10);
		// New distance: 150 pixels (scale = 1.5)
		engine.ProcessTouchMove(1, new SKPoint(75, 100));
		engine.ProcessTouchMove(2, new SKPoint(225, 100));

		Assert.NotNull(scale);
		Assert.Equal(1.5f, scale.Value, 0.1f);
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

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		engine.ProcessTouchMove(1, new SKPoint(500, 100));
		AdvanceTime(50);
		engine.ProcessTouchUp(1, new SKPoint(500, 100));

		Assert.NotNull(velocityX);
		// Movement of 400 pixels in 50ms = 8000 px/s
		Assert.True(velocityX.Value > 200); // At least above threshold
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
	public void CurrentState_TracksGestureProgress()
	{
		var engine = CreateEngine();
		
		Assert.Equal(GestureState.None, engine.CurrentState);
		
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		Assert.Equal(GestureState.Detecting, engine.CurrentState);
		
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 100)); // Move beyond slop
		Assert.Equal(GestureState.Panning, engine.CurrentState);
		
		engine.ProcessTouchUp(1, new SKPoint(150, 100));
		Assert.Equal(GestureState.None, engine.CurrentState);
	}

	#endregion

	#region Selection Mode Tests

	[Fact]
	public void ImmediateMode_StartsPanningOnMove()
	{
		var engine = CreateEngine();
		engine.SelectionMode = SKGestureSelectionMode.Immediate;
		var panRaised = false;
		engine.PanDetected += (s, e) => panRaised = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(panRaised);
	}

	[Fact]
	public void TapToSelectMode_RequiresSelectionBeforeDrag()
	{
		var engine = CreateEngine();
		engine.SelectionMode = SKGestureSelectionMode.TapToSelect;
		var dragStarted = false;
		engine.DragStarted += (s, e) => dragStarted = true;

		// Without selection
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100));
		engine.ProcessTouchUp(1, new SKPoint(120, 100));

		Assert.False(dragStarted);

		// Set selection
		engine.SelectedItemId = 1;

		// With selection
		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(dragStarted);
	}

	[Fact]
	public void LongPressToSelectMode_StartsDragOnLongPress()
	{
		var engine = CreateEngine();
		engine.SelectionMode = SKGestureSelectionMode.LongPressToSelect;
		var dragStarted = false;
		engine.DragStarted += (s, e) => dragStarted = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(600);
		engine.CheckLongPress();

		Assert.True(dragStarted);
		Assert.Equal(GestureState.Dragging, engine.CurrentState);
	}

	#endregion

	#region Drag Events Tests

	[Fact]
	public void DragStarted_ProvidesStartLocation()
	{
		var engine = CreateEngine();
		engine.SelectionMode = SKGestureSelectionMode.Immediate;
		SKPoint? startLocation = null;
		engine.DragStarted += (s, e) => startLocation = e.StartLocation;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotNull(startLocation);
		Assert.Equal(100, startLocation.Value.X);
		Assert.Equal(100, startLocation.Value.Y);
	}

	[Fact]
	public void DragEnded_RaisesOnTouchUp()
	{
		var engine = CreateEngine();
		engine.SelectionMode = SKGestureSelectionMode.LongPressToSelect;
		var dragEnded = false;
		engine.DragEnded += (s, e) => dragEnded = true;

		engine.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(600);
		engine.CheckLongPress();
		AdvanceTime(10);
		engine.ProcessTouchMove(1, new SKPoint(150, 150));
		engine.ProcessTouchUp(1, new SKPoint(150, 150));

		Assert.True(dragEnded);
	}

	#endregion

	#region Selection Changed Tests

	[Fact]
	public void SettingSelectedItemId_RaisesSelectionChanged()
	{
		var engine = CreateEngine();
		long? selectedId = null;
		engine.SelectionChanged += (s, e) => selectedId = e.SelectedItemId;

		engine.SelectedItemId = 42;

		Assert.Equal(42, selectedId);
	}

	[Fact]
	public void ClearingSelection_RaisesSelectionChangedWithNull()
	{
		var engine = CreateEngine();
		engine.SelectedItemId = 1;
		
		long? selectedId = -1;
		engine.SelectionChanged += (s, e) => selectedId = e.SelectedItemId;

		engine.SelectedItemId = null;

		Assert.Null(selectedId);
	}

	#endregion

	#region Reset Tests

	[Fact]
	public void Reset_ClearsState()
	{
		var engine = CreateEngine();
		engine.SelectedItemId = 1;
		engine.ProcessTouchDown(1, new SKPoint(100, 100));

		engine.Reset();

		Assert.Equal(GestureState.None, engine.CurrentState);
		Assert.Null(engine.SelectedItemId);
	}

	#endregion

	#region Configuration Tests

	[Fact]
	public void TouchSlop_CanBeCustomized()
	{
		var engine = CreateEngine();
		engine.TouchSlop = 20;
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
		engine.FlingThreshold = 1000; // Very high threshold
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
		Assert.Equal(GestureState.Detecting, engine.CurrentState);

		engine.ProcessTouchCancel(1);
		Assert.Equal(GestureState.None, engine.CurrentState);
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

	#endregion
}
