using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerTest
	{
		private const string jpgFirst = "First.jpg";
		private const string pngFirst = "First.png";
		private const string jpgSecond = "Second.jpg";
		private const string pngSecond = "Second.png";
		private const string jpgMask = "MaskJpg.png";
		private const string pngMask = "MaskPng.png";

		public static readonly string BaseImages = Path.Combine("images", "Comparer");

		public static string GetImagePath(string filename) =>
			Path.Combine(BaseImages, filename);

		[Theory]
		[InlineData(0xFFFFFFFF, 0xFF000000, 19125, 25)]
		[InlineData(0xFF000000, 0xFFFFFFFF, 19125, 25)]
		[InlineData(0xFF000000, 0xFF000001, 25, 25)]
		[InlineData(0xFFFFFFFF, 0xFFFFFFFF, 0, 0)]
		[InlineData(0xFF000000, 0xFF000000, 0, 0)]
		[InlineData(0xFF000001, 0xFF000001, 0, 0)]
		[InlineData(0xFF00FF00, 0xFF00FF00, 0, 0)]
		public void DifferentImagesReportDifferenceCorrectly(uint firstColor, uint secondColor, int absolute, int count)
		{
			using var first = CreateTestImage(firstColor);
			using var second = CreateTestImage(secondColor);

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(count, result.ErrorPixelCount);
			Assert.Equal(25, result.TotalPixels);
			Assert.Equal(absolute, result.AbsoluteError);
		}

		[Theory]
		[InlineData(0xFFFFFFFF, 0xFF000000, 19125, 25)]
		[InlineData(0xFF000000, 0xFFFFFFFF, 19125, 25)]
		[InlineData(0xFF000000, 0xFF000001, 25, 25)]
		[InlineData(0xFFFFFFFF, 0xFFFFFFFF, 0, 0)]
		[InlineData(0xFF000000, 0xFF000000, 0, 0)]
		[InlineData(0xFF000001, 0xFF000001, 0, 0)]
		[InlineData(0xFF00FF00, 0xFF00FF00, 0, 0)]
		public void DifferentBitmapsReportDifferenceCorrectly(uint firstColor, uint secondColor, int absolute, int count)
		{
			using var first = CreateTestBitmap(firstColor);
			using var second = CreateTestBitmap(secondColor);

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(count, result.ErrorPixelCount);
			Assert.Equal(25, result.TotalPixels);
			Assert.Equal(absolute, result.AbsoluteError);
		}

		[Theory]
		[InlineData(0xFFFFFFFF, 0xFF000000, 19125, 25)]
		[InlineData(0xFF000000, 0xFFFFFFFF, 19125, 25)]
		[InlineData(0xFF000000, 0xFF000001, 25, 25)]
		[InlineData(0xFFFFFFFF, 0xFFFFFFFF, 0, 0)]
		[InlineData(0xFF000000, 0xFF000000, 0, 0)]
		[InlineData(0xFF000001, 0xFF000001, 0, 0)]
		[InlineData(0xFF00FF00, 0xFF00FF00, 0, 0)]
		public void DifferentPixmapsReportDifferenceCorrectly(uint firstColor, uint secondColor, int absolute, int count)
		{
			using var firstBitmap = CreateTestBitmap(firstColor);
			using var first = firstBitmap.PeekPixels();
			using var secondBitmap = CreateTestBitmap(secondColor);
			using var second = secondBitmap.PeekPixels();

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(count, result.ErrorPixelCount);
			Assert.Equal(25, result.TotalPixels);
			Assert.Equal(absolute, result.AbsoluteError);
		}

		[Theory]
		[InlineData(jpgFirst, jpgFirst)]
		[InlineData(pngFirst, pngFirst)]
		[InlineData(jpgSecond, jpgSecond)]
		[InlineData(pngSecond, pngSecond)]
		public void SameImagesReportNoDifference(string first, string second)
		{
			using var firstImage = SKImage.FromEncodedData(GetImagePath(first));
			using var secondImage = SKImage.FromEncodedData(GetImagePath(second));

			var result = SKPixelComparer.Compare(firstImage, secondImage);

			Assert.Equal(0, result.ErrorPixelCount);
			Assert.Equal(300104, result.TotalPixels);
			Assert.Equal(0, result.AbsoluteError);
		}

		[Theory]
		[InlineData(jpgFirst, jpgSecond, 2259184, 15870, 0.05288166768853464)]
		[InlineData(pngFirst, pngSecond, 2249290, 12570, 0.041885479700370536)]
		public void SimilarFilesAreSimilar(string first, string second, int expAbsError, int expPixError, double expPixPercent)
		{
			var result = SKPixelComparer.Compare(GetImagePath(first), GetImagePath(second));

			Assert.Equal(expAbsError, result.AbsoluteError);
			Assert.Equal(expPixError, result.ErrorPixelCount);
			Assert.Equal(expPixPercent, result.ErrorPixelPercentage);
		}

		[Theory]
		[InlineData(jpgFirst, jpgSecond, 2259184, 15870, 0.05288166768853464)]
		[InlineData(pngFirst, pngSecond, 2249290, 12570, 0.041885479700370536)]
		public void SimilarImagesAreSimilar(string first, string second, int expAbsError, int expPixError, double expPixPercent)
		{
			using var firstImage = SKImage.FromEncodedData(GetImagePath(first));
			using var secondImage = SKImage.FromEncodedData(GetImagePath(second));

			var result = SKPixelComparer.Compare(firstImage, secondImage);

			Assert.Equal(expAbsError, result.AbsoluteError);
			Assert.Equal(expPixError, result.ErrorPixelCount);
			Assert.Equal(expPixPercent, result.ErrorPixelPercentage);
		}

		[Theory]
		[InlineData(jpgFirst, pngFirst, 884487, 231040, 0.7698664462986164)]
		[InlineData(jpgSecond, pngSecond, 873399, 221697, 0.7387339055793991)]
		public void SimilarFilesAreCompressedDifferent(string first, string second, int expAbsError, int expPixError, double expPixPercent)
		{
			var result = SKPixelComparer.Compare(GetImagePath(first), GetImagePath(second));

			Assert.Equal(expAbsError, result.AbsoluteError);
			Assert.Equal(expPixError, result.ErrorPixelCount);
			Assert.Equal(expPixPercent, result.ErrorPixelPercentage);
		}

		[Theory]
		[InlineData(jpgFirst, pngFirst, 884487, 231040, 0.7698664462986164)]
		[InlineData(jpgSecond, pngSecond, 873399, 221697, 0.7387339055793991)]
		public void SimilarImagesAreCompressedDifferent(string first, string second, int expAbsError, int expPixError, double expPixPercent)
		{
			using var firstImage = SKImage.FromEncodedData(GetImagePath(first));
			using var secondImage = SKImage.FromEncodedData(GetImagePath(second));

			var result = SKPixelComparer.Compare(firstImage, secondImage);

			Assert.Equal(expAbsError, result.AbsoluteError);
			Assert.Equal(expPixError, result.ErrorPixelCount);
			Assert.Equal(expPixPercent, result.ErrorPixelPercentage);
		}

		[Theory]
		[InlineData(jpgFirst, jpgSecond)]
		[InlineData(pngFirst, pngSecond)]
		public void GeneratedMaskResultsInZeroDifferences(string first, string second)
		{
			using var firstImage = SKImage.FromEncodedData(GetImagePath(first));
			using var secondImage = SKImage.FromEncodedData(GetImagePath(second));

			var mask = SKPixelComparer.GenerateDifferenceMask(firstImage, secondImage);
			var result = SKPixelComparer.Compare(firstImage, secondImage, mask);

			Assert.Equal(0, result.AbsoluteError);
			Assert.Equal(0, result.ErrorPixelCount);
			Assert.Equal(0, result.ErrorPixelPercentage);
		}

		[Theory]
		[InlineData(jpgFirst, jpgSecond, jpgMask)]
		[InlineData(pngFirst, pngSecond, pngMask)]
		public void LoadedMaskResultsInZeroDifferences(string first, string second, string mask)
		{
			using var firstImage = SKImage.FromEncodedData(GetImagePath(first));
			using var secondImage = SKImage.FromEncodedData(GetImagePath(second));
			using var maskImage = SKImage.FromEncodedData(GetImagePath(mask));

			var result = SKPixelComparer.Compare(firstImage, secondImage, maskImage);

			Assert.Equal(0, result.AbsoluteError);
			Assert.Equal(0, result.ErrorPixelCount);
			Assert.Equal(0, result.ErrorPixelPercentage);
		}

		[Theory]
		[InlineData(10, 3, 10, 10)]
		[InlineData(10, 10, 10, 3)]
		[InlineData(3, 10, 10, 10)]
		[InlineData(10, 10, 3, 10)]
		public void DifferentSizeImagesFail(int w1, int h1, int w2, int h2)
		{
			using var first = CreateTestImage(SKColors.Black, w1, h1);
			using var second = CreateTestImage(SKColors.Black, w2, h2);

			var ex = Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(first, second));
			Assert.Contains($"{w1}x{h1} vs {w2}x{h2}", ex.Message);
		}

		static SKImage CreateTestImage(SKColor color, int width = 5, int height = 5)
		{
			using var surface = SKSurface.Create(new SKImageInfo(width, height));
			surface.Canvas.Clear(color);
			return surface.Snapshot();
		}

		static SKBitmap CreateTestBitmap(SKColor color, int width = 5, int height = 5)
		{
			var bmp = new SKBitmap(new SKImageInfo(width, height));
			bmp.Erase(color);
			return bmp;
		}

		static void SaveOutputDiff(SKImage first, SKImage second)
		{
			using var img = SKPixelComparer.GenerateDifferenceMask(first, second);
			using var data = img.Encode(SKEncodedImageFormat.Png, 100);
			using (var str = File.Create(GetImagePath("diff.png")))
				data.SaveTo(str);
		}

		[Theory]
		[InlineData(0xFFFFFFFF, 0xFF000000, 4876875)]
		[InlineData(0xFF000000, 0xFFFFFFFF, 4876875)]
		[InlineData(0xFF000000, 0xFF000001, 25)]
		[InlineData(0xFFFFFFFF, 0xFFFFFFFF, 0)]
		[InlineData(0xFF000000, 0xFF000000, 0)]
		public void CompareTracksSumSquaredError(uint firstColor, uint secondColor, long expectedSumSquaredError)
		{
			using var first = CreateTestImage(firstColor);
			using var second = CreateTestImage(secondColor);

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(expectedSumSquaredError, result.SumSquaredError);
		}

		[Fact]
		public void IdenticalImagesHaveZeroMetrics()
		{
			using var first = CreateTestImage(0xFF808080);
			using var second = CreateTestImage(0xFF808080);

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(0, result.SumSquaredError);
			Assert.Equal(0.0, result.MeanAbsoluteError);
			Assert.Equal(0.0, result.MeanSquaredError);
			Assert.Equal(0.0, result.RootMeanSquaredError);
			Assert.Equal(0.0, result.NormalizedRootMeanSquaredError);
			Assert.Equal(double.PositiveInfinity, result.PeakSignalToNoiseRatio);
		}

		[Fact]
		public void MaxDifferenceImagesHaveCorrectMetrics()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFFFFFFFF);

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(255.0, result.MeanAbsoluteError);
			Assert.Equal(65025.0, result.MeanSquaredError);
			Assert.Equal(255.0, result.RootMeanSquaredError);
			Assert.Equal(1.0, result.NormalizedRootMeanSquaredError);
			Assert.Equal(0.0, result.PeakSignalToNoiseRatio);
		}

		[Fact]
		public void MetricsAreConsistentWithFormulas()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF010203);

			var result = SKPixelComparer.Compare(first, second);

			// Per pixel: ΔR=1, ΔG=2, ΔB=3
			// 25 pixels, 3 channels per pixel = 75 channel values
			var expectedAbsoluteError = (1 + 2 + 3) * 25;
			var expectedSumSquared = (long)(1 * 1 + 2 * 2 + 3 * 3) * 25;

			Assert.Equal(expectedAbsoluteError, result.AbsoluteError);
			Assert.Equal(expectedSumSquared, result.SumSquaredError);
			Assert.Equal((double)expectedAbsoluteError / (25 * 3.0), result.MeanAbsoluteError);
			Assert.Equal((double)expectedSumSquared / (25 * 3.0), result.MeanSquaredError);
			Assert.Equal(Math.Sqrt((double)expectedSumSquared / (25 * 3.0)), result.RootMeanSquaredError);
		}

		[Theory]
		[InlineData(0xFF000000, 0xFF030303, 0, 25)]
		[InlineData(0xFF000000, 0xFF030303, 8, 25)]
		[InlineData(0xFF000000, 0xFF030303, 9, 0)]
		[InlineData(0xFF000000, 0xFF030303, 100, 0)]
		[InlineData(0xFF000000, 0xFFFFFFFF, 764, 25)]
		[InlineData(0xFF000000, 0xFFFFFFFF, 765, 0)]
		public void ToleranceBasedCompareFiltersSmallDifferences(uint firstColor, uint secondColor, int tolerance, int expectedErrorPixels)
		{
			using var first = CreateTestImage(firstColor);
			using var second = CreateTestImage(secondColor);

			var result = SKPixelComparer.Compare(first, second, tolerance);

			Assert.Equal(expectedErrorPixels, result.ErrorPixelCount);
		}

		[Fact]
		public void ToleranceZeroMatchesStandardCompare()
		{
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF112233);

			var standard = SKPixelComparer.Compare(first, second);
			var withTolerance = SKPixelComparer.Compare(first, second, 0);

			Assert.Equal(standard.ErrorPixelCount, withTolerance.ErrorPixelCount);
			Assert.Equal(standard.AbsoluteError, withTolerance.AbsoluteError);
			Assert.Equal(standard.SumSquaredError, withTolerance.SumSquaredError);
		}

		[Fact]
		public void NegativeToleranceThrows()
		{
			using var first = CreateTestImage(SKColors.Black);
			using var second = CreateTestImage(SKColors.White);

			Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(first, second, -1));
		}

		[Theory]
		[InlineData(0xFF000000, 0xFF030303, 10)]
		public void ToleranceCompareWithBitmapsWorks(uint firstColor, uint secondColor, int tolerance)
		{
			using var first = CreateTestBitmap(firstColor);
			using var second = CreateTestBitmap(secondColor);

			var result = SKPixelComparer.Compare(first, second, tolerance);

			Assert.Equal(0, result.ErrorPixelCount);
		}

		[Theory]
		[InlineData(0xFF000000, 0xFF030303, 10)]
		public void ToleranceCompareWithPixmapsWorks(uint firstColor, uint secondColor, int tolerance)
		{
			using var firstBitmap = CreateTestBitmap(firstColor);
			using var first = firstBitmap.PeekPixels();
			using var secondBitmap = CreateTestBitmap(secondColor);
			using var second = secondBitmap.PeekPixels();

			var result = SKPixelComparer.Compare(first, second, tolerance);

			Assert.Equal(0, result.ErrorPixelCount);
		}

		[Fact]
		public void GenerateDifferenceImageShowsPerChannelDifferences()
		{
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF132639);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			Assert.Equal(5, diff.Width);
			Assert.Equal(5, diff.Height);

			using var bitmap = GetNormalizedBitmap(diff);
			using var pixmap = bitmap.PeekPixels();
			var pixel = pixmap.GetPixelSpan<SKColor>()[0];

			Assert.Equal(3, pixel.Red);
			Assert.Equal(6, pixel.Green);
			Assert.Equal(9, pixel.Blue);
		}

		[Fact]
		public void GenerateDifferenceImageIdenticalIsBlack()
		{
			using var first = CreateTestImage(0xFFAABBCC);
			using var second = CreateTestImage(0xFFAABBCC);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			using var bitmap = GetNormalizedBitmap(diff);
			using var pixmap = bitmap.PeekPixels();
			var pixel = pixmap.GetPixelSpan<SKColor>()[0];

			Assert.Equal(0, pixel.Red);
			Assert.Equal(0, pixel.Green);
			Assert.Equal(0, pixel.Blue);
		}

		[Fact]
		public void GenerateDifferenceImageWithBitmapsWorks()
		{
			using var first = CreateTestBitmap(0xFF000000);
			using var second = CreateTestBitmap(0xFF050505);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			Assert.NotNull(diff);
			Assert.Equal(5, diff.Width);
			Assert.Equal(5, diff.Height);
		}

		[Fact]
		public void GenerateDifferenceImageWithDifferentSizesFails()
		{
			using var first = CreateTestImage(SKColors.Black, 5, 5);
			using var second = CreateTestImage(SKColors.Black, 10, 10);

			Assert.Throws<InvalidOperationException>(() => SKPixelComparer.GenerateDifferenceImage(first, second));
		}

		static SKBitmap GetNormalizedBitmap(SKImage image)
		{
			var bitmap = new SKBitmap(new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888));
			using (var canvas = new SKCanvas(bitmap))
				canvas.DrawImage(image, 0, 0);
			return bitmap;
		}

		[Fact]
		public void ZeroTotalPixelsResultDoesNotThrow()
		{
			var result = new SKPixelComparisonResult(0, 0, 0, 0);

			Assert.Equal(0.0, result.ErrorPixelPercentage);
			Assert.Equal(0.0, result.MeanAbsoluteError);
			Assert.Equal(0.0, result.MeanSquaredError);
			Assert.Equal(0.0, result.RootMeanSquaredError);
			Assert.Equal(0.0, result.NormalizedRootMeanSquaredError);
			Assert.Equal(double.PositiveInfinity, result.PeakSignalToNoiseRatio);
		}

		[Theory]
		[InlineData(0xFF000000, 0xFF030303, 2, 25)]
		[InlineData(0xFF000000, 0xFF030303, 3, 0)]
		[InlineData(0xFF000000, 0xFF030303, 4, 0)]
		[InlineData(0xFF010203, 0xFF000000, 2, 25)]
		[InlineData(0xFF010203, 0xFF000000, 3, 0)]
		public void TolerancePerChannelChecksEachChannelIndependently(uint firstColor, uint secondColor, int tolerance, int expectedErrorPixels)
		{
			using var first = CreateTestImage(firstColor);
			using var second = CreateTestImage(secondColor);

			var result = SKPixelComparer.Compare(first, second, tolerance, tolerancePerChannel: true);

			Assert.Equal(expectedErrorPixels, result.ErrorPixelCount);
		}

		[Fact]
		public void TolerancePerChannelDiffersFromSumBehavior()
		{
			// Color diff: ΔR=1, ΔG=2, ΔB=3, sum=6
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF010203);

			// Sum-based: tolerance=5, sum(6) > 5 → error
			var sumResult = SKPixelComparer.Compare(first, second, 5, tolerancePerChannel: false);
			Assert.Equal(25, sumResult.ErrorPixelCount);

			// Per-channel: tolerance=5, each channel (1,2,3) ≤ 5 → no error
			var perChResult = SKPixelComparer.Compare(first, second, 5, tolerancePerChannel: true);
			Assert.Equal(0, perChResult.ErrorPixelCount);
		}

		[Fact]
		public void TolerancePerChannelPartialChannelExclusion()
		{
			// ΔR=1, ΔG=2, ΔB=10
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF01020A);

			// Per-channel with tolerance=5: R(1)≤5, G(2)≤5, B(10)>5 → only B counted
			var result = SKPixelComparer.Compare(first, second, 5, tolerancePerChannel: true);
			Assert.Equal(25, result.ErrorPixelCount);
			Assert.Equal(10 * 25, result.AbsoluteError);
			Assert.Equal((long)10 * 10 * 25, result.SumSquaredError);
		}

		[Fact]
		public void MaskTolerancePerChannelFalseUsesSumSemantics()
		{
			// ΔR=1, ΔG=2, ΔB=3, sum=6
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF010203);

			// Mask with per-channel values (2,2,2) → sum = 6
			// Per-channel (default): R(1)≤2 skip, G(2)≤2 skip, B(3)>2 count → error
			using var mask = CreateTestImage(0xFF020202);
			var perChResult = SKPixelComparer.Compare(first, second, mask, tolerancePerChannel: true);
			Assert.Equal(25, perChResult.ErrorPixelCount);

			// Sum mode: sum(6) > maskSum(6) → false, no error
			var sumResult = SKPixelComparer.Compare(first, second, mask, tolerancePerChannel: false);
			Assert.Equal(0, sumResult.ErrorPixelCount);
		}

		[Fact]
		public void GenerateDifferenceMaskProducesValidImage()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF050505);

			using var mask = SKPixelComparer.GenerateDifferenceMask(first, second);

			Assert.NotNull(mask);
			Assert.Equal(5, mask.Width);
			Assert.Equal(5, mask.Height);
		}

		[Fact]
		public void GenerateDifferenceImageProducesValidImage()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF050505);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			Assert.NotNull(diff);
			Assert.Equal(5, diff.Width);
			Assert.Equal(5, diff.Height);
		}

		// SKPixelComparerOptions tests

		[Fact]
		public void CompareWithOptionsNullUseDefaults()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF050505);

			var result = SKPixelComparer.Compare(first, second, (SKPixelComparerOptions)null);

			Assert.Equal(25, result.TotalPixels);
			Assert.Equal(25, result.ErrorPixelCount);
			Assert.Equal(3, result.ChannelCount);
		}

		[Fact]
		public void CompareWithOptionsDefaultsMatchBaseOverload()
		{
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF112233);

			var baseResult = SKPixelComparer.Compare(first, second);
			var optsResult = SKPixelComparer.Compare(first, second, new SKPixelComparerOptions());

			Assert.Equal(baseResult.AbsoluteError, optsResult.AbsoluteError);
			Assert.Equal(baseResult.ErrorPixelCount, optsResult.ErrorPixelCount);
			Assert.Equal(baseResult.SumSquaredError, optsResult.SumSquaredError);
		}

		[Fact]
		public void CompareAlphaIncludesAlphaChannelDifferences()
		{
			// Two images identical in RGB but different in alpha
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0x80000000); // alpha 128 vs 255

			var rgbResult = SKPixelComparer.Compare(first, second);
			var alphaResult = SKPixelComparer.Compare(first, second, new SKPixelComparerOptions { CompareAlpha = true });

			// RGB-only should show no error (both are black)
			Assert.Equal(0, rgbResult.ErrorPixelCount);
			Assert.Equal(0, rgbResult.AbsoluteError);
			Assert.Equal(3, rgbResult.ChannelCount);

			// Alpha-aware should detect the difference
			Assert.Equal(25, alphaResult.ErrorPixelCount);
			Assert.True(alphaResult.AbsoluteError > 0);
			Assert.Equal(4, alphaResult.ChannelCount);
		}

		[Fact]
		public void CompareAlphaIdenticalImagesReportsZero()
		{
			using var first = CreateTestImage(0x80FF0000);
			using var second = CreateTestImage(0x80FF0000);

			var result = SKPixelComparer.Compare(first, second, new SKPixelComparerOptions { CompareAlpha = true });

			Assert.Equal(0, result.ErrorPixelCount);
			Assert.Equal(0, result.AbsoluteError);
			Assert.Equal(4, result.ChannelCount);
		}

		[Fact]
		public void CompareAlphaWithToleranceExcludesSmallDifferences()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFE000000); // alpha diff = 1

			var opts = new SKPixelComparerOptions { CompareAlpha = true, TolerancePerChannel = true };
			var result = SKPixelComparer.Compare(first, second, 5, opts);

			// Alpha diff of 1 is within tolerance of 5
			Assert.Equal(0, result.ErrorPixelCount);
		}

		[Fact]
		public void CompareAlphaWithToleranceDetectsLargeDifferences()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0x80000000); // alpha diff = 127

			var opts = new SKPixelComparerOptions { CompareAlpha = true, TolerancePerChannel = true };
			var result = SKPixelComparer.Compare(first, second, 5, opts);

			Assert.Equal(25, result.ErrorPixelCount);
		}

		[Fact]
		public void CompareAlphaWithMaskPerChannel()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xF0000000); // alpha diff = 15
			using var mask = CreateTestImage(0x14141414); // tolerance 20 per channel (including alpha)

			var opts = new SKPixelComparerOptions { CompareAlpha = true, TolerancePerChannel = true };
			var result = SKPixelComparer.Compare(first, second, mask, opts);

			// Alpha diff of 15 is within mask alpha tolerance of 20
			Assert.Equal(0, result.ErrorPixelCount);
		}

		[Fact]
		public void TolerancePerChannelWithOptionsMatchesBoolOverload()
		{
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF152535);

			var boolResult = SKPixelComparer.Compare(first, second, 3, true);
			var optsResult = SKPixelComparer.Compare(first, second, 3, new SKPixelComparerOptions { TolerancePerChannel = true });

			Assert.Equal(boolResult.AbsoluteError, optsResult.AbsoluteError);
			Assert.Equal(boolResult.ErrorPixelCount, optsResult.ErrorPixelCount);
			Assert.Equal(boolResult.SumSquaredError, optsResult.SumSquaredError);
		}

		[Fact]
		public void ChannelCountReflectsCompareAlpha()
		{
			var rgb = new SKPixelComparisonResult(100, 10, 50, 200);
			var rgba = new SKPixelComparisonResult(100, 10, 50, 200, 4);

			Assert.Equal(3, rgb.ChannelCount);
			Assert.Equal(4, rgba.ChannelCount);

			// MAE differs due to different divisors
			Assert.Equal(50.0 / (100 * 3.0), rgb.MeanAbsoluteError);
			Assert.Equal(50.0 / (100 * 4.0), rgba.MeanAbsoluteError);
		}
	}
}
