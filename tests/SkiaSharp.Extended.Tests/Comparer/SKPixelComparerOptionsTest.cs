using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerOptionsTest : SKPixelComparerTestBase
	{
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
		public void CompareWithOptionsMatchesToleranceZeroWithOptions()
		{
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF152535);

			var opts = new SKPixelComparerOptions { CompareAlpha = true };
			var optsResult = SKPixelComparer.Compare(first, second, opts);
			var tolResult = SKPixelComparer.Compare(first, second, 0, opts);

			Assert.Equal(tolResult.AbsoluteError, optsResult.AbsoluteError);
			Assert.Equal(tolResult.ErrorPixelCount, optsResult.ErrorPixelCount);
			Assert.Equal(tolResult.SumSquaredError, optsResult.SumSquaredError);
			Assert.Equal(tolResult.ChannelCount, optsResult.ChannelCount);
		}

		[Fact]
		public void CompareWithBitmapsAndOptions()
		{
			using var first = new SKBitmap(5, 5);
			using var second = new SKBitmap(5, 5);
			first.Erase(new SKColor(0xFF000000));
			second.Erase(new SKColor(0x80000000));

			var opts = new SKPixelComparerOptions { CompareAlpha = true };
			var result = SKPixelComparer.Compare(first, second, opts);

			Assert.Equal(25, result.ErrorPixelCount);
			Assert.Equal(4, result.ChannelCount);
		}

		[Fact]
		public void CompareWithPixmapsAndOptions()
		{
			using var first = new SKBitmap(5, 5);
			using var second = new SKBitmap(5, 5);
			first.Erase(new SKColor(0xFF000000));
			second.Erase(new SKColor(0x80000000));

			using var firstPx = first.PeekPixels();
			using var secondPx = second.PeekPixels();

			var opts = new SKPixelComparerOptions { CompareAlpha = true };
			var result = SKPixelComparer.Compare(firstPx, secondPx, opts);

			Assert.Equal(25, result.ErrorPixelCount);
			Assert.Equal(4, result.ChannelCount);
		}

		[Fact]
		public void SemiTransparentPixelsCompareAlphaFalseIgnoresAlpha()
		{
			// Same RGB (0x80, 0x40, 0x20), different alpha (0xFF vs 0x80)
			using var first = CreateTestImage(0xFF804020);
			using var second = CreateTestImage(0x80804020);

			var result = SKPixelComparer.Compare(first, second);

			Assert.Equal(0, result.ErrorPixelCount);
			Assert.Equal(0, result.AbsoluteError);
		}

		[Fact]
		public void SemiTransparentPixelsCompareAlphaTrueDetectsAlpha()
		{
			// Same RGB (0x80, 0x40, 0x20), different alpha (0xFF vs 0x80)
			using var first = CreateTestImage(0xFF804020);
			using var second = CreateTestImage(0x80804020);

			var opts = new SKPixelComparerOptions { CompareAlpha = true };
			var result = SKPixelComparer.Compare(first, second, opts);

			Assert.Equal(25, result.ErrorPixelCount);
			Assert.True(result.AbsoluteError > 0);
			Assert.Equal(4, result.ChannelCount);
		}
	}
}
