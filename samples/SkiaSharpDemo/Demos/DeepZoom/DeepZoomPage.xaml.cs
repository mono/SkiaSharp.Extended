using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
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
            statusLabel.Text = "Loading...";

            using var dziStream = await FileSystem.OpenAppPackageFileAsync("TestGrid/testgrid.dzi");
            using var reader = new StreamReader(dziStream);
            var dziXml = await reader.ReadToEndAsync();

            var tileSource = SKDeepZoomImageSource.Parse(dziXml, "TestGrid/testgrid_files/");
            _controller?.Load(tileSource, new AppPackageFetcher());

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

    // Fetches tiles bundled as MAUI app-package assets.
    private sealed class AppPackageFetcher : ISKDeepZoomTileFetcher
    {
        public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(url);
                return SKBitmap.Decode(stream);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose() { }
    }
}
