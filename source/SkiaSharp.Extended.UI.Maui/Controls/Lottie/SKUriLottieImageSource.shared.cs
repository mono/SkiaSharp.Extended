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

			// Download to memory stream so we can detect format
			using var memoryStream = new MemoryStream();
			await response.Content.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
			memoryStream.Position = 0;

			return await LoadAnimationFromStreamAsync(memoryStream, cancellationToken);
		}
		catch (Exception ex) when (ex is not FileLoadException)
		{
			throw new FileLoadException($"Error loading Lottie animation uri \"{Uri}\".", ex);
		}
	}
}
