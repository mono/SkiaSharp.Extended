using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;

namespace MauiPivotViewer;

public partial class MainPage : ContentPage
{
    private CollectionImageProvider? _imageProvider;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("[PivotViewer] Loading concept cars collection...");

            // Load the concept cars collection from bundled resources
            using var stream = await FileSystem.OpenAppPackageFileAsync("Collections/conceptcars/conceptcars.cxml");

            // Parse the collection using CxmlCollectionSource
            var source = await CxmlCollectionSource.LoadAsync(stream);

            Console.WriteLine($"[PivotViewer] Loaded {source.Items.Count} items, {source.ItemProperties.Count} properties");

            // Load the DZC for thumbnail extraction
            using var dzcStream = await FileSystem.OpenAppPackageFileAsync("Collections/conceptcars/deepzoom/conceptcars.dzc");
            var dzc = DzcTileSource.Parse(dzcStream);
            Console.WriteLine($"[PivotViewer] DZC loaded: {dzc.Items.Count} sub-images, MaxLevel={dzc.MaxLevel}");

            // Create image provider with app package tile fetcher
            var fetcher = new AppPackageTileFetcher("Collections/conceptcars/");
            _imageProvider = CollectionImageProvider.FromCxmlSource(source, dzc, fetcher);

            // Bind to the PivotViewer
            pivotViewer.ItemsSource = source.Items.ToList();
            pivotViewer.PivotProperties = source.ItemProperties.ToList();

            if (_imageProvider != null)
            {
                pivotViewer.Controller.ImageProvider = _imageProvider;

                // Load thumbnails in the background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _imageProvider.LoadThumbnailsAsync(source.Items, 128);
                        Console.WriteLine($"[PivotViewer] Thumbnails loaded: {_imageProvider.CachedThumbnailCount}");
                        MainThread.BeginInvokeOnMainThread(() => pivotViewer.InvalidateSurface());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PivotViewer] Thumbnail loading error: {ex.Message}");
                    }
                });
            }

            Console.WriteLine("[PivotViewer] Collection bound to view");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PivotViewer] Failed to load collection: {ex}");
            Title = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Tile fetcher that loads from MAUI app package assets.
    /// </summary>
    private class AppPackageTileFetcher : ITileFetcher
    {
        private readonly string _prefix;

        public AppPackageTileFetcher(string prefix = "")
        {
            _prefix = prefix;
        }

        public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var assetPath = _prefix + url;
                using var stream = await FileSystem.OpenAppPackageFileAsync(assetPath);
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
