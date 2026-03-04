using SkiaSharp.Extended.UI.Blazor.Controls;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Controls;

public class SKAnimatedSurfaceViewTest
{
    /// <summary>
    /// Verifies that the default parameter values are as documented.
    /// </summary>
    [Fact]
    public void DefaultParameters_AreCorrect()
    {
        var view = new TestSKAnimatedSurfaceView();

        Assert.True(view.IsAnimationEnabled);
        Assert.False(view.UseGL);
        Assert.Null(view.OnUpdate);
        Assert.Null(view.OnPaintSurface);
        Assert.Null(view.AdditionalAttributes);
    }

    /// <summary>
    /// Verifies that setting <c>IsAnimationEnabled</c> to false then true round-trips correctly.
    /// </summary>
    [Fact]
    public void ToggleIsAnimationEnabled_DoesNotThrow()
    {
        var view = new TestSKAnimatedSurfaceView();

        Assert.True(view.IsAnimationEnabled);

        var ex1 = Record.Exception(() => view.IsAnimationEnabled = false);
        Assert.Null(ex1);
        Assert.False(view.IsAnimationEnabled);

        var ex2 = Record.Exception(() => view.IsAnimationEnabled = true);
        Assert.Null(ex2);
        Assert.True(view.IsAnimationEnabled);
    }

    /// <summary>
    /// Verifies that toggling <c>UseGL</c> does not throw.
    /// </summary>
    [Fact]
    public void ToggleUseGL_DoesNotThrow()
    {
        var view = new TestSKAnimatedSurfaceView();

        Assert.False(view.UseGL);

        var ex1 = Record.Exception(() => view.UseGL = true);
        Assert.Null(ex1);
        Assert.True(view.UseGL);

        var ex2 = Record.Exception(() => view.UseGL = false);
        Assert.Null(ex2);
        Assert.False(view.UseGL);
    }

    /// <summary>
    /// Verifies that <c>UpdateAsync</c> calls the <c>OnUpdate</c> callback.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_CallsOnUpdateCallback()
    {
        var capturedDelta = TimeSpan.MinValue;
        var view = new TestSKAnimatedSurfaceView
        {
            OnUpdate = delta => capturedDelta = delta
        };
        var expectedDelta = TimeSpan.FromMilliseconds(16);

        await view.InvokeUpdateAsync(expectedDelta);

        Assert.Equal(expectedDelta, capturedDelta);
    }

    /// <summary>
    /// Verifies that <c>UpdateAsync</c> does not throw when <c>OnUpdate</c> is null.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithNullCallback_DoesNotThrow()
    {
        var view = new TestSKAnimatedSurfaceView { OnUpdate = null };

        var ex = await Record.ExceptionAsync(() => view.InvokeUpdateAsync(TimeSpan.FromMilliseconds(16)));
        Assert.Null(ex);
    }

    /// <summary>
    /// A test subclass that exposes protected members for unit testing without
    /// requiring a real Blazor/WASM environment.
    /// </summary>
    private sealed class TestSKAnimatedSurfaceView : SKAnimatedSurfaceView
    {
        public new bool IsAnimationEnabled
        {
            get => base.IsAnimationEnabled;
            set => base.IsAnimationEnabled = value;
        }

        public new bool UseGL
        {
            get => base.UseGL;
            set => base.UseGL = value;
        }

        public new Action<TimeSpan>? OnUpdate
        {
            get => base.OnUpdate;
            set => base.OnUpdate = value;
        }

        public new Action<SKSurface, SKSize>? OnPaintSurface
        {
            get => base.OnPaintSurface;
            set => base.OnPaintSurface = value;
        }

        public new IDictionary<string, object>? AdditionalAttributes
        {
            get => base.AdditionalAttributes;
            set => base.AdditionalAttributes = value;
        }

        public Task InvokeUpdateAsync(TimeSpan delta) => UpdateAsync(delta);
    }
}
