using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CollectionImageProviderTest
{
    // Creates a DZC with composite mosaic items (Source is null).
    private static DzcTileSource CreateCompositeDzc(int itemCount = 4)
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

    // Creates a DZC with IsPath items (Source is set to a .dzi file).
    private static DzcTileSource CreateIsPathDzc(int itemCount = 4)
    {
        var items = new List<DzcSubImage>();
        for (int i = 0; i < itemCount; i++)
        {
            var sub = new DzcSubImage(i, i, 256, 256, $"img{i}.dzi")
            {
                ViewportWidth = 1.0,
                ViewportX = 0,
                ViewportY = 0,
            };
            items.Add(sub);
        }
        return new DzcTileSource(8, 256, "jpg", items);
    }

    private static SKBitmap CreateTestTile(int width = 256, int height = 256, SKColor? color = null)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color ?? SKColors.Red);
        return bitmap;
    }

    private static PivotViewerItem CreateItemWithImage(string id, int imageIndex)
    {
        var item = new PivotViewerItem(id);
        var imgProp = new PivotViewerStringProperty("#Image") { DisplayName = "Image" };
        item.Set(imgProp, new object[] { $"#{imageIndex}" });
        return item;
    }

    #region GetItemImageIndex

    [Fact]
    public void GetItemImageIndex_ParsesHashFormat()
    {
        var item = CreateItemWithImage("test", 42);
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Equal(42, idx);
    }

    [Fact]
    public void GetItemImageIndex_HandlesZeroIndex()
    {
        var item = CreateItemWithImage("test", 0);
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Equal(0, idx);
    }

    [Fact]
    public void GetItemImageIndex_HandlesLargeIndex()
    {
        var item = CreateItemWithImage("test", 99999);
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Equal(99999, idx);
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
    public void GetItemImageIndex_ReturnsNullForNonNumericHash()
    {
        var item = new PivotViewerItem("bad");
        var imgProp = new PivotViewerStringProperty("#Image") { DisplayName = "Image" };
        item.Set(imgProp, new object[] { "#abc" });
        var idx = CollectionImageProvider.GetItemImageIndex(item);
        Assert.Null(idx);
    }

    #endregion

    #region Constructor

    [Fact]
    public void Constructor_ThrowsOnNullDzc()
    {
        var fetcher = new TrackingTileFetcher();
        Assert.Throws<ArgumentNullException>(() => new CollectionImageProvider(null!, fetcher, "base"));
    }

    [Fact]
    public void Constructor_ThrowsOnNullFetcher()
    {
        var dzc = CreateCompositeDzc();
        Assert.Throws<ArgumentNullException>(() => new CollectionImageProvider(dzc, null!, "base"));
    }

    [Fact]
    public void Constructor_ThrowsOnNullBasePath()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        Assert.Throws<ArgumentNullException>(() => new CollectionImageProvider(dzc, fetcher, null!));
    }

    #endregion

    #region GetThumbnail

    [Fact]
    public void GetThumbnail_ReturnsNullWhenNotLoaded()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        Assert.Null(provider.GetThumbnail(0));
        Assert.Equal(0, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task GetThumbnail_ReturnsBitmapAfterLoad()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(0, 64);
        var result = provider.GetThumbnail(0);

        Assert.NotNull(result);
    }

    #endregion

    #region LoadThumbnailAsync – invalid / edge cases

    [Fact]
    public async Task LoadThumbnailAsync_ReturnsNullForNegativeIndex()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(-1);
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadThumbnailAsync_ReturnsNullForOutOfRangeIndex()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(100);
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadThumbnailAsync_ReturnsNullAfterDispose()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        provider.Dispose();

        var result = await provider.LoadThumbnailAsync(0, 64);
        Assert.Null(result);
    }

    #endregion

    #region LoadThumbnailAsync – IsPath items

    [Fact]
    public async Task LoadThumbnailAsync_IsPath_FetchesCorrectUrl()
    {
        // Item 0: Source="img0.dzi", 256x256
        // filesDir = "img0_files"
        // maxDim=256, maxLevel=ceil(log2(256))=8
        // targetSize=128: 1<<7=128 >= 128 → bestLevel=7
        // URL: "base/img0_files/7/0_0.jpg"
        var dzc = CreateIsPathDzc(4);
        var fetcher = new TrackingTileFetcher();
        var tile = CreateTestTile();
        fetcher.Add("base/img0_files/7/0_0.jpg", tile);
        using var provider = new CollectionImageProvider(dzc, fetcher, "base");

        var result = await provider.LoadThumbnailAsync(0, 128);

        Assert.NotNull(result);
        Assert.Contains("base/img0_files/7/0_0.jpg", fetcher.RequestedUrls);
    }

    [Fact]
    public async Task LoadThumbnailAsync_IsPath_ReturnsFetchedBitmapDirectly()
    {
        var dzc = CreateIsPathDzc(1);
        var fetcher = new TrackingTileFetcher();
        var tile = CreateTestTile(64, 64, SKColors.Blue);
        // Use Add so the same bitmap instance is returned (wildcard now creates fresh bitmaps)
        fetcher.AddWildcard(() => tile);
        using var provider = new CollectionImageProvider(dzc, fetcher, "base");

        var result = await provider.LoadThumbnailAsync(0, 32);

        // IsPath code returns the fetched tile directly (no cropping)
        Assert.Same(tile, result);
    }

    [Fact]
    public async Task LoadThumbnailAsync_IsPath_ReturnsNullWhenFetchFails()
    {
        var dzc = CreateIsPathDzc(1);
        var fetcher = new TrackingTileFetcher(); // No tiles registered → returns null
        using var provider = new CollectionImageProvider(dzc, fetcher, "base");

        var result = await provider.LoadThumbnailAsync(0, 128);

        Assert.Null(result);
        Assert.True(fetcher.RequestedUrls.Count > 0, "Should have attempted a fetch");
    }

    #endregion

    #region LoadThumbnailAsync – composite mosaic items

    [Fact]
    public async Task LoadThumbnailAsync_Composite_FetchesCompositeTileUrl()
    {
        // 4 items, gridSize = 2^ceil(log2(ceil(sqrt(4)))) = 2^1 = 2
        // MortonToGrid(0) = (0,0)
        // At level 0: levelTotalWidth=256, itemPixWidth=256/2=128
        // itemPxX=0, itemPxY=0 → tileCol=0, tileRow=0
        // URL: "test_files/0/0_0.jpg"
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.Add("test_files/0/0_0.jpg", CreateTestTile());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(0, 64);

        Assert.NotNull(result);
        Assert.Contains("test_files/0/0_0.jpg", fetcher.RequestedUrls);
    }

    [Fact]
    public async Task LoadThumbnailAsync_Composite_ExtractsCroppedSubImage()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.Add("test_files/0/0_0.jpg", CreateTestTile());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(0, 64);

        // The composite path crops a sub-region → produces a new bitmap (not the same tile instance)
        Assert.NotNull(result);
        Assert.True(result!.Width > 0 && result.Height > 0, "Cropped bitmap should have non-zero dimensions");
    }

    [Fact]
    public async Task LoadThumbnailAsync_Composite_ReturnsNullWhenFetchFails()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher(); // No tiles → null
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(0, 64);

        Assert.Null(result);
    }

    #endregion

    #region Caching behavior

    [Fact]
    public async Task LoadThumbnailAsync_CachesResult()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.Add("test_files/0/0_0.jpg", CreateTestTile());
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result1 = await provider.LoadThumbnailAsync(0, 64);
        var fetchCountAfterFirst = fetcher.RequestedUrls.Count;

        var result2 = await provider.LoadThumbnailAsync(0, 64);

        Assert.Same(result1, result2);
        Assert.Equal(fetchCountAfterFirst, fetcher.RequestedUrls.Count);
    }

    [Fact]
    public async Task CachedThumbnailCount_TracksLoadedItems()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        Assert.Equal(0, provider.CachedThumbnailCount);

        await provider.LoadThumbnailAsync(0, 64);
        Assert.Equal(1, provider.CachedThumbnailCount);

        await provider.LoadThumbnailAsync(1, 64);
        Assert.Equal(2, provider.CachedThumbnailCount);

        // Re-loading same index should not increase count
        await provider.LoadThumbnailAsync(0, 64);
        Assert.Equal(2, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task ClearCache_RemovesAllCachedThumbnails()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(0, 64);
        await provider.LoadThumbnailAsync(1, 64);
        Assert.True(provider.CachedThumbnailCount >= 2, "Should have at least 2 cached");

        provider.ClearCache();

        Assert.Equal(0, provider.CachedThumbnailCount);
        Assert.Null(provider.GetThumbnail(0));
        Assert.Null(provider.GetThumbnail(1));
    }

    [Fact]
    public async Task ClearCache_AllowsReloading()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(0, 64);
        provider.ClearCache();

        var result = await provider.LoadThumbnailAsync(0, 64);
        Assert.NotNull(result);
        Assert.Equal(1, provider.CachedThumbnailCount);
    }

    #endregion

    #region GetThumbnailForItem

    [Fact]
    public void GetThumbnailForItem_ReturnsNullWhenNotLoaded()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var item = CreateItemWithImage("car1", 0);
        Assert.Null(provider.GetThumbnailForItem(item));
    }

    [Fact]
    public async Task GetThumbnailForItem_ReturnsThumbnailAfterLoad()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(2, 64);

        var item = CreateItemWithImage("car2", 2);
        var result = provider.GetThumbnailForItem(item);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetThumbnailForItem_ReturnsNullForItemWithoutImage()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var item = new PivotViewerItem("noimg");
        Assert.Null(provider.GetThumbnailForItem(item));
    }

    #endregion

    #region LoadThumbnailsAsync batch

    [Fact]
    public async Task LoadThumbnailsAsync_LoadsAllReferencedItems()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var items = new[]
        {
            CreateItemWithImage("a", 0),
            CreateItemWithImage("b", 1),
            CreateItemWithImage("c", 2),
        };

        await provider.LoadThumbnailsAsync(items, 64);

        Assert.True(provider.CachedThumbnailCount >= 3, "Should have cached at least 3 thumbnails");
    }

    [Fact]
    public async Task LoadThumbnailsAsync_SkipsItemsWithoutImageRef()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var items = new[]
        {
            CreateItemWithImage("a", 0),
            new PivotViewerItem("noimg"),
        };

        await provider.LoadThumbnailsAsync(items, 64);

        Assert.Equal(1, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task LoadThumbnailsAsync_SkipsAlreadyCachedItems()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(0, 64);
        var fetchCountBefore = fetcher.RequestedUrls.Count;

        var items = new[] { CreateItemWithImage("a", 0) };
        await provider.LoadThumbnailsAsync(items, 64);

        // No additional fetches should have been made
        Assert.Equal(fetchCountBefore, fetcher.RequestedUrls.Count);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        provider.Dispose();
        provider.Dispose(); // Should not throw
    }

    [Fact]
    public async Task Dispose_ClearsCache()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(0, 64);
        provider.Dispose();

        Assert.Equal(0, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task Dispose_PreventsSubsequentLoads()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        provider.Dispose();

        var result = await provider.LoadThumbnailAsync(0, 64);
        Assert.Null(result);
        Assert.Equal(0, provider.CachedThumbnailCount);
    }

    #endregion

    #region FlushEvictedTiles

    [Fact]
    public void FlushEvictedTiles_DoesNotThrow_WhenEmpty()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var ex = Record.Exception(() => provider.FlushEvictedTiles());
        Assert.Null(ex);
    }

    [Fact]
    public async Task FlushEvictedTiles_DoesNotThrow_AfterLoading()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        await provider.LoadThumbnailAsync(0, 64);
        await provider.LoadThumbnailAsync(1, 64);

        var ex = Record.Exception(() => provider.FlushEvictedTiles());
        Assert.Null(ex);
    }

    [Fact]
    public void FlushEvictedTiles_MultipleCallsAreIdempotent()
    {
        var dzc = CreateCompositeDzc();
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        // Multiple calls should be safe
        for (int i = 0; i < 5; i++)
        {
            var ex = Record.Exception(() => provider.FlushEvictedTiles());
            Assert.Null(ex);
        }
    }

    #endregion

    #region GetThumbnail edge cases

    [Fact]
    public void GetThumbnail_OutOfRangeIndex_ReturnsNull()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        Assert.Null(provider.GetThumbnail(999));
    }

    [Fact]
    public void GetThumbnail_NegativeIndex_ReturnsNull()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        Assert.Null(provider.GetThumbnail(-1));
    }

    [Fact]
    public async Task GetThumbnail_NullResult_NotCached_RetriesOnNextCall()
    {
        // Regression: null results from failed loads must NOT be cached.
        // A second call should re-attempt the fetch, not return a cached null.
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher(); // No tiles registered → returns null
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result1 = await provider.LoadThumbnailAsync(0, 64);
        Assert.Null(result1);
        int fetchCount1 = fetcher.RequestedUrls.Count;
        Assert.True(fetchCount1 > 0, "Should have attempted a fetch");

        var result2 = await provider.LoadThumbnailAsync(0, 64);
        Assert.Null(result2);
        Assert.True(fetcher.RequestedUrls.Count > fetchCount1,
            "Should attempt fetch again for null result (not return cached null)");
        Assert.Equal(0, provider.CachedThumbnailCount);
    }

    #endregion

    #region Non-square aspect ratio

    [Fact]
    public async Task LoadThumbnailAsync_Composite_NonSquareAspectRatio_UsesCorrectYOffset()
    {
        var items = new List<DzcSubImage>();
        for (int i = 0; i < 4; i++)
        {
            var sub = new DzcSubImage(i, i, 512, 256, null)
            {
                ViewportWidth = 1.0,
                ViewportX = 0,
                ViewportY = 0,
            };
            items.Add(sub);
        }
        var dzc = new DzcTileSource(8, 256, "jpg", items);

        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var result = await provider.LoadThumbnailAsync(0, 64);
        Assert.NotNull(result);
        Assert.True(result!.Width >= result.Height,
            $"Non-square aspect 2:1 should produce wider-than-tall thumbnail: {result.Width}x{result.Height}");
    }

    #endregion

    #region Concurrent access

    [Fact]
    public async Task LoadThumbnailAsync_ParallelDifferentIndices_AllSucceed()
    {
        var dzc = CreateCompositeDzc(10);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        var tasks = Enumerable.Range(0, 10)
            .Select(i => provider.LoadThumbnailAsync(i, 64))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.NotNull(r));
        Assert.Equal(10, provider.CachedThumbnailCount);
    }

    [Fact]
    public async Task LoadThumbnailAsync_ParallelSameIndex_UseSemaphoreCorrectly()
    {
        var dzc = CreateCompositeDzc(4);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        // 5 parallel calls for the same index
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => provider.LoadThumbnailAsync(0, 64))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // All should return the same bitmap instance (cached after first load)
        Assert.All(results, r => Assert.NotNull(r));
        Assert.Equal(1, provider.CachedThumbnailCount);
        var first = results[0];
        Assert.All(results, r => Assert.Same(first, r));
    }

    [Fact]
    public async Task LoadThumbnailAsync_ParallelMixed_CacheCountMatchesExpected()
    {
        var dzc = CreateCompositeDzc(8);
        var fetcher = new TrackingTileFetcher();
        fetcher.AddWildcard();
        using var provider = new CollectionImageProvider(dzc, fetcher, "test_files");

        // Mix of different and duplicate indices
        var indices = new[] { 0, 1, 2, 0, 1, 3, 2, 3, 4, 4 };
        var tasks = indices
            .Select(i => provider.LoadThumbnailAsync(i, 64))
            .ToArray();
        await Task.WhenAll(tasks);

        // Only unique indices should be cached: 0,1,2,3,4 = 5
        Assert.Equal(5, provider.CachedThumbnailCount);
    }

    #endregion
}

/// <summary>
/// In-memory tile fetcher for testing that tracks all requested URLs.
/// </summary>
internal class TrackingTileFetcher : ITileFetcher
{
    private readonly Dictionary<string, SKBitmap> _tiles = new();
    private Func<SKBitmap>? _wildcardFactory;

    /// <summary>All URLs that were requested via FetchTileAsync.</summary>
    public List<string> RequestedUrls { get; } = new();

    /// <summary>Registers a tile for a specific URL.</summary>
    public void Add(string url, SKBitmap bitmap) => _tiles[url] = bitmap;

    /// <summary>Registers a factory that creates a fresh bitmap for any URL not explicitly mapped.</summary>
    public void AddWildcard(Func<SKBitmap> factory) => _wildcardFactory = factory;

    /// <summary>Convenience overload: creates a factory returning 256×256 red bitmaps.</summary>
    public void AddWildcard() => AddWildcard(() =>
    {
        var bmp = new SKBitmap(256, 256);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Red);
        return bmp;
    });

    public Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        RequestedUrls.Add(url);

        if (_tiles.TryGetValue(url, out var bitmap))
            return Task.FromResult<SKBitmap?>(bitmap);

        return Task.FromResult<SKBitmap?>(_wildcardFactory?.Invoke());
    }

    public void Dispose() { }
}
