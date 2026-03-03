using SkiaSharp;
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for matrix composition, pivot, and SetScale/SetRotation in <see cref="SKGestureTracker"/>.</summary>
public class SKGestureTrackerTransformTests
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
	public void SetScale_WithPivot_NonZeroInitialOffset_PivotRemainsFixed()
	{
		// Regression: AdjustOffsetForPivot previously ignored _offset when converting
		// pivot to content space, causing the pivot point to jump when content was panned.
		var tracker = CreateTracker();
		tracker.SetTransform(scale: 1f, rotation: 0f, offset: new SKPoint(50, 30));

		// Choose a content-space point and find its current screen position (the pivot)
		var contentPoint = new SKPoint(100, 100);
		var screenPivot = tracker.Matrix.MapPoint(contentPoint);

		tracker.SetScale(2f, screenPivot);

		// The content point should still map to the same screen position
		var after = tracker.Matrix.MapPoint(contentPoint);
		Assert.Equal(screenPivot.X, after.X, 1);
		Assert.Equal(screenPivot.Y, after.Y, 1);
	}

	[Fact]
	public void SetRotation_WithPivot_NonZeroInitialOffset_PivotRemainsFixed()
	{
		// Regression: AdjustOffsetForPivot previously ignored _offset when converting
		// pivot to content space, causing the pivot point to drift during rotation.
		var tracker = CreateTracker();
		tracker.SetTransform(scale: 1f, rotation: 0f, offset: new SKPoint(50, 30));

		var contentPoint = new SKPoint(100, 100);
		var screenPivot = tracker.Matrix.MapPoint(contentPoint);

		tracker.SetRotation(45f, screenPivot);

		var after = tracker.Matrix.MapPoint(contentPoint);
		Assert.Equal(screenPivot.X, after.X, 1);
		Assert.Equal(screenPivot.Y, after.Y, 1);
	}

	[Fact]
	public void SetScale_WithPivot_NonZeroScaleAndRotation_PivotRemainsFixed()
	{
		// Verify pivot correctness when both scale and rotation are non-trivial.
		var tracker = CreateTracker();
		tracker.SetTransform(scale: 1.5f, rotation: 30f, offset: new SKPoint(20, -10));

		var contentPoint = new SKPoint(80, 60);
		var screenPivot = tracker.Matrix.MapPoint(contentPoint);

		tracker.SetScale(3f, screenPivot);

		var after = tracker.Matrix.MapPoint(contentPoint);
		Assert.Equal(screenPivot.X, after.X, 1);
		Assert.Equal(screenPivot.Y, after.Y, 1);
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


}
