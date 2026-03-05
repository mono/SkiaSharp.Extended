using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerCompareTest : SKPixelComparerTestBase
	{
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
