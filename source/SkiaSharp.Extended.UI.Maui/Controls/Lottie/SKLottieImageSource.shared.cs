using SkiaSharp.Resources;

namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public abstract class SKLottieImageSource : Element
{
	private readonly WeakEventManager weakEventManager = new();

	/// <summary>
	/// Gets or sets the default resource provider used by all Lottie image sources.
	/// This can be overridden per instance using the <see cref="ResourceProvider"/> property.
	/// </summary>
	public static ResourceProvider? DefaultResourceProvider { get; set; }

	/// <summary>
	/// Gets or sets the default font manager used by all Lottie image sources.
	/// This can be overridden per instance using the <see cref="FontManager"/> property.
	/// </summary>
	public static SKFontManager? DefaultFontManager { get; set; }

	/// <summary>
	/// Gets or sets the resource provider for this specific image source.
	/// If null, uses <see cref="DefaultResourceProvider"/> or the built-in default.
	/// </summary>
	public ResourceProvider? ResourceProvider { get; set; }

	/// <summary>
	/// Gets or sets the font manager for this specific image source.
	/// If null, uses <see cref="DefaultFontManager"/> or SKFontManager.Default.
	/// </summary>
	public SKFontManager? FontManager { get; set; }

	public virtual bool IsEmpty => true;

	public abstract Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default);

	internal Skottie.AnimationBuilder CreateAnimationBuilder()
	{
		var builder = Skottie.Animation.CreateBuilder();

		// Use instance override, then static default, then built-in default
		var resourceProvider = ResourceProvider
			?? DefaultResourceProvider
			?? new CachingResourceProvider(new DataUriResourceProvider());
		builder.SetResourceProvider(resourceProvider);

		// Use instance override, then static default, then built-in default
		var fontManager = FontManager
			?? DefaultFontManager
			?? SKFontManager.Default;
		builder.SetFontManager(fontManager);

		return builder;
	}

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
