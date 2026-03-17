using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
    private const string DefaultDziUrl =
        "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/main/resources/collections/testgrid/testgrid.dzi";

    private SKDeepZoomController? _controller;
    private SKDeepZoomRenderer? _renderer;

    public DeepZoomPage()
    {
        InitializeComponent();
        urlEntry.Text = DefaultDziUrl;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _controller = new SKDeepZoomController();
        _renderer = new SKDeepZoomRenderer();
        _controller.InvalidateRequired += (_, _) => canvas.InvalidateSurface();

        LoadFromUrlAsync(DefaultDziUrl);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _controller?.Dispose();
        _renderer?.Dispose();
        _controller = null;
        _renderer = null;
    }

    // --- URL loading ---
    private void OnUrlEntryCompleted(object? sender, EventArgs e)
    {
        if (!loadButton.IsEnabled) return;
        LoadFromUrlAsync(urlEntry.Text?.Trim() ?? DefaultDziUrl);
    }

    private void OnLoadButtonClicked(object? sender, EventArgs e) =>
        LoadFromUrlAsync(urlEntry.Text?.Trim() ?? DefaultDziUrl);

    private async void LoadFromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || _controller == null) return;

        loadButton.IsEnabled = false;
        statusLabel.Text = "Loading…";

        try
        {
            using var client = new HttpClient();
            var xml = await client.GetStringAsync(url);

            // Derive tiles base URL from the manifest URL
            bool isDzc    = url.EndsWith(".dzc", StringComparison.OrdinalIgnoreCase);
            string baseDir = url[..url.LastIndexOf('/')] + "/";
            string stem    = System.IO.Path.GetFileNameWithoutExtension(url);

            if (isDzc)
            {
                var coll = SKDeepZoomCollectionSource.Parse(xml);
                coll.TilesBaseUri = baseDir;
                _controller.Load(coll, new SKDeepZoomHttpTileFetcher(new SKDeepZoomBitmapTileDecoder()));
                SyncSliderFromViewport();
                statusLabel.Text = $"⚠️ Collection loaded ({coll.ItemCount} images) — DZC collection rendering not yet supported. Use a .dzi URL instead.";
            }
            else
            {
                string tilesBase = $"{baseDir}{stem}_files/";
                var tileSource = SKDeepZoomImageSource.Parse(xml, tilesBase);
                _controller.Load(tileSource, new SKDeepZoomHttpTileFetcher(new SKDeepZoomBitmapTileDecoder()));
                SyncSliderFromViewport();
                statusLabel.Text = $"{tileSource.ImageWidth}×{tileSource.ImageHeight}  ({tileSource.MaxLevel + 1} levels)";
            }

            canvas.InvalidateSurface();
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Error: {ex.Message}";
        }
        finally
        {
            loadButton.IsEnabled = true;
        }
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

    private const double MinZoom = 0.01;
    private const double MaxZoom = 100.0;
    private static double SliderToZoom(double s) => MinZoom * Math.Pow(MaxZoom / MinZoom, s);
    private static double ZoomToSlider(double z) =>
        (Math.Log(z) - Math.Log(MinZoom)) / (Math.Log(MaxZoom) - Math.Log(MinZoom));

    private void SyncSliderFromViewport()
    {
        if (_controller == null) return;
        var zoom = _controller.Viewport.Zoom;
        zoomSlider.Value = Math.Clamp(ZoomToSlider(zoom), 0, 1);
        zoomLabel.Text = $"{zoom:F2}×";
    }

    private void OnZoomChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_controller == null) return;
        var zoom = SliderToZoom(e.NewValue);
        _controller.SetZoom(zoom);
        zoomLabel.Text = $"{zoom:F2}×";
        canvas.InvalidateSurface();
    }

    private double _lastPanX, _lastPanY;

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
                canvas.InvalidateSurface();
                break;
        }
    }
}
