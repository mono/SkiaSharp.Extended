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

	[Fact]
	public async Task AnimationStartsWhenWindowBecomesAvailable()
	{
		// This test verifies the fix for the CarouselView issue where the first
		// animation doesn't show because Window is null during initial rendering
		var view = new TestAnimatedSurfaceView();
		var page = new ContentPage { Content = view };

		// Enable animation before Window is assigned
		view.IsAnimationEnabled = true;

		// Initially, no updates should occur because Window is null
		await Task.Delay(50);
		Assert.Equal(0, view.UpdateCount);

		// Assign the Window - this should trigger animation to start
		var window = new Window { Page = page };
		await Task.Delay(50);

		// Now updates should be occurring
		Assert.True(view.UpdateCount > 0, "Animation should start after Window is assigned");
	}

	private class TestAnimatedSurfaceView : SKAnimatedSurfaceView
	{
		public int UpdateCount { get; private set; }

		protected override void Update(TimeSpan deltaTime)
		{
			base.Update(deltaTime);
			if (deltaTime > TimeSpan.Zero)
				UpdateCount++;
		}
	}
}
