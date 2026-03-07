namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A Lottie image source that loads animations from a URI.
/// </summary>
public class SKUriLottieImageSource : SKLottieImageSource
{
	/// <summary>
	/// Identifies the <see cref="Uri"/> bindable property.
	/// </summary>
	public static readonly BindableProperty UriProperty = BindableProperty.Create(
		nameof(Uri), typeof(Uri), typeof(SKUriLottieImageSource),
		propertyChanged: OnSourceChanged);

	/// <summary>
	/// Gets or sets the URI of the Lottie animation.
	/// </summary>
	public Uri? Uri
	{
		get => (Uri?)GetValue(UriProperty);
		set => SetValue(UriProperty, value);
	}

	/// <inheritdoc/>
	public override bool IsEmpty => Uri is null;

	/// <inheritdoc/>
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

			return await LoadAnimationFromStreamAsync(stream, cancellationToken);
		}
		catch (Exception ex) when (ex is not FileLoadException)
		{
			throw new FileLoadException($"Error loading Lottie animation uri \"{Uri}\".", ex);
		}
	}
}
