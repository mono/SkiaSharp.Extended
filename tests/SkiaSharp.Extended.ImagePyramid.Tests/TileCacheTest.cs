using SkiaSharp.Extended;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class TileCacheTest
{
    [Fact]
    public void Empty_Cache_HasZeroCount()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Put_Get_RoundTrips()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var id = new SKImagePyramidTileId(5, 2, 3);
        // Use null bitmap for testing (real bitmaps would be SKBitmap)
        cache.Put(id, null);

        Assert.True(cache.TryGet(id, out var bitmap));
        Assert.Null(bitmap); // we stored null
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Contains_ReturnsTrueForCachedTiles()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var id = new SKImagePyramidTileId(5, 2, 3);
        cache.Put(id, null);

        Assert.True(cache.Contains(id));
        Assert.False(cache.Contains(new SKImagePyramidTileId(5, 2, 4)));
    }

    [Fact]
    public void LRU_EvictsOldestEntry()
    {
        using var cache = new SKImagePyramidMemoryTileCache(2);
        var id0 = new SKImagePyramidTileId(0, 0, 0);
        var id1 = new SKImagePyramidTileId(1, 0, 0);
        var id2 = new SKImagePyramidTileId(2, 0, 0);

        cache.Put(id0, null);
        cache.Put(id1, null);
        Assert.Equal(2, cache.Count);

        // Adding id2 should evict id0 (oldest)
        cache.Put(id2, null);
        Assert.Equal(2, cache.Count);
        Assert.False(cache.Contains(id0));
        Assert.True(cache.Contains(id1));
        Assert.True(cache.Contains(id2));
    }

    [Fact]
    public void LRU_AccessRefreshesEntry()
    {
        using var cache = new SKImagePyramidMemoryTileCache(2);
        var id0 = new SKImagePyramidTileId(0, 0, 0);
        var id1 = new SKImagePyramidTileId(1, 0, 0);
        var id2 = new SKImagePyramidTileId(2, 0, 0);

        cache.Put(id0, null);
        cache.Put(id1, null);

        // Access id0 to make it most recently used
        cache.TryGet(id0, out _);

        // Add id2 — should evict id1 (now the least recently used)
        cache.Put(id2, null);
        Assert.True(cache.Contains(id0));
        Assert.False(cache.Contains(id1));
        Assert.True(cache.Contains(id2));
    }

    [Fact]
    public void Remove_RemovesEntry()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var id = new SKImagePyramidTileId(5, 2, 3);
        cache.Put(id, null);
        Assert.True(cache.Remove(id));
        Assert.Equal(0, cache.Count);
        Assert.False(cache.Contains(id));
    }

    [Fact]
    public void Remove_NonExistent_ReturnsFalse()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        Assert.False(cache.Remove(new SKImagePyramidTileId(0, 0, 0)));
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        for (int i = 0; i < 5; i++)
            cache.Put(new SKImagePyramidTileId(i, 0, 0), null);

        Assert.Equal(5, cache.Count);
        cache.Clear();
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Put_UpdatesExistingEntry()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var id = new SKImagePyramidTileId(5, 2, 3);
        cache.Put(id, null);
        cache.Put(id, null); // update

        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void TileId_Equality()
    {
        var a = new SKImagePyramidTileId(5, 2, 3);
        var b = new SKImagePyramidTileId(5, 2, 3);
        var c = new SKImagePyramidTileId(5, 2, 4);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.NotEqual(a, c);
        Assert.True(a != c);
    }

    [Fact]
    public void TileId_ToString()
    {
        Assert.Equal("(5,2,3)", new SKImagePyramidTileId(5, 2, 3).ToString());
    }

    [Fact]
    public void MaxEntries_Property()
    {
        using var cache = new SKImagePyramidMemoryTileCache(50);
        Assert.Equal(50, cache.MaxEntries);
    }

    [Fact]
    public void Constructor_InvalidMaxEntries_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKImagePyramidMemoryTileCache(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKImagePyramidMemoryTileCache(-1));
    }

    [Fact]
    public void LRU_MaxEntries3_Put4_EvictsOldestAndCountIs3()
    {
        using var cache = new SKImagePyramidMemoryTileCache(3);
        var id0 = new SKImagePyramidTileId(0, 0, 0);
        var id1 = new SKImagePyramidTileId(1, 0, 0);
        var id2 = new SKImagePyramidTileId(2, 0, 0);
        var id3 = new SKImagePyramidTileId(3, 0, 0);

        cache.Put(id0, null);
        cache.Put(id1, null);
        cache.Put(id2, null);
        cache.Put(id3, null);

        Assert.Equal(3, cache.Count);
        Assert.False(cache.Contains(id0));
        Assert.True(cache.Contains(id1));
        Assert.True(cache.Contains(id2));
        Assert.True(cache.Contains(id3));
    }

    [Fact]
    public void Put_UpdateExisting_CountStays1_ReturnsNewBitmap()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var id = new SKImagePyramidTileId(3, 1, 2);

        var bmp1 = new SkiaSharp.SKBitmap(64, 64);
        cache.Put(id, new SKImagePyramidImageTile(SKImage.FromBitmap(bmp1)));
        Assert.Equal(1, cache.Count);

        var bmp2 = new SkiaSharp.SKBitmap(128, 128);
        cache.Put(id, new SKImagePyramidImageTile(SKImage.FromBitmap(bmp2)));
        Assert.Equal(1, cache.Count);

        Assert.True(cache.TryGet(id, out var result));
        var img = ((SKImagePyramidImageTile)result!).Image; Assert.Equal(128, img.Width);

        bmp2.Dispose();
    }

    [Fact]
    public void FlushEvicted_DisposesEvictedBitmaps()
    {
        using var cache = new SKImagePyramidMemoryTileCache(2);
        var bmp0 = new SKBitmap(1, 1);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidImageTile(SKImage.FromBitmap(bmp0)));
        cache.Put(new SKImagePyramidTileId(1, 0, 0), new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(1, 1))));

        // Evict id0 by adding a 3rd item
        cache.Put(new SKImagePyramidTileId(2, 0, 0), new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(1, 1))));
        Assert.Equal(2, cache.Count);
        Assert.False(cache.Contains(new SKImagePyramidTileId(0, 0, 0)));

        // FlushEvicted disposes deferred bitmaps without crashing
        cache.FlushEvicted();
    }

    [Fact]
    public void Remove_DefersDisposal_FlushEvictedCleansThem()
    {
        using var cache = new SKImagePyramidMemoryTileCache(10);
        var bmp = new SKBitmap(1, 1);
        var id = new SKImagePyramidTileId(0, 0, 0);
        cache.Put(id, new SKImagePyramidImageTile(SKImage.FromBitmap(bmp)));

        Assert.True(cache.Remove(id));
        Assert.Equal(0, cache.Count);

        // FlushEvicted cleans up removed bitmap without crashing
        cache.FlushEvicted();
    }

    [Fact]
    public void Clear_DisposesEverything_IncludingPendingEvictions()
    {
        using var cache = new SKImagePyramidMemoryTileCache(2);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(1, 1))));
        cache.Put(new SKImagePyramidTileId(1, 0, 0), new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(1, 1))));
        cache.Put(new SKImagePyramidTileId(2, 0, 0), new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(1, 1)))); // evicts id0

        // Clear disposes all entries and pending evictions
        cache.Clear();
        Assert.Equal(0, cache.Count);

        // FlushEvicted after Clear is a no-op
        cache.FlushEvicted();
    }

    [Fact]
    public void Put_AfterDispose_DoesNotThrow_AndDisposesBitmap()
    {
        var cache = new SKImagePyramidMemoryTileCache(10);
        cache.Dispose();

        var bmp = new SKBitmap(32, 32);
        cache.Put(new SKImagePyramidTileId(0, 0, 0), new SKImagePyramidImageTile(SKImage.FromBitmap(bmp))); // should not throw
        // The bitmap was disposed by Put — verify by checking the cache didn't retain it
        Assert.Equal(0, cache.Count);
        Assert.False(cache.Contains(new SKImagePyramidTileId(0, 0, 0)));
    }

    [Fact]
    public void TryGet_AfterDispose_ReturnsFalse()
    {
        var cache = new SKImagePyramidMemoryTileCache(10);
        var tileId = new SKImagePyramidTileId(3, 1, 2);
        cache.Put(tileId, new SKImagePyramidImageTile(SKImage.Create(new SKImageInfo(32, 32))));

        cache.Dispose();

        Assert.False(cache.TryGet(tileId, out var bitmap));
        Assert.Null(bitmap);
    }
}
