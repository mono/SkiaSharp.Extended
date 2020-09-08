using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class Base83Test
	{
		[Fact]
		public void NullDecodeThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Base83.Decode(null));
		}

		[Theory]
		[InlineData("", 0)]
		[InlineData("0", 0)]
		[InlineData("1", 1)]
		[InlineData("01", 1)]
		[InlineData("A", 10)]
		[InlineData("10", 83)]
		[InlineData("010", 83)]
		[InlineData("G9", 1337)]
		[InlineData("0000000000000G9", 1337)]
		public void DecodeIsCorrect(string encoded, int expected)
		{
			var decoded = Base83.Decode(encoded);

			Assert.Equal(expected, decoded);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		public void NumbersTo83AreVBalid(int prefixZeros)
		{
			var pre = new string('0', prefixZeros);

			for (var i = 0; i < 83; i++)
			{
				var encoded = Base83.Encode(i, prefixZeros + 1);

				Assert.Equal(pre + Base83.Charset[i], encoded);
			}
		}

		[Theory]
		[InlineData(0, "00")]
		[InlineData(1, "01")]
		[InlineData(10, "0A")]
		[InlineData(83, "10")]
		[InlineData(1092, "DD")]
		[InlineData(1337, "G9")]
		public void EncodeIsCorrect(int value, string expected)
		{
			var encoded = Base83.Encode(value, 2);

			Assert.Equal(expected, encoded);
		}
	}
}
