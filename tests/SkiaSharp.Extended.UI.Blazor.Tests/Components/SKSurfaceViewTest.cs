using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Runtime.Versioning;
using SkiaSharp;
using SkiaSharp.Extended.UI.Blazor.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Components;

[SupportedOSPlatform("browser")]
public class SKSurfaceViewTest
{
    [Fact]
    public void DefaultSurfaceType_IsCanvasViewWithoutRenderLoopParameter()
    {
        var view = new TestSKSurfaceView();

        Assert.Equal(typeof(SKCanvasView), view.SurfaceType);
        Assert.Null(view.GetRenderedParameter("EnableRenderLoop"));
    }

    [Fact]
    public void UnsupportedSurfaceType_IsRejected()
    {
        var view = new TestSKSurfaceView();
        view.SetSurfaceType(typeof(ComponentBase));

        var exception = Assert.Throws<ArgumentException>(view.ApplyParameters);

        Assert.Equal("SurfaceType", exception.ParamName);
    }

    [Fact]
    public void DerivedSurfaceType_IsAcceptedAndNotifiesTheSurfaceLifecycle()
    {
        var view = new TestSKSurfaceView();
        view.ApplyParameters();
        view.SetSurfaceType(typeof(CustomCanvasView));

        view.ApplyParameters();

        Assert.Equal(1, view.SurfaceChangeCount);
    }

    [Fact]
    public void SurfaceType_SwitchesTheRenderedNativeComponent()
    {
        var view = new TestSKSurfaceView();
        view.ApplyParameters();

        Assert.Equal(typeof(SKCanvasView), view.GetRenderedSurfaceType());

        view.SetSurfaceType(typeof(SKGLView));
        view.ApplyParameters();

        Assert.Equal(typeof(SKGLView), view.GetRenderedSurfaceType());
    }

    [Fact]
    public void PaintCore_InvokesTheConsumerPaintCallback()
    {
        var calls = new List<string>();
        var view = new TestSKSurfaceView();
        view.SetPaintCallback((_, _) => calls.Add("paint"));

        view.Render();

        Assert.Equal(["paint"], calls);
    }

    [Fact]
    public void ClearedAdditionalAttributes_AreForwardedAsEmpty()
    {
        var view = new TestSKSurfaceView();
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
        var view = new TestSKSurfaceView();

#pragma warning disable CA1416 // Invalidation is a browser-only API.
        var exception = Record.Exception(view.Invalidate);
#pragma warning restore CA1416

        Assert.Null(exception);
    }

    private sealed class TestSKSurfaceView : SKSurfaceView
    {
        public int SurfaceChangeCount { get; private set; }

        public void ApplyParameters() => OnParametersSet();

        public void SetSurfaceType(Type surfaceType) => SurfaceType = surfaceType;

        public void SetPaintCallback(Action<SKCanvas, SKSize> onPaintSurface) =>
            OnPaintSurface = onPaintSurface;

        public void SetAdditionalAttributes(IReadOnlyDictionary<string, object>? additionalAttributes) =>
            AdditionalAttributes = additionalAttributes;

#pragma warning disable BL0006 // Inspect the render tree only to verify the selected component type.
        public Type GetRenderedSurfaceType()
        {
            var builder = new RenderTreeBuilder();
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
            var builder = new RenderTreeBuilder();
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

        public void Render()
        {
            using var surface = SKSurface.Create(new SKImageInfo(1, 1));
            OnPaintSurfaceCore(surface.Canvas, new SKSize(1, 1));
        }

        protected override void OnSurfaceChanged() => SurfaceChangeCount++;
    }

    private sealed class CustomCanvasView : SKCanvasView
    {
    }
}
