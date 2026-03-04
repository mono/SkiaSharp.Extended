using SkiaSharp.Resources;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Abstract base class for Lottie animation image sources.
/// </summary>
[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public abstract class SKLottieImageSource : Element
{
	private readonly WeakEventManager weakEventManager = new();

	/// <summary>
	/// Gets a value indicating whether this image source has no content.
	/// </summary>
	public virtual bool IsEmpty => true;

	/// <summary>
	/// Loads the Lottie animation asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>An <see cref="SKLottieAnimation"/> containing the loaded animation.</returns>
	public abstract Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default);

	internal Skottie.AnimationBuilder CreateAnimationBuilder() =>
		Skottie.Animation.CreateBuilder()
			.SetResourceProvider(new CachingResourceProvider(new DataUriResourceProvider()))
			.SetFontManager(SKFontManager.Default);

	/// <summary>
	/// Creates a Lottie image source from a URI.
	/// </summary>
	/// <param name="uri">The URI of the animation.</param>
	/// <returns>An <see cref="SKUriLottieImageSource"/>.</returns>
	public static object FromUri(Uri uri) =>
		new SKUriLottieImageSource { Uri = uri };

	/// <summary>
	/// Creates a Lottie image source from a file path.
	/// </summary>
	/// <param name="file">The file path of the animation.</param>
	/// <returns>An <see cref="SKFileLottieImageSource"/>.</returns>
	public static object FromFile(string file) =>
		new SKFileLottieImageSource { File = file };

	/// <summary>
	/// Creates a Lottie image source from a stream factory.
	/// </summary>
	/// <param name="getter">A factory function that provides the animation stream.</param>
	/// <returns>An <see cref="SKStreamLottieImageSource"/>.</returns>
	public static object FromStream(Func<CancellationToken, Task<Stream?>> getter) =>
		new SKStreamLottieImageSource { Stream = getter };

	/// <summary>
	/// Creates a Lottie image source from a stream.
	/// </summary>
	/// <param name="stream">The stream containing the animation data.</param>
	/// <returns>An <see cref="SKStreamLottieImageSource"/>.</returns>
	public static object FromStream(Stream stream) =>
		FromStream(token => Task.FromResult<Stream?>(stream));

	/// <summary>
	/// Occurs when the underlying source data changes.
	/// </summary>
	public event EventHandler SourceChanged
	{
		add => weakEventManager.AddEventHandler(value);
		remove => weakEventManager.RemoveEventHandler(value);
	}

	/// <summary>
	/// Called when a source property changes on a derived image source to raise the <see cref="SourceChanged"/> event.
	/// </summary>
	/// <param name="bindable">The bindable object.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	protected static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKLottieImageSource source)
			source.weakEventManager.HandleEvent(source, EventArgs.Empty, nameof(SourceChanged));
	}
}
