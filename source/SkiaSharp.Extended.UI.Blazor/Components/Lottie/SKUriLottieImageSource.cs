namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKUriLottieImageSource : SKLottieImageSource
{
	private readonly Uri uri;

	internal SKUriLottieImageSource(Uri uri)
	{
		this.uri = uri;
	}

	internal override async Task<string> LoadJsonAsync(
		HttpClient? httpClient,
		CancellationToken cancellationToken)
	{
		if (httpClient is null)
		{
			throw new InvalidOperationException(
				"Loading a URI source requires SKLottieView.HttpClient or a registered HttpClient service.");
		}

		return await httpClient.GetStringAsync(uri, cancellationToken).ConfigureAwait(false);
	}

	internal override bool EqualsCore(SKLottieImageSource other) =>
		uri.Equals(((SKUriLottieImageSource)other).uri);

	internal override int GetHashCodeCore() =>
		HashCode.Combine(typeof(SKUriLottieImageSource), uri);
}
