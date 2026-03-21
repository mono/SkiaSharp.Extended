#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

// ---------------------------------------------------------------------------
// TileFailureTracker
// ---------------------------------------------------------------------------

public class TileFailureTrackerTest
{
    private static SKImagePyramidTileId Id(int level = 0, int col = 0, int row = 0) =>
        new SKImagePyramidTileId(level, col, row);

    [Fact]
    public void NewTracker_HasZeroCount()
    {
        var tracker = new TileFailureTracker();
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public void ShouldSkip_UnknownTile_ReturnsFalse()
    {
        var tracker = new TileFailureTracker();
        Assert.False(tracker.ShouldSkip(Id()));
    }

    [Fact]
    public void RecordFailure_IncreasesCount()
    {
        var tracker = new TileFailureTracker();
        tracker.RecordFailure(Id(0, 0, 0));
        tracker.RecordFailure(Id(0, 0, 1));
        Assert.Equal(2, tracker.Count);
    }

    [Fact]
    public void ShouldSkip_FirstFailure_TrueWithinBackoffWindow()
    {
        // baseDelay=5s means first backoff window is 5s — should skip immediately after failure
        var tracker = new TileFailureTracker(baseDelay: TimeSpan.FromSeconds(5));
        var id = Id();
        tracker.RecordFailure(id);
        Assert.True(tracker.ShouldSkip(id));
    }

    [Fact]
    public void ShouldSkip_AfterBackoffExpires_ReturnsFalse()
    {
        // Very short backoff — should not skip after delay
        var tracker = new TileFailureTracker(baseDelay: TimeSpan.FromMilliseconds(1));
        var id = Id();
        tracker.RecordFailure(id);
        Thread.Sleep(20); // wait for backoff window to expire
        Assert.False(tracker.ShouldSkip(id));
    }

    [Fact]
    public void ShouldSkip_AtMaxRetries_ReturnsTruePermanently()
    {
        var tracker = new TileFailureTracker(
            baseDelay: TimeSpan.FromMilliseconds(1),
            maxRetries: 3);
        var id = Id();

        // Record enough failures to exceed maxRetries, waiting between each to clear backoff
        for (int i = 0; i < 3; i++)
        {
            tracker.RecordFailure(id);
            Thread.Sleep(10);
        }

        // At maxRetries, should skip even after the window expires
        Thread.Sleep(50);
        Assert.True(tracker.ShouldSkip(id));
    }

    [Fact]
    public void Reset_ClearsSingleTile()
    {
        var tracker = new TileFailureTracker();
        var id = Id();
        tracker.RecordFailure(id);
        Assert.True(tracker.ShouldSkip(id));

        tracker.Reset(id);
        Assert.False(tracker.ShouldSkip(id));
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public void ResetAll_ClearsAll()
    {
        var tracker = new TileFailureTracker();
        tracker.RecordFailure(Id(0, 0, 0));
        tracker.RecordFailure(Id(0, 0, 1));
        tracker.RecordFailure(Id(0, 0, 2));
        Assert.Equal(3, tracker.Count);

        tracker.ResetAll();
        Assert.Equal(0, tracker.Count);
        Assert.False(tracker.ShouldSkip(Id(0, 0, 0)));
    }

    [Fact]
    public void ExponentialBackoff_SecondFailureHasLongerDelay()
    {
        // baseDelay=50ms → first backoff=50ms, second=100ms
        var tracker = new TileFailureTracker(baseDelay: TimeSpan.FromMilliseconds(50), maxRetries: 5);
        var id = Id();

        // First failure
        tracker.RecordFailure(id);
        Thread.Sleep(60); // wait past first backoff (50ms)
        Assert.False(tracker.ShouldSkip(id)); // backoff expired

        // Second failure — now backoff is 100ms
        tracker.RecordFailure(id);
        Thread.Sleep(60); // only 60ms, still within 100ms window
        Assert.True(tracker.ShouldSkip(id)); // still in backoff
    }
}

// ---------------------------------------------------------------------------
// SKNullTileCacheStore
// ---------------------------------------------------------------------------

public class SKNullTileCacheStoreTest
{
    [Fact]
    public async Task TryGetAsync_AlwaysReturnsNull()
    {
        using var store = new SKNullTileCacheStore();
        var result = await store.TryGetAsync("anykey");
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_DoesNotThrow()
    {
        using var store = new SKNullTileCacheStore();
        await store.SetAsync("key", new SKImagePyramidTileData(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public async Task RemoveAsync_DoesNotThrow()
    {
        using var store = new SKNullTileCacheStore();
        await store.RemoveAsync("key");
    }

    [Fact]
    public async Task ClearAsync_DoesNotThrow()
    {
        using var store = new SKNullTileCacheStore();
        await store.ClearAsync();
    }
}

// ---------------------------------------------------------------------------
// SKDiskTileCacheStore
// ---------------------------------------------------------------------------

public class SKDiskTileCacheStoreTest : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "sk_disk_test_" + Guid.NewGuid());

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    [Fact]
    public async Task SetAndGet_RoundTrips()
    {
        using var store = new SKDiskTileCacheStore(_tempDir);
        var data = new SKImagePyramidTileData(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });

        await store.SetAsync("key1", data);
        var result = await store.TryGetAsync("key1");

        Assert.NotNull(result);
        Assert.Equal(data.Data, result!.Data);
    }

    [Fact]
    public async Task TryGetAsync_MissingKey_ReturnsNull()
    {
        using var store = new SKDiskTileCacheStore(_tempDir);
        var result = await store.TryGetAsync("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task TryGetAsync_ExpiredEntry_ReturnsNull()
    {
        using var store = new SKDiskTileCacheStore(_tempDir, expiry: TimeSpan.FromMilliseconds(1));
        var data = new SKImagePyramidTileData(new byte[] { 1, 2, 3 });
        await store.SetAsync("expkey", data);
        Thread.Sleep(20); // wait past 1ms expiry
        var result = await store.TryGetAsync("expkey");
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_DeletesEntry()
    {
        using var store = new SKDiskTileCacheStore(_tempDir);
        var data = new SKImagePyramidTileData(new byte[] { 1 });
        await store.SetAsync("rmkey", data);
        Assert.NotNull(await store.TryGetAsync("rmkey"));

        await store.RemoveAsync("rmkey");
        Assert.Null(await store.TryGetAsync("rmkey"));
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        using var store = new SKDiskTileCacheStore(_tempDir);
        await store.SetAsync("k1", new SKImagePyramidTileData(new byte[] { 1 }));
        await store.SetAsync("k2", new SKImagePyramidTileData(new byte[] { 2 }));

        await store.ClearAsync();

        Assert.Null(await store.TryGetAsync("k1"));
        Assert.Null(await store.TryGetAsync("k2"));
    }

    [Fact]
    public async Task MultipleKeys_StoredInBucketedDirs()
    {
        using var store = new SKDiskTileCacheStore(_tempDir);
        // Store several keys with different hashes — all should round-trip
        for (int i = 0; i < 5; i++)
        {
            var data = new SKImagePyramidTileData(new byte[] { (byte)i });
            await store.SetAsync($"https://example.com/tile/{i}", data);
        }
        for (int i = 0; i < 5; i++)
        {
            var result = await store.TryGetAsync($"https://example.com/tile/{i}");
            Assert.NotNull(result);
            Assert.Equal(new byte[] { (byte)i }, result!.Data);
        }
    }
}

// ---------------------------------------------------------------------------
// SKChainedTileCacheStore
// ---------------------------------------------------------------------------

public class SKChainedTileCacheStoreTest
{
    [Fact]
    public async Task TryGetAsync_FirstStoreHit_ReturnsWithoutCheckingSecond()
    {
        var store1 = new RecordingNullStore(returnData: new SKImagePyramidTileData(new byte[] { 1 }));
        var store2 = new RecordingNullStore(returnData: null);
        using var chained = new SKChainedTileCacheStore(store1, store2);

        var result = await chained.TryGetAsync("key");

        Assert.NotNull(result);
        Assert.Equal(1, store1.GetCount);
        Assert.Equal(0, store2.GetCount); // not consulted
    }

    [Fact]
    public async Task TryGetAsync_FirstStoreMiss_ChecksSecond()
    {
        var store1 = new RecordingNullStore(returnData: null);
        var store2 = new RecordingNullStore(returnData: new SKImagePyramidTileData(new byte[] { 2 }));
        using var chained = new SKChainedTileCacheStore(store1, store2);

        var result = await chained.TryGetAsync("key");

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 2 }, result!.Data);
        Assert.Equal(1, store1.GetCount);
        Assert.Equal(1, store2.GetCount);
    }

    [Fact]
    public async Task TryGetAsync_AllMiss_ReturnsNull()
    {
        var store1 = new RecordingNullStore(returnData: null);
        var store2 = new RecordingNullStore(returnData: null);
        using var chained = new SKChainedTileCacheStore(store1, store2);

        var result = await chained.TryGetAsync("key");
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WritesToAllStores()
    {
        var store1 = new RecordingNullStore(returnData: null);
        var store2 = new RecordingNullStore(returnData: null);
        using var chained = new SKChainedTileCacheStore(store1, store2);

        await chained.SetAsync("key", new SKImagePyramidTileData(new byte[] { 42 }));

        Assert.Equal(1, store1.SetCount);
        Assert.Equal(1, store2.SetCount);
    }

    [Fact]
    public void Constructor_EmptyArray_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SKChainedTileCacheStore());
    }

    // Minimal in-memory stub for testing
    private sealed class RecordingNullStore(SKImagePyramidTileData? returnData) : ISKTileCacheStore
    {
        public int GetCount;
        public int SetCount;

        public Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default)
        {
            GetCount++;
            return Task.FromResult(returnData);
        }

        public Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default)
        {
            SetCount++;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }
}

// ---------------------------------------------------------------------------
// SKCompositeTileFetcher
// ---------------------------------------------------------------------------

public class SKCompositeTileFetcherTest
{
    [Fact]
    public async Task FetchAsync_FirstFetcherHit_ReturnsWithoutTryingSecond()
    {
        var data = new SKImagePyramidTileData(new byte[] { 0xFF });
        var f1 = new StubFetcher(data);
        var f2 = new StubFetcher(null);
        using var composite = new SKCompositeTileFetcher(f1, f2);

        var result = await composite.FetchAsync("url");

        Assert.NotNull(result);
        Assert.Equal(1, f1.CallCount);
        Assert.Equal(0, f2.CallCount);
    }

    [Fact]
    public async Task FetchAsync_FirstFetcherMiss_TriesSecond()
    {
        var data = new SKImagePyramidTileData(new byte[] { 0x42 });
        var f1 = new StubFetcher(null);
        var f2 = new StubFetcher(data);
        using var composite = new SKCompositeTileFetcher(f1, f2);

        var result = await composite.FetchAsync("url");

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 0x42 }, result!.Data);
        Assert.Equal(1, f1.CallCount);
        Assert.Equal(1, f2.CallCount);
    }

    [Fact]
    public async Task FetchAsync_AllMiss_ReturnsNull()
    {
        var f1 = new StubFetcher(null);
        var f2 = new StubFetcher(null);
        using var composite = new SKCompositeTileFetcher(f1, f2);

        var result = await composite.FetchAsync("url");
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchAsync_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var f1 = new StubFetcher(null);
        using var composite = new SKCompositeTileFetcher(f1);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => composite.FetchAsync("url", cts.Token));
    }

    private sealed class StubFetcher(SKImagePyramidTileData? returnData) : ISKTileFetcher
    {
        public int CallCount;

        public Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(returnData);
        }

        public void Dispose() { }
    }
}

// ---------------------------------------------------------------------------
// SKTieredTileProvider
// ---------------------------------------------------------------------------

public class SKTieredTileProviderTest
{
    private static SKImagePyramidTileData MakeData() =>
        new SKImagePyramidTileData(CreateMinimalJpegBytes());

    // Create minimal valid image bytes so SKImage.FromEncodedData succeeds
    private static byte[] CreateMinimalJpegBytes()
    {
        using var bmp = new SKBitmap(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    [Fact]
    public async Task GetTileAsync_CacheHit_ReturnsTileWithoutFetching()
    {
        var fetcher = new CountingFetcher(null); // returns null if called
        var cache = new DictionaryCacheStore();
        cache.Store["key-placeholder"] = MakeData(); // pre-populate via SetAsync to use same key

        // Pre-populate with matching key (we need the provider's key computation)
        // Instead, use the provider and verify: after a cache hit the fetcher isn't called
        var data = MakeData();
        var providerForPrepop = new SKTieredTileProvider(new CountingFetcher(data), cache);
        var prepopUrl = "https://example.com/tile/0_0_0.jpg";
        await providerForPrepop.GetTileAsync(prepopUrl); // warms cache, fire-and-forget persist runs sync in test

        // Wait for the fire-and-forget persist task
        await Task.Delay(50);
        providerForPrepop.Dispose();

        // Now use fresh provider with null fetcher — cache should serve it
        var freshFetcher = new CountingFetcher(null);
        using var provider = new SKTieredTileProvider(freshFetcher, cache);
        var tile = await provider.GetTileAsync(prepopUrl);

        Assert.NotNull(tile);
        Assert.Equal(0, freshFetcher.CallCount); // fetcher not called — cache served it
    }

    [Fact]
    public async Task GetTileAsync_CacheMiss_FetchesAndDecodes()
    {
        var data = MakeData();
        var fetcher = new CountingFetcher(data);
        using var provider = new SKTieredTileProvider(fetcher);

        var tile = await provider.GetTileAsync("https://example.com/tile.jpg");

        Assert.NotNull(tile);
        Assert.Equal(1, fetcher.CallCount);
        Assert.NotNull(tile!.Image);
        Assert.NotNull(tile.RawData);
    }

    [Fact]
    public async Task GetTileAsync_FetcherReturnsNull_ReturnsNull()
    {
        var fetcher = new CountingFetcher(null);
        using var provider = new SKTieredTileProvider(fetcher);

        var tile = await provider.GetTileAsync("https://example.com/missing.jpg");

        Assert.Null(tile);
    }

    [Fact]
    public async Task GetTileAsync_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var fetcher = new CountingFetcher(null);
        using var provider = new SKTieredTileProvider(fetcher);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => provider.GetTileAsync("url", cts.Token));
    }

    [Fact]
    public async Task GetTileAsync_PersistsCacheMissToStore()
    {
        var data = MakeData();
        var fetcher = new CountingFetcher(data);
        var cache = new DictionaryCacheStore();
        using var provider = new SKTieredTileProvider(fetcher, cache);

        await provider.GetTileAsync("https://example.com/tile.jpg");
        await Task.Delay(50); // wait for fire-and-forget persist

        Assert.NotEmpty(cache.Store); // something was written to cache
    }

    // ---- Stubs ----

    private sealed class CountingFetcher(SKImagePyramidTileData? returnData) : ISKTileFetcher
    {
        public int CallCount;

        public Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(returnData);
        }

        public void Dispose() { }
    }

    private sealed class DictionaryCacheStore : ISKTileCacheStore
    {
        public readonly Dictionary<string, SKImagePyramidTileData> Store = new();

        public Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default) =>
            Task.FromResult(Store.TryGetValue(key, out var v) ? v : null);

        public Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default)
        {
            Store[key] = data;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            Store.Remove(key);
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken ct = default)
        {
            Store.Clear();
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
