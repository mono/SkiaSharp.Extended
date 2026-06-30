using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>
/// Tests for the nesting tile-provider pipeline: origins (<see cref="SKFileTileProvider"/>,
/// <see cref="SKHttpTileProvider"/>) and decorators (<see cref="SKCompositeTileProvider"/>,
/// <see cref="SKCachedTileProvider"/>, <see cref="SKDiskCacheTileProvider"/>). Providers deal
/// only in encoded bytes; decoding happens once in the controller.
/// </summary>
public class TileProvidersTest : IDisposable
{
    private readonly List<string> _tempDirs = new();

    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // Best effort cleanup.
            }
        }
    }

    private string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ip-providers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (condition())
                return true;
            await Task.Delay(15);
        }
        return condition();
    }

    // ---------------------------------------------------------------------
    // SKFileTileProvider
    // ---------------------------------------------------------------------

    [Fact]
    public async Task File_ExistingPath_ReturnsBytes()
    {
        var dir = NewTempDir();
        var file = Path.Combine(dir, "tile.jpg");
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(file, bytes);

        using var provider = new SKFileTileProvider();
        var result = await provider.GetTileAsync(file);

        Assert.NotNull(result);
        Assert.Equal(bytes, result!.Data);
    }

    [Fact]
    public async Task File_MissingPath_ReturnsNull()
    {
        var dir = NewTempDir();
        using var provider = new SKFileTileProvider();

        var result = await provider.GetTileAsync(Path.Combine(dir, "does-not-exist.jpg"));

        Assert.Null(result);
    }

    [Fact]
    public async Task File_FileUri_ReturnsBytes()
    {
        var dir = NewTempDir();
        var file = Path.Combine(dir, "tile.png");
        var bytes = new byte[] { 9, 8, 7 };
        File.WriteAllBytes(file, bytes);

        using var provider = new SKFileTileProvider();
        var result = await provider.GetTileAsync(new Uri(file).AbsoluteUri);

        Assert.NotNull(result);
        Assert.Equal(bytes, result!.Data);
    }

    [Fact]
    public async Task File_MissingFileUri_ReturnsNull()
    {
        var dir = NewTempDir();
        using var provider = new SKFileTileProvider();

        var uri = new Uri(Path.Combine(dir, "nope.png")).AbsoluteUri;
        var result = await provider.GetTileAsync(uri);

        Assert.Null(result);
    }

    [Fact]
    public async Task File_CancelledToken_Throws()
    {
        var dir = NewTempDir();
        var file = Path.Combine(dir, "tile.jpg");
        File.WriteAllBytes(file, new byte[] { 1 });

        using var provider = new SKFileTileProvider();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetTileAsync(file, cts.Token));
    }

    // ---------------------------------------------------------------------
    // SKHttpTileProvider
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Http_Success_ReturnsBytes()
    {
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
        using var handler = new MockHttpMessageHandler(HttpStatusCode.OK, bytes);
        using var client = new HttpClient(handler);
        using var provider = new SKHttpTileProvider(client);

        var result = await provider.GetTileAsync("http://example.com/0/0_0.jpg");

        Assert.NotNull(result);
        Assert.Equal(bytes, result!.Data);
    }

    [Fact]
    public async Task Http_NotFound_ReturnsNull()
    {
        using var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound);
        using var client = new HttpClient(handler);
        using var provider = new SKHttpTileProvider(client);

        var result = await provider.GetTileAsync("http://example.com/missing.jpg");

        Assert.Null(result);
    }

    [Fact]
    public async Task Http_ServerError_ReturnsNull()
    {
        using var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError);
        using var client = new HttpClient(handler);
        using var provider = new SKHttpTileProvider(client);

        var result = await provider.GetTileAsync("http://example.com/boom.jpg");

        Assert.Null(result);
    }

    [Fact]
    public async Task Http_RequestException_ReturnsNull()
    {
        using var handler = new ThrowingHttpMessageHandler(new HttpRequestException("network down"));
        using var client = new HttpClient(handler);
        using var provider = new SKHttpTileProvider(client);

        var result = await provider.GetTileAsync("http://example.com/0/0_0.jpg");

        Assert.Null(result);
    }

    [Fact]
    public async Task Http_TimeoutException_ReturnsNull()
    {
        // A TaskCanceledException with no caller cancellation models an HttpClient timeout.
        using var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("timeout"));
        using var client = new HttpClient(handler);
        using var provider = new SKHttpTileProvider(client);

        var result = await provider.GetTileAsync("http://example.com/0/0_0.jpg");

        Assert.Null(result);
    }

    [Fact]
    public async Task Http_CancelledToken_Throws()
    {
        using var handler = new MockHttpMessageHandler(HttpStatusCode.OK, new byte[] { 1 });
        using var client = new HttpClient(handler);
        using var provider = new SKHttpTileProvider(client);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetTileAsync("http://example.com/0/0_0.jpg", cts.Token));
    }

    [Fact]
    public void Http_ExternalClient_NotDisposed()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, new byte[] { 1 });
        var client = new HttpClient(handler);
        var provider = new SKHttpTileProvider(client);

        provider.Dispose();

        Assert.False(handler.Disposed);
        client.Dispose();
    }

    [Fact]
    public void Http_NullClient_DisposeDoesNotThrow()
    {
        var provider = new SKHttpTileProvider();
        provider.Dispose();
    }

    // ---------------------------------------------------------------------
    // SKCompositeTileProvider
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Composite_FirstHit_ShortCircuits()
    {
        var first = new StubProvider(new byte[] { 1 });
        var second = new StubProvider(new byte[] { 2 });
        using var composite = new SKCompositeTileProvider(first, second);

        var result = await composite.GetTileAsync("u");

        Assert.Equal(new byte[] { 1 }, result!.Data);
        Assert.Equal(1, first.CallCount);
        Assert.Equal(0, second.CallCount);
    }

    [Fact]
    public async Task Composite_FirstMiss_FallsThrough()
    {
        var first = new StubProvider(null);
        var second = new StubProvider(new byte[] { 2 });
        using var composite = new SKCompositeTileProvider(first, second);

        var result = await composite.GetTileAsync("u");

        Assert.Equal(new byte[] { 2 }, result!.Data);
        Assert.Equal(1, first.CallCount);
        Assert.Equal(1, second.CallCount);
    }

    [Fact]
    public async Task Composite_AllMiss_ReturnsNull()
    {
        var first = new StubProvider(null);
        var second = new StubProvider(null);
        using var composite = new SKCompositeTileProvider(first, second);

        var result = await composite.GetTileAsync("u");

        Assert.Null(result);
    }

    [Fact]
    public void Composite_NoProviders_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SKCompositeTileProvider());
    }

    [Fact]
    public async Task Composite_CancelledToken_Throws()
    {
        using var composite = new SKCompositeTileProvider(new StubProvider(new byte[] { 1 }));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => composite.GetTileAsync("u", cts.Token));
    }

    [Fact]
    public void Composite_Dispose_CascadesToInner()
    {
        var first = new StubProvider(null);
        var second = new StubProvider(null);
        var composite = new SKCompositeTileProvider(first, second);

        composite.Dispose();

        Assert.True(first.IsDisposed);
        Assert.True(second.IsDisposed);
    }

    // ---------------------------------------------------------------------
    // SKCachedTileProvider (via an in-memory test double)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Cached_Miss_CallsInnerAndPersists_ThenHitSkipsInner()
    {
        var inner = new StubProvider(new byte[] { 7, 7, 7 });
        using var cache = new DictCacheProvider(inner);

        var first = await cache.GetTileAsync("http://x/tile.jpg");
        Assert.NotNull(first);
        Assert.Equal(1, inner.CallCount);

        // Wait for the fire-and-forget write to land in the backing store.
        Assert.True(await WaitUntilAsync(() => cache.WriteCount >= 1));

        var second = await cache.GetTileAsync("http://x/tile.jpg");
        Assert.NotNull(second);
        Assert.Equal(new byte[] { 7, 7, 7 }, second!.Data);
        Assert.Equal(1, inner.CallCount); // served from cache, inner not called again
        Assert.Equal(2, cache.ReadCount);
    }

    [Fact]
    public async Task Cached_ReadThrows_TreatedAsMiss()
    {
        var inner = new StubProvider(new byte[] { 4, 2 });
        using var cache = new DictCacheProvider(inner) { ThrowOnRead = true };

        var result = await cache.GetTileAsync("http://x/tile.jpg");

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 4, 2 }, result!.Data);
        Assert.Equal(1, inner.CallCount);
    }

    [Fact]
    public async Task Cached_WriteThrows_Swallowed_TileStillReturned()
    {
        var inner = new StubProvider(new byte[] { 5, 5 });
        using var cache = new DictCacheProvider(inner) { ThrowOnWrite = true };

        var result = await cache.GetTileAsync("http://x/tile.jpg");

        Assert.NotNull(result);
        Assert.Equal(new byte[] { 5, 5 }, result!.Data);
        Assert.True(await WaitUntilAsync(() => cache.WriteCount >= 1));
    }

    [Fact]
    public async Task Cached_CancelledToken_Throws()
    {
        var inner = new StubProvider(new byte[] { 1 });
        using var cache = new DictCacheProvider(inner);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => cache.GetTileAsync("http://x/tile.jpg", cts.Token));
    }

    [Fact]
    public void Cached_Dispose_CascadesToInner()
    {
        var inner = new StubProvider(null);
        var cache = new DictCacheProvider(inner);

        cache.Dispose();

        Assert.True(inner.IsDisposed);
    }

    [Fact]
    public void Cached_ComputeKey_StableAndDistinct()
    {
        Assert.Equal(DictCacheProvider.KeyFor("http://x/a.jpg"), DictCacheProvider.KeyFor("http://x/a.jpg"));
        Assert.NotEqual(DictCacheProvider.KeyFor("http://x/a.jpg"), DictCacheProvider.KeyFor("http://x/b.jpg"));
        Assert.Equal(16, DictCacheProvider.KeyFor("http://x/a.jpg").Length);
    }

    // ---------------------------------------------------------------------
    // SKDiskCacheTileProvider
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Disk_Miss_FetchesPersists_ThenServesFromDisk()
    {
        var dir = NewTempDir();
        var bytes = new byte[] { 10, 20, 30, 40 };
        var inner = new StubProvider(bytes);
        using var disk = new SKDiskCacheTileProvider(inner, dir, TimeSpan.FromMinutes(5));

        var first = await disk.GetTileAsync("http://x/0/0_0.jpg");
        Assert.NotNull(first);
        Assert.Equal(1, inner.CallCount);

        Assert.True(await WaitUntilAsync(() => HasTileFiles(dir)));

        var second = await disk.GetTileAsync("http://x/0/0_0.jpg");
        Assert.NotNull(second);
        Assert.Equal(bytes, second!.Data); // exact round-trip
        Assert.Equal(1, inner.CallCount);  // served from disk
    }

    [Fact]
    public async Task Disk_Expired_RefetchesFromInner()
    {
        var dir = NewTempDir();
        var inner = new StubProvider(new byte[] { 1, 2 });
        using var disk = new SKDiskCacheTileProvider(inner, dir, TimeSpan.FromMilliseconds(1));

        await disk.GetTileAsync("http://x/0/0_0.jpg");
        Assert.True(await WaitUntilAsync(() => HasTileFiles(dir)));

        await Task.Delay(40); // let the entry expire

        var result = await disk.GetTileAsync("http://x/0/0_0.jpg");
        Assert.NotNull(result);
        Assert.Equal(2, inner.CallCount); // expired entry forced a refetch
    }

    [Fact]
    public async Task Disk_Clear_RemovesCachedTiles()
    {
        var dir = NewTempDir();
        var inner = new StubProvider(new byte[] { 1 });
        using var disk = new SKDiskCacheTileProvider(inner, dir, TimeSpan.FromMinutes(5));

        await disk.GetTileAsync("http://x/0/0_0.jpg");
        Assert.True(await WaitUntilAsync(() => HasTileFiles(dir)));

        disk.Clear();

        Assert.False(HasTileFiles(dir));
    }

    [Fact]
    public void Disk_Dispose_CascadesToInner()
    {
        var dir = NewTempDir();
        var inner = new StubProvider(null);
        var disk = new SKDiskCacheTileProvider(inner, dir);

        disk.Dispose();

        Assert.True(inner.IsDisposed);
    }

    [Fact]
    public void Disk_NullBasePath_Throws()
    {
        var inner = new StubProvider(null);
        Assert.Throws<ArgumentNullException>(() => new SKDiskCacheTileProvider(inner, null!));
        inner.Dispose();
    }

    private static bool HasTileFiles(string dir) =>
        Directory.EnumerateFiles(dir, "*.tile", SearchOption.AllDirectories).Any();

    // ---------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------

    /// <summary>An origin provider that returns fixed bytes (or null) and counts calls.</summary>
    private sealed class StubProvider : ISKImagePyramidTileProvider
    {
        private readonly byte[]? _bytes;
        private int _callCount;

        public StubProvider(byte[]? bytes) => _bytes = bytes;

        public int CallCount => _callCount;
        public bool IsDisposed { get; private set; }

        public Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _callCount);
            return Task.FromResult<SKImagePyramidTileData?>(
                _bytes is null ? null : new SKImagePyramidTileData(_bytes));
        }

        public void Dispose() => IsDisposed = true;
    }

    /// <summary>An in-memory <see cref="SKCachedTileProvider"/> for exercising the cache flow.</summary>
    private sealed class DictCacheProvider : SKCachedTileProvider
    {
        private readonly ConcurrentDictionary<string, byte[]> _store = new();
        private int _readCount;
        private int _writeCount;

        public DictCacheProvider(ISKImagePyramidTileProvider inner)
            : base(inner)
        {
        }

        public int ReadCount => _readCount;
        public int WriteCount => _writeCount;
        public bool ThrowOnRead { get; set; }
        public bool ThrowOnWrite { get; set; }

        public static string KeyFor(string url) => ComputeKey(url);

        protected override Task<SKImagePyramidTileData?> ReadAsync(string key, CancellationToken ct)
        {
            Interlocked.Increment(ref _readCount);
            if (ThrowOnRead)
                throw new InvalidOperationException("read failure");

            return Task.FromResult<SKImagePyramidTileData?>(
                _store.TryGetValue(key, out var bytes) ? new SKImagePyramidTileData(bytes) : null);
        }

        protected override Task WriteAsync(string key, SKImagePyramidTileData data, CancellationToken ct)
        {
            if (ThrowOnWrite)
            {
                Interlocked.Increment(ref _writeCount);
                throw new InvalidOperationException("write failure");
            }

            _store[key] = data.Data;
            Interlocked.Increment(ref _writeCount);
            return Task.CompletedTask;
        }
    }

    /// <summary>Returns a fixed status code and content for every request.</summary>
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly byte[] _content;

        public MockHttpMessageHandler(HttpStatusCode status, byte[]? content = null)
        {
            _status = status;
            _content = content ?? Array.Empty<byte>();
        }

        public bool Disposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var response = new HttpResponseMessage(_status)
            {
                Content = new ByteArrayContent(_content),
            };
            return Task.FromResult(response);
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    /// <summary>Always faults with a configured exception.</summary>
    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception) => _exception = exception;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromException<HttpResponseMessage>(_exception);
    }
}
