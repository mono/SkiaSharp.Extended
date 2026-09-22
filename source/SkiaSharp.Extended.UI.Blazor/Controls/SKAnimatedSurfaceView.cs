using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// Renders SkiaSharp content through the browser's animation-frame loop.
/// </summary>
/// <remarks>
/// <para>
/// The component delegates scheduling to the selected <see cref="SurfaceType"/>.
/// When animation is enabled, <see cref="OnUpdate"/> runs immediately before
/// <see cref="OnPaintSurface"/> on each rendered frame.
/// </para>
/// <para>
/// Calling <see cref="Invalidate"/> repaints once, including while animation is paused.
/// </para>
/// </remarks>
public class SKAnimatedSurfaceView : ComponentBase
{
    private object? _surface;
    private Action? _invalidate;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type _surfaceType = typeof(SKCanvasView);
    private bool _hasParameters;
    private bool _wasAnimationEnabled;
    private long? _lastTimestamp;

    /// <summary>
    /// Gets or sets the SkiaSharp browser view type used for rendering.
    /// The type must derive from <see cref="SKCanvasView"/> or <see cref="SKGLView"/>.
    /// </summary>
    [Parameter]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type SurfaceType { get; set; } = typeof(SKCanvasView);

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
        ValidateSurfaceType(SurfaceType);

        var surfaceChanged = _surfaceType != SurfaceType;
        if (surfaceChanged)
        {
            _surface = null;
            _invalidate = null;
        }

        if (!_hasParameters ||
            surfaceChanged ||
            (!_wasAnimationEnabled && IsAnimationEnabled))
        {
            ResetTiming();
        }

        _surfaceType = SurfaceType;
        _wasAnimationEnabled = IsAnimationEnabled;
        _hasParameters = true;
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, SurfaceType);
        builder.SetKey(SurfaceType);
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "EnableRenderLoop", IsAnimationEnabled);
        builder.AddAttribute(3, "IgnorePixelScaling", IgnorePixelScaling);
        builder.AddAttribute(4, "OnPaintSurface", GetPaintCallback());
        builder.AddComponentReferenceCapture(5, AttachSurface);
        builder.CloseComponent();
    }

    /// <summary>
    /// Schedules an on-demand repaint. This works even while animation is paused.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public void Invalidate() => _invalidate?.Invoke();

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

    private void AttachSurface(object surface)
    {
        if (!ReferenceEquals(_surface, surface))
            ResetTiming();

        _surface = surface;
        _invalidate = GetInvalidateCallback(surface);
    }

    private object GetPaintCallback() =>
        typeof(SKCanvasView).IsAssignableFrom(SurfaceType)
            ? (Action<SKPaintSurfaceEventArgs>)HandleCanvasPaintSurface
            : (Action<SKPaintGLSurfaceEventArgs>)HandleGlPaintSurface;

    private static Action GetInvalidateCallback(object surface)
    {
#pragma warning disable CA1416 // Supported surface components are browser-only.
        return surface switch
        {
            SKCanvasView canvasView => canvasView.Invalidate,
            SKGLView glView => glView.Invalidate,
            _ => throw new InvalidOperationException(
                $"The rendered surface type '{surface.GetType()}' is not supported."),
        };
#pragma warning restore CA1416
    }

    private static void ValidateSurfaceType(Type? surfaceType)
    {
        if (surfaceType is null ||
            (!typeof(SKCanvasView).IsAssignableFrom(surfaceType) &&
             !typeof(SKGLView).IsAssignableFrom(surfaceType)))
        {
            throw new ArgumentException(
                $"SurfaceType must derive from {nameof(SKCanvasView)} or {nameof(SKGLView)}.",
                nameof(SurfaceType));
        }
    }

    private void ResetTiming() => _lastTimestamp = null;
}
