using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Components;

/// <summary>
/// Renders SkiaSharp content through a browser canvas or OpenGL surface.
/// </summary>
[SupportedOSPlatform("browser")]
public partial class SKSurfaceView : ComponentBase
{
    private static readonly IReadOnlyDictionary<string, object> EmptyAttributes =
        new Dictionary<string, object>();

    private DynamicComponent? dynamicSurface;
    private object? surface;
    private Action? invalidate;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type surfaceType = typeof(SKCanvasView);

    /// <summary>
    /// Gets or sets the SkiaSharp browser view type used for rendering.
    /// The type must derive from <see cref="SKCanvasView"/> or <see cref="SKGLView"/>.
    /// </summary>
    [Parameter]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type SurfaceType { get; set; } = typeof(SKCanvasView);

    /// <summary>
    /// Gets or sets whether rendering uses CSS pixels instead of physical pixels.
    /// </summary>
    [Parameter]
    public bool IgnorePixelScaling { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked to paint the current surface.
    /// </summary>
    [Parameter]
    public Action<SKCanvas, SKSize>? OnPaintSurface { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes forwarded to the underlying canvas.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private protected IDictionary<string, object> SurfaceParameters { get; private set; } =
        new Dictionary<string, object>();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        ValidateSurfaceType(SurfaceType);
        SurfaceParameters = CreateSurfaceParameters();

        if (surfaceType != SurfaceType)
        {
            dynamicSurface = null;
            surface = null;
            invalidate = null;
            OnSurfaceChanged();
        }

        surfaceType = SurfaceType;
    }

    /// <inheritdoc />
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        var instance = dynamicSurface?.Instance;
        if (!ReferenceEquals(surface, instance) && instance is not null)
        {
            surface = instance;
            invalidate = GetInvalidateCallback(instance);
            OnSurfaceChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Schedules an on-demand repaint.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public void Invalidate() => invalidate?.Invoke();

    /// <summary>
    /// Invoked when the selected native surface type or captured instance changes.
    /// </summary>
    protected virtual void OnSurfaceChanged()
    {
    }

    /// <summary>
    /// Invoked when the native surface needs to be painted.
    /// </summary>
    protected virtual void OnPaintSurfaceCore(SKCanvas canvas, SKSize size) =>
        OnPaintSurface?.Invoke(canvas, size);

    private IDictionary<string, object> CreateSurfaceParameters()
    {
        var parameters = new Dictionary<string, object>
        {
            [nameof(SKCanvasView.AdditionalAttributes)] =
                AdditionalAttributes ?? EmptyAttributes,
            [nameof(SKCanvasView.IgnorePixelScaling)] = IgnorePixelScaling,
            [nameof(SKCanvasView.OnPaintSurface)] = GetPaintCallback(),
        };

        return parameters;
    }

    private object GetPaintCallback() =>
        typeof(SKCanvasView).IsAssignableFrom(SurfaceType)
            ? (Action<SKPaintSurfaceEventArgs>)HandleCanvasPaintSurface
            : (Action<SKPaintGLSurfaceEventArgs>)HandleGlPaintSurface;

    private void HandleCanvasPaintSurface(SKPaintSurfaceEventArgs e) =>
        OnPaintSurfaceCore(e.Surface.Canvas, new SKSize(e.Info.Width, e.Info.Height));

    private void HandleGlPaintSurface(SKPaintGLSurfaceEventArgs e) =>
        OnPaintSurfaceCore(e.Surface.Canvas, new SKSize(e.Info.Width, e.Info.Height));

    private static Action GetInvalidateCallback(object instance)
    {
#pragma warning disable CA1416 // Supported surface components are browser-only.
        return instance switch
        {
            SKCanvasView canvasView => canvasView.Invalidate,
            SKGLView glView => glView.Invalidate,
            _ => throw new InvalidOperationException(
                $"The rendered surface type '{instance.GetType()}' is not supported."),
        };
#pragma warning restore CA1416
    }

    private static void ValidateSurfaceType(Type? value)
    {
        if (value is null ||
            (!typeof(SKCanvasView).IsAssignableFrom(value) &&
             !typeof(SKGLView).IsAssignableFrom(value)))
        {
            throw new ArgumentException(
                $"SurfaceType must derive from {nameof(SKCanvasView)} or {nameof(SKGLView)}.",
                nameof(SurfaceType));
        }
    }
}
