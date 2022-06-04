using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Views.Android;

namespace SkiaSharp.Extended.UI.Extensions;

public static partial class SKImageSourceExtensions
{
	private static async Task<SKImage?> PlatformToSKImageAsync(ImageSource imageSource, CancellationToken cancellationToken = default)
	{
		var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
		if (handler == null)
			throw new InvalidOperationException($"Unable to determine the handler for the image source ({imageSource.GetType().Name}).");

		using var bitmap = await handler.LoadImageAsync(imageSource, Android.App.Application.Context, cancellationToken).ConfigureAwait(false);
		if (bitmap == null)
			return null;

		var image = bitmap.ToSKImage();

		bitmap.Recycle();

		return image;
	}
}
