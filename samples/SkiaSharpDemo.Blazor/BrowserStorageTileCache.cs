using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Extended;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A tile cache that persists decoded tile bitmaps in browser sessionStorage via JS interop.
/// Bitmaps are encoded as PNG bytes and stored as base64 strings.
/// This demonstrates L2 cache in a tiered memory → storage → network hierarchy.
///
/// Architecture note: async TryGetAsync is the right approach for storage-backed caches.
/// Chaining fetchers instead would conflate "retrieve from source" with "check if already stored",
/// leading to incorrect semantics. The cache-aside pattern (controller checks L1 → TryGetAsync L2
/// → network fetch → populate both) is the standard approach used by Coil, SDWebImage, and Glide.
/// </summary>
public sealed class BrowserStorageTileCache(IJSRuntime js) : ISKDeepZoomTileCache
{
    private readonly IJSRuntime _js = js;
    // In-memory index so Contains/TryGet (sync) can return fast without JS interop round-trips.
    // Capped to prevent unbounded growth (tiles still persist in sessionStorage beyond this limit).
    private const int MaxMemoryEntries = 512;
    private readonly ConcurrentDictionary<SKDeepZoomTileId, SKDeepZoomBitmapTile> _memIndex = new();
    private int _storageCount;

    public int Count => _memIndex.Count;

    // Sync TryGet only looks up the in-memory index (populated when we put tiles).
    // The renderer uses TryGet during drawing -- it must not block on JS interop.
    public bool TryGet(SKDeepZoomTileId id, out ISKDeepZoomTile? tile)
    {
        if (_memIndex.TryGetValue(id, out var bmpTile))
        {
            tile = bmpTile;
            return true;
        }
        tile = null;
        return false;
    }

    // TryGetAsync checks browser storage and decodes the bitmap.
    // Called by the controller before hitting the network -- no need to double-fetch.
    public async Task<ISKDeepZoomTile?> TryGetAsync(SKDeepZoomTileId id, CancellationToken ct = default)
    {
        // Fast-path: already in memory index.
        if (_memIndex.TryGetValue(id, out var cached))
            return cached;

        try
        {
            var base64 = await _js.InvokeAsync<string?>(
                "deepZoomCacheGet", ct, TileKey(id)).ConfigureAwait(false);

            if (base64 is null) return null;

            var bytes = Convert.FromBase64String(base64);
            var bitmap = SKBitmap.Decode(bytes);
            if (bitmap is null) return null;

            var tile = new SKDeepZoomBitmapTile(bitmap);
            if (_memIndex.Count < MaxMemoryEntries)
                _memIndex.TryAdd(id, tile);
            return tile;
        }
        catch { return null; }
    }

    public bool Contains(SKDeepZoomTileId id) => _memIndex.ContainsKey(id);

    public void Put(SKDeepZoomTileId id, ISKDeepZoomTile? tile)
    {
        if (tile is not SKDeepZoomBitmapTile bmpTile) return;
        if (_memIndex.Count < MaxMemoryEntries)
            _memIndex[id] = bmpTile;
        // Fire-and-forget write to storage (best-effort, no await)
        _ = WriteToBrowserAsync(id, bmpTile.Bitmap, CancellationToken.None);
    }

    public async Task PutAsync(SKDeepZoomTileId id, ISKDeepZoomTile? tile, CancellationToken ct = default)
    {
        if (tile is not SKDeepZoomBitmapTile bmpTile) return;
        bool stored = await WriteToBrowserAsync(id, bmpTile.Bitmap, ct).ConfigureAwait(false);
        // Only cache in memory if the storage write succeeded and we have room.
        if (stored && _memIndex.Count < MaxMemoryEntries)
            _memIndex.TryAdd(id, bmpTile);
    }

    private async Task<bool> WriteToBrowserAsync(SKDeepZoomTileId id, SKBitmap bitmap, CancellationToken ct)
    {
        try
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());
            await _js.InvokeVoidAsync("deepZoomCacheSet", ct, TileKey(id), base64)
                     .ConfigureAwait(false);
            Interlocked.Increment(ref _storageCount);
            return true;
        }
        catch { return false; /* quota exceeded or interop unavailable */ }
    }

    public bool Remove(SKDeepZoomTileId id)
    {
        bool removed = _memIndex.TryRemove(id, out var tile);
        tile?.Dispose();
        return removed;
    }

    public void Clear()
    {
        foreach (var tile in _memIndex.Values) tile?.Dispose();
        _memIndex.Clear();
        _storageCount = 0;
        _ = _js.InvokeVoidAsync("deepZoomCacheClear");
    }

    // Browser sessionStorage tiles don't need deferred disposal — no GPU resources involved.
    public void FlushEvicted() { }

    public void Dispose() => Clear();

    private static string TileKey(SKDeepZoomTileId id) => $"{id.Level}_{id.Col}_{id.Row}";
}
