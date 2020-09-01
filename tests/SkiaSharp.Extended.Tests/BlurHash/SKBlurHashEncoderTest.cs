using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKBlurHashEncoderTest
	{
		private const string Image1 = "LLHV6nae2ek8lAo0aeR*%fkCMxn%";
		private const string Image1_8x6 = "qEHV6nWB2yk8$NxujFNGpyoJadR*=ss:I[R%.7kCMdnjx]S2NHs:S#M|%1%2ENRis9a$Sis.slNHW:WBxZ%2ogaekBW;ofo0NHS4";
		private const string Image2 = "LGF5]+Yk^6#M@-5c,1J5@[or[Q6.";
		private const string Image2_8x6 = "qHF5]+Yk^6#M9wKSW@j=@-5b,1J5O[V=R:s;@[or[k6.O[TLtJnN};FxngOZE3NgNHspjMFxS#OtcXnzRjxZj]OYNeR:JCs9xunh";
		private const string Image3 = "L6Pj0^i_.AyE_3t7t7R**0o#DgR4";
		private const string Image4 = "LKO2?U%2Tw=w]~RBVZRi};RPxuwH";
		private const string Image6 = "LlMF%n00%#MwS|WCWEM{R*bbWBbH";
		private const string Image7 = "LjIY5?00?bIUofWBWBM{WBofWBj[";

		[Fact]
		public void NullEncodeThrows()
		{
			var blur = new SKBlurHashEncoder();

			Assert.Throws<ArgumentNullException>(() => blur.Encode((SKBitmap)null, 4, 3));
		}

		[Theory]
		[InlineData("img1.jpg", 4, 3, Image1)]
		[InlineData("img1.jpg", 8, 6, Image1_8x6)]
		[InlineData("img2.jpg", 4, 3, Image2)]
		[InlineData("img2.jpg", 8, 6, Image2_8x6)]
		[InlineData("img3.jpg", 4, 3, Image3)]
		[InlineData("img4.jpg", 4, 3, Image4)]
		[InlineData("img6.png", 4, 3, Image6)]
		[InlineData("img7.png", 4, 3, Image7)]
		public void InvalidDataThrows(string source, int compX, int compY, string expected)
		{
			using var img = SKBitmap.Decode(Path.Combine("images", source));

			var blur = new SKBlurHashEncoder();

			var result = blur.Encode(img, compX, compY);

			Assert.Equal(expected.Length, result.Length);
			Assert.Equal(expected.Substring(0, 6), result.Substring(0, 6));
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(0f, 0f, 0f, 0)]
		public void EncodeDCReturnsCorrectValue(float r, float g, float b, int dc)
		{
			var color = new SKPoint3(r, g, b);
			var encoded = EncodeDC(color);

			Assert.Equal(encoded, dc);
		}

		private static int EncodeDC(SKPoint3 color)
		{
			var type = typeof(SKBlurHashEncoder);
			var method = type.GetMethod("EncodeDC", BindingFlags.NonPublic | BindingFlags.Static);
			return (int)method.Invoke(null, new object[] { color });
		}
	}
}
