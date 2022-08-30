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
		public void SameFilesReportNoDifference(string first, string second)
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
	}
}
