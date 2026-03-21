using SkiaSharp;
using SkiaSharp.Extended;

namespace SkiaSharpDemo.Demos;

public partial class ImagePyramidPage : ContentPage
{
    private const string DefaultDziUrl =
        "https://raw.githubusercontent.com/mono/SkiaSharp.Extended/refs/heads/main/resources/collections/testgrid/testgrid.dzi";

    // The page manages provider lifecycle; the view receives it as a property
    private ISKImagePyramidTileProvider? _provider;

    public ImagePyramidPage()
    {
        InitializeComponent();
        urlEntry.Text = DefaultDziUrl;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        pyramidView.Initialize();
        LoadFromUrlAsync(DefaultDziUrl);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        pyramidView.Cleanup();
        var old = _provider;
        _provider = null;
        old?.Dispose();
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
        if (string.IsNullOrWhiteSpace(url)) return;

        loadButton.IsEnabled = false;
        statusLabel.Text = "Loading…";

        try
        {
            using var client = new HttpClient();

            bool isDzc  = url.EndsWith(".dzc", StringComparison.OrdinalIgnoreCase);
            bool isDzi  = url.EndsWith(".dzi", StringComparison.OrdinalIgnoreCase);
            bool isIiif = url.EndsWith("/info.json", StringComparison.OrdinalIgnoreCase)
                || url.Contains("/iiif/", StringComparison.OrdinalIgnoreCase)
                || url.Contains("iiif.io", StringComparison.OrdinalIgnoreCase);

            string fetchUrl = url;
            if (isIiif && !url.EndsWith("/info.json", StringComparison.OrdinalIgnoreCase))
                fetchUrl = url.TrimEnd('/') + "/info.json";

            var content = await client.GetStringAsync(fetchUrl);
            string baseDir = fetchUrl[..fetchUrl.LastIndexOf('/')] + "/";
            string stem    = System.IO.Path.GetFileNameWithoutExtension(fetchUrl);

            ReplaceProvider(new SKTieredTileProvider(new SKHttpTileFetcher()));

            if (isDzc)
            {
                var coll = SKImagePyramidDziCollectionSource.Parse(content);
                coll.TilesBaseUri = baseDir;
                // DZC collections use a dedicated Load overload — not ISKImagePyramidSource
                pyramidView.Controller?.Load(coll, _provider!);
                SyncSliderFromViewport();
                statusLabel.Text = $"⚠️ Collection loaded ({coll.ItemCount} images) — DZC rendering not yet supported.";
            }
            else if (isIiif || (!isDzi && content.TrimStart().StartsWith("{")))
            {
                var tileSource = SKImagePyramidIiifSource.Parse(content);
                pyramidView.Source   = tileSource;
                pyramidView.Provider = _provider;
                SyncSliderFromViewport();
                statusLabel.Text = $"{tileSource.ImageWidth}×{tileSource.ImageHeight}  ({tileSource.MaxLevel + 1} levels, IIIF)";
            }
            else
            {
                string tilesBase = $"{baseDir}{stem}_files/";
                var tileSource = SKImagePyramidDziSource.Parse(content, tilesBase);
                pyramidView.Source   = tileSource;
                pyramidView.Provider = _provider;
                SyncSliderFromViewport();
                statusLabel.Text = $"{tileSource.ImageWidth}×{tileSource.ImageHeight}  ({tileSource.MaxLevel + 1} levels)";
            }
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

    private void ReplaceProvider(ISKImagePyramidTileProvider newProvider)
    {
        var old = _provider;
        _provider = newProvider;
        old?.Dispose();
    }

    // --- Zoom ---

    private const double MinZoom = 0.01;
    private const double MaxZoom = 100.0;
    private static double SliderToZoom(double s) => MinZoom * Math.Pow(MaxZoom / MinZoom, s);
    private static double ZoomToSlider(double z) =>
        (Math.Log(z) - Math.Log(MinZoom)) / (Math.Log(MaxZoom) - Math.Log(MinZoom));

    private void SyncSliderFromViewport()
    {
        var zoom = pyramidView.Zoom;
        zoomSlider.Value = Math.Clamp(ZoomToSlider(zoom), 0, 1);
        zoomLabel.Text = $"{zoom:F2}×";
    }

    private void OnZoomChanged(object? sender, ValueChangedEventArgs e)
    {
        var zoom = SliderToZoom(e.NewValue);
        pyramidView.SetZoom(zoom);
        zoomLabel.Text = $"{zoom:F2}×";
    }
}

