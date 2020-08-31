using System;
using System.Buffers;

namespace SkiaSharp.Extended
{
	public class SKBlurHash
	{
		private static readonly ArrayPool<SKPoint3> ColorArrayPool = ArrayPool<SKPoint3>.Shared;
		private static readonly ArrayPool<double> DoubleArrayPool = ArrayPool<double>.Shared;

		public SKBlurHash()
		{
		}

		public SKBitmap? DecodeBitmap(string? blurHash, int width, int height, float punch = 1f)
		{
			if (blurHash == null)
				throw new ArgumentNullException(nameof(blurHash));

			return DecodeBitmap(blurHash.AsSpan(), width, height, punch);
		}

		public SKBitmap? DecodeBitmap(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			var data = ParseData(blurHash, punch);

			var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));

			using var pixmap = bitmap.PeekPixels();
			CreatePixelData(pixmap, data);

			data.Return();

			return bitmap;
		}

		private void CreatePixelData(SKPixmap pixmap, Data data)
		{
			var pixels = pixmap.GetPixelSpan<SKColor>();

			var height = pixmap.Height;
			var width = pixmap.Width;
			var comp = data.Components;
			var colors = data.Colors;

			var cosinesX = DoubleArrayPool.Rent(width * comp.X);
			var cosinesY = DoubleArrayPool.Rent(height * comp.Y);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var r = 0f;
					var g = 0f;
					var b = 0f;

					for (var j = 0; j < comp.Y; j++)
					{
						for (var i = 0; i < comp.X; i++)
						{
							var cosX = GetCos(cosinesX, i, comp.X, x, width);
							var cosY = GetCos(cosinesY, j, comp.Y, y, height);

							var basis = (float)(cosX * cosY);
							var color = colors[j * comp.X + i];

							r += color.X * basis;
							g += color.Y * basis;
							b += color.Z * basis;
						}
					}

					pixels[x + width * y] = new SKColor(
						(byte)LinearToSrgb(r),
						(byte)LinearToSrgb(g),
						(byte)LinearToSrgb(b));
				}
			}

			static double GetCos(double[] array, int x, int comp, int y, int size) =>
				array[x + (comp * y)] = Math.Cos(Math.PI * y * x / size);
		}

		private static Data ParseData(ReadOnlySpan<char> blurHash, float punch = 1f)
		{
			if (blurHash.Length < 6)
				throw new ArgumentException("BlurHash data must have at least 6 characters.", nameof(blurHash));

			// read components
			var compEnc = Base83.Decode(blurHash, 0, 1);
			var comp = new SKPointI(
				(compEnc % 9) + 1,
				(compEnc / 9) + 1);

			var expectedLength = 4 + 2 * comp.X * comp.Y;
			if (blurHash.Length != expectedLength)
				throw new ArgumentException($"BlurHash contains invalid data. Length should be {expectedLength} but was {blurHash.Length}.", nameof(blurHash));

			// read maximum AC
			var acMaxEnc = Base83.Decode(blurHash, 1, 1);
			var acMax = (acMaxEnc + 1) / 166f;

			// build color array
			var colorsLength = comp.X * comp.Y;
			var colors = ColorArrayPool.Rent(colorsLength);

			// the first color
			var dcEnc = Base83.Decode(blurHash, 2, 4);
			colors[0] = DecodeDC(dcEnc);

			// the rest of the colors
			for (var i = 1; i < colorsLength; i++)
			{
				var start = 4 + i * 2;
				var acEnc = Base83.Decode(blurHash, start, 2);
				colors[i] = DecodeAC(acEnc, acMax * punch);
			}

			return new Data(colors, colors.AsSpan(0, colorsLength), comp);
		}

		private static SKPoint3 DecodeDC(int dc)
		{
			var r = dc >> 16;
			var g = (dc >> 8) & 255;
			var b = dc & 255;

			return new SKPoint3(SrgbToLinear(r), SrgbToLinear(g), SrgbToLinear(b));
		}

		private static SKPoint3 DecodeAC(int ac, float acMax)
		{
			var r = ac / (19 * 19);
			var g = (ac / 19) % 19;
			var b = ac % 19;

			return new SKPoint3(
				SignedPow2((r - 9) / 9.0f) * acMax,
				SignedPow2((g - 9) / 9.0f) * acMax,
				SignedPow2((b - 9) / 9.0f) * acMax);

			static float SignedPow2(float v)
			{
				var pow = (float)Math.Pow(v, 2);
				return v < 0 ? -pow : pow;
			}
		}

		private static float SrgbToLinear(int srgb)
		{
			var v = srgb / 255f;
			return v <= 0.04045f
				? v / 12.92f
				: (float)Math.Pow((v + 0.055f) / 1.055f, 2.4f);
		}

		private static int LinearToSrgb(float linear)
		{
			var v = Math.Max(0, Math.Min(1f, linear));

			return v <= 0.0031308f
				? (int)(v * 12.92f * 255f + 0.5f)
				: (int)((1.055f * Math.Pow(v, 1 / 2.4f) - 0.055f) * 255 + 0.5f);
		}

		private readonly ref struct Data
		{
			private readonly SKPoint3[] array;

			public Data(SKPoint3[] poolArray, Span<SKPoint3> colors, SKPointI comp)
			{
				array = poolArray;
				Colors = colors;
				Components = comp;
			}

			public Span<SKPoint3> Colors { get; }

			public SKPointI Components { get; }

			public void Return() =>
				ColorArrayPool.Return(array);
		}
	}
}
