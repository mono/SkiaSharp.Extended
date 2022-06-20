namespace SkiaSharp.Extended.UI.Controls;

public class SKFileLottieImageSource : SKLottieImageSource
{
	public string? File { get; set; }

	public override bool IsEmpty =>
		string.IsNullOrEmpty(File);

	internal override async Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || string.IsNullOrEmpty(File))
			return null;

		try
		{
			using var stream = await FileSystem.OpenAppPackageFileAsync(File!);

			if (stream is null)
				return null;

			return Skottie.Animation.Create(stream);
		}
		catch (Exception ex)
		{
#if XAMARIN_FORMS
			Xamarin.Forms.Internals.Log.Warning(nameof(SKFileLottieImageSource), $"Unable to load Lottie animation \"{File}\": " + ex.Message);
			return null;
#else
			throw new ArgumentException($"Unable to load Lottie animation \"{File}\".", ex);
#endif
		}
	}
}
