using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Media.Extensions
{
	public static partial class SKImageSourceExtensions
	{
		private static async Task<SKImage?> PlatformToSKImageAsync(FileImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrEmpty(imageSource.File))
				return null;

			var filePath = imageSource.File;

			using var stream = File.OpenRead(filePath);

			var image = SKImage.FromEncodedData(stream);

			return image;
		}

		private static async Task<SKImage?> PlatformToSKImageAsync(FontImageSource imageSource, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}
	}
}
