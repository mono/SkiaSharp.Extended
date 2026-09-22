namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKStreamLottieImageSource : SKLottieImageSource
{
	private readonly Func<CancellationToken, ValueTask<Stream>> streamFactory;

	internal SKStreamLottieImageSource(
		Func<CancellationToken, ValueTask<Stream>> streamFactory)
	{
		this.streamFactory = streamFactory;
	}

	internal override async Task<string> LoadJsonAsync(
		HttpClient? httpClient,
		CancellationToken cancellationToken)
	{
		var stream = await streamFactory(cancellationToken).ConfigureAwait(false);
		if (stream is null)
			throw new InvalidOperationException("The Lottie stream factory returned null.");

		await using (stream)
			return await ReadJsonAsync(stream, cancellationToken).ConfigureAwait(false);
	}

	internal override bool EqualsCore(SKLottieImageSource other) =>
		streamFactory.Equals(((SKStreamLottieImageSource)other).streamFactory);

	internal override int GetHashCodeCore() =>
		HashCode.Combine(typeof(SKStreamLottieImageSource), streamFactory);
}
