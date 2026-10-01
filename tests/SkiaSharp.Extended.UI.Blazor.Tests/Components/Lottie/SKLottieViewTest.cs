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
		"""
		{
		  "v": "5.7.4",
		  "fr": 60,
		  "ip": 0,
		  "op": 60,
		  "w": 100,
		  "h": 100,
		  "nm": "test",
		  "ddd": 0,
		  "assets": [],
		  "layers": [
		    {
		      "ddd": 0,
		      "ind": 1,
		      "ty": 4,
		      "nm": "layer",
		      "sr": 1,
		      "ks": {
		        "o": { "a": 0, "k": 100, "ix": 11 },
		        "r": { "a": 0, "k": 0, "ix": 10 },
		        "p": { "a": 0, "k": [50, 50, 0], "ix": 2 },
		        "a": { "a": 0, "k": [0, 0, 0], "ix": 1 },
		        "s": { "a": 0, "k": [100, 100, 100], "ix": 6 }
		      },
		      "ao": 0,
		      "shapes": [],
		      "ip": 0,
		      "op": 60,
		      "st": 0,
		      "bm": 0
		    }
		  ]
		}
		""";

	[Fact]
	public void DefaultSurfaceType_IsCanvasView()
	{
		var view = new SKLottieView();

		Assert.Equal(typeof(SKCanvasView), view.SurfaceType);
	}

	[Fact]
	public void SurfaceRunsOnlyWhileAnimationIsPlayable()
	{
		var view = new TestLottieView();

		Assert.False(view.IsRenderLoopEnabled);

		view.ReplaceAnimation(CreateAnimation());

		Assert.True(view.IsRenderLoopEnabled);

		view.HandleUpdate(view.Duration);

		Assert.False(view.IsRenderLoopEnabled);

		view.Dispose();
	}

	[Fact]
	public void ApplyingSettings_PassesRepeatAndSpeedToThePlayer()
	{
		var view = new TestLottieView();
		SetParameter(view, nameof(SKLottieView.Repeat), SKLottieRepeat.Reverse(2));
		SetParameter(view, nameof(SKLottieView.AnimationSpeed), -1.5);

		view.ApplyParameters();

		Assert.Equal(SKLottieRepeat.Reverse(2), view.Player.Repeat);
		Assert.Equal(-1.5, view.Player.AnimationSpeed);

		view.Dispose();
	}

	[Fact]
	public void ZeroSpeed_DisablesTheRenderLoop()
	{
		var view = new TestLottieView();
		view.ReplaceAnimation(CreateAnimation());
		SetParameter(view, nameof(SKLottieView.AnimationSpeed), 0.0);
		view.ApplyParameters();

		Assert.False(view.IsRenderLoopEnabled);
		view.Dispose();
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
		view.ReplaceAnimation(CreateAnimation());

		view.HandleUpdate(view.Duration);

		Assert.True(view.IsComplete);
		Assert.Equal(0, completed);

		await view.AfterRenderAsync();

		Assert.Equal(1, completed);

		view.Dispose();
	}

	[Fact]
	public async Task ProgressChanged_IsThrottledAndDispatchedAfterRender()
	{
		var view = new TestLottieView();
		var callbackReceiver = new object();
		var progress = new List<TimeSpan>();
		SetParameter(
			view,
			nameof(SKLottieView.ProgressChanged),
			EventCallback.Factory.Create<TimeSpan>(callbackReceiver, progress.Add));
		view.ReplaceAnimation(CreateAnimation());
		await view.AfterRenderAsync();
		progress.Clear();

		view.HandleUpdate(TimeSpan.FromMilliseconds(50));
		await view.AfterRenderAsync();

		Assert.Empty(progress);

		view.HandleUpdate(TimeSpan.FromMilliseconds(50));
		await view.AfterRenderAsync();

		Assert.Equal([TimeSpan.FromMilliseconds(100)], progress);
		view.Dispose();
	}

	[Fact]
	public void Seek_ResumesCompletedPlayerAndRequestsThePlayableSurface()
	{
		var view = new TestLottieView();
		view.ReplaceAnimation(CreateAnimation());
		view.HandleUpdate(view.Duration);

		view.Seek(TimeSpan.Zero);

		Assert.False(view.IsComplete);
		Assert.Equal(TimeSpan.Zero, view.Progress);
		Assert.True(view.IsRenderLoopEnabled);

		view.Dispose();
	}

	[Fact]
	public async Task ParameterSource_IsDeferredUntilAfterRender()
	{
		var view = new TestLottieView();
		SetParameter(view, nameof(SKLottieView.Source), SKLottieImageSource.FromJson(MinimalLottieJson));

		view.ApplyParameters();

		await view.AfterRenderAsync();

		Assert.True(view.HasAnimation);
		view.Dispose();
	}

	[Fact]
	public async Task EquivalentSource_DoesNotReloadAfterParentRender()
	{
		var view = new TestLottieView();
		SetParameter(view, nameof(SKLottieView.Source), SKLottieImageSource.FromJson(MinimalLottieJson));
		view.ApplyParameters();
		await view.AfterRenderAsync();
		var animation = view.Player.Animation;

		SetParameter(view, nameof(SKLottieView.Source), SKLottieImageSource.FromJson(MinimalLottieJson));
		view.ApplyParameters();
		await view.AfterRenderAsync();

		Assert.Same(animation, view.Player.Animation);
		view.Dispose();
	}

	[Fact]
	public async Task SupersededLoad_IsCancelledAndLatestSourceWins()
	{
		var view = new TestLottieView();
		var delayedSource = new DelayedLottieImageSource();
		SetParameter(view, nameof(SKLottieView.Source), delayedSource);
		view.ApplyParameters();

		var firstLoad = view.AfterRenderAsync();
		await delayedSource.WaitForLoadAsync();

		SetParameter(view, nameof(SKLottieView.Source), SKLottieImageSource.FromJson(MinimalLottieJson));
		view.ApplyParameters();
		await view.AfterRenderAsync();
		await firstLoad;

		Assert.True(delayedSource.WasCancelled);
		Assert.True(view.HasAnimation);
		view.Dispose();
	}

	[Fact]
	public async Task Dispose_CancelsPendingSourceLoad()
	{
		var view = new TestLottieView();
		var delayedSource = new DelayedLottieImageSource();
		SetParameter(view, nameof(SKLottieView.Source), delayedSource);
		view.ApplyParameters();

		var load = view.AfterRenderAsync();
		await delayedSource.WaitForLoadAsync();
		view.Dispose();
		await load;

		Assert.True(delayedSource.WasCancelled);
		Assert.False(view.HasAnimation);
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
		public bool IsRenderLoopEnabled =>
			Assert.IsType<bool>(GetRenderedParameter(nameof(SKAnimatedSurfaceView.IsAnimationEnabled)));

		public Task AfterRenderAsync() => base.OnAfterRenderAsync(firstRender: false);

		public void ApplyParameters() => base.OnParametersSet();

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

	private sealed class DelayedLottieImageSource : SKLottieImageSource
	{
		private readonly TaskCompletionSource<SKLottieAnimation> loadStarted =
			new(TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly TaskCompletionSource<SKLottieAnimation> loadCompletion =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		public bool WasCancelled { get; private set; }

		public Task WaitForLoadAsync() => loadStarted.Task;

		protected internal override Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken)
		{
			loadStarted.TrySetResult(null!);
			cancellationToken.Register(() =>
			{
				WasCancelled = true;
				loadCompletion.TrySetCanceled(cancellationToken);
			});
			return loadCompletion.Task;
		}
	}
}
