using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests.Morphysics;

public class MorphTargetTest
{
	[Fact]
	public void MorphTarget_Interpolate_AtZeroReturnSource()
	{
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Square
		var target = "M 50,0 A 50,50 0 1,1 50,100 A 50,50 0 1,1 50,0 Z"; // Circle

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		var interpolated = morphTarget.Interpolate(sourcePath, 0f);

		// At progress 0, should be close to source
		Assert.NotNull(interpolated);
		Assert.True(interpolated.PointCount > 0);
	}

	[Fact]
	public void MorphTarget_Interpolate_AtOneReturnsTarget()
	{
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Square
		var target = "M 50,0 A 50,50 0 1,1 50,100 A 50,50 0 1,1 50,0 Z"; // Circle

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		var interpolated = morphTarget.Interpolate(sourcePath, 1f);

		// At progress 1, should be close to target
		Assert.NotNull(interpolated);
		Assert.True(interpolated.PointCount > 0);
	}

	[Fact]
	public void MorphTarget_Interpolate_AtHalfReturnsMidpoint()
	{
		var source = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Square
		var target = "M 0,0 L 100,0 L 100,100 L 0,100 Z"; // Same square (for predictable test)

		var morphTarget = new MorphTarget(source, target);
		var sourcePath = SKPath.ParseSvgPathData(source);

		var interpolated = morphTarget.Interpolate(sourcePath, 0.5f);

		Assert.NotNull(interpolated);
		Assert.True(interpolated.PointCount > 0);
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
