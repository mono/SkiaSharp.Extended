using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests
{
	public class SKConfettiSystemTest
	{
		[Fact]
		public void DefaultIsNotRunning()
		{
			var system = new SKConfettiSystem();

			Assert.True(system.IsAnimationEnabled);
			Assert.False(system.IsRunning);
		}

		[Fact]
		public void NotRunningIsReallyNotRunning()
		{
			var system = new SKConfettiSystem();
			system.IsAnimationEnabled = false;

			Assert.False(system.IsAnimationEnabled);
			Assert.False(system.IsRunning);
		}
	}
}
