﻿namespace SkiaSharp.Extended.UI.Extensions;

public static partial class SKImageSourceExtensions
{
	private static Task<SKImage?> PlatformToSKImageAsync(FileImageSource imageSource, CancellationToken cancellationToken = default)
	{
		var file = imageSource.File;
		if (string.IsNullOrEmpty(file))
			return Task.FromResult<SKImage?>(null);

		// TODO: test this to see if it is a real file path
		file = ResourcePath.GetPath(file);

		var image = SKImage.FromEncodedData(file);

		return Task.FromResult<SKImage?>(image);
	}

	private static Task<SKImage?> PlatformToSKImageAsync(FontImageSource imageSource, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}
}
