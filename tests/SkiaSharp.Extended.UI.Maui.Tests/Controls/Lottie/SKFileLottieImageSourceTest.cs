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
	public void IsEmptyReturnsTrueForDotLottieWhenFileIsNull()
	{
		var source = new SKFileLottieImageSource();
		Assert.True(source.IsEmpty);
	}

	[Fact]
	public void IsEmptyReturnsFalseForDotLottieWhenFileIsSet()
	{
		var source = new SKFileLottieImageSource { File = TestLottieFile };
		Assert.False(source.IsEmpty);
	}

	[Fact]
	public async Task LoadAnimationAsyncReturnsEmptyAnimationForDotLottieWhenFileIsNull()
	{
		var source = new SKFileLottieImageSource();
		var animation = await source.LoadAnimationAsync();
		Assert.False(animation.IsLoaded);
	}
}
