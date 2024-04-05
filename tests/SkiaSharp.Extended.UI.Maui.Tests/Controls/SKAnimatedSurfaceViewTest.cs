using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKAnimatedSurfaceViewTest : DispatchingBaseTest
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task DoesNotLeak(bool animate)
	{
		var weak = Setup(animate);

		await Task.Yield();

		GC.Collect();
		GC.WaitForPendingFinalizers();

		Assert.False(weak.IsAlive);

		static WeakReference Setup(bool animate)
		{
			var view = new SKAnimatedSurfaceView();
			var window = new Window
			{
				Page = new ContentPage
				{
					Content = view
				}
			};

			if (animate)
			{
				view.IsAnimationEnabled = true;
			}

			return new WeakReference(view);
		}
	}
}
