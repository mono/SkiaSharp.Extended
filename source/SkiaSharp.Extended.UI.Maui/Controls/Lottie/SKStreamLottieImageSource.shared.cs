namespace SkiaSharp.Extended.UI.Controls;

public class SKStreamLottieImageSource : SKLottieImageSource
{
	public static readonly BindableProperty StreamProperty = BindableProperty.Create(
		nameof(Stream), typeof(Func<CancellationToken, Task<Stream?>>), typeof(SKStreamLottieImageSource),
		propertyChanged: OnSourceChanged);

	public Func<CancellationToken, Task<Stream?>>? Stream
	{
		get => (Func<CancellationToken, Task<Stream?>>?)GetValue(StreamProperty);
		set => SetValue(StreamProperty, value);
	}

	public override bool IsEmpty => Stream is null;

	public override async Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || Stream is null)
			return new SKLottieAnimation();

		try
		{
			using var stream = await Stream.Invoke(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				throw new FileLoadException($"Unable to load Lottie animation stream.");

			return await LoadAnimationFromStreamAsync(stream, cancellationToken);
		}
		catch (Exception ex) when (ex is not FileLoadException)
		{
			throw new FileLoadException($"Error loading Lottie animation from stream.", ex);
		}
	}
}
