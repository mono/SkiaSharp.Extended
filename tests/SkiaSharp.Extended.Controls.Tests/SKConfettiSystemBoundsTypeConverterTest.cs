using System;
using Xamarin.Forms;
using Xunit;

namespace SkiaSharp.Extended.Controls.Tests
{
	public class SKConfettiSystemBoundsTypeConverterTest
	{
		[Theory]
		[InlineData("center", SKConfettiSystemSide.Center)]
		[InlineData(" center ", SKConfettiSystemSide.Center)]
		[InlineData("top", SKConfettiSystemSide.Top)]
		[InlineData("left", SKConfettiSystemSide.Left)]
		[InlineData("right", SKConfettiSystemSide.Right)]
		[InlineData("bottom", SKConfettiSystemSide.Bottom)]
		public void CanParseSideValues(string value, SKConfettiSystemSide side)
		{
			var converter = new SKConfettiSystemBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var bounds = Assert.IsType<SKConfettiSystemBounds>(result);

			Assert.Equal(side, bounds.Side);
			Assert.Equal(Rect.Zero, bounds.Rect);
		}

		[Theory]
		[InlineData("12,34")]
		[InlineData("12, 34")]
		[InlineData(" 12, 34 ")]
		public void CanParsePointValues(string value)
		{
			var converter = new SKConfettiSystemBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var bounds = Assert.IsType<SKConfettiSystemBounds>(result);

			Assert.Equal(SKConfettiSystemSide.Bounds, bounds.Side);
			Assert.Equal(new Rect(12, 34, 0, 0), bounds.Rect);
		}

		[Theory]
		[InlineData("12,34,56,78")]
		[InlineData("12, 34, 56, 78")]
		[InlineData(" 12, 34, 56, 78 ")]
		public void CanParseRectValues(string value)
		{
			var converter = new SKConfettiSystemBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var bounds = Assert.IsType<SKConfettiSystemBounds>(result);

			Assert.Equal(SKConfettiSystemSide.Bounds, bounds.Side);
			Assert.Equal(new Rect(12, 34, 56, 78), bounds.Rect);
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("1,2,3")]
		public void ThrowsOnBadValues(string value)
		{
			var converter = new SKConfettiSystemBoundsTypeConverter();

			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(value));
		}

		[Fact]
		public void NullIsNull()
		{
			var converter = new SKConfettiSystemBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(null);

			Assert.Null(result);
		}
	}
}
