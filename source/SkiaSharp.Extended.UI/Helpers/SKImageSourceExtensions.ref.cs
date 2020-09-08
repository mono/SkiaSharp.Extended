using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI
{
	public static partial class SKImageSourceExtensions
	{
		private static Task<SKImage?> PlatformToSKImageAsync(ImageSource imageSource, CancellationToken cancellationToken = default) =>
			throw new NotImplementedException();
	}
}
