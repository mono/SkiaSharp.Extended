namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKUriLottieImageSource : SKLottieImageSource
{
	private readonly Uri uri;

	internal SKUriLottieImageSource(Uri uri)
	{
		this.uri = uri;
	}

	protected internal override Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken) =>
		throw new InvalidOperationException("Loading a URI source requires a registered HttpClient service.");

	internal async Task<SKLottieAnimation> LoadUriAnimationAsync(
		HttpClient? httpClient,
		CancellationToken cancellationToken)
	{
		var client = httpClient ?? throw new InvalidOperationException(
			"Loading a URI source requires a registered HttpClient service.");

		await using var stream = await client.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
		return new SKLottieAnimation(await SKLottieAnimationLoader.LoadAsync(stream, cancellationToken).ConfigureAwait(false));
	}

	internal override bool IsSameSource(SKLottieImageSource? other) =>
		other is SKUriLottieImageSource source && uri.Equals(source.uri);
}
