using System;

namespace SkiaSharp.Extended
{
	/// <summary>
	/// Provides methods for pixel-by-pixel comparison of images.
	/// This class is thread-safe as all methods are stateless static operations.
	/// </summary>
	public static class SKPixelComparer
	{
		/// <summary>
		/// Compares two images loaded from file paths pixel by pixel.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return Compare(first, second);
		}

		/// <summary>
		/// Compares two bitmaps pixel by pixel.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return Compare(firstPixmap, secondPixmap);
		}

		/// <summary>
		/// Compares two pixmaps pixel by pixel.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return Compare(firstWrapper, secondWrapper);
		}

		/// <summary>
		/// Compares two images pixel by pixel.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The images have different dimensions.</exception>
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

		/// <summary>
		/// Compares two images loaded from file paths pixel by pixel, using a tolerance mask.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <param name="maskFilename">The file path of the mask image defining per-channel tolerance thresholds.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, string maskFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			using var mask = SKImage.FromEncodedData(maskFilename);
			return Compare(first, second, mask);
		}

		/// <summary>
		/// Compares two bitmaps pixel by pixel, using a tolerance mask.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <param name="mask">The mask bitmap defining per-channel tolerance thresholds.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKBitmap mask)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			using var maskPixmap = mask.PeekPixels();
			return Compare(firstPixmap, secondPixmap, maskPixmap);
		}

		/// <summary>
		/// Compares two pixmaps pixel by pixel, using a tolerance mask.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <param name="mask">The mask pixmap defining per-channel tolerance thresholds.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixmap mask)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			using var maskWrapper = SKImage.FromPixels(mask);
			return Compare(firstWrapper, secondWrapper, maskWrapper);
		}

		/// <summary>
		/// Compares two images pixel by pixel, using a tolerance mask. Pixel differences that fall within
		/// the mask's per-channel values are not counted as errors. Each channel is checked independently.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <param name="mask">The mask image defining per-channel tolerance thresholds.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/>, <paramref name="second"/>, or <paramref name="mask"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The images or mask have different dimensions.</exception>
		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKImage mask) =>
			Compare(first, second, mask, tolerancePerChannel: true);

		/// <summary>
		/// Compares two images loaded from file paths pixel by pixel, using a tolerance mask and specifying the tolerance mode.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <param name="maskFilename">The file path of the mask image defining per-channel tolerance thresholds.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the corresponding mask channel value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the sum of the mask's channel values.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, string maskFilename, bool tolerancePerChannel)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			using var mask = SKImage.FromEncodedData(maskFilename);
			return Compare(first, second, mask, tolerancePerChannel);
		}

		/// <summary>
		/// Compares two bitmaps pixel by pixel, using a tolerance mask and specifying the tolerance mode.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <param name="mask">The mask bitmap defining per-channel tolerance thresholds.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the corresponding mask channel value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the sum of the mask's channel values.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKBitmap mask, bool tolerancePerChannel)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			using var maskPixmap = mask.PeekPixels();
			return Compare(firstPixmap, secondPixmap, maskPixmap, tolerancePerChannel);
		}

		/// <summary>
		/// Compares two pixmaps pixel by pixel, using a tolerance mask and specifying the tolerance mode.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <param name="mask">The mask pixmap defining per-channel tolerance thresholds.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the corresponding mask channel value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the sum of the mask's channel values.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixmap mask, bool tolerancePerChannel)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			using var maskWrapper = SKImage.FromPixels(mask);
			return Compare(firstWrapper, secondWrapper, maskWrapper, tolerancePerChannel);
		}

		/// <summary>
		/// Compares two images pixel by pixel, using a tolerance mask and specifying the tolerance mode.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <param name="mask">The mask image defining per-channel tolerance thresholds.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the corresponding mask channel value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the sum of the mask's channel values.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/>, <paramref name="second"/>, or <paramref name="mask"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The images or mask have different dimensions.</exception>
		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKImage mask, bool tolerancePerChannel)
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

				if (tolerancePerChannel)
				{
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
				else
				{
					var d = r + g + b;
					var maskSum = maskPixel.Red + maskPixel.Green + maskPixel.Blue;
					if (d > maskSum)
					{
						absoluteError += d;
						sumSquaredError += (long)r * r + (long)g * g + (long)b * b;
						errorPixels++;
					}
				}
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError, sumSquaredError);
		}

		/// <summary>
		/// Compares two images loaded from file paths pixel by pixel, using a uniform per-pixel tolerance threshold.
		/// Pixels where the total per-channel difference (<c>|ΔR| + |ΔG| + |ΔB|</c>) is at or below the tolerance are not counted as errors.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <param name="tolerance">The maximum allowed sum of per-channel differences per pixel. Must be non-negative.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, int tolerance)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return Compare(first, second, tolerance);
		}

		/// <summary>
		/// Compares two bitmaps pixel by pixel, using a uniform per-pixel tolerance threshold.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <param name="tolerance">The maximum allowed sum of per-channel differences per pixel. Must be non-negative.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, int tolerance)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return Compare(firstPixmap, secondPixmap, tolerance);
		}

		/// <summary>
		/// Compares two pixmaps pixel by pixel, using a uniform per-pixel tolerance threshold.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <param name="tolerance">The maximum allowed sum of per-channel differences per pixel. Must be non-negative.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, int tolerance)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return Compare(firstWrapper, secondWrapper, tolerance);
		}

		/// <summary>
		/// Compares two images pixel by pixel, using a uniform per-pixel tolerance threshold.
		/// Pixels where the total per-channel difference (<c>|ΔR| + |ΔG| + |ΔB|</c>) is at or below the tolerance are not counted as errors.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <param name="tolerance">The maximum allowed sum of per-channel differences per pixel. Must be non-negative.</param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
		/// <exception cref="InvalidOperationException">The images have different dimensions.</exception>
		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, int tolerance) =>
			Compare(first, second, tolerance, tolerancePerChannel: false);

		/// <summary>
		/// Compares two images loaded from file paths pixel by pixel, using a uniform tolerance threshold and specifying the tolerance mode.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <param name="tolerance">The maximum allowed per-channel or total per-pixel difference. Must be non-negative.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the tolerance value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the tolerance value.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, int tolerance, bool tolerancePerChannel)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return Compare(first, second, tolerance, tolerancePerChannel);
		}

		/// <summary>
		/// Compares two bitmaps pixel by pixel, using a uniform tolerance threshold and specifying the tolerance mode.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <param name="tolerance">The maximum allowed per-channel or total per-pixel difference. Must be non-negative.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the tolerance value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the tolerance value.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, int tolerance, bool tolerancePerChannel)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return Compare(firstPixmap, secondPixmap, tolerance, tolerancePerChannel);
		}

		/// <summary>
		/// Compares two pixmaps pixel by pixel, using a uniform tolerance threshold and specifying the tolerance mode.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <param name="tolerance">The maximum allowed per-channel or total per-pixel difference. Must be non-negative.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the tolerance value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the tolerance value.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, int tolerance, bool tolerancePerChannel)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return Compare(firstWrapper, secondWrapper, tolerance, tolerancePerChannel);
		}

		/// <summary>
		/// Compares two images pixel by pixel, using a uniform tolerance threshold and specifying the tolerance mode.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <param name="tolerance">The maximum allowed per-channel or total per-pixel difference. Must be non-negative.</param>
		/// <param name="tolerancePerChannel">
		/// When <c>true</c>, each channel is checked independently against the tolerance value.
		/// A pixel is counted as an error only if at least one channel exceeds the tolerance.
		/// When <c>false</c>, the sum of per-channel differences (<c>|ΔR| + |ΔG| + |ΔB|</c>) is checked against the tolerance value.
		/// </param>
		/// <returns>An <see cref="SKPixelComparisonResult"/> containing the comparison statistics.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
		/// <exception cref="InvalidOperationException">The images have different dimensions.</exception>
		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, int tolerance, bool tolerancePerChannel)
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

				if (tolerancePerChannel)
				{
					var d = 0;
					var sq = 0L;
					if (r > tolerance)
					{
						d += r;
						sq += (long)r * r;
					}
					if (g > tolerance)
					{
						d += g;
						sq += (long)g * g;
					}
					if (b > tolerance)
					{
						d += b;
						sq += (long)b * b;
					}

					absoluteError += d;
					sumSquaredError += sq;
					if (d > 0)
						errorPixels++;
				}
				else
				{
					var d = r + g + b;
					if (d > tolerance)
					{
						absoluteError += d;
						sumSquaredError += (long)r * r + (long)g * g + (long)b * b;
						errorPixels++;
					}
				}
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError, sumSquaredError);
		}

		/// <summary>
		/// Generates a black-and-white mask image highlighting pixel differences between two images loaded from file paths.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <returns>An <see cref="SKImage"/> where differing pixels are white and matching pixels are black.</returns>
		public static SKImage GenerateDifferenceMask(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return GenerateDifferenceMask(first, second);
		}

		/// <summary>
		/// Generates a black-and-white mask image highlighting pixel differences between two bitmaps.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <returns>An <see cref="SKImage"/> where differing pixels are white and matching pixels are black.</returns>
		public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return GenerateDifferenceMask(firstPixmap, secondPixmap);
		}

		/// <summary>
		/// Generates a black-and-white mask image highlighting pixel differences between two pixmaps.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <returns>An <see cref="SKImage"/> where differing pixels are white and matching pixels are black.</returns>
		public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return GenerateDifferenceMask(firstWrapper, secondWrapper);
		}

		/// <summary>
		/// Generates a black-and-white mask image highlighting pixel differences between two images.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <returns>An <see cref="SKImage"/> where differing pixels are white and matching pixels are black.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The images have different dimensions.</exception>
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

		/// <summary>
		/// Generates a per-channel difference image from two images loaded from file paths.
		/// Each pixel in the result contains the absolute per-channel differences as RGB values.
		/// </summary>
		/// <param name="firstFilename">The file path of the first image.</param>
		/// <param name="secondFilename">The file path of the second image.</param>
		/// <returns>An <see cref="SKImage"/> where each pixel's RGB values represent the absolute per-channel differences.</returns>
		public static SKImage GenerateDifferenceImage(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return GenerateDifferenceImage(first, second);
		}

		/// <summary>
		/// Generates a per-channel difference image from two bitmaps.
		/// </summary>
		/// <param name="first">The first bitmap.</param>
		/// <param name="second">The second bitmap.</param>
		/// <returns>An <see cref="SKImage"/> where each pixel's RGB values represent the absolute per-channel differences.</returns>
		public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return GenerateDifferenceImage(firstPixmap, secondPixmap);
		}

		/// <summary>
		/// Generates a per-channel difference image from two pixmaps.
		/// </summary>
		/// <param name="first">The first pixmap.</param>
		/// <param name="second">The second pixmap.</param>
		/// <returns>An <see cref="SKImage"/> where each pixel's RGB values represent the absolute per-channel differences.</returns>
		public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return GenerateDifferenceImage(firstWrapper, secondWrapper);
		}

		/// <summary>
		/// Generates a per-channel difference image from two images. Each pixel in the result contains
		/// the absolute per-channel differences as RGB values, unlike <see cref="GenerateDifferenceMask(SKImage, SKImage)"/>
		/// which produces a binary black-and-white mask.
		/// </summary>
		/// <param name="first">The first image.</param>
		/// <param name="second">The second image.</param>
		/// <returns>An <see cref="SKImage"/> where each pixel's RGB values represent the absolute per-channel differences.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The images have different dimensions.</exception>
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
