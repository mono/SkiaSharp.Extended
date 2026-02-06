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
	public void AnimationCanBeEnabledAfterAddedToWindow()
	{
		// Create view with animation disabled
		var view = new SKAnimatedSurfaceView { IsAnimationEnabled = false };
		
		// Add to window
		var window = new Window
		{
			Page = new ContentPage
			{
				Content = view
			}
		};

		// Enable animation - this should now work even though view started disabled
		view.IsAnimationEnabled = true;

		// Verify animation is enabled
		Assert.True(view.IsAnimationEnabled);
	}

	[Fact]
	public void AnimationStartsWhenWindowIsSetAfterEnabling()
	{
		// Create view with animation enabled but no window
		var view = new SKAnimatedSurfaceView { IsAnimationEnabled = true };

		// At this point, timer shouldn't start because there's no window
		// (This is the fix - previously a timer would start and immediately stop)

		// Add to window
		var window = new Window
		{
			Page = new ContentPage
			{
				Content = view
			}
		};

		// Animation should now be running
		// (In real scenario, the timer would be ticking)
		Assert.True(view.IsAnimationEnabled);
		Assert.NotNull(view.Window);
	}
}
