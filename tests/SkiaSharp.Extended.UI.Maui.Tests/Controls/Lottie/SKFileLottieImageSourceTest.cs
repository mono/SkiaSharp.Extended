namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKFileLottieImageSourceTest : SKLottieImageSourceTest<SKFileLottieImageSource>
{
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";
	private const string LoloJson = "TestAssets/Lottie/lolo.json";

	protected override SKFileLottieImageSource CreateEmptyImageSource() =>
		new SKFileLottieImageSource { };

	protected override SKFileLottieImageSource CreateCompleteImageSource() =>
		new SKFileLottieImageSource { File = TrophyJson };

	protected override void UpdateImageSource(SKFileLottieImageSource imageSource, bool first) =>
		imageSource.File = first ? TrophyJson : LoloJson;

	protected override void ResetImageSource(SKFileLottieImageSource imageSource) =>
		imageSource.File = null;
}
