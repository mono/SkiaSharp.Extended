using System;

namespace SkiaSharp.Extended
{
	public static class SKBlurHash
	{
		// Deserialize

		public static SKBitmap DeserializeBitmap(string blurHash, int width, int height, float punch = 1f)
		{
			if (blurHash == null)
				throw new ArgumentNullException(nameof(blurHash));
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeBitmap(blurHash.AsSpan(), width, height, punch);
		}

		public static SKBitmap DeserializeBitmap(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeBitmap(blurHash, width, height, punch);
		}

		public static SKImage DeserializeImage(string blurHash, int width, int height, float punch = 1f)
		{
			if (blurHash == null)
				throw new ArgumentNullException(nameof(blurHash));
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeImage(blurHash.AsSpan(), width, height, punch);
		}

		public static SKImage DeserializeImage(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeImage(blurHash, width, height, punch);
		}

		// Serialize

		public static string Serialize(SKBitmap bitmap, int componentsX, int componentsY)
		{
			if (bitmap == null)
				throw new ArgumentNullException(nameof(bitmap));
			if (componentsX <= 1 || componentsX > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsX), "The X components must be in the range [1..9].");
			if (componentsY <= 1 || componentsY > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsY), "The Y components must be in the range [1..9].");

			return SKBlurHashSerializer.Serialize(bitmap, componentsX, componentsY);
		}

		public static string Serialize(SKImage image, int componentsX, int componentsY)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			if (componentsX <= 1 || componentsX > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsX), "The X components must be in the range [1..9].");
			if (componentsY <= 1 || componentsY > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsY), "The Y components must be in the range [1..9].");

			return SKBlurHashSerializer.Serialize(image, componentsX, componentsY);
		}

		public static string Serialize(SKPixmap pixmap, int componentsX, int componentsY)
		{
			if (pixmap == null)
				throw new ArgumentNullException(nameof(pixmap));
			if (componentsX <= 1 || componentsX > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsX), "The X components must be in the range [1..9].");
			if (componentsY <= 1 || componentsY > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsY), "The Y components must be in the range [1..9].");

			return SKBlurHashSerializer.Serialize(pixmap, componentsX, componentsY);
		}
	}
}
