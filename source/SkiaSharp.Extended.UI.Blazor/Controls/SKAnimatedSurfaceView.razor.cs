using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// Chooses the browser-backed SkiaSharp view used by <see cref="SKAnimatedSurfaceView"/>.
/// </summary>
public enum SKAnimatedSurfaceViewBackend
{
    /// <summary>Uses a software-rendered <see cref="SKCanvasView"/>.</summary>
    Canvas,

    /// <summary>Uses a GPU-rendered <see cref="SKGLView"/>.</summary>
    OpenGL,
}

/// <summary>
/// Renders SkiaSharp content through the browser's animation-frame loop.
/// </summary>
/// <remarks>
/// <para>
/// The component delegates scheduling to <see cref="SKCanvasView"/> or
/// <see cref="SKGLView"/>. When animation is enabled, <see cref="OnUpdate"/> runs
/// immediately before <see cref="OnPaintSurface"/> on each rendered frame.
/// </para>
/// <para>
/// Calling <see cref="Invalidate"/> repaints once, including while animation is paused.
/// </para>
/// </remarks>
public partial class SKAnimatedSurfaceView : ComponentBase
{
    private SKCanvasView? _canvasView;
    private SKGLView? _glView;
    private SKAnimatedSurfaceViewBackend _backend;
    private bool _hasParameters;
    private bool _wasAnimationEnabled;
    private long? _lastTimestamp;

    /// <summary>
    /// Gets or sets the SkiaSharp browser view used for rendering.
    /// Defaults to <see cref="SKAnimatedSurfaceViewBackend.Canvas"/>.
    /// </summary>
    [Parameter]
    public SKAnimatedSurfaceViewBackend Backend { get; set; }

    /// <summary>
    /// Gets or sets whether browser animation frames update and repaint the view.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool IsAnimationEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether rendering uses CSS pixels instead of physical pixels.
    /// </summary>
    [Parameter]
    public bool IgnorePixelScaling { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked immediately before each animated paint.
    /// </summary>
    [Parameter]
    public Action<TimeSpan>? OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked to paint the current frame.
    /// </summary>
    [Parameter]
    public Action<SKCanvas, SKSize>? OnPaintSurface { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes forwarded to the underlying canvas.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (!_hasParameters ||
            _backend != Backend ||
            (!_wasAnimationEnabled && IsAnimationEnabled))
        {
            ResetTiming();
        }

        _backend = Backend;
        _wasAnimationEnabled = IsAnimationEnabled;
        _hasParameters = true;
    }

    /// <summary>
    /// Schedules an on-demand repaint. This works even while animation is paused.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public void Invalidate()
    {
        if (Backend == SKAnimatedSurfaceViewBackend.OpenGL)
            _glView?.Invalidate();
        else
            _canvasView?.Invalidate();
    }

    internal void RenderFrame(SKCanvas canvas, SKSize size)
    {
        if (IsAnimationEnabled)
        {
            var timestamp = Stopwatch.GetTimestamp();
            var delta = _lastTimestamp is long lastTimestamp
                ? TimeSpan.FromSeconds((double)(timestamp - lastTimestamp) / Stopwatch.Frequency)
                : TimeSpan.Zero;

            _lastTimestamp = timestamp;
            OnUpdate?.Invoke(delta);
        }

        OnPaintSurface?.Invoke(canvas, size);
    }

    private void HandleCanvasPaintSurface(SKPaintSurfaceEventArgs e) =>
        RenderFrame(e.Surface.Canvas, new SKSize(e.Info.Width, e.Info.Height));

    private void HandleGlPaintSurface(SKPaintGLSurfaceEventArgs e) =>
        RenderFrame(e.Surface.Canvas, new SKSize(e.Info.Width, e.Info.Height));

    private void ResetTiming() => _lastTimestamp = null;
}
