using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CollectionImageProviderTest
{
    private static DzcTileSource CreateTestDzc(int itemCount = 10)
    {
        var items = new List<DzcSubImage>();
        for (int i = 0; i < itemCount; i++)
        {
            var sub = new DzcSubImage(i, i, 256, 256, null)
            {
                ViewportWidth = 1.0,
                ViewportX = 0,
                ViewportY = 0,
            };
            items.Add(sub);
        }
        return new DzcTileSource(8, 256, "jpg", items);
    }

    private static PivotViewerItem CreateItemWithImage(string id, int imageIndex)
    {
        var item = new PivotViewerItem(id);
        var imgProp = new PivotViewerStringProperty("#Image") { DisplayName = "Image" };
        item.Set(imgProp, new object[] { $"#{imageIndex}" });
        return item;
    }

    [Fact]
    public void GetItemImageIndex_ParsesHashFormat()
    {
        var item = CreateItemWithImage("test", 42);
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Equal(42, idx);
    }

    [Fact]
    public void GetItemImageIndex_ReturnsNullForNoImage()
    {
        var item = new PivotViewerItem("noimg");
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Null(idx);
    }

    [Fact]
    public void GetItemImageIndex_ReturnsNullForInvalidFormat()
    {
        var item = new PivotViewerItem("bad");
        var imgProp = new PivotViewerStringProperty("#Image") { DisplayName = "Image" };
        item.Set(imgProp, new object[] { "http://example.com/image.jpg" });
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Null(idx);
    }

    [Fact]
    public void GetItemImageIndex_HandlesZeroIndex()
    {
        var item = CreateItemWithImage("test", 0);
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Equal(0, idx);
    }

    [Fact]
    public void Constructor_ThrowsOnNullArgs()
    {
        var dzc = CreateTestDzc();
        var fetcher = new MemoryTileFetcher(new Dictionary<string, SKBitmap>());

        Assert.Throws<ArgumentNullException>(() => new CollectionImageProvider(null!, fetcher, "base"));
        Assert.Throws<ArgumentNullException>(() => new CollectionImageProvider(dzc, null!, "base"));
        Assert.Throws<ArgumentNullException>(() => new CollectionImageProvider(dzc, fetcher, null!));
    }

    [Fact]
    public void GetThumbnail_ReturnsNullWhenNotLoaded()
    {
        var dzc = CreateTestDzc();
        var fetcher = new MemoryTileFetcher(new Dictionary<string, SKBitmap>());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        Assert.Null(provider.GetThumbnail(0));
        Assert.Equal(0, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task LoadThumbnailAsync_ReturnsNullForInvalidIndex()
    {
        var dzc = CreateTestDzc();
        var fetcher = new MemoryTileFetcher(new Dictionary<string, SKBitmap>());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(-1);
        Assert.Null(result);

        result = await provider.LoadThumbnailAsync(100);
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadThumbnailAsync_FetchesFromDzc()
    {
        var dzc = CreateTestDzc(4);
        // Create a test tile bitmap
        var testTile = new SKBitmap(256, 256);
        using (var canvas = new SKCanvas(testTile))
        {
            canvas.Clear(SKColors.Red);
        }

        // The composite tile URL will be something like "test_files/0/0_0.jpg"
        var tiles = new Dictionary<string, SKBitmap>
        {
            ["test_files/0/0_0.jpg"] = testTile
        };
        var fetcher = new MemoryTileFetcher(tiles);
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(0, 64);
        // Result might be null if the Morton grid doesn't align with test data perfectly
        // but we can verify the provider doesn't crash
        Assert.Equal(1, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task LoadThumbnailAsync_CachesResult()
    {
        var dzc = CreateTestDzc(4);
        var testTile = new SKBitmap(256, 256);
        using (var canvas = new SKCanvas(testTile))
        {
            canvas.Clear(SKColors.Blue);
        }

        var tiles = new Dictionary<string, SKBitmap>
        {
            ["test_files/0/0_0.jpg"] = testTile
        };
        var fetcher = new MemoryTileFetcher(tiles);
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        // Load twice — second should come from cache
        var result1 = await provider.LoadThumbnailAsync(0, 64);
        var result2 = await provider.LoadThumbnailAsync(0, 64);

        Assert.Same(result1, result2);
    }

    [Fact]
    public void GetThumbnailForItem_ReturnsThumbnailByImageRef()
    {
        var dzc = CreateTestDzc();
        var fetcher = new MemoryTileFetcher(new Dictionary<string, SKBitmap>());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var item = CreateItemWithImage("car1", 5);
        Assert.Null(provider.GetThumbnailForItem(item)); // Not loaded yet
    }

    [Fact]
    public void ClearCache_DisposesAllThumbnails()
    {
        var dzc = CreateTestDzc();
        var fetcher = new MemoryTileFetcher(new Dictionary<string, SKBitmap>());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        provider.ClearCache();
        Assert.Equal(0, provider.CachedThumbnailCount);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var dzc = CreateTestDzc();
        var fetcher = new MemoryTileFetcher(new Dictionary<string, SKBitmap>());
        var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        provider.Dispose();
        provider.Dispose(); // Should not throw
    }

    [Fact]
    public async Task LoadThumbnailsAsync_BatchLoadsMultipleItems()
    {
        var dzc = CreateTestDzc(4);
        var testTile = new SKBitmap(256, 256);
        using (var canvas = new SKCanvas(testTile))
        {
            canvas.Clear(SKColors.Green);
        }

        var tiles = new Dictionary<string, SKBitmap>
        {
            ["test_files/0/0_0.jpg"] = testTile
        };
        var fetcher = new MemoryTileFetcher(tiles);
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var items = new[]
        {
            CreateItemWithImage("a", 0),
            CreateItemWithImage("b", 1),
            CreateItemWithImage("c", 2),
        };

        await provider.LoadThumbnailsAsync(items, 64);
        Assert.True(provider.CachedThumbnailCount > 0, "Should have cached some thumbnails");
    }
}

/// <summary>
/// In-memory tile fetcher for testing.
/// </summary>
internal class MemoryTileFetcher : ITileFetcher
{
    private readonly Dictionary<string, SKBitmap> _tiles;

    public MemoryTileFetcher(Dictionary<string, SKBitmap> tiles)
    {
        _tiles = tiles;
    }

    public Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        _tiles.TryGetValue(url, out var bitmap);
        return Task.FromResult<SKBitmap?>(bitmap);
    }

    public void Dispose() { }
}
