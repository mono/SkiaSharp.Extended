using System;
using SkiaSharp.Extended.UI.Controls.Converters;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests
{
	public class SKConfettiPhysicsTypeConverterTest
	{
		[Theory]
		[InlineData("12,34")]
		[InlineData("12, 34")]
		[InlineData(" 12, 34 ")]
		public void CanParsePointValues(string value)
		{
			var converter = new SKConfettiPhysicsTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var physics = Assert.IsType<SKConfettiPhysics>(result);

			Assert.Equal(12, physics.Size);
			Assert.Equal(34, physics.Mass);
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("1,2,3")]
		public void ThrowsOnBadValues(string value)
		{
			var converter = new SKConfettiPhysicsTypeConverter();

			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(value));
		}

		[Fact]
		public void NullIsNull()
		{
			var converter = new SKConfettiPhysicsTypeConverter();

			var result = converter.ConvertFromInvariantString(null);

			Assert.Null(result);
		}
	}
}
