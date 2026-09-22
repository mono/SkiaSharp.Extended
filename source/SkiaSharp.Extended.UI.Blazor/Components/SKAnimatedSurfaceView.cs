using System.Runtime.Versioning;

using Microsoft.AspNetCore.Components;
using SkiaSharp.Extended.Internal;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Components;

/// <summary>
/// Renders SkiaSharp content through the browser's animation-frame loop.
/// </summary>
/// <remarks>
/// <para>
/// When animation is enabled, <see cref="OnUpdate"/> runs immediately before
/// <see cref="SKSurfaceView.OnPaintSurface"/> on each rendered frame.
/// </para>
/// <para>
/// Calling <see cref="SKSurfaceView.Invalidate"/> repaints once, including while animation is paused.
/// </para>
/// </remarks>
[SupportedOSPlatform("browser")]
public class SKAnimatedSurfaceView : SKSurfaceView
{
    private readonly SKFrameCounter _frameCounter = new();
    private bool _wasAnimationEnabled;

#if DEBUG
    private const float DebugStatusMargin = 12f;
#endif

    /// <summary>
    /// Gets or sets whether browser animation frames update and repaint the view.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool IsAnimationEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback invoked immediately before each animated paint.
    /// </summary>
    [Parameter]
    public Action<TimeSpan>? OnUpdate { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var isResuming = !_wasAnimationEnabled && IsAnimationEnabled;
        base.OnParametersSet();
        SurfaceParameters[nameof(SKCanvasView.EnableRenderLoop)] = IsAnimationEnabled;

        if (isResuming)
            _frameCounter.Reset();

        _wasAnimationEnabled = IsAnimationEnabled;
    }

    /// <inheritdoc />
    protected override void OnSurfaceChanged() => _frameCounter.Reset();

    /// <inheritdoc />
    protected override void OnPaintSurfaceCore(SKCanvas canvas, SKSize size)
    {
        if (IsAnimationEnabled)
            OnUpdate?.Invoke(_frameCounter.NextFrame());

        base.OnPaintSurfaceCore(canvas, size);

#if DEBUG
        WriteDebugStatus(canvas);
#endif
    }

#if DEBUG
    private void WriteDebugStatus(SKCanvas canvas)
    {
        using var font = new SKFont { Size = 12 };
        using var paint = new SKPaint { IsAntialias = true };
        canvas.DrawText(
            $"FPS: {_frameCounter.Rate:0.0}",
            DebugStatusMargin,
            DebugStatusMargin + font.Size,
            SKTextAlign.Left,
            font,
            paint);
    }
#endif
}
