#nullable enable
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerMaskTest : SKPixelComparerTestBase
	{
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

		[Fact]
		public void MaskTolerancePerChannelFalseUsesSumSemantics()
		{
			// ΔR=1, ΔG=2, ΔB=3, sum=6
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF010203);

			// Mask with per-channel values (2,2,2) → sum = 6
			// Per-channel (default): R(1)≤2 skip, G(2)≤2 skip, B(3)>2 count → error
			using var mask = CreateTestImage(0xFF020202);
			var perChResult = SKPixelComparer.Compare(first, second, mask, new SKPixelComparerOptions { TolerancePerChannel = true });
			Assert.Equal(25, perChResult.ErrorPixelCount);

			// Sum mode: sum(6) > maskSum(6) → false, no error
			var sumResult = SKPixelComparer.Compare(first, second, mask, new SKPixelComparerOptions { TolerancePerChannel = false });
			Assert.Equal(0, sumResult.ErrorPixelCount);
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
		public void CompareAlphaWithMaskSumBased()
		{
			// RGB identical, alpha diff = 10
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xF5000000);
			// Mask sum = 0+0+0+20 = 20, alpha diff 10 ≤ 20 → no error
			using var mask = CreateTestImage(0x14000000);

			var opts = new SKPixelComparerOptions { CompareAlpha = true, TolerancePerChannel = false };
			var result = SKPixelComparer.Compare(first, second, mask, opts);
			Assert.Equal(0, result.ErrorPixelCount);
		}

		[Fact]
		public void NullMaskThrows()
		{
			using var first = CreateTestImage(SKColors.Black);
			using var second = CreateTestImage(SKColors.White);

			Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare(first, second, (SKImage)null!));
		}

		[Fact]
		public void MaskDimensionMismatchThrows()
		{
			using var first = CreateTestImage(SKColors.Black, 5, 5);
			using var second = CreateTestImage(SKColors.White, 5, 5);
			using var mask = CreateTestImage(SKColors.White, 10, 10);

			Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(first, second, mask));
		}

		[Fact]
		public void MaskCompareWithBitmapsWorks()
		{
			using var first = CreateTestBitmap(0xFF000000);
			using var second = CreateTestBitmap(0xFF050505);
			using var mask = CreateTestBitmap(0xFF050505);

			using var firstImg = CreateTestImage(0xFF000000);
			using var secondImg = CreateTestImage(0xFF050505);
			using var maskImg = CreateTestImage(0xFF050505);

			var bmpResult = SKPixelComparer.Compare(first, second, mask);
			var imgResult = SKPixelComparer.Compare(firstImg, secondImg, maskImg);

			Assert.Equal(imgResult.ErrorPixelCount, bmpResult.ErrorPixelCount);
			Assert.Equal(imgResult.AbsoluteError, bmpResult.AbsoluteError);
		}

		[Fact]
		public void MaskCompareWithPixmapsWorks()
		{
			using var firstBmp = CreateTestBitmap(0xFF000000);
			using var secondBmp = CreateTestBitmap(0xFF050505);
			using var maskBmp = CreateTestBitmap(0xFF050505);

			using var first = firstBmp.PeekPixels();
			using var second = secondBmp.PeekPixels();
			using var mask = maskBmp.PeekPixels();

			using var firstImg = CreateTestImage(0xFF000000);
			using var secondImg = CreateTestImage(0xFF050505);
			using var maskImg = CreateTestImage(0xFF050505);

			var pxResult = SKPixelComparer.Compare(first, second, mask);
			var imgResult = SKPixelComparer.Compare(firstImg, secondImg, maskImg);

			Assert.Equal(imgResult.ErrorPixelCount, pxResult.ErrorPixelCount);
			Assert.Equal(imgResult.AbsoluteError, pxResult.AbsoluteError);
		}
	}
}
