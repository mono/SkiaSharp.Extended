namespace SkiaSharp.Extended.UI.Extensions;

public static partial class SKImageSourceExtensions
{
	private static Task<SKImage?> PlatformToSKImageAsync(ImageSource imageSource, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();
}
