namespace SkiaSharp.Extended.UI.Controls;

public class SKUriLottieImageSource : SKLottieImageSource
{
	public static BindableProperty UriProperty = BindableProperty.Create(
		nameof(Uri), typeof(Uri), typeof(SKUriLottieImageSource),
		propertyChanged: OnSourceChanged);

	public Uri? Uri
	{
		get => (Uri?)GetValue(UriProperty);
		set => SetValue(UriProperty, value);
	}

	public override bool IsEmpty => Uri is null;

	internal override async Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || Uri is null)
			return null;

		try
		{
			using var client = new HttpClient();

			using var response = await client.GetAsync(Uri, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				return null;

			return Skottie.Animation.Create(stream);
		}
		catch (Exception ex)
		{
#if XAMARIN_FORMS
			Xamarin.Forms.Internals.Log.Warning(nameof(SKUriLottieImageSource), $"Unable to load Lottie animation \"{Uri}\": " + ex.Message);
			return null;
#else
			throw new ArgumentException($"Unable to load Lottie animation \"{Uri}\".", ex);
#endif
		}
	}
}
