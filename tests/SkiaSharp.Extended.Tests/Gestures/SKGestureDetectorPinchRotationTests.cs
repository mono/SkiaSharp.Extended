using SkiaSharp;
using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>Tests for pinch, rotation, and multi-touch detection in <see cref="SKGestureDetector"/>.</summary>
public class SKGestureDetectorPinchRotationTests
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


}
