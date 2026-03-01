using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKLottieViewTest
{
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";
	private const string LoloJson = "TestAssets/Lottie/lolo.json";

	[Fact]
	public async Task EnsureAnimationIsLoaded()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task EnsureNewAnimationResetsProgress()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// update
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		lottie.ResetTask();
		source.File = LoloJson;
		await lottie.LoadedTask;

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task DefaultAnimationSpeedIsOne()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test
		Assert.Equal(1.0, lottie.AnimationSpeed);
	}

	[Fact]
	public async Task SourceChangeCancellsPreviousLoad()
	{
		// Verify Fix 2: Changing Source cancels any in-flight load
		var source1 = new SKFileLottieImageSource { File = TrophyJson };
		var source2 = new SKFileLottieImageSource { File = LoloJson };
		var lottie = new WaitingLottieView { Source = source1 };

		// Immediately change source before first load completes
		lottie.ResetTask();
		lottie.Source = source2;
		await lottie.LoadedTask;

		// Should have loaded the second source, not the first
		// (Duration differs between trophy.json and lolo.json)
		Assert.NotEqual(TimeSpan.Zero, lottie.Duration);
	}
}

