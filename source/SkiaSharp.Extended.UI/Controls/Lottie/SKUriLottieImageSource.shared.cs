namespace SkiaSharp.Extended.UI.Controls;

public class SKUriLottieImageSource : SKLottieImageSource
{
	public static readonly BindableProperty UriProperty = BindableProperty.Create(
		nameof(Uri), typeof(Uri), typeof(SKUriLottieImageSource),
		propertyChanged: OnSourceChanged);

	public Uri? Uri
	{
		get => (Uri?)GetValue(UriProperty);
		set => SetValue(UriProperty, value);
	}

	public override bool IsEmpty => Uri is null;

	public override async Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || Uri is null)
			return new SKLottieAnimation();

		try
		{
			using var client = new HttpClient();

			using var response = await client.GetAsync(Uri, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				throw new FileLoadException($"Unable to load Lottie animation uri \"{Uri}\".");

			var animation = Skottie.Animation.Create(stream);
			if (animation is null)
				throw new FileLoadException($"Unable to parse Lottie animation \"{Uri}\".");

			return new SKLottieAnimation(animation);
		}
		catch (Exception ex)
		{
			throw new FileLoadException($"Error loading Lottie animation uri \"{Uri}\".", ex);
		}
	}
}
