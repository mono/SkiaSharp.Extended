using System.Text;

namespace SkiaSharp.Extended.Tests;

public class SKLottiePlayerTest
{
	private const string MinimalLottieJson =
		"""{"v":"5.7.4","fr":60,"ip":0,"op":60,"w":100,"h":100,"nm":"test","ddd":0,"assets":[],"layers":[]}""";

	private static Skottie.Animation CreateAnimation()
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MinimalLottieJson));
		return SKLottieAnimationLoader.Load(stream);
	}

	[Fact]
	public void Animation_IsNonOwningAndSetsPlaybackState()
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer { Animation = animation };

		Assert.True(player.HasAnimation);
		Assert.Same(animation, player.Animation);
		Assert.Equal(TimeSpan.FromSeconds(1), player.Duration);
		Assert.Equal(TimeSpan.Zero, player.Progress);
	}

	[Fact]
	public void RestartRepeat_CompletesAfterInitialAndAdditionalPlays()
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer
		{
			Animation = animation,
			Repeat = SKLottieRepeat.Restart(2),
		};

		player.Update(TimeSpan.FromSeconds(3));

		Assert.True(player.IsComplete);
		Assert.Equal(player.Duration, player.Progress);
	}

	[Fact]
	public void ReverseRepeat_CompletesAfterForwardAndReverseCycle()
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer
		{
			Animation = animation,
			Repeat = SKLottieRepeat.Reverse(0),
		};

		player.Update(player.Duration);
		Assert.False(player.IsComplete);

		player.Update(player.Duration);

		Assert.True(player.IsComplete);
		Assert.Equal(TimeSpan.Zero, player.Progress);
	}

	[Fact]
	public void Seek_ClampsAndResumesCompletedPlayback()
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer { Animation = animation };
		player.Update(TimeSpan.FromSeconds(2));

		player.Seek(TimeSpan.FromSeconds(-1));

		Assert.False(player.IsComplete);
		Assert.Equal(TimeSpan.Zero, player.Progress);
	}

	[Theory]
	[InlineData(1.0, false)]
	[InlineData(-1.0, true)]
	public void NegativeDelta_SaturatesWithoutOverflow(double speed, bool reachesEnd)
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer { AnimationSpeed = speed, Animation = animation };
		player.Seek(TimeSpan.FromTicks(1));

		player.Update(TimeSpan.MinValue);

		Assert.Equal(reachesEnd ? player.Duration : TimeSpan.Zero, player.Progress);
		Assert.False(player.IsComplete);
	}

	[Fact]
	public void NegativeDelta_WithNegativeSpeedMovesForwardWithoutConsumingRepeats()
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer
		{
			AnimationSpeed = -1,
			Repeat = SKLottieRepeat.Restart(1),
			Animation = animation,
		};
		player.Seek(TimeSpan.FromSeconds(0.25));

		player.Update(TimeSpan.FromSeconds(-0.5));

		Assert.Equal(TimeSpan.FromSeconds(0.75), player.Progress);
		Assert.False(player.IsComplete);
	}

	[Theory]
	[InlineData(double.NaN, 0.5)]
	[InlineData(0, 0.5)]
	[InlineData(double.PositiveInfinity, 1)]
	[InlineData(double.NegativeInfinity, 0)]
	public void SpeedEdgeCases_ScaleAndClamp(double speed, double expectedSeconds)
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer { AnimationSpeed = speed, Animation = animation };
		player.Seek(TimeSpan.FromSeconds(0.5));

		player.Update(TimeSpan.FromSeconds(1));

		Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), player.Progress);
		Assert.Equal(double.IsInfinity(speed), player.IsComplete);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void LargeDelta_FiniteRepeatsCompleteAtExpectedBoundary(bool reverse)
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer
		{
			Repeat = reverse ? SKLottieRepeat.Reverse(1) : SKLottieRepeat.Restart(1),
			Animation = animation,
		};

		player.Update(TimeSpan.MaxValue);

		Assert.True(player.IsComplete);
		Assert.Equal(reverse ? TimeSpan.Zero : player.Duration, player.Progress);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void NegativeSpeed_FiniteRepeatsCompleteAtExpectedBoundary(bool reverse)
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer
		{
			AnimationSpeed = -1,
			Repeat = reverse ? SKLottieRepeat.Reverse(1) : SKLottieRepeat.Restart(1),
			Animation = animation,
		};

		player.Update(TimeSpan.FromSeconds(4));

		Assert.True(player.IsComplete);
		Assert.Equal(reverse ? player.Duration : TimeSpan.Zero, player.Progress);
	}

	[Fact]
	public void AnimationUpdated_FiresForProcessedNoOpButNotAfterCompletion()
	{
		using var animation = CreateAnimation();
		var player = new SKLottiePlayer { Animation = animation };
		var updates = 0;
		player.AnimationUpdated += (_, _) => updates++;

		player.Seek(TimeSpan.Zero);
		player.AnimationSpeed = 0;
		player.Update(TimeSpan.FromSeconds(1));
		Assert.Equal(2, updates);

		player.AnimationSpeed = 1;
		player.Update(player.Duration);
		player.Update(player.Duration);
		Assert.Equal(3, updates);
	}
}
