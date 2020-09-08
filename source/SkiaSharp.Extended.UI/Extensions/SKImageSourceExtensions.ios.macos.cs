using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
#if __MOBILE__
using SkiaSharp.Views.iOS;
using Xamarin.Forms.Platform.iOS;
#else
using SkiaSharp.Views.Mac;
using Xamarin.Forms.Platform.MacOS;
#endif

namespace SkiaSharp.Extended.UI.Extensions
{
	public static partial class SKImageSourceExtensions
	{
		private static async Task<SKImage?> PlatformToSKImageAsync(ImageSource imageSource, CancellationToken cancellationToken = default)
		{
			var handler = Xamarin.Forms.Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
			if (handler == null)
				throw new InvalidOperationException($"Unable to determine the handler for the image source ({imageSource.GetType().Name}).");

			using var nativeImage = await handler.LoadImageAsync(imageSource, cancellationToken).ConfigureAwait(false);
			if (nativeImage == null)
				return null;

			var image = nativeImage.ToSKImage();

			return image;
		}
	}
}
