using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKLottieViewTest
{
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";
	private const string LoloJson = "TestAssets/Lottie/lolo.json";

	[Fact]
	public async Task EnsureAnimationIsLoaded()
	{
		var lottie = new WaitingLottieView { Source = new SKFileLottieImageSource { File = TrophyJson } };
		await lottie.LoadedTask;

		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task EnsureNewAnimationResetsProgress()
	{
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		lottie.ResetTask();
		source.File = LoloJson;
		await lottie.LoadedTask;

		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task ReverseWithZeroRepeatCount_CompletesAfterForwardAndBackCycle()
	{
		var lottie = new WaitingLottieView
		{
			RepeatMode = SKLottieRepeatMode.Reverse,
			RepeatCount = 0,
			Source = new SKFileLottieImageSource { File = TrophyJson },
		};
		await lottie.LoadedTask;

		lottie.CallUpdate(lottie.Duration);
		Assert.False(lottie.IsComplete);

		lottie.CallUpdate(lottie.Duration);
		Assert.True(lottie.IsComplete);
	}

	[Fact]
	public async Task ChangingRepeatAfterCompletion_UpdatesIsComplete()
	{
		var lottie = new WaitingLottieView { Source = new SKFileLottieImageSource { File = TrophyJson } };
		await lottie.LoadedTask;

		lottie.CallUpdate(lottie.Duration);
		Assert.True(lottie.IsComplete);

		lottie.RepeatCount = 1;

		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task SourceChangeCancelsPreviousLoad()
	{
		var lottie = new WaitingLottieView { Source = new SKFileLottieImageSource { File = TrophyJson } };
		lottie.ResetTask();
		lottie.Source = new SKFileLottieImageSource { File = LoloJson };
		await lottie.LoadedTask;

		Assert.NotEqual(TimeSpan.Zero, lottie.Duration);
	}
}
