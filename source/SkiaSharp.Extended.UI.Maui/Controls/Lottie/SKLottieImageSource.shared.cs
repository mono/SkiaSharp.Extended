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
	/// <returns>An animation result. Implementations must return a fresh native animation for each successful load.</returns>
	public abstract Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default);

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
	/// Creates a replayable Lottie image source from the unread bytes in a stream.
	/// </summary>
	/// <param name="stream">The stream to snapshot. The caller retains ownership; its position advances to its end.</param>
	/// <returns>An <see cref="SKStreamLottieImageSource"/> that produces a fresh memory stream for every load.</returns>
	public static object FromStream(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);
		if (!stream.CanRead)
			throw new ArgumentException("The stream must be readable.", nameof(stream));

		const int maximumSnapshotBytes = 32 * 1024 * 1024;
		using var snapshot = new MemoryStream();
		var buffer = new byte[81920];
		int read;
		while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
		{
			if (snapshot.Length + read > maximumSnapshotBytes)
				throw new ArgumentException("The stream exceeds the 32 MiB Lottie source limit.", nameof(stream));
			snapshot.Write(buffer, 0, read);
		}

		var bytes = snapshot.ToArray();
		return FromStream(_ => Task.FromResult<Stream?>(new MemoryStream(bytes, writable: false)));
	}

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
