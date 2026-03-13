using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
    private const string DziUrl =
        "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/main/resources/collections/testgrid/testgrid.dzi";
    private const string TilesBaseUrl =
        "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/main/resources/collections/testgrid/testgrid_files/";

    private SKDeepZoomController? _controller;

    public DeepZoomPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _controller = new SKDeepZoomController();
        _controller.InvalidateRequired += (_, _) => canvas.InvalidateSurface();

        LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _controller?.Dispose();
        _controller = null;
    }

    private async void LoadAsync()
    {
        try
        {
            statusLabel.Text = "Loading…";

            using var client = new HttpClient();
            var dziXml = await client.GetStringAsync(DziUrl);
            var tileSource = SKDeepZoomImageSource.Parse(dziXml, TilesBaseUrl);
            _controller?.Load(tileSource, new SKDeepZoomHttpTileFetcher());

            statusLabel.Text = $"{tileSource.ImageWidth}×{tileSource.ImageHeight}  ({tileSource.MaxLevel + 1} levels)";
            canvas.InvalidateSurface();
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Error: {ex.Message}";
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
