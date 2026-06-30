using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaSharpDemo.Demos;

/// <summary>
/// A self-contained MAUI view that renders an image pyramid.
/// Owns the <see cref="SKCanvasView"/>, <see cref="SKImagePyramidController"/>,
/// and pan gesture handling.
///
/// The host page supplies an <see cref="ISKImagePyramidSource"/> and
/// <see cref="ISKImagePyramidTileProvider"/> via bindable properties, and
/// reads back <see cref="Controller"/> for status/diagnostic use.
///
/// Call <see cref="Initialize"/> when the page appears and <see cref="Cleanup"/>
/// when it disappears to manage the controller lifecycle.
/// </summary>
public class ImagePyramidView : ContentView
{
    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(nameof(Source), typeof(ISKImagePyramidSource), typeof(ImagePyramidView),
            propertyChanged: (b, _, _) => ((ImagePyramidView)b).ApplySourceAndProvider());

    public static readonly BindableProperty ProviderProperty =
        BindableProperty.Create(nameof(Provider), typeof(ISKImagePyramidTileProvider), typeof(ImagePyramidView),
            propertyChanged: (b, _, _) => ((ImagePyramidView)b).ApplySourceAndProvider());

    public ISKImagePyramidSource? Source
    {
        get => (ISKImagePyramidSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public ISKImagePyramidTileProvider? Provider
    {
        get => (ISKImagePyramidTileProvider?)GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    /// <summary>The internal controller. Available after <see cref="Initialize"/> is called.</summary>
    public SKImagePyramidController? Controller => _controller;

    /// <summary>Current zoom level (linear, not log).</summary>
    public double Zoom => _controller?.Viewport.Zoom ?? 1.0;

    private readonly SKCanvasView _canvas;
    private SKImagePyramidController? _controller;
    private SKImagePyramidRenderer? _renderer;
    private EventHandler? _invalidateHandler;
    private ISKImagePyramidSource? _appliedSource;
    private ISKImagePyramidTileProvider? _appliedProvider;
    private double _lastPanX, _lastPanY;

    public ImagePyramidView()
    {
        _canvas = new SKCanvasView();
        _canvas.PaintSurface += OnPaintSurface;

        var pan = new PanGestureRecognizer { TouchPoints = 1 };
        pan.PanUpdated += OnPanUpdated;
        _canvas.GestureRecognizers.Add(pan);

        Content = _canvas;
    }

    /// <summary>
    /// Creates the controller and renderer. Call from the host page's <c>OnAppearing</c>.
    /// </summary>
    public void Initialize()
    {
        Cleanup();

        _controller = new SKImagePyramidController();
        _renderer = new SKImagePyramidRenderer();
        _invalidateHandler = (_, _) => _canvas.InvalidateSurface();
        _controller.InvalidateRequired += _invalidateHandler;

        // Apply any property values already set on the view
        _appliedSource   = null;
        _appliedProvider = null;
        ApplySourceAndProvider();
    }

    /// <summary>
    /// Disposes the controller and renderer. Call from the host page's <c>OnDisappearing</c>.
    /// </summary>
    public void Cleanup()
    {
        if (_controller != null)
        {
            _controller.InvalidateRequired -= _invalidateHandler;
            _invalidateHandler = null;
            _controller.Dispose();
            _controller = null;
        }
        _renderer?.Dispose();
        _renderer = null;
    }

    /// <summary>Resets the viewport to fit the loaded image.</summary>
    public void ResetView()
    {
        _controller?.ResetView();
        _canvas.InvalidateSurface();
    }

    /// <summary>Sets the viewport zoom level.</summary>
    public void SetZoom(double zoom)
    {
        _controller?.SetZoom(zoom);
        _canvas.InvalidateSurface();
    }

    private void ApplySourceAndProvider()
    {
        if (_controller == null) return;
        var source   = Source;
        var provider = Provider;
        if (source == null || provider == null) return;
        if (ReferenceEquals(source, _appliedSource) && ReferenceEquals(provider, _appliedProvider)) return;

        _appliedSource   = source;
        _appliedProvider = provider;
        _controller.Load(source, provider);
        _canvas.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_controller == null || _renderer == null) return;
        e.Surface.Canvas.Clear(SKColors.White);
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _renderer.Canvas = e.Surface.Canvas;
        _controller.Render(_renderer);
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_controller == null) return;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _lastPanX = 0;
                _lastPanY = 0;
                break;
            case GestureStatus.Running:
                var dx = e.TotalX - _lastPanX;
                var dy = e.TotalY - _lastPanY;
                _lastPanX = e.TotalX;
                _lastPanY = e.TotalY;
                _controller.Pan(dx, dy);
                _canvas.InvalidateSurface();
                break;
        }
    }
}
