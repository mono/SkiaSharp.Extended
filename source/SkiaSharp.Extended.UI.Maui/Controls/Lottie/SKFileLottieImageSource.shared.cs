namespace SkiaSharp.Extended.UI.Controls;

public class SKFileLottieImageSource : SKLottieImageSource
{
	public static readonly BindableProperty FileProperty = BindableProperty.Create(
		nameof(File), typeof(string), typeof(SKFileLottieImageSource),
		propertyChanged: OnSourceChanged);

	public string? File
	{
		get => (string?)GetValue(FileProperty);
		set => SetValue(FileProperty, value);
	}

	public override bool IsEmpty =>
		string.IsNullOrEmpty(File);

	public override async Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || string.IsNullOrEmpty(File))
			return new SKLottieAnimation();

		try
		{
			using var stream = await LoadFile(File);
			if (stream is null)
				throw new FileLoadException($"Unable to load Lottie animation file \"{File}\".");

			var animation = CreateAnimationBuilder().Build(stream);
			if (animation is null)
				throw new FileLoadException($"Unable to parse Lottie animation \"{File}\".");

			return new SKLottieAnimation(animation);
		}
		catch (Exception ex)
		{
			throw new FileLoadException($"Error loading Lottie animation file \"{File}\".", ex);
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
