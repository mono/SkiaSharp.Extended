using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Blazor;

namespace SkiaSharpDemo.Blazor.Components.ImagePyramid;

/// <summary>
/// A self-contained image pyramid view component. Owns the canvas, controller, and
/// rendering loop. The host page supplies an <see cref="ISKImagePyramidSource"/> and
/// <see cref="ISKImagePyramidTileProvider"/> and reads back <see cref="Controller"/>
/// for inspector/diagnostic use.
/// </summary>
public partial class ImagePyramidView : IAsyncDisposable
{
    // ---- Injected ----
    [Inject] private IJSRuntime JS2 { get; set; } = null!;

    // ---- Parameters ----

    /// <summary>
    /// The image source to display. Setting this loads it into the controller.
    /// </summary>
    [Parameter] public ISKImagePyramidSource? Source { get; set; }

    /// <summary>
    /// The tile provider. Setting this calls <see cref="SKImagePyramidController.SetProvider"/>.
    /// The caller retains ownership and must dispose it.
    /// </summary>
    [Parameter] public ISKImagePyramidTileProvider? Provider { get; set; }

    /// <summary>
    /// Renderer used for each paint cycle. Defaults to <see cref="SKImagePyramidRenderer"/>.
    /// Provide a custom renderer (e.g. <see cref="DebugBorderRenderer"/>) to add overlays.
    /// </summary>
    [Parameter]
    public ISKImagePyramidRenderer? Renderer { get; set; }

    /// <summary>
    /// Zoom range minimum (default: 0.01).
    /// </summary>
    [Parameter] public double MinZoom { get; set; } = 0.01;

    /// <summary>
    /// Zoom range maximum (default: 100).
    /// </summary>
    [Parameter] public double MaxZoom { get; set; } = 100.0;

    /// <summary>
    /// Debug zoom scale (0.1–1.0). Values below 1.0 shrink the rendered content so
    /// off-screen tiles are visible. Useful for inspector/debugging.
    /// </summary>
    [Parameter] public float DebugZoom { get; set; } = 1.0f;

    /// <summary>Raised when the canvas requests a repaint (tile arrived, etc.).</summary>
    [Parameter] public EventCallback OnInvalidate { get; set; }

    /// <summary>Captures any additional HTML attributes (e.g. style, class) to splat on the root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ---- Exposed state (for inspector/host pages) ----

    /// <summary>
    /// The internal controller. Available after the component is first rendered.
    /// Read-only: use <see cref="Source"/> and <see cref="Provider"/> to configure it.
    /// </summary>
    public SKImagePyramidController? Controller => _controller;

    /// <summary>Current zoom level (linear, not log).</summary>
    public double Zoom => _controller?.Viewport.Zoom ?? 1.0;

    // ---- Public methods ----

    /// <summary>
    /// Resets the viewport to fit the loaded image.
    /// </summary>
    public void ResetView()
    {
        _controller?.ResetView();
        SyncZoom();
        _canvas?.Invalidate();
    }

    /// <summary>
    /// Sets the viewport zoom. Updates the internal zoom tracking.
    /// </summary>
    public void SetZoom(double zoom)
    {
        _controller?.SetZoom(zoom);
        SyncZoom();
    }

    /// <summary>Syncs internal zoom tracking from the controller viewport.</summary>
    public void SyncZoom() => _zoom = _controller?.Viewport.Zoom ?? 1.0;

    // ---- Private state ----

    private SKGLView? _canvas;
    private SKImagePyramidController? _controller;
    private ISKImagePyramidRenderer? _defaultRenderer;
    // Track last-applied values to detect changes in OnParametersSet
    private ISKImagePyramidSource? _appliedSource;
    private ISKImagePyramidTileProvider? _appliedProvider;
    private double _zoom = 1.0;

    private bool _isDragging;
    private double _lastMouseX;
    private double _lastMouseY;
    private int _lastControlWidth;
    private int _lastControlHeight;

    private DotNetObjectReference<ImagePyramidView>? _thisRef;

    // ---- Lifecycle ----

    protected override void OnParametersSet()
    {
        if (_controller == null) return; // Not yet initialised; OnAfterRenderAsync handles first apply

        var providerChanged = !ReferenceEquals(Provider, _appliedProvider);
        var sourceChanged   = !ReferenceEquals(Source,   _appliedSource);

        if (providerChanged)
        {
            _appliedProvider = Provider;
            if (Provider != null)
                _controller.SetProvider(Provider);
        }

        if ((providerChanged || sourceChanged) && Source != null && Provider != null)
        {
            _appliedSource = Source;
            _controller.Load(Source, Provider);
            _canvas?.Invalidate();
        }
        else if (sourceChanged)
        {
            _appliedSource = Source;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _thisRef = DotNetObjectReference.Create(this);
        await JS2.InvokeVoidAsync("deepZoomRegisterResize", _thisRef);

        _defaultRenderer = new SKImagePyramidRenderer();
        _controller = new SKImagePyramidController();
        _controller.InvalidateRequired += OnInvalidateRequired;

        // Apply any source/provider that was set before the controller was ready
        _appliedProvider = Provider;
        _appliedSource   = Source;
        if (Provider != null)
            _controller.SetProvider(Provider);
        if (Source != null && Provider != null)
            _controller.Load(Source, Provider);

        SyncZoom();
        _canvas?.Invalidate();
    }

    // ---- Rendering ----

    private void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        if (_controller == null) return;
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        var w = e.BackendRenderTarget.Width;
        var h = e.BackendRenderTarget.Height;

        if (w != _lastControlWidth || h != _lastControlHeight)
        {
            _lastControlWidth  = w;
            _lastControlHeight = h;
            _controller.SetControlSize(w, h);
        }

        bool hasDebugZoom = DebugZoom < 0.999f;
        if (hasDebugZoom)
        {
            canvas.Save();
            canvas.Translate(w * (1f - DebugZoom) / 2f, h * (1f - DebugZoom) / 2f);
            canvas.Scale(DebugZoom);
        }

        _controller.Update();
        var renderer = Renderer ?? _defaultRenderer;
        if (renderer != null)
        {
            renderer.Canvas = canvas;
            _controller.Render(renderer);
        }

        if (hasDebugZoom)
        {
            using var dashEffect = SKPathEffect.CreateDash(
                new float[] { 12f / DebugZoom, 6f / DebugZoom }, 0);
            using var borderPaint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f / DebugZoom,
                PathEffect = dashEffect,
                IsAntialias = true,
            };
            canvas.DrawRect(0, 0, w, h, borderPaint);
            canvas.Restore();
        }
    }

    private void OnInvalidateRequired(object? sender, EventArgs e)
    {
        InvokeAsync(() =>
        {
            _canvas?.Invalidate();
            if (OnInvalidate.HasDelegate)
                OnInvalidate.InvokeAsync();
        });
    }

    [JSInvokable]
    public void OnWindowResize() => InvokeAsync(() => _canvas?.Invalidate());

    // ---- Pan/zoom input ----

    private void OnMouseDown(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
        _isDragging = true;
        _lastMouseX = e.ClientX;
        _lastMouseY = e.ClientY;
    }

    private void OnMouseMove(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
        if (!_isDragging || _controller == null) return;
        var dx = e.ClientX - _lastMouseX;
        var dy = e.ClientY - _lastMouseY;
        _lastMouseX = e.ClientX;
        _lastMouseY = e.ClientY;
        _controller.Pan(dx, dy);
        InvokeAsync(() => _canvas?.Invalidate());
    }

    private void OnMouseUp(Microsoft.AspNetCore.Components.Web.MouseEventArgs e) => _isDragging = false;

    private void OnTouchStart(Microsoft.AspNetCore.Components.Web.TouchEventArgs e)
    {
        if (e.Touches.Length > 0)
        {
            _isDragging = true;
            _lastMouseX = e.Touches[0].ClientX;
            _lastMouseY = e.Touches[0].ClientY;
        }
    }

    private void OnTouchMove(Microsoft.AspNetCore.Components.Web.TouchEventArgs e)
    {
        if (!_isDragging || _controller == null || e.Touches.Length == 0) return;
        var dx = e.Touches[0].ClientX - _lastMouseX;
        var dy = e.Touches[0].ClientY - _lastMouseY;
        _lastMouseX = e.Touches[0].ClientX;
        _lastMouseY = e.Touches[0].ClientY;
        _controller.Pan(dx, dy);
        InvokeAsync(() => _canvas?.Invalidate());
    }

    private void OnTouchEnd(Microsoft.AspNetCore.Components.Web.TouchEventArgs e) => _isDragging = false;

    // ---- Disposal ----

    public async ValueTask DisposeAsync()
    {
        try { await JS2.InvokeVoidAsync("deepZoomUnregisterResize"); } catch { }
        _thisRef?.Dispose();
        if (_controller != null)
        {
            _controller.InvalidateRequired -= OnInvalidateRequired;
            _controller.Dispose();
        }
        _defaultRenderer?.Dispose();
    }
}
