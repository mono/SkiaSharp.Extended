using Xunit;

namespace SkiaSharp.Extended.Controls.Tests
{
	public class SKConfettiPhysicsTest
	{
		[Fact]
		public void CanCreateInstance()
		{
			var physics = new SKConfettiPhysics(2, 4);

			Assert.Equal(2, physics.Size);
			Assert.Equal(4, physics.Mass);
		}
	}
}
