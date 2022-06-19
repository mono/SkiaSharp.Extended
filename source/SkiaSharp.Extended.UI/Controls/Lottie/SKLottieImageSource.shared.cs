namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public class SKLottieImageSource : Element
{
	public virtual bool IsEmpty => true;

	internal virtual Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	internal static object FromUri(Uri uri) =>
		new SKUriLottieImageSource { Uri = uri };

	internal static object FromFile(string file) =>
		new SKFileLottieImageSource { File = file };
}
