using System;
using Xunit;

namespace SkiaSharp.Extended.Tests.Animation;

public class SKAnimationEasingTest
{
	// All easing functions should map t=0 → 0 and t=1 → 1
	[Theory]
	[InlineData(0.0, 0.0)]
	[InlineData(1.0, 1.0)]
	public void Linear_BoundaryValues(double t, double expected)
	{
		Assert.Equal(expected, SKAnimationEasing.Linear(t), precision: 10);
	}

	[Fact]
	public void Linear_IsIdentity()
	{
		Assert.Equal(0.5, SKAnimationEasing.Linear(0.5), precision: 10);
	}

	[Theory]
	[InlineData(0.0, 0.0)]
	[InlineData(1.0, 1.0)]
	public void CubicOut_BoundaryValues(double t, double expected)
	{
		Assert.Equal(expected, SKAnimationEasing.CubicOut(t), precision: 10);
	}

	[Fact]
	public void CubicOut_GreaterThanLinearMidpoint()
	{
		// CubicOut starts fast, so at t=0.5 it should be > 0.5
		var cubicOut = SKAnimationEasing.CubicOut(0.5);
		Assert.True(cubicOut > 0.5, $"CubicOut(0.5) = {cubicOut} should be > 0.5");
	}

	[Fact]
	public void CubicOut_IsMonotonic()
	{
		double prev = SKAnimationEasing.CubicOut(0.0);
		for (int i = 1; i <= 10; i++)
		{
			double current = SKAnimationEasing.CubicOut(i / 10.0);
			Assert.True(current >= prev, $"CubicOut not monotonic at t={i / 10.0}: {current} < {prev}");
			prev = current;
		}
	}

	[Theory]
	[InlineData(0.0, 0.0)]
	[InlineData(1.0, 1.0)]
	public void QuadOut_BoundaryValues(double t, double expected)
	{
		Assert.Equal(expected, SKAnimationEasing.QuadOut(t), precision: 10);
	}

	[Fact]
	public void QuadIn_BoundaryValues()
	{
		Assert.Equal(0.0, SKAnimationEasing.QuadIn(0.0), precision: 10);
		Assert.Equal(1.0, SKAnimationEasing.QuadIn(1.0), precision: 10);
	}

	[Fact]
	public void QuadIn_SlowerThanLinearMidpoint()
	{
		// QuadIn starts slow, so at t=0.5 it should be < 0.5
		var quadIn = SKAnimationEasing.QuadIn(0.5);
		Assert.True(quadIn < 0.5, $"QuadIn(0.5) = {quadIn} should be < 0.5");
	}

	[Fact]
	public void QuadOut_FasterThanLinearMidpoint()
	{
		// QuadOut starts fast, so at t=0.5 it should be > 0.5
		var quadOut = SKAnimationEasing.QuadOut(0.5);
		Assert.True(quadOut > 0.5, $"QuadOut(0.5) = {quadOut} should be > 0.5");
	}

	[Theory]
	[InlineData(0.0, 0.0)]
	[InlineData(1.0, 1.0)]
	public void CubicIn_BoundaryValues(double t, double expected)
	{
		Assert.Equal(expected, SKAnimationEasing.CubicIn(t), precision: 10);
	}

	[Theory]
	[InlineData(0.0, 0.0)]
	[InlineData(1.0, 1.0)]
	public void QuadInOut_BoundaryValues(double t, double expected)
	{
		Assert.Equal(expected, SKAnimationEasing.QuadInOut(t), precision: 10);
	}

	[Fact]
	public void QuadInOut_MidpointIsHalf()
	{
		// InOut easing should pass through (0.5, 0.5)
		Assert.Equal(0.5, SKAnimationEasing.QuadInOut(0.5), precision: 10);
	}

	[Theory]
	[InlineData(0.0, 0.0)]
	[InlineData(1.0, 1.0)]
	public void CubicInOut_BoundaryValues(double t, double expected)
	{
		Assert.Equal(expected, SKAnimationEasing.CubicInOut(t), precision: 10);
	}

	[Fact]
	public void CubicInOut_MidpointIsHalf()
	{
		// InOut easing should pass through (0.5, 0.5)
		Assert.Equal(0.5, SKAnimationEasing.CubicInOut(0.5), precision: 10);
	}

	[Fact]
	public void CubicOut_KnownValue()
	{
		// CubicOut(0.5) = 1 - (1-0.5)^3 = 1 - 0.125 = 0.875
		Assert.Equal(0.875, SKAnimationEasing.CubicOut(0.5), precision: 10);
	}

	[Fact]
	public void QuadIn_KnownValue()
	{
		// QuadIn(0.5) = 0.5^2 = 0.25
		Assert.Equal(0.25, SKAnimationEasing.QuadIn(0.5), precision: 10);
	}

	[Fact]
	public void QuadOut_KnownValue()
	{
		// QuadOut(0.5) = 0.5 * (2 - 0.5) = 0.75
		Assert.Equal(0.75, SKAnimationEasing.QuadOut(0.5), precision: 10);
	}

	[Fact]
	public void CubicIn_KnownValue()
	{
		// CubicIn(0.5) = 0.5^3 = 0.125
		Assert.Equal(0.125, SKAnimationEasing.CubicIn(0.5), precision: 10);
	}
}
