using System;

namespace SkiaSharp.Extended
{
	public static class SKPixelComparer
	{
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return Compare(first, second);
		}

		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return Compare(firstPixmap, secondPixmap);
		}

		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return Compare(firstWrapper, secondWrapper);
		}

		public static SKPixelComparisonResult Compare(SKImage first, SKImage second)
		{
			Validate(first, second);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;
			var errorPixels = 0;
			var absoluteError = 0;
			var sumSquaredError = 0L;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];

				var r = Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = Math.Abs(secondPixel.Blue - firstPixel.Blue);
				var d = r + g + b;

				absoluteError += d;
				sumSquaredError += (long)r * r + (long)g * g + (long)b * b;
				if (d > 0)
					errorPixels++;
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError, sumSquaredError);
		}

		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, string maskFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			using var mask = SKImage.FromEncodedData(maskFilename);
			return Compare(first, second, mask);
		}

		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKBitmap mask)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			using var maskPixmap = mask.PeekPixels();
			return Compare(firstPixmap, secondPixmap, maskPixmap);
		}

		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixmap mask)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			using var maskWrapper = SKImage.FromPixels(mask);
			return Compare(firstWrapper, secondWrapper, maskWrapper);
		}

		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKImage mask)
		{
			Validate(first, second);
			ValidateMask(first, mask);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;
			var errorPixels = 0;
			var absoluteError = 0;
			var sumSquaredError = 0L;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			using var maskBitmap = GetNormalizedBitmap(mask);
			using var maskPixmap = maskBitmap.PeekPixels();
			var maskPixels = maskPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];
				var maskPixel = maskPixels[idx];

				var r = Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = Math.Abs(secondPixel.Blue - firstPixel.Blue);

				var d = 0;
				var sq = 0L;
				if (r > maskPixel.Red)
				{
					d += r;
					sq += (long)r * r;
				}
				if (g > maskPixel.Green)
				{
					d += g;
					sq += (long)g * g;
				}
				if (b > maskPixel.Blue)
				{
					d += b;
					sq += (long)b * b;
				}

				absoluteError += d;
				sumSquaredError += sq;
				if (d > 0)
					errorPixels++;
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError, sumSquaredError);
		}

		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, int tolerance)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return Compare(first, second, tolerance);
		}

		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, int tolerance)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return Compare(firstPixmap, secondPixmap, tolerance);
		}

		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, int tolerance)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return Compare(firstWrapper, secondWrapper, tolerance);
		}

		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, int tolerance)
		{
			if (tolerance < 0)
				throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be non-negative.");

			Validate(first, second);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;
			var errorPixels = 0;
			var absoluteError = 0;
			var sumSquaredError = 0L;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];

				var r = Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = Math.Abs(secondPixel.Blue - firstPixel.Blue);
				var d = r + g + b;

				if (d > tolerance)
				{
					absoluteError += d;
					sumSquaredError += (long)r * r + (long)g * g + (long)b * b;
					errorPixels++;
				}
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError, sumSquaredError);
		}

		public static SKImage GenerateDifferenceMask(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return GenerateDifferenceMask(first, second);
		}

		public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return GenerateDifferenceMask(firstPixmap, secondPixmap);
		}

		public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return GenerateDifferenceMask(firstWrapper, secondWrapper);
		}

		public static SKImage GenerateDifferenceMask(SKImage first, SKImage second)
		{
			Validate(first, second);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			var diffBitmap = new SKBitmap(new SKImageInfo(width, height));
			using var diffPixmap = diffBitmap.PeekPixels();
			var diffPixels = diffPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];

				var r = (byte)Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = (byte)Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = (byte)Math.Abs(secondPixel.Blue - firstPixel.Blue);

				diffPixels[idx] = (r + g + b) > 0 ? SKColors.White : SKColors.Black;
			}

			return SKImage.FromBitmap(diffBitmap);
		}

		public static SKImage GenerateDifferenceImage(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return GenerateDifferenceImage(first, second);
		}

		public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return GenerateDifferenceImage(firstPixmap, secondPixmap);
		}

		public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return GenerateDifferenceImage(firstWrapper, secondWrapper);
		}

		public static SKImage GenerateDifferenceImage(SKImage first, SKImage second)
		{
			Validate(first, second);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			var diffBitmap = new SKBitmap(new SKImageInfo(width, height));
			using var diffPixmap = diffBitmap.PeekPixels();
			var diffPixels = diffPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];

				var r = (byte)Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = (byte)Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = (byte)Math.Abs(secondPixel.Blue - firstPixel.Blue);

				diffPixels[idx] = new SKColor(r, g, b);
			}

			return SKImage.FromBitmap(diffBitmap);
		}

		private static void ValidateMask(SKImage first, SKImage mask)
		{
			_ = first ?? throw new ArgumentNullException(nameof(first));
			_ = mask ?? throw new ArgumentNullException(nameof(mask));

			var s1 = first.Info.Size;
			var s2 = mask.Info.Size;

			if (s1 != s2)
				throw new InvalidOperationException($"Unable to compare using mask of a different size: {s1.Width}x{s1.Height} vs {s2.Width}x{s2.Height}.");
		}

		private static void Validate(SKImage first, SKImage second)
		{
			_ = first ?? throw new ArgumentNullException(nameof(first));
			_ = second ?? throw new ArgumentNullException(nameof(second));

			var s1 = first.Info.Size;
			var s2 = second.Info.Size;

			if (s1 != s2)
				throw new InvalidOperationException($"Unable to compare images of different sizes: {s1.Width}x{s1.Height} vs {s2.Width}x{s2.Height}.");
		}

		private static SKBitmap GetNormalizedBitmap(SKImage image)
		{
			var width = image.Width;
			var height = image.Height;

			var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));

			using (var canvas = new SKCanvas(bitmap))
				canvas.DrawImage(image, 0, 0);

			return bitmap;
		}
	}
}
