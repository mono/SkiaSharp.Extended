using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Extended.UI.Blazor.Controls;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Controls;

public class SKAnimatedCanvasViewTest
{
    /// <summary>
    /// Verifies that <see cref="SKAnimatedCanvasView"/> has <c>IsAnimationEnabled</c>
    /// defaulting to <see langword="true"/> and that it exposes the
    /// <c>OnPaintSurface</c> callback parameter.
    /// </summary>
    [Fact]
    public void DefaultParameters_IsAnimationEnabled_IsTrue()
    {
        // Arrange – use a test-double subclass so we never need the real JS canvas.
        var component = new SKAnimatedCanvasViewAccessor();

        // Assert defaults are sensible before rendering.
        Assert.True(component.IsAnimationEnabled);
        Assert.Equal(default, component.OnPaintSurface);
        Assert.Null(component.AdditionalAttributes);
    }

    /// <summary>
    /// Verifies that toggling <c>IsAnimationEnabled</c> from true → false → true
    /// does not throw and leaves the flag in the expected state.
    /// </summary>
    [Fact]
    public void ToggleIsAnimationEnabled_DoesNotThrow()
    {
        var component = new SKAnimatedCanvasViewAccessor();

        // Start enabled (default)
        Assert.True(component.IsAnimationEnabled);

        // Disable
        var ex1 = Record.Exception(() => component.IsAnimationEnabled = false);
        Assert.Null(ex1);
        Assert.False(component.IsAnimationEnabled);

        // Re-enable
        var ex2 = Record.Exception(() => component.IsAnimationEnabled = true);
        Assert.Null(ex2);
        Assert.True(component.IsAnimationEnabled);
    }

    /// <summary>
    /// A minimal subclass that exposes protected members for unit-testing
    /// without rendering into a real Blazor/WASM environment.
    /// </summary>
    private sealed class SKAnimatedCanvasViewAccessor : SKAnimatedCanvasView
    {
        // Expose the loop-start / stop calls as no-ops by overriding UpdateAsync
        // so the base class can be exercised without a running event loop.
        protected override Task UpdateAsync(TimeSpan deltaTime) => Task.CompletedTask;

        // Shadow the auto-property so the setter logic can be exercised.
        public new bool IsAnimationEnabled
        {
            get => base.IsAnimationEnabled;
            set => base.IsAnimationEnabled = value;
        }
    }
}
