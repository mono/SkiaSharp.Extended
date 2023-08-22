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

	public override async Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || Stream is null)
			return null;

		using var stream = await Stream.Invoke(cancellationToken).ConfigureAwait(false);

		if (stream is null)
			return null;

		return Skottie.Animation.Create(stream);
	}
}
