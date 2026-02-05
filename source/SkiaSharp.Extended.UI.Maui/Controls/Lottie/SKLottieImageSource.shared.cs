using SkiaSharp.Resources;

namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public abstract class SKLottieImageSource : Element
{
	private readonly WeakEventManager weakEventManager = new();

	public static readonly BindableProperty ImageAssetsFolderProperty = BindableProperty.Create(
		nameof(ImageAssetsFolder),
		typeof(string),
		typeof(SKLottieImageSource),
		null,
		propertyChanged: OnSourceChanged);

	public virtual bool IsEmpty => true;

	public string? ImageAssetsFolder
	{
		get => (string?)GetValue(ImageAssetsFolderProperty);
		set => SetValue(ImageAssetsFolderProperty, value);
	}

	public abstract Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default);

	internal Skottie.AnimationBuilder CreateAnimationBuilder()
	{
		var builder = Skottie.Animation.CreateBuilder();

		// Create the resource provider chain
		ResourceProvider resourceProvider;
		if (!string.IsNullOrEmpty(ImageAssetsFolder))
		{
			// Chain DataUriResourceProvider with FileResourceProvider
			// DataUriResourceProvider first handles base64 embedded images (data: URIs)
			// FileResourceProvider (fallback) loads external image files from the specified folder
			// This allows animations to use both embedded and external images
			resourceProvider = new CachingResourceProvider(
				new DataUriResourceProvider(
					new FileResourceProvider(ImageAssetsFolder)));
		}
		else
		{
			// Default: only handle base64 embedded images
			resourceProvider = new CachingResourceProvider(new DataUriResourceProvider());
		}

		return builder
			.SetResourceProvider(resourceProvider)
			.SetFontManager(SKFontManager.Default);
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
