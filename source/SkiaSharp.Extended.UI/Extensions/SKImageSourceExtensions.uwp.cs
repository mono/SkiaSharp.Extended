using Windows.Storage;

#if XAMARIN_FORMS
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Application = Xamarin.Forms.Application;
#endif

namespace SkiaSharp.Extended.UI.Extensions;

public static partial class SKImageSourceExtensions
{
	private static async Task<SKImage?> PlatformToSKImageAsync(FileImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(imageSource.File))
			return null;

		var filePath = imageSource.File;

		var imageDirectory = Application.Current.OnThisPlatform().GetImageDirectory();
		if (!string.IsNullOrEmpty(imageDirectory))
		{
			var directory = Path.GetDirectoryName(filePath);
			if (string.IsNullOrEmpty(directory) || !Path.GetFullPath(directory).Equals(Path.GetFullPath(imageDirectory)))
				filePath = Path.Combine(imageDirectory, filePath);
		}

		var uri = new Uri($"ms-appx:///{filePath}");

		var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
		var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false);
		var image = SKImage.FromEncodedData(stream);

		return image;
	}

	private static Task<SKImage?> PlatformToSKImageAsync(FontImageSource imageSource, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}
}
