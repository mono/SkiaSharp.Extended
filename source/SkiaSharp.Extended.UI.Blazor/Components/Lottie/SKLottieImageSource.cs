namespace SkiaSharp.Extended.UI.Blazor.Components;

/// <summary>
/// A browser-specific source that loads Lottie animations.
/// </summary>
public abstract class SKLottieImageSource
{
	/// <summary>
	/// Initializes a Lottie image source.
	/// </summary>
	protected SKLottieImageSource()
	{
	}

	/// <summary>Creates a source that downloads Lottie JSON from a URI.</summary>
	public static SKLottieImageSource FromUri(Uri uri) =>
		new SKUriLottieImageSource(uri ?? throw new ArgumentNullException(nameof(uri)));

	/// <summary>Creates a source from raw Lottie JSON.</summary>
	public static SKLottieImageSource FromJson(string json) =>
		new SKJsonLottieImageSource(json ?? throw new ArgumentNullException(nameof(json)));

	/// <summary>
	/// Creates a source that obtains a fresh, readable stream for every load.
	/// The component disposes each returned stream after reading it.
	/// </summary>
	public static SKLottieImageSource FromStream(Func<CancellationToken, ValueTask<Stream>> streamFactory) =>
		new SKStreamLottieImageSource(streamFactory ?? throw new ArgumentNullException(nameof(streamFactory)));

	/// <summary>
	/// Converts a URI string to a source. Strings always represent relative or absolute URIs,
	/// never raw JSON.
	/// </summary>
	public static implicit operator SKLottieImageSource?(string? value) =>
		value is null ? null : FromUri(new Uri(value, UriKind.RelativeOrAbsolute));

	/// <summary>
	/// Loads a fresh animation result.
	/// </summary>
	/// <param name="httpClient">The view's configured HTTP client, or <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>An animation result. Implementations must return a fresh native animation for each successful load.</returns>
	protected internal abstract Task<SKLottieAnimation> LoadAnimationAsync(HttpClient? httpClient, CancellationToken cancellationToken);

	internal virtual bool IsSameSource(SKLottieImageSource? other) => ReferenceEquals(this, other);
}
