using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKBlurHashDecoderTest
	{
		private const string Image1 = "LEHV6nWB2yk8pyo0adR*.7kCMdnj";
		private const string Image1_8x6 = "qEHV6nWB2yk8$NxujFNGpyoJadR*=ss:I[R%.7kCMdnjx]S2NHs:S#M|%1%2ENRis9a$Sis.slNHW:WBxZ%2ogaekBW;ofo0NHS4";
		private const string Image2 = "LGF5]+Yk^6#M@-5c,1J5@[or[Q6.";
		private const string Image2_8x6 = "qHF5]+Yk^6#M9wKSW@j=@-5b,1J5O[V=R:s;@[or[k6.O[TLtJnN};FxngOZE3NgNHspjMFxS#OtcXnzRjxZj]OYNeR:JCs9xunh";
		private const string Image3 = "L6Pj0^i_.AyE_3t7t7R**0o#DgR4";
		private const string Image4 = "LKO2?U%2Tw=w]~RBVZRi};RPxuwH";
		private const string Image6 = "LlMF%n00%#MwS|WCWEM{R*bbWBbH";
		private const string Image7 = "LjIY5?00?bIUofWBWBM{WBofWBj[";

		[Fact]
		public void NullDecodeThrows()
		{
			var blur = new SKBlurHashDecoder();

			Assert.Throws<ArgumentNullException>(() => blur.DecodeBitmap(null, 20, 10));
		}

		[Theory]
		[InlineData("")]
		[InlineData("1234")]
		[InlineData("123456")]
		[InlineData("1234567890")]
		public void InvalidDataThrows(string encoded)
		{
			var blur = new SKBlurHashDecoder();

			Assert.Throws<ArgumentException>(() => blur.DecodeBitmap(encoded, 20, 10));
		}

		[Theory]
		[InlineData(Image1)]
		[InlineData(Image1_8x6)]
		[InlineData(Image2)]
		[InlineData(Image2_8x6)]
		[InlineData(Image3)]
		[InlineData(Image4)]
		[InlineData(Image6)]
		[InlineData(Image7)]
		public void GeometryGeneratesRectPath(string encoded)
		{
			var blur = new SKBlurHashDecoder();

			var bmp = blur.DecodeBitmap(encoded, 20, 10);

			Assert.NotNull(bmp);
		}
	}
}
