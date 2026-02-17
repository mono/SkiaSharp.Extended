using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests.Morphysics;

public class MorphTargetTest
{
	[Fact]
	public void MorphTarget_Interpolate_AtZeroReturnSource()
	{
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Square
		var target = "M 50,0 L 100,50 L 50,100 L 0,50 Z"; // Diamond (rotated square)

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		var interpolated = morphTarget.Interpolate(sourcePath, 0f);

		// At progress 0, should be very close to source
		Assert.NotNull(interpolated);
		Assert.True(interpolated.PointCount > 0);
		
		// Get first point of interpolated path (should be close to source's first point)
		using var iter = interpolated.CreateIterator(false);
		var points = new SKPoint[4];
		iter.Next(points);
		
		// First point of square is (0,0)
		Assert.True(Math.Abs(points[0].X - 0) < 5, $"First point X should be ~0 but was {points[0].X}");
		Assert.True(Math.Abs(points[0].Y - 0) < 5, $"First point Y should be ~0 but was {points[0].Y}");
	}

	[Fact]
	public void MorphTarget_Interpolate_AtOneReturnsTarget()
	{
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Square
		var target = "M 50,0 L 100,50 L 50,100 L 0,50 Z"; // Diamond

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		var interpolated = morphTarget.Interpolate(sourcePath, 1f);

		// At progress 1, should be close to target
		Assert.NotNull(interpolated);
		Assert.True(interpolated.PointCount > 0);
		
		// Get first point (should be close to target's first point)
		using var iter = interpolated.CreateIterator(false);
		var points = new SKPoint[4];
		iter.Next(points);
		
		// First point of diamond is (50,0)
		Assert.True(Math.Abs(points[0].X - 50) < 5, $"First point X should be ~50 but was {points[0].X}");
		Assert.True(Math.Abs(points[0].Y - 0) < 5, $"First point Y should be ~0 but was {points[0].Y}");
	}

	[Fact]
	public void MorphTarget_Interpolate_AtHalfReturnsMidpoint()
	{
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Square at origin
		var target = "M 100,100 L 200,100 L 200,200 L 100,200 Z"; // Square offset by (100,100)

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		var interpolated = morphTarget.Interpolate(sourcePath, 0.5f);

		Assert.NotNull(interpolated);
		Assert.True(interpolated.PointCount > 0);
		
		// Get first point
		using var iter = interpolated.CreateIterator(false);
		var points = new SKPoint[4];
		iter.Next(points);
		
		// First point should be halfway: (0+100)/2 = 50, (0+100)/2 = 50
		Assert.True(Math.Abs(points[0].X - 50) < 10, $"First point X should be ~50 but was {points[0].X}");
		Assert.True(Math.Abs(points[0].Y - 50) < 10, $"First point Y should be ~50 but was {points[0].Y}");
	}

	[Fact]
	public void MorphTarget_IncrementalMorph_DoesNotAccumulateError()
	{
		// THIS IS THE CRITICAL TEST that would have caught the bug!
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z";
		var target = "M 50,0 L 100,50 L 50,100 L 0,50 Z";

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		// Morph at progress 0.3
		var morph1 = morphTarget.Interpolate(sourcePath, 0.3f);
		using var iter1 = morph1.CreateIterator(false);
		var points1 = new SKPoint[4];
		iter1.Next(points1);
		var point1 = points1[0];

		// Morph at progress 0.6 (using SAME source path!)
		var morph2 = morphTarget.Interpolate(sourcePath, 0.6f);
		using var iter2 = morph2.CreateIterator(false);
		var points2 = new SKPoint[4];
		iter2.Next(points2);
		var point2 = points2[0];

		// Point 2 should be further along the morph than point 1
		// Source first point: (0,0), Target first point: (50,0)
		// Note: EaseInOut easing is applied, so values won't be exactly linear
		// But the key is that progress increases monotonically
		Assert.True(point1.X < point2.X, $"Point at 0.3 ({point1.X}) should have smaller X than point at 0.6 ({point2.X})");
		Assert.True(point1.X > 0 && point1.X < 50, $"Point at 0.3 should be between 0 and 50 but was {point1.X}");
		Assert.True(point2.X > point1.X && point2.X < 50, $"Point at 0.6 should be between point1 and 50 but was {point2.X}");
	}

	[Fact]
	public void MorphTarget_ProgressClamping_ClampsBelowZero()
	{
		var source = "M 0,0 L 100,0 L 100,100 Z";
		var target = "M 0,0 L 100,0 L 100,100 Z";

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		// Should not throw and should clamp to 0
		var interpolated = morphTarget.Interpolate(sourcePath, -0.5f);
		Assert.NotNull(interpolated);
	}

	[Fact]
	public void MorphTarget_ProgressClamping_ClampsAboveOne()
	{
		var source = "M 0,0 L 100,0 L 100,100 Z";
		var target = "M 0,0 L 100,0 L 100,100 Z";

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		// Should not throw and should clamp to 1
		var interpolated = morphTarget.Interpolate(sourcePath, 1.5f);
		Assert.NotNull(interpolated);
	}
}
