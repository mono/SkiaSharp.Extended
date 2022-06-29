namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public class SKLottieImageSource : Element
{
	private readonly WeakEventManager weakEventManager = new();

	public virtual bool IsEmpty => true;

	internal virtual Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	internal static object FromUri(Uri uri) =>
		new SKUriLottieImageSource { Uri = uri };

	internal static object FromFile(string file) =>
		new SKFileLottieImageSource { File = file };

	internal event EventHandler SourceChanged
	{
		add => weakEventManager.AddEventHandler(value);
		remove => weakEventManager.RemoveEventHandler(value);
	}

	internal static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKLottieImageSource source)
			source.weakEventManager.HandleEvent(source, EventArgs.Empty, nameof(SourceChanged));
	}
}
