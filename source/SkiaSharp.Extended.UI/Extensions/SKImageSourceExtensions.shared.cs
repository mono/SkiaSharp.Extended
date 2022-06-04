namespace SkiaSharp.Extended.UI.Extensions;

public static partial class SKImageSourceExtensions
{
	public static Task<SKImage?> ToSKImageAsync(this ImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource == null)
			throw new ArgumentNullException(nameof(imageSource));

		return imageSource switch
		{
			// 1. first try SkiaSharp sources
			SKImageImageSource iis => FromSkia(iis.Image),
			SKBitmapImageSource bis => FromSkia(SKImage.FromBitmap(bis.Bitmap)),
			SKPixmapImageSource xis => FromSkia(SKImage.FromPixels(xis.Pixmap)),
			SKPictureImageSource pis => FromSkia(SKImage.FromPicture(pis.Picture, pis.Dimensions)),

			// 2. then try Stream sources
			StreamImageSource stream => FromStream(stream.Stream.Invoke(cancellationToken)),
			IStreamImageSource stream => FromStream(stream.GetStreamAsync(cancellationToken)),
#if XAMARIN_FORMS
			UriImageSource uri => FromStream(uri.GetStreamAsync(cancellationToken)),
#endif

			// 3. finally, use the handlers
			FileImageSource file => FromHandler(PlatformToSKImageAsync(file, cancellationToken)),
			FontImageSource font => FromHandler(PlatformToSKImageAsync(font, cancellationToken)),

			// 4. all is lost
			_ => throw new ArgumentException("Unable to determine the type of image source.", nameof(imageSource))
		};

		static Task<SKImage?> FromSkia(SKImage? image)
		{
			return Task.FromResult(image);
		}

		static Task<SKImage?> FromHandler(Task<SKImage?> handlerTask)
		{
			if (handlerTask == null)
				return Task.FromResult<SKImage?>(null);

			return handlerTask;
		}

		static async Task<SKImage?> FromStream(Task<Stream> streamTask)
		{
			if (streamTask == null)
				return null;

			var stream = await streamTask.ConfigureAwait(false);
			if (stream == null)
				return null;

			var image = SKImage.FromEncodedData(stream);
			return image;
		}
	}
}
