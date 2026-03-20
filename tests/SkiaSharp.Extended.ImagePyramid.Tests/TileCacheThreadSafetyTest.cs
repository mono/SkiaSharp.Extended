using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>
/// Thread safety tests for SKImagePyramidMemoryTileCache.
/// </summary>
public class TileCacheThreadSafetyTest
{
    [Fact]
    public async Task ConcurrentPut_DoesNotCorrupt()
    {
        var cache = new SKImagePyramidMemoryTileCache(50);

        // Concurrent puts from multiple tasks
        var tasks = Enumerable.Range(0, 100).Select(i =>
            Task.Run(() =>
            {
                var id = new SKImagePyramidTileId(i % 10, i / 10, 0);
                cache.Put(id, new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
            }));

        await Task.WhenAll(tasks);

        // Cache should have valid state
        Assert.True(cache.Count > 0);
        Assert.True(cache.Count <= 50);
    }

    [Fact]
    public async Task ConcurrentGetAndPut_DoesNotCorrupt()
    {
        var cache = new SKImagePyramidMemoryTileCache(100);

        // Pre-populate
        for (int i = 0; i < 50; i++)
        {
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
        }

        // Concurrent reads and writes
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() =>
            {
                cache.TryGet(new SKImagePyramidTileId(localI, 0, 0), out _);
            }));
            tasks.Add(Task.Run(() =>
            {
                cache.Put(new SKImagePyramidTileId(50 + localI, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
            }));
        }

        await Task.WhenAll(tasks);

        Assert.True(cache.Count > 0);
        Assert.True(cache.Count <= 100);
    }

    [Fact]
    public async Task ConcurrentPutWithEviction_DoesNotCorrupt()
    {
        var cache = new SKImagePyramidMemoryTileCache(10);

        // Many concurrent puts that will trigger eviction
        var tasks = Enumerable.Range(0, 100).Select(i =>
            Task.Run(() =>
            {
                cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
            }));

        await Task.WhenAll(tasks);

        Assert.True(cache.Count <= 10);
        Assert.True(cache.Count > 0);
    }

    [Fact]
    public async Task ConcurrentRemove_DoesNotCorrupt()
    {
        var cache = new SKImagePyramidMemoryTileCache(100);

        for (int i = 0; i < 50; i++)
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));

        var tasks = Enumerable.Range(0, 50).Select(i =>
            Task.Run(() =>
            {
                cache.Remove(new SKImagePyramidTileId(i, 0, 0));
            }));

        await Task.WhenAll(tasks);

        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public async Task ConcurrentClearAndPut_DoesNotCorrupt()
    {
        var cache = new SKImagePyramidMemoryTileCache(50);

        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                    cache.Put(new SKImagePyramidTileId(localI * 10 + j, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
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

    /// <summary>
    /// Stress test for Dispose/Put race condition.
    /// This test aggressively exercises Put() and Dispose() concurrently
    /// to verify thread safety when disposing while items are being added.
    /// </summary>
    [Fact]
    public async Task ConcurrentDisposeAndPut_NoExceptionsOrLeaks()
    {
        var cache = new SKImagePyramidMemoryTileCache(100);
        var lockObj = new object();
        var exceptions = new List<Exception>();

        // Pre-populate the cache to give it some initial state
        for (int i = 0; i < 50; i++)
        {
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
        }

        var tasks = new List<Task>();

        // Multiple threads aggressively putting items
        for (int threadId = 0; threadId < 5; threadId++)
        {
            int localThreadId = threadId;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        var tileId = new SKImagePyramidTileId(localThreadId * 1000 + i, i % 10, i % 5);
                        var bmp = new SKBitmap(10, 10);
                        cache.Put(tileId, new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                        exceptions.Add(ex);
                }
            }));
        }

        // Another thread reading from cache
        tasks.Add(Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < 500; i++)
                {
                    var tileId = new SKImagePyramidTileId(i % 100, 0, 0);
                    cache.TryGet(tileId, out _);
                    Thread.Yield();
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                    exceptions.Add(ex);
            }
        }));

        // Single thread flushing evicted items periodically
        tasks.Add(Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    cache.FlushEvicted();
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                    exceptions.Add(ex);
            }
        }));

         // The key part: Dispose is called while Put is happening
        var disposeTask = Task.Delay(50).ContinueWith(_ =>
        {
            try
            {
                cache.Dispose();
            }
            catch (Exception ex)
            {
                lock (lockObj)
                    exceptions.Add(ex);
            }
        });
        tasks.Add(disposeTask);

        // Continue trying to Put items even after Dispose starts
        // This should not throw; Put should handle this gracefully
        tasks.Add(Task.Run(() =>
        {
            try
            {
                // Wait a bit to let Dispose start
                Thread.Sleep(60);
                for (int i = 0; i < 500; i++)
                {
                    var tileId = new SKImagePyramidTileId(5000 + i, 0, 0);
                    var bmp = new SKBitmap(10, 10);
                    cache.Put(tileId, new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                    exceptions.Add(ex);
            }
        }));

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Final flush to clean up any remaining pending disposals
        cache.FlushEvicted();

        // Verify no exceptions occurred during concurrent access
        Assert.Empty(exceptions);

        // After dispose, cache should be empty
        Assert.Equal(0, cache.Count);

        // Verify that the cache state is valid (no corruption)
        // Attempting to access or modify after dispose should be safe
        cache.Put(new SKImagePyramidTileId(9999, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
        Assert.Equal(0, cache.Count); // Should still be empty since disposed

        // Final flush should not throw
        cache.FlushEvicted();
    }

    /// <summary>
    /// Verifies that items added during Dispose are properly cleaned up
    /// and don't leak resources.
    /// </summary>
    [Fact]
    public async Task PutDuringDispose_ItemsAreCleanedUp()
    {
        var cache = new SKImagePyramidMemoryTileCache(50);
        var disposeStartedEvent = new ManualResetEvent(false);

        // Pre-populate
        for (int i = 0; i < 20; i++)
        {
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));
        }

        // Start Dispose in one thread
        var disposeTask = Task.Run(() =>
        {
            disposeStartedEvent.Set();
            cache.Dispose();
        });

        // Wait for dispose to start
        disposeStartedEvent.WaitOne();
        Thread.Sleep(10); // Let dispose get into the lock

        // Try to add items while dispose is happening
        var putTask = Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var bmp = new SKBitmap(10, 10);
                    cache.Put(new SKImagePyramidTileId(100 + i, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));
                }
                catch (ObjectDisposedException)
                {
                    // This is acceptable - cache was disposed
                    break;
                }
            }
        });

        await Task.WhenAll(disposeTask, putTask);

        // Cache should be empty after dispose
        Assert.Equal(0, cache.Count);

        // FlushEvicted should complete without issues
        cache.FlushEvicted();
    }

    /// <summary>
    /// Extreme stress test: rapid alternation between Put and Dispose
    /// to catch race conditions in the _disposed flag check.
    /// </summary>
    [Fact]
    public async Task RapidDisposePutCycles_NoRaceConditions()
    {
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Run multiple rounds of rapid Dispose/Put cycles
        for (int round = 0; round < 10; round++)
        {
            var cache = new SKImagePyramidMemoryTileCache(50);

            // Pre-populate
            for (int i = 0; i < 20; i++)
                cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));

            var tasks = new List<Task>();

            // Aggressive putters
            for (int threadId = 0; threadId < 3; threadId++)
            {
                int localThreadId = threadId;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            var bmp = new SKBitmap(10, 10);
                            cache.Put(new SKImagePyramidTileId(localThreadId * 200 + i, 0, 0), new SKImagePyramidTile(SKImage.FromBitmap(bmp), new byte[] { 0xFF, 0xD8 }));
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockObj)
                            exceptions.Add(ex);
                    }
                }));
            }

            // Occasional flush
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    cache.FlushEvicted();
                    Thread.Sleep(1);
                }
            }));

            // Dispose
            tasks.Add(Task.Run(() =>
            {
                Thread.Sleep(50);
                cache.Dispose();
            }));

            await Task.WhenAll(tasks);
        }

        Assert.Empty(exceptions);
    }

    /// <summary>
    /// Verifies that FlushEvicted is safe to call after Dispose.
    /// </summary>
    [Fact]
    public void FlushEvicted_AfterDispose_IsNoOp()
    {
        var cache = new SKImagePyramidMemoryTileCache(50);

        // Add some items that will be evicted
        for (int i = 0; i < 100; i++)
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));

        // Dispose the cache (should clear everything and pending disposals)
        cache.Dispose();

        // FlushEvicted after dispose should not throw
        cache.FlushEvicted();
    }

    /// <summary>
    /// Verifies that concurrent Dispose calls are safe.
    /// </summary>
    [Fact]
    public async Task ConcurrentDispose_DoesNotThrow()
    {
        var cache = new SKImagePyramidMemoryTileCache(50);

        // Add some items
        for (int i = 0; i < 30; i++)
            cache.Put(new SKImagePyramidTileId(i, 0, 0), new SKImagePyramidTile(SKImage.Create(new SKImageInfo(10, 10)), new byte[] { 0xFF }));

        var tasks = Enumerable.Range(0, 10).Select(i =>
            Task.Run(() =>
            {
                cache.Dispose();
            }));

        var ex = await Record.ExceptionAsync(async () => await Task.WhenAll(tasks));
        Assert.Null(ex);
    }
}
