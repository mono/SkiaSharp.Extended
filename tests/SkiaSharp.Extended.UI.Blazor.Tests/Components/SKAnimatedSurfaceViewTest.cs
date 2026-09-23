using Microsoft.AspNetCore.Components;
using System.Runtime.Versioning;
using SkiaSharp;
using SkiaSharp.Extended.UI.Blazor.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Components;

[SupportedOSPlatform("browser")]
public class SKAnimatedSurfaceViewTest
{
	[Fact]
	public void DefaultSurfaceType_IsCanvasView()
	{
		var view = new TestSKAnimatedSurfaceView();

		Assert.Equal(typeof(SKCanvasView), view.SurfaceType);
		Assert.Equal(typeof(SKCanvasView), view.GetRenderedSurfaceType());
	}

	[Fact]
	public void UnsupportedSurfaceType_IsRejected()
	{
		var view = new TestSKAnimatedSurfaceView();
		view.SetSurfaceType(typeof(ComponentBase));

		var exception = Assert.Throws<ArgumentException>(view.ApplyParameters);

		Assert.Equal("SurfaceType", exception.ParamName);
	}

	[Fact]
	public void DerivedSurfaceType_IsAccepted()
	{
		var view = new TestSKAnimatedSurfaceView();
		view.SetSurfaceType(typeof(CustomCanvasView));

		var exception = Record.Exception(view.ApplyParameters);

		Assert.Null(exception);
		Assert.Equal(typeof(CustomCanvasView), view.GetRenderedSurfaceType());
	}

	[Fact]
	public void FirstAnimatedFrame_UpdatesWithZeroDeltaBeforePainting()
	{
		var calls = new List<string>();
		var view = new TestSKAnimatedSurfaceView();
		view.Configure(
			onUpdate: delta => calls.Add($"update:{delta.TotalMilliseconds}"),
			onPaintSurface: (_, _) => calls.Add("paint"));

		view.ApplyParameters();
		view.Render();

		Assert.Equal(["update:0", "paint"], calls);
	}

	[Fact]
	public void AnimationState_IsForwardedToTheNativeSurface()
	{
		var view = new TestSKAnimatedSurfaceView();

		view.ApplyParameters();
		Assert.Equal(true, view.GetRenderedParameter("EnableRenderLoop"));

		view.SetAnimationEnabled(false);
		view.ApplyParameters();
		Assert.Equal(false, view.GetRenderedParameter("EnableRenderLoop"));
	}

	[Fact]
	public void ResumingAnimation_ResetsDeltaBeforeTheNextPaint()
	{
		var deltas = new List<TimeSpan>();
		var view = new TestSKAnimatedSurfaceView();
		view.Configure(onUpdate: deltas.Add);

		view.ApplyParameters();
		view.Render();

		view.SetAnimationEnabled(false);
		view.ApplyParameters();
		view.Render();

		view.SetAnimationEnabled(true);
		view.ApplyParameters();
		view.Render();

		Assert.Equal([TimeSpan.Zero, TimeSpan.Zero], deltas);
	}

	[Fact]
	public void PausedFrame_PaintsWithoutUpdating()
	{
		var updateCount = 0;
		var paintCount = 0;
		var view = new TestSKAnimatedSurfaceView();
		view.Configure(
			isAnimationEnabled: false,
			onUpdate: _ => updateCount++,
			onPaintSurface: (_, _) => paintCount++);

		view.ApplyParameters();
		view.Render();

		Assert.Equal(0, updateCount);
		Assert.Equal(1, paintCount);
	}

	[Fact]
	public void ClearedAdditionalAttributes_AreForwardedAsEmpty()
	{
		var view = new TestSKAnimatedSurfaceView();
		view.SetAdditionalAttributes(new Dictionary<string, object> { ["class"] = "sample" });
		view.ApplyParameters();

		var initial = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
			view.GetRenderedParameter(nameof(SKCanvasView.AdditionalAttributes)));
		Assert.Equal("sample", initial["class"]);

		view.SetAdditionalAttributes(null);
		view.ApplyParameters();

		var cleared = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(
			view.GetRenderedParameter(nameof(SKCanvasView.AdditionalAttributes)));
		Assert.Empty(cleared);
	}

	[Fact]
	public void InvalidateWithoutCapturedSurface_DoesNothing()
	{
		var view = new TestSKAnimatedSurfaceView();

#pragma warning disable CA1416 // Invalidation is a browser-only API.
		var exception = Record.Exception(view.Invalidate);
#pragma warning restore CA1416

		Assert.Null(exception);
	}

	[Fact]
	public void ReplacingSurfaceType_ResetsTheNextAnimatedDelta()
	{
		var deltas = new List<TimeSpan>();
		var view = new TestSKAnimatedSurfaceView();
		view.Configure(onUpdate: deltas.Add);

		view.ApplyParameters();
		view.Render();

		view.SetSurfaceType(typeof(SKGLView));
		view.ApplyParameters();
		view.Render();

		Assert.Equal([TimeSpan.Zero, TimeSpan.Zero], deltas);
		Assert.Equal(typeof(SKGLView), view.GetRenderedSurfaceType());
	}

#if DEBUG
	[Fact]
	public void DebugBuild_DrawsFpsAfterConsumerPaint()
	{
		var view = new TestSKAnimatedSurfaceView();
		view.Configure(onPaintSurface: (canvas, _) => canvas.Clear(SKColors.White));

		view.ApplyParameters();

		using var surface = SKSurface.Create(new SKImageInfo(200, 50));
		view.Render(surface.Canvas, new SKSize(200, 50));
		using var image = surface.Snapshot();
		using var bitmap = SKBitmap.FromImage(image);

		Assert.Contains(
			Enumerable.Range(0, bitmap.Width * bitmap.Height)
				.Select(index => bitmap.GetPixel(index % bitmap.Width, index / bitmap.Width)),
			color => color != SKColors.White);
	}
#endif

	[Fact]
	public void CallbackFailure_IsNotSuppressed()
	{
		var view = new TestSKAnimatedSurfaceView();
		view.Configure(onUpdate: _ => throw new InvalidOperationException("update failure"));

		view.ApplyParameters();

		var exception = Assert.Throws<InvalidOperationException>(view.Render);

		Assert.Equal("update failure", exception.Message);
	}

	[Fact]
	public void Component_DoesNotOwnAnAsyncFrameLoop()
	{
		Assert.False(typeof(IAsyncDisposable).IsAssignableFrom(typeof(SKAnimatedSurfaceView)));
		Assert.DoesNotContain(
			typeof(SKAnimatedSurfaceView).GetFields(
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic),
			field => typeof(Task).IsAssignableFrom(field.FieldType));
	}

	private sealed class TestSKAnimatedSurfaceView : SKAnimatedSurfaceView
	{
		public void Configure(
			bool? isAnimationEnabled = null,
			Action<TimeSpan>? onUpdate = null,
			Action<SKCanvas, SKSize>? onPaintSurface = null)
		{
			if (isAnimationEnabled is not null)
				IsAnimationEnabled = isAnimationEnabled.Value;

			OnUpdate = onUpdate;
			OnPaintSurface = onPaintSurface;
		}

		public void ApplyParameters() => OnParametersSet();

		public void SetAnimationEnabled(bool isAnimationEnabled) =>
			IsAnimationEnabled = isAnimationEnabled;

		public void SetSurfaceType(Type surfaceType) => SurfaceType = surfaceType;

		public void SetAdditionalAttributes(IReadOnlyDictionary<string, object>? additionalAttributes) =>
			AdditionalAttributes = additionalAttributes;

		public void Render()
		{
			using var surface = SKSurface.Create(new SKImageInfo(1, 1));
			Paint(surface.Canvas, new SKSize(1, 1));
		}

		public void Render(SKCanvas canvas, SKSize size) => Paint(canvas, size);

		private void Paint(SKCanvas canvas, SKSize size) => OnPaintSurfaceCore(canvas, size);

#pragma warning disable BL0006 // Inspect the generated render tree to verify native parameters.
		public Type GetRenderedSurfaceType()
		{
			var builder = new Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder();
			BuildRenderTree(builder);
			var frames = builder.GetFrames();

			for (var i = 0; i < frames.Count; i++)
			{
				var frame = frames.Array[i];
				if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Attribute &&
					frame.AttributeName == nameof(DynamicComponent.Type))
				{
					return (Type)frame.AttributeValue!;
				}
			}

			throw new InvalidOperationException("Dynamic surface type parameter was not rendered.");
		}

		public object? GetRenderedParameter(string name)
		{
			var builder = new Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder();
			BuildRenderTree(builder);
			var frames = builder.GetFrames();

			for (var i = 0; i < frames.Count; i++)
			{
				var frame = frames.Array[i];
				if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Attribute &&
					frame.AttributeName == nameof(DynamicComponent.Parameters) &&
					frame.AttributeValue is IDictionary<string, object> parameters &&
					parameters.TryGetValue(name, out var value))
				{
					return value;
				}
			}

			return null;
		}
#pragma warning restore BL0006
	}

	private sealed class CustomCanvasView : SKCanvasView
	{
	}
}
