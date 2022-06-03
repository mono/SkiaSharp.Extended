using System;
using Xunit;

namespace SkiaSharp.Extended.UI.Forms.Controls.Tests
{
	public class SKConfettiSystemTest
	{
		[Fact]
		public void DefaultIsNotComplete()
		{
			var system = new SKConfettiSystem();

			Assert.True(system.IsRunning);
			Assert.False(system.IsComplete);
		}

		[Fact]
		public void NotRunningIsNotComplete()
		{
			var system = new SKConfettiSystem();
			system.IsRunning = false;

			Assert.False(system.IsRunning);
			Assert.False(system.IsComplete);
		}
	}
}
