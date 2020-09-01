using System;
using System.IO;

namespace SkiaSharp.Extended
{
	public static class SKBlurHash
	{
		public static SKBitmap? DecodeBitmap(string? blurHash, int width, int height, float punch = 1f)
		{
			if (blurHash == null)
				throw new ArgumentNullException(nameof(blurHash));

			return DecodeBitmap(blurHash.AsSpan(), width, height, punch);
		}

		public static SKBitmap? DecodeBitmap(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			var decoder = new SKBlurHashDecoder();
			return decoder.DecodeBitmap(blurHash, width, height, punch);
		}

		public static string Encode(SKBitmap bitmap, int componentsX, int componentsY)
		{
			var encoder = new SKBlurHashEncoder();
			return encoder.Encode(bitmap, componentsX, componentsY);
		}
	}
}
