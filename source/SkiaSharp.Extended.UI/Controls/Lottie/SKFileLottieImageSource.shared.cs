namespace SkiaSharp.Extended.UI.Controls;

public class SKFileLottieImageSource : SKLottieImageSource
{
	public static BindableProperty FileProperty = BindableProperty.Create(
		nameof(File), typeof(string), typeof(SKFileLottieImageSource),
		propertyChanged: OnSourceChanged);

	public string? File
	{
		get => (string?)GetValue(FileProperty);
		set => SetValue(FileProperty, value);
	}

	public override bool IsEmpty =>
		string.IsNullOrEmpty(File);

	internal override async Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || string.IsNullOrEmpty(File))
			return null;

		try
		{
			using var stream = await LoadFile(File);

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

	private static async Task<Stream> LoadFile(string filename)
	{
		try
		{
			return await FileSystem.OpenAppPackageFileAsync(filename).ConfigureAwait(false);
		}
		catch (NotImplementedException)
		{
			var root = AppContext.BaseDirectory;
			var path = Path.Combine(root, filename);
			return System.IO.File.OpenRead(path);
		}
	}
}
