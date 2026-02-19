using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.UI.Maui.DeepZoom;

namespace MauiDeepZoom;

public partial class MainPage : ContentPage
{
    public MainPage()
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
            var tileSource = DziTileSource.Parse(dziXml, tileBase);

            deepZoomView.ShowTileBorders = debugSwitch.IsToggled;
            deepZoomView.ViewportChanged += (s, e) => UpdateZoomLabel();
            deepZoomView.Load(tileSource, new AppPackageTileFetcher());

            statusLabel.Text = $"{tileSource.ImageWidth}×{tileSource.ImageHeight}, " +
                $"{tileSource.MaxLevel + 1} levels — each level has a different color";
            UpdateZoomLabel();
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Error: {ex.Message}";
            Console.WriteLine($"[DeepZoom] Failed: {ex}");
        }
    }

    private void UpdateZoomLabel()
    {
        var vw = deepZoomView.ViewportWidth;
        var zoom = vw > 0 ? 1.0 / vw : 1.0;
        MainThread.BeginInvokeOnMainThread(() =>
            zoomLabel.Text = $"VP={vw:F3} ({zoom:F1}x)");
    }

    private void OnReset(object? sender, EventArgs e)
    {
        deepZoomView.ResetView();
        UpdateZoomLabel();
    }

    private void OnZoomIn(object? sender, EventArgs e)
    {
        deepZoomView.ZoomAboutLogicalPoint(2.0, 0.5, 0.5);
        UpdateZoomLabel();
    }

    private void OnZoomOut(object? sender, EventArgs e)
    {
        deepZoomView.ZoomAboutLogicalPoint(0.5, 0.5, 0.5);
        UpdateZoomLabel();
    }

    private void OnDebugToggled(object? sender, ToggledEventArgs e)
    {
        deepZoomView.ShowTileBorders = e.Value;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        deepZoomView.Dispose();
    }

    /// <summary>
    /// Loads tiles from MAUI app package raw assets.
    /// URLs are "asset://TestGrid/testgrid_files/{level}/{col}_{row}.png".
    /// </summary>
    private class AppPackageTileFetcher : ITileFetcher
    {
        public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var path = url.Replace("asset://", "");
                using var stream = await FileSystem.OpenAppPackageFileAsync(path);
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
