using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Resources;
using SkiaSharp.Skottie;

namespace SkiaSharp.Extended;

internal static class SKLottieAnimationLoader
{
	internal static async Task<Animation> LoadAsync(Stream stream, CancellationToken cancellationToken)
	{
		if (stream is null)
			throw new ArgumentNullException(nameof(stream));
		if (!stream.CanRead)
			throw new ArgumentException("The Lottie stream must be readable.", nameof(stream));

		using var buffer = new MemoryStream();
		await stream.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
		buffer.Position = 0;
		return Load(buffer);
	}

	internal static Animation Load(Stream stream)
	{
		if (stream is null)
			throw new ArgumentNullException(nameof(stream));
		if (!stream.CanRead)
			throw new ArgumentException("The Lottie stream must be readable.", nameof(stream));

		return Animation.CreateBuilder()
			.SetResourceProvider(new CachingResourceProvider(new DataUriResourceProvider()))
			.SetFontManager(SKFontManager.Default)
			.Build(stream)
			?? throw new InvalidDataException("Unable to parse the Lottie animation.");
	}
}
