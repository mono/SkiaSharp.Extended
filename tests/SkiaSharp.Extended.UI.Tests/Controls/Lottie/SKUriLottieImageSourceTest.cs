namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKUriLottieImageSourceTest : SKLottieImageSourceTest<SKUriLottieImageSource>
{
	private const string TrophyJson = "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/5d259235cff2f12e4f1c803ce18af082eca43ea8/tests/SkiaSharp.Extended.UI.Tests/TestAssets/Lottie/trophy.json";
	private const string LoloJson = "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/5d259235cff2f12e4f1c803ce18af082eca43ea8/tests/SkiaSharp.Extended.UI.Tests/TestAssets/Lottie/lolo.json";

	protected override SKUriLottieImageSource CreateEmptyImageSource() =>
		new SKUriLottieImageSource { };

	protected override SKUriLottieImageSource CreateCompleteImageSource() =>
		new SKUriLottieImageSource { Uri = new Uri(TrophyJson) };

	protected override void UpdateImageSource(SKUriLottieImageSource imageSource, bool first) =>
		imageSource.Uri = new Uri(first ? TrophyJson : LoloJson);

	protected override void ResetImageSource(SKUriLottieImageSource imageSource) =>
		imageSource.Uri = null;
}
