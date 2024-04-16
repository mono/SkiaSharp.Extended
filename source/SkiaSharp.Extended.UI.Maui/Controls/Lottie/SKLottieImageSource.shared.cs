using SkiaSharp.Resources;

namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public abstract class SKLottieImageSource : Element
{
	private readonly WeakEventManager weakEventManager = new();

	public virtual bool IsEmpty => true;

	public abstract Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default);

	internal Skottie.AnimationBuilder CreateAnimationBuilder() =>
		Skottie.Animation.CreateBuilder()
			.SetResourceProvider(new CachingResourceProvider(new DataUriResourceProvider()))
			.SetFontManager(SKFontManager.Default);

	public static object FromUri(Uri uri) =>
		new SKUriLottieImageSource { Uri = uri };

	public static object FromFile(string file) =>
		new SKFileLottieImageSource { File = file };

	public static object FromStream(Func<CancellationToken, Task<Stream?>> getter) =>
		new SKStreamLottieImageSource { Stream = getter };

	public static object FromStream(Stream stream) =>
		FromStream(token => Task.FromResult<Stream?>(stream));

	public event EventHandler SourceChanged
	{
		add => weakEventManager.AddEventHandler(value);
		remove => weakEventManager.RemoveEventHandler(value);
	}

	protected static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKLottieImageSource source)
			source.weakEventManager.HandleEvent(source, EventArgs.Empty, nameof(SourceChanged));
	}
}
