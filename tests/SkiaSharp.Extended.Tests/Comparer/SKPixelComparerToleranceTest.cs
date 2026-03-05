using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerToleranceTest : SKPixelComparerTestBase
	{
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

			var result = SKPixelComparer.Compare(first, second, tolerance, new SKPixelComparerOptions { TolerancePerChannel = true });

			Assert.Equal(expectedErrorPixels, result.ErrorPixelCount);
		}

		[Fact]
		public void TolerancePerChannelDiffersFromSumBehavior()
		{
			// Color diff: ΔR=1, ΔG=2, ΔB=3, sum=6
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF010203);

			// Sum-based: tolerance=5, sum(6) > 5 → error
			var sumResult = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = false });
			Assert.Equal(25, sumResult.ErrorPixelCount);

			// Per-channel: tolerance=5, each channel (1,2,3) ≤ 5 → no error
			var perChResult = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = true });
			Assert.Equal(0, perChResult.ErrorPixelCount);
		}

		[Fact]
		public void TolerancePerChannelPartialChannelExclusion()
		{
			// ΔR=1, ΔG=2, ΔB=10
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF01020A);

			// Per-channel with tolerance=5: R(1)≤5, G(2)≤5, B(10)>5 → only B counted
			var result = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = true });
			Assert.Equal(25, result.ErrorPixelCount);
			Assert.Equal(10 * 25, result.AbsoluteError);
			Assert.Equal((long)10 * 10 * 25, result.SumSquaredError);
		}

		[Fact]
		public void TolerancePerChannelOptionControlsBehavior()
		{
			// ΔR=5, ΔG=5, ΔB=5, sum=15
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF152535);

			var perCh = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = true });
			var sum = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = false });

			// Per-channel: each channel exactly at tolerance → no error
			Assert.Equal(0, perCh.ErrorPixelCount);
			// Sum: 15 > 5 → error
			Assert.Equal(25, sum.ErrorPixelCount);
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
		public void CompareAlphaWithToleranceSumBased()
		{
			// RGB identical, alpha diff = 10
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xF5000000); // alpha diff = 10

			var opts = new SKPixelComparerOptions { CompareAlpha = true, TolerancePerChannel = false };

			// Sum of diffs = 0 + 0 + 0 + 10 = 10; tolerance 15 → no error
			var withinResult = SKPixelComparer.Compare(first, second, 15, opts);
			Assert.Equal(0, withinResult.ErrorPixelCount);

			// tolerance 5 → error (10 > 5)
			var overResult = SKPixelComparer.Compare(first, second, 5, opts);
			Assert.Equal(25, overResult.ErrorPixelCount);
		}

		[Fact]
		public void CompareAlphaOnlyDiffWithRgbTolerance()
		{
			// RGB diff = (5,5,5), alpha diff = 127
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0x80050505);

			// Without alpha: tolerance 5 per-channel covers RGB → no error
			var rgbResult = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = true });
			Assert.Equal(0, rgbResult.ErrorPixelCount);

			// With alpha: RGB within tolerance but alpha (127) exceeds → error
			var alphaResult = SKPixelComparer.Compare(first, second, 5, new SKPixelComparerOptions { TolerancePerChannel = true, CompareAlpha = true });
			Assert.Equal(25, alphaResult.ErrorPixelCount);
		}

		[Fact]
		public void ToleranceBitmapAndPixmapWithOptions()
		{
			using var first = new SKBitmap(5, 5);
			using var second = new SKBitmap(5, 5);
			first.Erase(new SKColor(0xFF000000));
			second.Erase(new SKColor(0xFF030303));

			var opts = new SKPixelComparerOptions { TolerancePerChannel = true };

			// Bitmap overload
			var bmpResult = SKPixelComparer.Compare(first, second, 3, opts);
			Assert.Equal(0, bmpResult.ErrorPixelCount);

			// Pixmap overload
			using var firstPx = first.PeekPixels();
			using var secondPx = second.PeekPixels();
			var pxResult = SKPixelComparer.Compare(firstPx, secondPx, 3, opts);
			Assert.Equal(0, pxResult.ErrorPixelCount);
		}
	}
}
