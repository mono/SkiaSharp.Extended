using System;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests
{
	public class SKConfettiSystemTest
	{
		[Fact]
		public void DefaultIsNotComplete()
		{
			var system = new SKConfettiSystem();

			Assert.True(system.IsAnimationEnabled);
			Assert.False(system.IsComplete);
		}

		[Fact]
		public void NotEnabledIsStillNotComplete()
		{
			var system = new SKConfettiSystem();
			system.IsAnimationEnabled = false;

			Assert.False(system.IsAnimationEnabled);
			Assert.False(system.IsComplete);
		}
	}
}
