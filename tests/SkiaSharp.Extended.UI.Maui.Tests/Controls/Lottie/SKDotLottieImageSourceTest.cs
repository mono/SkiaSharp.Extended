using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKDotLottieImageSourceTest : SKLottieImageSourceTest<SKDotLottieImageSource>
{
	private const string TestLottieFile = "TestAssets/Lottie/test.lottie";

	protected override SKDotLottieImageSource CreateEmptyImageSource() =>
		new SKDotLottieImageSource { };

	protected override SKDotLottieImageSource CreateCompleteImageSource() =>
		new SKDotLottieImageSource { File = TestLottieFile };

	protected override void UpdateImageSource(SKDotLottieImageSource imageSource, bool first) =>
		imageSource.File = TestLottieFile;

	protected override void ResetImageSource(SKDotLottieImageSource imageSource) =>
		imageSource.File = null;

	[Fact]
	public async Task DotLottieFileLoadsSuccessfully()
	{
		// create & set source
		var lottie = new WaitingLottieView
		{
			Source = new SKDotLottieImageSource { File = TestLottieFile }
		};

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public void FilePropertyCanBeSetAndRetrieved()
	{
		var source = new SKDotLottieImageSource();
		Assert.Null(source.File);

		source.File = TestLottieFile;
		Assert.Equal(TestLottieFile, source.File);
	}
}
