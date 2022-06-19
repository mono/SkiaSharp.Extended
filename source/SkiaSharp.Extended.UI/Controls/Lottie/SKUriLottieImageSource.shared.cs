namespace SkiaSharp.Extended.UI.Controls;

public class SKUriLottieImageSource : SKLottieImageSource
{
	public Uri? Uri { get; set; }

	public override bool IsEmpty => Uri is null;
}
