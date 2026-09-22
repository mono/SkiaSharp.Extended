using SkiaSharp;
using SkiaSharp.Extended.UI.Blazor.Controls;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Controls;

public class SKAnimatedSurfaceViewTest
{
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
    public void ReplacingBackend_ResetsTheNextAnimatedDelta()
    {
        var deltas = new List<TimeSpan>();
        var view = new TestSKAnimatedSurfaceView();
        view.Configure(onUpdate: deltas.Add);

        view.ApplyParameters();
        view.Render();

        view.SetBackend(SKAnimatedSurfaceViewBackend.OpenGL);
        view.ApplyParameters();
        view.Render();

        Assert.Equal([TimeSpan.Zero, TimeSpan.Zero], deltas);
    }

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

        public void SetBackend(SKAnimatedSurfaceViewBackend backend) => Backend = backend;

        public void Render()
        {
            using var surface = SKSurface.Create(new SKImageInfo(1, 1));
            RenderFrame(surface.Canvas, new SKSize(1, 1));
        }
    }
}
