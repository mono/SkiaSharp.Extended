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
}
