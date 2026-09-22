using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Runtime.Versioning;
using SkiaSharp;
using SkiaSharp.Extended.UI.Blazor.Components;
using SkiaSharp.Skottie;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Components.Lottie;

[SupportedOSPlatform("browser")]
public class SKLottieViewTest
{
	private const string MinimalLottieJson =
		"""{"v":"5.7.4","fr":60,"ip":0,"op":60,"w":100,"h":100,"nm":"test","ddd":0,"assets":[],"layers":[{"ddd":0,"ind":1,"ty":4,"nm":"layer","sr":1,"ks":{"o":{"a":0,"k":100,"ix":11},"r":{"a":0,"k":0,"ix":10},"p":{"a":0,"k":[50,50,0],"ix":2},"a":{"a":0,"k":[0,0,0],"ix":1},"s":{"a":0,"k":[100,100,100],"ix":6}},"ao":0,"shapes":[],"ip":0,"op":60,"st":0,"bm":0}]}""";

	[Fact]
	public void DefaultSurfaceType_IsCanvasView()
	{
		var view = new SKLottieView();

		Assert.Equal(typeof(SKCanvasView), view.SurfaceType);
	}

	[Fact]
	public async Task SurfaceRunsOnlyWhileAnimationIsPlayable()
	{
		var view = new SKLottieView();

		Assert.False(view.ShouldRenderContinuously);

		view.SetAnimation(CreateAnimation());

		Assert.True(view.ShouldRenderContinuously);

		view.HandleUpdate(view.Duration);

		Assert.False(view.ShouldRenderContinuously);

		await view.DisposeAsync();
	}

	[Fact]
	public async Task ApplyingSettings_PassesRepeatAndSpeedToThePlayer()
	{
		var view = new SKLottieView();
		SetParameter(view, nameof(SKLottieView.Repeat), SKLottieRepeat.Reverse(2));
		SetParameter(view, nameof(SKLottieView.AnimationSpeed), -1.5);

		view.ApplySettings();

		Assert.Equal(SKLottieRepeat.Reverse(2), view.Player.Repeat);
		Assert.Equal(-1.5, view.Player.AnimationSpeed);

		await view.DisposeAsync();
	}

	[Fact]
	public async Task UpdatingToCompletion_DispatchesCompletionAfterRender()
	{
		var view = new TestLottieView();
		var callbackReceiver = new object();
		var completed = 0;
		SetParameter(
			view,
			nameof(SKLottieView.AnimationCompleted),
			EventCallback.Factory.Create(callbackReceiver, () => completed++));
		view.SetAnimation(CreateAnimation());

		view.HandleUpdate(view.Duration);

		Assert.True(view.IsComplete);
		Assert.Equal(0, completed);

		await view.AfterRenderAsync();

		Assert.Equal(1, completed);

		await view.DisposeAsync();
	}

	[Fact]
	public async Task Restart_ResetsCompletedPlayerAndRequestsThePlayableSurface()
	{
		var view = new SKLottieView();
		view.SetAnimation(CreateAnimation());
		view.HandleUpdate(view.Duration);

		view.Restart();

		Assert.False(view.IsComplete);
		Assert.Equal(TimeSpan.Zero, view.Progress);
		Assert.True(view.ShouldRenderContinuously);

		await view.DisposeAsync();
	}

	[Fact]
	public async Task ParameterSource_IsDeferredUntilAfterRender()
	{
		var view = new TestLottieView();
		SetParameter(view, nameof(SKLottieView.Source), SKLottieImageSource.FromJson(MinimalLottieJson));

		await view.ParametersAsync();

		Assert.False(view.IsLoading);

		await view.DisposeAsync();
	}

	[Fact]
	public async Task ReloadBeforeFirstRender_IsQueued()
	{
		var view = new TestLottieView();
		var reload = view.ReloadAsync(TestContext.Current.CancellationToken);

		Assert.False(reload.IsCompleted);

		await view.DisposeAsync();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reload);
	}

	[Fact]
	public void ClearedAdditionalAttributes_AreForwardedAsEmpty()
	{
		var view = new TestLottieView();
		SetParameter(
			view,
			nameof(SKLottieView.AdditionalAttributes),
			new Dictionary<string, object> { ["class"] = "sample" });

		var initial = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
			view.GetRenderedParameter(nameof(SKAnimatedSurfaceView.AdditionalAttributes)));
		Assert.Equal("sample", initial["class"]);

		SetParameter<IReadOnlyDictionary<string, object>?>(
			view,
			nameof(SKLottieView.AdditionalAttributes),
			null);

		var cleared = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
			view.GetRenderedParameter(nameof(SKAnimatedSurfaceView.AdditionalAttributes)));
		Assert.Empty(cleared);
	}

	private static Animation CreateAnimation() =>
		Animation.Parse(MinimalLottieJson)
		?? throw new InvalidOperationException("Failed to parse test animation.");

	private static void SetParameter<T>(SKLottieView view, string name, T value) =>
		typeof(SKLottieView).GetProperty(name)!.SetValue(view, value);

	private sealed class TestLottieView : SKLottieView
	{
		public Task AfterRenderAsync() => base.OnAfterRenderAsync(firstRender: false);

		public Task ParametersAsync() => base.OnParametersSetAsync();

#pragma warning disable BL0006 // Inspect generated component parameters without a renderer.
		public object? GetRenderedParameter(string name)
		{
			var builder = new RenderTreeBuilder();
			BuildRenderTree(builder);
			var frames = builder.GetFrames();

			for (var i = 0; i < frames.Count; i++)
			{
				var frame = frames.Array[i];
				if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Attribute &&
					frame.AttributeName == name)
				{
					return frame.AttributeValue;
				}
			}

			return null;
		}
#pragma warning restore BL0006
	}
}
