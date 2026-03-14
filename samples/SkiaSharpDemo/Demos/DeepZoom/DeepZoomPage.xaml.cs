using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
    private const string DefaultDziUrl =
        "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/main/resources/collections/testgrid/testgrid.dzi";

    private SKDeepZoomController? _controller;

    public DeepZoomPage()
    {
        InitializeComponent();
        urlEntry.Text = DefaultDziUrl;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _controller = new SKDeepZoomController();
        _controller.InvalidateRequired += (_, _) => canvas.InvalidateSurface();

        LoadFromUrlAsync(DefaultDziUrl);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _controller?.Dispose();
        _controller = null;
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
                _controller.Load(coll, new SKDeepZoomHttpTileFetcher());
                statusLabel.Text = $"⚠️ Collection loaded ({coll.ItemCount} images) — DZC collection rendering not yet supported. Use a .dzi URL instead.";
            }
            else
            {
                string tilesBase = $"{baseDir}{stem}_files/";
                var tileSource = SKDeepZoomImageSource.Parse(xml, tilesBase);
                _controller.Load(tileSource, new SKDeepZoomHttpTileFetcher());
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
        if (_controller == null) return;
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _controller.Render(e.Surface.Canvas);
    }

    private void OnZoomChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_controller == null) return;
        _controller.SetZoom(e.NewValue);
        zoomLabel.Text = $"{e.NewValue:F2}×";
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
