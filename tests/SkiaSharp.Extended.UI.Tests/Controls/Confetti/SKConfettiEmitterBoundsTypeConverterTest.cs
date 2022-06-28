using System;
using SkiaSharp.Extended.UI.Controls.Converters;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests
{
	public class SKConfettiEmitterBoundsTypeConverterTest
	{
		[Theory]
		[InlineData("center", SKConfettiEmitterSide.Center)]
		[InlineData(" center ", SKConfettiEmitterSide.Center)]
		[InlineData("top", SKConfettiEmitterSide.Top)]
		[InlineData("left", SKConfettiEmitterSide.Left)]
		[InlineData("right", SKConfettiEmitterSide.Right)]
		[InlineData("bottom", SKConfettiEmitterSide.Bottom)]
		public void CanParseSideValues(string value, SKConfettiEmitterSide side)
		{
			var converter = new SKConfettiEmitterBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var bounds = Assert.IsType<SKConfettiEmitterBounds>(result);

			Assert.Equal(side, bounds.Side);
			Assert.Equal(Rect.Zero, bounds.Rect);
		}

		[Theory]
		[InlineData("12,34")]
		[InlineData("12, 34")]
		[InlineData(" 12, 34 ")]
		public void CanParsePointValues(string value)
		{
			var converter = new SKConfettiEmitterBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var bounds = Assert.IsType<SKConfettiEmitterBounds>(result);

			Assert.Equal(SKConfettiEmitterSide.Bounds, bounds.Side);
			Assert.Equal(new Rect(12, 34, 0, 0), bounds.Rect);
		}

		[Theory]
		[InlineData("12,34,56,78")]
		[InlineData("12, 34, 56, 78")]
		[InlineData(" 12, 34, 56, 78 ")]
		public void CanParseRectValues(string value)
		{
			var converter = new SKConfettiEmitterBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var bounds = Assert.IsType<SKConfettiEmitterBounds>(result);

			Assert.Equal(SKConfettiEmitterSide.Bounds, bounds.Side);
			Assert.Equal(new Rect(12, 34, 56, 78), bounds.Rect);
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("1,2,3")]
		public void ThrowsOnBadValues(string value)
		{
			var converter = new SKConfettiEmitterBoundsTypeConverter();

			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(value));
		}

		[Fact]
		public void NullIsNull()
		{
			var converter = new SKConfettiEmitterBoundsTypeConverter();

			var result = converter.ConvertFromInvariantString(null);

			Assert.Null(result);
		}
	}
}
