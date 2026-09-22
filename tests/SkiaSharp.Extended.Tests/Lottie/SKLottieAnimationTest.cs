using System;
using SkiaSharp.Skottie;
using Xunit;

namespace SkiaSharp.Extended.Tests;

public class SKLottieAnimationTest
{
	private const string MinimalLottieJson =
		"""{"v":"5.7.4","fr":60,"ip":0,"op":60,"w":100,"h":100,"nm":"test","ddd":0,"assets":[],"layers":[]}""";

	[Fact]
	public void EmptyResult_HasNoAnimation()
	{
		var result = new SKLottieAnimation();

		Assert.False(result.IsLoaded);
		Assert.Null(result.Animation);
	}

	[Fact]
	public void Result_ExposesAnimation()
	{
		using var animation = Animation.Parse(MinimalLottieJson)
			?? throw new InvalidOperationException("Failed to parse test animation.");
		var result = new SKLottieAnimation(animation);

		Assert.True(result.IsLoaded);
		Assert.Same(animation, result.Animation);
	}
}
