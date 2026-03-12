using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
    public DeepZoomPage()
    {
        InitializeComponent();
        LoadTestGrid();
    }

    private async void LoadTestGrid()
    {
        try
        {
            statusLabel.Text = "Loading test DZI...";

            using var dziStream = await FileSystem.OpenAppPackageFileAsync("TestGrid/testgrid.dzi");
            using var reader = new StreamReader(dziStream);
            var dziXml = await reader.ReadToEndAsync();

            var tileBase = "asset://TestGrid/testgrid_files/";
            var tileSource = SKDeepZoomImageSource.Parse(dziXml, tileBase);

            deepZoomView.ShowTileBorders = debugSwitch.IsToggled;
            deepZoomView.ShowDebugStats = statsSwitch.IsToggled;
            deepZoomView.Load(tileSource, new AppPackageTileFetcher());

            statusLabel.Text = $"{tileSource.ImageWidth}×{tileSource.ImageHeight}, " +
                $"{tileSource.MaxLevel + 1} levels";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private void OnReset(object? sender, EventArgs e) => deepZoomView.ResetView();
    private void OnZoomIn(object? sender, EventArgs e) => deepZoomView.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);
    private void OnZoomOut(object? sender, EventArgs e) => deepZoomView.ZoomAboutLogicalPoint(0.5, 0.5, 0.5);
    private void OnDebugToggled(object? sender, ToggledEventArgs e) => deepZoomView.ShowTileBorders = e.Value;
    private void OnStatsToggled(object? sender, ToggledEventArgs e) => deepZoomView.ShowDebugStats = e.Value;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        deepZoomView.Dispose();
    }

    private class AppPackageTileFetcher : ISKDeepZoomTileFetcher
    {
        public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var path = url.Replace("asset://", "");
                using var stream = await FileSystem.OpenAppPackageFileAsync(path);
                return SKBitmap.Decode(stream);
            }
            catch { return null; }
        }

        public void Dispose() { }
    }
}
