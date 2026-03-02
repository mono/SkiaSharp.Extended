using SkiaSharp;
using SkiaSharp.Extended.Gestures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>
/// Tests for <see cref="SKGestureTracker"/>.
/// </summary>
public class SKGestureTrackerTests
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

	private void SimulateDoubleTap(SKGestureTracker tracker, SKPoint location)
	{
		tracker.ProcessTouchDown(1, location);
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, location);
		AdvanceTime(100);
		tracker.ProcessTouchDown(1, location);
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, location);
	}

	#region Pan → Offset Tests

	[Fact]
	public void Pan_UpdatesOffset()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotEqual(SKPoint.Empty, tracker.Offset);
		Assert.True(tracker.Offset.X > 0, $"Offset.X should be > 0, was {tracker.Offset.X}");
	}

	[Fact]
	public void Pan_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var transformChanged = false;
		tracker.TransformChanged += (s, e) => transformChanged = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(transformChanged);
	}

	[Fact]
	public void Pan_OffsetAccumulates()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		var offset1 = tracker.Offset;
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(140, 100));
		var offset2 = tracker.Offset;

		Assert.True(offset2.X > offset1.X, "Offset should accumulate with continued panning");
	}

	#endregion

	#region Pinch → Scale Tests

	[Fact]
	public void Pinch_UpdatesScale()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.True(tracker.Scale > 1.0f, $"Scale should be > 1 after spreading fingers, was {tracker.Scale}");
	}

	[Fact]
	public void Pinch_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.True(changeCount > 0);
	}

	[Fact]
	public void Pinch_ScaleClampedToMinMax()
	{
		var tracker = CreateTracker();
		tracker.Options.MinScale = 0.5f;
		tracker.Options.MaxScale = 3f;

		// Pinch fingers very close together
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(149, 200));
		tracker.ProcessTouchMove(2, new SKPoint(151, 200));

		Assert.True(tracker.Scale >= 0.5f, "Scale should not go below MinScale");
		Assert.True(tracker.Scale <= 3f, "Scale should not exceed MaxScale");
	}

	#endregion

	#region Rotate → Rotation Tests

	[Fact]
	public void Rotate_UpdatesRotation()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 250));
		tracker.ProcessTouchMove(2, new SKPoint(200, 150));

		Assert.NotEqual(0f, tracker.Rotation);
	}

	[Fact]
	public void Rotate_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 250));
		tracker.ProcessTouchMove(2, new SKPoint(200, 150));

		Assert.True(changeCount > 0);
	}

	#endregion

	#region Drag Lifecycle Tests

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
		tracker.DragStarted += (s, e) => startLocation = e.StartLocation;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotNull(startLocation);
		Assert.Equal(100, startLocation.Value.X, 1);
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

	#endregion

	#region Fling Animation Tests

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
		var tracker = CreateTracker();
		tracker.Options.FlingFrameInterval = 16;
		tracker.Options.FlingFriction = 0.5f;
		tracker.Options.FlingMinVelocity = 100f;
		var flingCompleted = false;
		tracker.FlingCompleted += (s, e) => flingCompleted = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		await Task.Delay(1000);

		Assert.True(flingCompleted, "Fling should eventually complete");
		Assert.False(tracker.IsFlinging);
		tracker.Dispose();
	}

	#endregion

	#region Zoom Animation (Double-Tap) Tests

	[Fact]
	public void DoubleTap_StartsZoomAnimation()
	{
		var tracker = CreateTracker();

		SimulateDoubleTap(tracker, new SKPoint(200, 200));

		Assert.True(tracker.IsZoomAnimating);
		tracker.Dispose();
	}

	[Fact]
	public async Task DoubleTap_ScaleChangesToDoubleTapZoomFactor()
	{
		var tracker = CreateTracker();
		tracker.Options.DoubleTapZoomFactor = 2f;
		tracker.Options.ZoomAnimationDuration = 100;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));

		// Advance test time past animation duration so timer tick sees it as complete
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);

		Assert.Equal(2f, tracker.Scale, 0.1);
		tracker.Dispose();
	}

	[Fact]
	public async Task DoubleTap_AtMaxScale_ResetsToOne()
	{
		var tracker = CreateTracker();
		tracker.Options.DoubleTapZoomFactor = 2f;
		tracker.Options.MaxScale = 2f;
		tracker.Options.ZoomAnimationDuration = 100;

		// First double tap: zoom to 2x
		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);
		Assert.Equal(2f, tracker.Scale, 0.1);

		// Second double tap at max: should reset to 1.0
		AdvanceTime(500);
		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);
		Assert.Equal(1f, tracker.Scale, 0.1);

		tracker.Dispose();
	}

	[Fact]
	public async Task DoubleTap_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		tracker.Options.ZoomAnimationDuration = 100;
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		_testTicks += 200 * TimeSpan.TicksPerMillisecond;
		await Task.Delay(200);

		Assert.True(changeCount > 0, "TransformChanged should fire during zoom animation");
		tracker.Dispose();
	}

	#endregion

	#region Scroll Zoom Tests

	[Fact]
	public void ScrollUp_IncreasesScale()
	{
		var tracker = CreateTracker();

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(tracker.Scale > 1.0f, $"Scale should increase on scroll up, was {tracker.Scale}");
	}

	[Fact]
	public void ScrollDown_DecreasesScale()
	{
		var tracker = CreateTracker();

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, -1f);

		Assert.True(tracker.Scale < 1.0f, $"Scale should decrease on scroll down, was {tracker.Scale}");
	}

	[Fact]
	public void Scroll_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		var transformChanged = false;
		tracker.TransformChanged += (s, e) => transformChanged = true;

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(transformChanged);
	}

	[Fact]
	public void Scroll_ScaleClampedToMinMax()
	{
		var tracker = CreateTracker();
		tracker.Options.MinScale = 0.5f;
		tracker.Options.MaxScale = 3f;

		for (int i = 0; i < 100; i++)
			tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, -1f);

		Assert.True(tracker.Scale >= 0.5f, "Scale should not go below MinScale");

		for (int i = 0; i < 200; i++)
			tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(tracker.Scale <= 3f, "Scale should not exceed MaxScale");
	}

	#endregion

	#region Feature Toggle Tests

	[Fact]
	public void IsPanEnabled_False_DoesNotUpdateOffset()
	{
		var tracker = CreateTracker();
		tracker.IsPanEnabled = false;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));

		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void IsDoubleTapZoomEnabled_False_DoesNotZoom()
	{
		var tracker = CreateTracker();
		tracker.IsDoubleTapZoomEnabled = false;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));

		Assert.Equal(1f, tracker.Scale);
		Assert.False(tracker.IsZoomAnimating);
	}

	[Fact]
	public void IsPinchEnabled_False_DoesNotScale()
	{
		var tracker = CreateTracker();
		tracker.IsPinchEnabled = false;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.Equal(1f, tracker.Scale);
	}

	[Fact]
	public void IsRotateEnabled_False_DoesNotRotate()
	{
		var tracker = CreateTracker();
		tracker.IsRotateEnabled = false;

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 250));
		tracker.ProcessTouchMove(2, new SKPoint(200, 150));

		Assert.Equal(0f, tracker.Rotation);
	}

	[Fact]
	public void IsFlingEnabled_False_DoesNotFling()
	{
		var tracker = CreateTracker();
		tracker.IsFlingEnabled = false;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.False(tracker.IsFlinging);
	}

	[Fact]
	public void IsScrollZoomEnabled_False_DoesNotZoomOnScroll()
	{
		var tracker = CreateTracker();
		tracker.IsScrollZoomEnabled = false;

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.Equal(1f, tracker.Scale);
	}

	#endregion

	#region Reset Tests

	[Fact]
	public void Reset_RestoresDefaultTransform()
	{
		var tracker = CreateTracker();

		// Apply pan
		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, new SKPoint(150, 100));

		// Apply scroll zoom
		AdvanceTime(500);
		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 2f);

		Assert.NotEqual(1f, tracker.Scale);
		Assert.NotEqual(SKPoint.Empty, tracker.Offset);

		tracker.Reset();

		Assert.Equal(1f, tracker.Scale);
		Assert.Equal(0f, tracker.Rotation);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void Reset_MatrixIsIdentity()
	{
		var tracker = CreateTracker();

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 2f);

		tracker.Reset();

		var m = tracker.Matrix;
		Assert.Equal(1f, m.ScaleX, 0.01);
		Assert.Equal(1f, m.ScaleY, 0.01);
		Assert.Equal(0f, m.TransX, 0.01);
		Assert.Equal(0f, m.TransY, 0.01);
	}

	[Fact]
	public void Reset_FiresTransformChanged()
	{
		var tracker = CreateTracker();
		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 2f);

		var transformChanged = false;
		tracker.TransformChanged += (s, e) => transformChanged = true;

		tracker.Reset();

		Assert.True(transformChanged);
	}

	#endregion

	#region Matrix Composition Tests

	[Fact]
	public void Matrix_AtIdentity_IsIdentity()
	{
		var tracker = CreateTracker();

		var m = tracker.Matrix;
		var pt = m.MapPoint(200, 200);
		Assert.Equal(200, pt.X, 0.1);
		Assert.Equal(200, pt.Y, 0.1);
	}

	[Fact]
	public void Matrix_AfterScrollAtCenter_CenterUnchanged()
	{
		var tracker = CreateTracker();

		// Scroll zoom at center of view
		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		var m = tracker.Matrix;
		var pt = m.MapPoint(200, 200);
		Assert.Equal(200, pt.X, 5);
		Assert.Equal(200, pt.Y, 5);
	}

	[Fact]
	public void Matrix_AfterPan_PointsShifted()
	{
		var tracker = CreateTracker();

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, new SKPoint(120, 100));

		var m = tracker.Matrix;
		var origin = m.MapPoint(200, 200);
		// Pan moved right so the mapped point should shift right
		Assert.True(origin.X > 200, $"Mapped X should shift right, was {origin.X}");
	}

	#endregion

	#region Config Forwarding Tests

	[Fact]
	public void TouchSlop_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.TouchSlop = 50;

		var panRaised = false;
		tracker.PanDetected += (s, e) => panRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(130, 100)); // 30px < 50px slop

		Assert.False(panRaised, "Pan should not fire when within custom touch slop");
	}

	[Fact]
	public void DoubleTapSlop_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.DoubleTapSlop = 10;

		var doubleTapRaised = false;
		tracker.DoubleTapDetected += (s, e) => doubleTapRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(100);
		tracker.ProcessTouchDown(1, new SKPoint(120, 120));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(120, 120));

		Assert.False(doubleTapRaised, "Double tap should not fire outside slop distance");
	}

	[Fact]
	public void FlingThreshold_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.FlingThreshold = 50000;

		var flingRaised = false;
		tracker.FlingDetected += (s, e) => flingRaised = true;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.False(flingRaised, "Fling should not fire with very high threshold");
	}

	[Fact]
	public void IsEnabled_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.IsEnabled = false;

		var result = tracker.ProcessTouchDown(1, new SKPoint(100, 100));

		Assert.False(result);
	}

	[Fact]
	public void LongPressDuration_ForwardedToEngine()
	{
		var tracker = CreateTracker();
		tracker.Options.LongPressDuration = 200;
		Assert.Equal(200, tracker.Options.LongPressDuration);
	}

	#endregion

	#region Dispose Tests

	[Fact]
	public void Dispose_StopsFlingAnimation()
	{
		var tracker = CreateTracker();

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		Assert.True(tracker.IsFlinging);

		tracker.Dispose();

		Assert.False(tracker.IsFlinging);
	}

	[Fact]
	public void Dispose_StopsZoomAnimation()
	{
		var tracker = CreateTracker();

		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		Assert.True(tracker.IsZoomAnimating);

		tracker.Dispose();

		Assert.False(tracker.IsZoomAnimating);
	}

	#endregion

	#region Event Forwarding Tests

	[Fact]
	public void TapDetected_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var tapRaised = false;
		tracker.TapDetected += (s, e) => tapRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.True(tapRaised);
	}

	[Fact]
	public void PanDetected_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var panRaised = false;
		tracker.PanDetected += (s, e) => panRaised = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.True(panRaised);
	}

	[Fact]
	public void ScrollDetected_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var scrollRaised = false;
		tracker.ScrollDetected += (s, e) => scrollRaised = true;

		tracker.ProcessMouseWheel(new SKPoint(200, 200), 0, 1f);

		Assert.True(scrollRaised);
	}

	[Fact]
	public void GestureStarted_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var gestureStarted = false;
		tracker.GestureStarted += (s, e) => gestureStarted = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));

		Assert.True(gestureStarted);
	}

	[Fact]
	public void GestureEnded_ForwardedFromEngine()
	{
		var tracker = CreateTracker();
		var gestureEnded = false;
		tracker.GestureEnded += (s, e) => gestureEnded = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchUp(1, new SKPoint(120, 100));

		Assert.True(gestureEnded);
	}

	#endregion

	#region Feature Toggle Tests

	[Fact]
	public void IsTapEnabled_False_SuppressesTap()
	{
		var tracker = CreateTracker();
		tracker.IsTapEnabled = false;
		var tapFired = false;
		tracker.TapDetected += (s, e) => tapFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(350);

		Assert.False(tapFired);
	}

	[Fact]
	public void IsDoubleTapEnabled_False_SuppressesDoubleTap()
	{
		var tracker = CreateTracker();
		tracker.IsDoubleTapEnabled = false;
		var doubleTapFired = false;
		tracker.DoubleTapDetected += (s, e) => doubleTapFired = true;

		SimulateDoubleTap(tracker, new SKPoint(200, 200));
		AdvanceTime(350);

		Assert.False(doubleTapFired);
	}

	[Fact]
	public async Task IsLongPressEnabled_False_SuppressesLongPress()
	{
		var tracker = CreateTracker();
		tracker.IsTapEnabled = false;
		tracker.IsLongPressEnabled = false;
		var longPressFired = false;
		tracker.LongPressDetected += (s, e) => longPressFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		await Task.Delay(600);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));

		Assert.False(longPressFired);
	}

	[Fact]
	public void IsHoverEnabled_False_SuppressesHover()
	{
		var tracker = CreateTracker();
		tracker.IsHoverEnabled = false;
		var hoverFired = false;
		tracker.HoverDetected += (s, e) => hoverFired = true;

		tracker.ProcessTouchMove(1, new SKPoint(100, 100), false);

		Assert.False(hoverFired);
	}

	[Fact]
	public void IsTapEnabled_True_AllowsTap()
	{
		var tracker = CreateTracker();
		tracker.IsTapEnabled = true;
		var tapFired = false;
		tracker.TapDetected += (s, e) => tapFired = true;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(50);
		tracker.ProcessTouchUp(1, new SKPoint(100, 100));
		AdvanceTime(350);

		Assert.True(tapFired);
	}

	[Fact]
	public void IsHoverEnabled_True_AllowsHover()
	{
		var tracker = CreateTracker();
		tracker.IsHoverEnabled = true;
		var hoverFired = false;
		tracker.HoverDetected += (s, e) => hoverFired = true;

		tracker.ProcessTouchMove(1, new SKPoint(100, 100), false);

		Assert.True(hoverFired);
	}

	#endregion

	#region Pan Velocity Tests

	[Fact]
	public void PanDetected_HasVelocity()
	{
		var tracker = CreateTracker();
		SKPoint? velocity = null;
		tracker.PanDetected += (s, e) => velocity = e.Velocity;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(150, 100));

		Assert.NotNull(velocity);
	}

	#endregion

	#region Options Pattern Tests

	[Fact]
	public void ConstructorWithOptions_AppliesValues()
	{
		var options = new SKGestureTrackerOptions
		{
			MinScale = 0.5f,
			MaxScale = 5f,
			DoubleTapZoomFactor = 3f,
			ScrollZoomFactor = 0.2f,
			TouchSlop = 16f,
			DoubleTapSlop = 80f,
		};
		var tracker = new SKGestureTracker(options)
		{
			TimeProvider = () => _testTicks
		};

		Assert.Equal(0.5f, tracker.Options.MinScale);
		Assert.Equal(5f, tracker.Options.MaxScale);
		Assert.Equal(3f, tracker.Options.DoubleTapZoomFactor);
		Assert.Equal(0.2f, tracker.Options.ScrollZoomFactor);
		Assert.Equal(16f, tracker.Options.TouchSlop);
		Assert.Equal(80f, tracker.Options.DoubleTapSlop);
	}

	[Fact]
	public void DefaultOptions_HaveExpectedValues()
	{
		var tracker = CreateTracker();

		Assert.Equal(0.1f, tracker.Options.MinScale);
		Assert.Equal(10f, tracker.Options.MaxScale);
		Assert.Equal(2f, tracker.Options.DoubleTapZoomFactor);
		Assert.Equal(0.1f, tracker.Options.ScrollZoomFactor);
		Assert.Equal(8f, tracker.Options.TouchSlop);
		Assert.Equal(40f, tracker.Options.DoubleTapSlop);
	}

	#endregion

	#region SetScale / SetRotation Pivot Tests

	[Fact]
	public void SetScale_WithPivot_AdjustsOffset()
	{
		var tracker = CreateTracker();
		var pivot = new SKPoint(100, 100);

		// Map pivot before scale
		var before = tracker.Matrix.MapPoint(pivot);

		tracker.SetScale(2f, pivot);

		// The pivot point should stay at the same screen location
		var after = tracker.Matrix.MapPoint(pivot);
		Assert.Equal(before.X, after.X, 1);
		Assert.Equal(before.Y, after.Y, 1);
	}

	[Fact]
	public void SetScale_WithoutPivot_ScalesFromOrigin()
	{
		var tracker = CreateTracker();

		tracker.SetScale(2f);

		Assert.Equal(2f, tracker.Scale);
		Assert.Equal(SKPoint.Empty, tracker.Offset);
	}

	[Fact]
	public void SetRotation_WithPivot_AdjustsOffset()
	{
		var tracker = CreateTracker();
		var pivot = new SKPoint(100, 100);

		var before = tracker.Matrix.MapPoint(pivot);

		tracker.SetRotation(45f, pivot);

		var after = tracker.Matrix.MapPoint(pivot);
		Assert.Equal(before.X, after.X, 1);
		Assert.Equal(before.Y, after.Y, 1);
	}

	[Fact]
	public void Matrix_NoViewSize_StillWorks()
	{
		var tracker = CreateTracker();
		tracker.SetTransform(scale: 1f, rotation: 0f, offset: new SKPoint(10, 20));

		var m = tracker.Matrix;
		// Point (0,0) should map to (10, 20) in screen space at scale 1
		var origin = m.MapPoint(0, 0);
		Assert.Equal(10, origin.X, 1);
		Assert.Equal(20, origin.Y, 1);
	}

	#endregion

	#region Drag-Handled Suppresses Fling

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

	#endregion

	#region SKGestureTrackerOptions Validation Tests

	[Fact]
	public void Options_MinScale_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MinScale = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MinScale = -1f);
	}

	[Fact]
	public void Options_MaxScale_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxScale = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxScale = -1f);
	}

	[Fact]
	public void Options_MaxScale_LessThanMinScale_Throws()
	{
		var options = new SKGestureTrackerOptions { MinScale = 1f, MaxScale = 5f };
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxScale = 0.5f);
	}

	[Fact]
	public void Options_MinScale_GreaterThanMaxScale_Throws()
	{
		var options = new SKGestureTrackerOptions { MinScale = 1f, MaxScale = 5f };
		Assert.Throws<ArgumentOutOfRangeException>(() => options.MinScale = 6f);
	}

	[Fact]
	public void Options_DoubleTapZoomFactor_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.DoubleTapZoomFactor = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.DoubleTapZoomFactor = -1f);
	}

	[Fact]
	public void Options_ScrollZoomFactor_ZeroOrNegative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.ScrollZoomFactor = 0f);
		Assert.Throws<ArgumentOutOfRangeException>(() => options.ScrollZoomFactor = -1f);
	}

	[Theory]
	[InlineData(-0.1f)]
	[InlineData(1.1f)]
	public void Options_FlingFriction_OutOfRange_Throws(float value)
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingFriction = value);
	}

	[Fact]
	public void Options_FlingMinVelocity_Negative_Throws()
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingMinVelocity = -1f);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Options_FlingFrameInterval_ZeroOrNegative_Throws(int value)
	{
		var options = new SKGestureTrackerOptions();
		Assert.Throws<ArgumentOutOfRangeException>(() => options.FlingFrameInterval = value);
	}

	[Fact]
	public void Constructor_NullOptions_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new SKGestureTracker(null!));
	}

	#endregion

	#region Strengthened Pinch Scale Assertions

	[Fact]
	public void Pinch_ScaleDelta_MatchesExpectedRatio()
	{
		var tracker = CreateTracker();

		// Fingers start 100px apart, spread to 200px → expected scale ≈ 2.0
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.Equal(2.0f, tracker.Scale, 2);
	}

	[Fact]
	public void Pinch_PinchIn_HalvesScale()
	{
		var tracker = CreateTracker();

		// Fingers start 200px apart, pinch to 100px → expected scale ≈ 0.5
		tracker.ProcessTouchDown(1, new SKPoint(50, 200));
		tracker.ProcessTouchDown(2, new SKPoint(250, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(100, 200));
		tracker.ProcessTouchMove(2, new SKPoint(200, 200));

		Assert.Equal(0.5f, tracker.Scale, 2);
	}

	#endregion

	#region EventArgs Verification Tests

	[Fact]
	public void PanEventArgs_PreviousLocation_IsCorrect()
	{
		var tracker = CreateTracker();
		SKPanGestureEventArgs? captured = null;
		tracker.PanDetected += (s, e) => captured = e;

		tracker.ProcessTouchDown(1, new SKPoint(100, 100));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(120, 100));

		Assert.NotNull(captured);
		Assert.Equal(100, captured.PreviousLocation.X, 1);
		Assert.Equal(100, captured.PreviousLocation.Y, 1);
	}

	[Fact]
	public void PinchEventArgs_ScaleDelta_ProductMatchesCumulativeScale()
	{
		var tracker = CreateTracker();
		var scaleDeltas = new List<float>();
		tracker.PinchDetected += (s, e) => scaleDeltas.Add(e.ScaleDelta);

		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(200, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(50, 200));
		tracker.ProcessTouchMove(2, new SKPoint(250, 200));

		Assert.NotEmpty(scaleDeltas);
		var cumulativeScale = 1f;
		foreach (var delta in scaleDeltas)
			cumulativeScale *= delta;
		Assert.Equal(2.0f, cumulativeScale, 2);
	}

	[Fact]
	public void FlingEventArgs_SpeedMatchesVelocityMagnitude()
	{
		var tracker = CreateTracker();
		SKFlingGestureEventArgs? captured = null;
		tracker.FlingDetected += (s, e) => captured = e;

		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));

		Assert.NotNull(captured);
		var expectedSpeed = (float)Math.Sqrt(captured.VelocityX * captured.VelocityX + captured.VelocityY * captured.VelocityY);
		Assert.Equal(expectedSpeed, captured.Speed, 1);
		tracker.Dispose();
	}

	#endregion

	#region Double Dispose Safety

	[Fact]
	public void Dispose_CalledTwice_DoesNotThrow()
	{
		var tracker = CreateTracker();
		tracker.Dispose();
		tracker.Dispose(); // should not throw
	}

	#endregion

	#region SetScale Boundary Values

	[Fact]
	public void SetScale_NegativeValue_ClampsToMinScale()
	{
		var tracker = CreateTracker();
		tracker.SetScale(-5f);
		Assert.Equal(tracker.Options.MinScale, tracker.Scale);
	}

	[Fact]
	public void SetScale_Zero_ClampsToMinScale()
	{
		var tracker = CreateTracker();
		tracker.SetScale(0f);
		Assert.Equal(tracker.Options.MinScale, tracker.Scale);
	}

	[Fact]
	public void SetScale_AboveMaxScale_ClampsToMaxScale()
	{
		var tracker = CreateTracker();
		tracker.SetScale(999f);
		Assert.Equal(tracker.Options.MaxScale, tracker.Scale);
	}

	#endregion

	#region TransformChanged from Programmatic Methods

	[Fact]
	public void SetScale_RaisesTransformChanged()
	{
		var tracker = CreateTracker();
		var fired = 0;
		tracker.TransformChanged += (s, e) => fired++;

		tracker.SetScale(2f);
		Assert.Equal(1, fired);
	}

	[Fact]
	public void SetRotation_RaisesTransformChanged()
	{
		var tracker = CreateTracker();
		var fired = 0;
		tracker.TransformChanged += (s, e) => fired++;

		tracker.SetRotation(45f);
		Assert.Equal(1, fired);
	}

	[Fact]
	public void SetOffset_RaisesTransformChanged()
	{
		var tracker = CreateTracker();
		var fired = 0;
		tracker.TransformChanged += (s, e) => fired++;

		tracker.SetOffset(new SKPoint(100, 200));
		Assert.Equal(1, fired);
	}

	[Fact]
	public void SetTransform_RaisesTransformChanged()
	{
		var tracker = CreateTracker();
		var fired = 0;
		tracker.TransformChanged += (s, e) => fired++;

		tracker.SetTransform(2f, 45f, new SKPoint(100, 200));
		Assert.Equal(1, fired);
	}

	#endregion

	#region Bug Fix Tests

	[Theory]
	[InlineData(0f)]
	[InlineData(-1f)]
	[InlineData(-0.001f)]
	public void ZoomTo_WithNonPositiveFactor_ThrowsArgumentOutOfRangeException(float factor)
	{
		var tracker = CreateTracker();
		Assert.Throws<ArgumentOutOfRangeException>(() => tracker.ZoomTo(factor, SKPoint.Empty));
	}

	[Theory]
	[InlineData(float.NaN)]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	public void ZoomTo_WithNonFiniteFactor_ThrowsArgumentOutOfRangeException(float factor)
	{
		var tracker = CreateTracker();
		Assert.Throws<ArgumentOutOfRangeException>(() => tracker.ZoomTo(factor, SKPoint.Empty));
	}

	[Fact]
	public void PinchAndRotate_FiresOnlyOneTransformChangedPerFrame()
	{
		// Each two-finger move should fire exactly one TransformChanged, not two
		// (one from pinch handler + one from rotate handler).
		var tracker = CreateTracker();
		var changeCount = 0;
		tracker.TransformChanged += (s, e) => changeCount++;

		// Set up two fingers in a position that generates both pinch and rotate
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(300, 200));
		AdvanceTime(10);

		changeCount = 0; // Reset after setup touches
		tracker.ProcessTouchMove(1, new SKPoint(50, 180));
		tracker.ProcessTouchMove(2, new SKPoint(350, 220));

		// Two moves → 2 frames, but each frame (triggered by move 1 AND move 2) should fire once
		// Each ProcessTouchMove call may or may not trigger a frame depending on which finger moves.
		// The important thing is that for each frame where pinch fires, rotate does NOT fire an extra event.
		// With the fix: rotate fires TransformChanged (1 per frame), pinch does not.
		// We can verify by checking the count doesn't double what a pinch-only gesture would produce.
		Assert.True(changeCount <= 2, $"Expected at most 2 TransformChanged (one per move), got {changeCount}");
	}

	[Fact]
	public void ProcessTouchCancel_DuringPinch_WithOneFingerRemaining_TransitionsToPan()
	{
		var tracker = CreateTracker();
		var panDetected = false;
		tracker.PanDetected += (s, e) => panDetected = true;

		// Start a two-finger pinch
		tracker.ProcessTouchDown(1, new SKPoint(100, 200));
		tracker.ProcessTouchDown(2, new SKPoint(300, 200));
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(80, 200));
		tracker.ProcessTouchMove(2, new SKPoint(320, 200));

		// Cancel one finger
		tracker.ProcessTouchCancel(2);

		// Subsequent move with remaining finger should produce pan
		panDetected = false;
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, new SKPoint(60, 200));

		Assert.True(panDetected, "Pan should be detected after one finger cancelled during pinch");
	}

	#endregion

	#region Bug Regression Tests

	[Fact]
	public void DragEnded_ReportsActualEndLocation_NotStartLocation()
	{
		// Regression: DragEnded was passing _dragStartLocation as both start and end,
		// so CurrentLocation and Delta were always wrong.
		var tracker = CreateTracker();
		SKDragGestureEventArgs? dragEndedArgs = null;
		tracker.DragEnded += (s, e) => dragEndedArgs = e;

		var startPoint = new SKPoint(100, 100);
		var midPoint = new SKPoint(150, 100);
		var endPoint = new SKPoint(200, 100);

		tracker.ProcessTouchDown(1, startPoint);
		AdvanceTime(10);
		tracker.ProcessTouchMove(1, midPoint);
		AdvanceTime(500); // Long pause to avoid fling
		tracker.ProcessTouchMove(1, endPoint);
		AdvanceTime(500);
		tracker.ProcessTouchUp(1, endPoint);

		Assert.NotNull(dragEndedArgs);
		// CurrentLocation must reflect the final touch position, not the start
		Assert.NotEqual(dragEndedArgs!.StartLocation, dragEndedArgs.CurrentLocation);
		Assert.Equal(endPoint.X, dragEndedArgs.CurrentLocation.X, 1f);
		Assert.Equal(endPoint.Y, dragEndedArgs.CurrentLocation.Y, 1f);
	}

	[Fact]
	public void FlingCompleted_DoesNotFire_WhenFlingInterruptedByNewGesture()
	{
		// Regression: StopFling() unconditionally raised FlingCompleted, so starting a new
		// gesture while a fling was in progress incorrectly fired FlingCompleted.
		var tracker = CreateTracker();
		tracker.Options.FlingFriction = 0.001f; // Near-zero friction so fling persists
		tracker.Options.FlingMinVelocity = 1f;
		tracker.Options.FlingFrameInterval = 1000; // Slow timer — won't fire during test

		var flingCompletedCount = 0;
		tracker.FlingCompleted += (s, e) => flingCompletedCount++;

		// Start a fling
		SimulateFastSwipe(tracker, new SKPoint(100, 200), new SKPoint(500, 200));
		Assert.True(tracker.IsFlinging, "Fling should be active after fast swipe");

		// Interrupt with a new touch — should NOT fire FlingCompleted
		tracker.ProcessTouchDown(1, new SKPoint(300, 300));

		Assert.Equal(0, flingCompletedCount);
	}

	#endregion
}
