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

		using var stream = await Stream.Invoke(cancellationToken).ConfigureAwait(false);
		if (stream is null)
			throw new FileLoadException($"Unable to load Lottie animation stream.");

		var animation = CreateAnimationBuilder().Build(stream);
		if (animation is null)
			throw new FileLoadException($"Unable to parse Lottie animation.");

		return new SKLottieAnimation(animation);
	}
}
