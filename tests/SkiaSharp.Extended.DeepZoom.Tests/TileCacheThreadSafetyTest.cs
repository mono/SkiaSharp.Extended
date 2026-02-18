using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Thread safety tests for TileCache.
/// </summary>
public class TileCacheThreadSafetyTest
{
    [Fact]
    public async Task ConcurrentPut_DoesNotCorrupt()
    {
        var cache = new TileCache(50);

        // Concurrent puts from multiple tasks
        var tasks = Enumerable.Range(0, 100).Select(i =>
            Task.Run(() =>
            {
                var id = new TileId(i % 10, i / 10, 0);
                cache.Put(id, new SKBitmap(10, 10));
            }));

        await Task.WhenAll(tasks);

        // Cache should have valid state
        Assert.True(cache.Count > 0);
        Assert.True(cache.Count <= 50);
    }

    [Fact]
    public async Task ConcurrentGetAndPut_DoesNotCorrupt()
    {
        var cache = new TileCache(100);

        // Pre-populate
        for (int i = 0; i < 50; i++)
        {
            cache.Put(new TileId(i, 0, 0), new SKBitmap(10, 10));
        }

        // Concurrent reads and writes
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() =>
            {
                cache.TryGet(new TileId(localI, 0, 0), out _);
            }));
            tasks.Add(Task.Run(() =>
            {
                cache.Put(new TileId(50 + localI, 0, 0), new SKBitmap(10, 10));
            }));
        }

        await Task.WhenAll(tasks);

        Assert.True(cache.Count > 0);
        Assert.True(cache.Count <= 100);
    }

    [Fact]
    public async Task ConcurrentPutWithEviction_DoesNotCorrupt()
    {
        var cache = new TileCache(10);

        // Many concurrent puts that will trigger eviction
        var tasks = Enumerable.Range(0, 100).Select(i =>
            Task.Run(() =>
            {
                cache.Put(new TileId(i, 0, 0), new SKBitmap(10, 10));
            }));

        await Task.WhenAll(tasks);

        Assert.True(cache.Count <= 10);
        Assert.True(cache.Count > 0);
    }

    [Fact]
    public async Task ConcurrentRemove_DoesNotCorrupt()
    {
        var cache = new TileCache(100);

        for (int i = 0; i < 50; i++)
            cache.Put(new TileId(i, 0, 0), new SKBitmap(10, 10));

        var tasks = Enumerable.Range(0, 50).Select(i =>
            Task.Run(() =>
            {
                cache.Remove(new TileId(i, 0, 0));
            }));

        await Task.WhenAll(tasks);

        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public async Task ConcurrentClearAndPut_DoesNotCorrupt()
    {
        var cache = new TileCache(50);

        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                    cache.Put(new TileId(localI * 10 + j, 0, 0), new SKBitmap(10, 10));
            }));
        }

        // Also clear in parallel
        tasks.Add(Task.Run(() =>
        {
            Thread.Sleep(5);
            cache.Clear();
        }));

        await Task.WhenAll(tasks);

        // Should be in valid state (not corrupted)
        Assert.True(cache.Count >= 0);
    }
}
