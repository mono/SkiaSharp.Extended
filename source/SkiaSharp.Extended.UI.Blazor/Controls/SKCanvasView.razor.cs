using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A SkiaSharp canvas view for Blazor that wraps <see cref="SkiaSharp.Views.Blazor.SKCanvasView"/>.
/// </summary>
public partial class SKCanvasView : ComponentBase, IDisposable
{
    private SkiaSharp.Views.Blazor.SKCanvasView? _skCanvasView;

    /// <summary>Gets or sets the paint surface callback.</summary>
    [Parameter]
    public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

    /// <summary>Gets or sets whether continuous rendering is enabled.</summary>
    [Parameter]
    public bool EnableRenderLoop { get; set; }

    /// <summary>Gets or sets whether pixel scaling is ignored.</summary>
    [Parameter]
    public bool IgnorePixelScaling { get; set; }

    /// <summary>Gets or sets the CSS style string.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Gets or sets the CSS class string.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Gets or sets additional HTML attributes.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Gets the current DPI of the display.</summary>
    public float Dpi => _skCanvasView is not null ? (float)_skCanvasView.Dpi : 1f;

    /// <summary>Requests a redraw of the canvas.</summary>
    public void Invalidate() => _skCanvasView?.Invalidate();

    private void OnPaintSurfaceInternal(SKPaintSurfaceEventArgs e)
    {
        OnPaintSurface?.Invoke(e);
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        _skCanvasView?.Dispose();
    }
}
