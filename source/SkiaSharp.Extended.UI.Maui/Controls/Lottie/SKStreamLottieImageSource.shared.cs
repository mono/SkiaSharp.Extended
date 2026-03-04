namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A Lottie image source that loads animations from a stream.
/// </summary>
public class SKStreamLottieImageSource : SKLottieImageSource
{
	/// <summary>
	/// Identifies the <see cref="Stream"/> bindable property.
	/// </summary>
	public static readonly BindableProperty StreamProperty = BindableProperty.Create(
		nameof(Stream), typeof(Func<CancellationToken, Task<Stream?>>), typeof(SKStreamLottieImageSource),
		propertyChanged: OnSourceChanged);

	/// <summary>
	/// Gets or sets the factory function that provides the animation stream.
	/// </summary>
	public Func<CancellationToken, Task<Stream?>>? Stream
	{
		get => (Func<CancellationToken, Task<Stream?>>?)GetValue(StreamProperty);
		set => SetValue(StreamProperty, value);
	}

	/// <inheritdoc/>
	public override bool IsEmpty => Stream is null;

	/// <inheritdoc/>
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
