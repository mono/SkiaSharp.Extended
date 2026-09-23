namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKStreamLottieImageSource : SKLottieImageSource
{
	private readonly Func<CancellationToken, ValueTask<Stream>> streamFactory;

	internal SKStreamLottieImageSource(Func<CancellationToken, ValueTask<Stream>> streamFactory)
	{
		this.streamFactory = streamFactory;
	}

	protected internal override async Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken)
	{
		var stream = await streamFactory(cancellationToken).ConfigureAwait(false);
		if (stream is null)
			throw new InvalidOperationException("The Lottie stream factory returned null.");

		await using (stream)
		{
			return new SKLottieAnimation(await SKLottieAnimationLoader.LoadAsync(stream, cancellationToken).ConfigureAwait(false));
		}
	}

	internal override bool IsSameSource(SKLottieImageSource? other) =>
		other is SKStreamLottieImageSource source && streamFactory.Equals(source.streamFactory);
}
