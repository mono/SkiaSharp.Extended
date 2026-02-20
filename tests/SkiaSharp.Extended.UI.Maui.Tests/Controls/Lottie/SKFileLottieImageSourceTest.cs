namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKFileLottieImageSourceTest : SKLottieImageSourceTest<SKFileLottieImageSource>
{
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";
	private const string LoloJson = "TestAssets/Lottie/lolo.json";
	private const string TestLottieFile = "TestAssets/Lottie/test.lottie";

	protected override SKFileLottieImageSource CreateEmptyImageSource() =>
		new SKFileLottieImageSource { };

	protected override SKFileLottieImageSource CreateCompleteImageSource() =>
		new SKFileLottieImageSource { File = TrophyJson };

	protected override void UpdateImageSource(SKFileLottieImageSource imageSource, bool first) =>
		imageSource.File = first ? TrophyJson : LoloJson;

	protected override void ResetImageSource(SKFileLottieImageSource imageSource) =>
		imageSource.File = null;

	[Fact]
	public async Task DotLottieFileLoadsSuccessfully()
	{
		// create & set source - SKFileLottieImageSource auto-detects .lottie format
		var lottie = new WaitingLottieView
		{
			Source = new SKFileLottieImageSource { File = TestLottieFile }
		};

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task JsonFileLoadsSuccessfully()
	{
		// create & set source - SKFileLottieImageSource auto-detects JSON format
		var lottie = new WaitingLottieView
		{
			Source = new SKFileLottieImageSource { File = TrophyJson }
		};

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}
}
