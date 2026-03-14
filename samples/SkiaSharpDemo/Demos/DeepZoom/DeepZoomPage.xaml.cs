using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

public partial class DeepZoomPage : ContentPage
{
    // Branch is read from the embedded build-info.json generated at build time.
    // Priority: explicit MSBuild property → GitHub Actions env vars → AzDO env vars → local git → main
    private static readonly string GitBranch = ReadBuildBranch();

    private static string ReadBuildBranch()
    {
        try
        {
            var asm = typeof(DeepZoomPage).Assembly;
            using var stream = asm.GetManifestResourceStream("build-info.json");
            if (stream == null) return "main";
            using var reader = new System.IO.StreamReader(stream);
            var json = reader.ReadToEnd();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("branch").GetString() ?? "main";
        }
        catch { return "main"; }
    }

    private static string RawBase =>
        $"https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/{GitBranch}/resources/collections/testgrid";

    private static string DziUrl     => $"{RawBase}/testgrid.dzi";
    private static string TilesBaseUrl => $"{RawBase}/testgrid_files/";

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
