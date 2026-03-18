using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Extended;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A tile cache that persists decoded tile images in browser sessionStorage via JS interop.
/// Images are encoded as PNG bytes and stored as base64 strings.
/// This demonstrates L2 cache in a tiered memory → storage → network hierarchy.
///
/// Architecture note: async TryGetAsync is the right approach for storage-backed caches.
/// Chaining fetchers instead would conflate "retrieve from source" with "check if already stored",
/// leading to incorrect semantics. The cache-aside pattern (controller checks L1 → TryGetAsync L2
/// → network fetch → populate both) is the standard approach used by Coil, SDWebImage, and Glide.
/// </summary>
public sealed class BrowserStorageTileCache(IJSRuntime js) : ISKImagePyramidTileCache
{
    private readonly IJSRuntime _js = js;
    // In-memory index so Contains/TryGet (sync) can return fast without JS interop round-trips.
    // When full, oldest entry is evicted (it remains in sessionStorage and can be re-hydrated).
    private const int MaxMemoryEntries = 512;
    private readonly ConcurrentDictionary<SKImagePyramidTileId, SKImagePyramidImageTile> _memIndex = new();
    private int _storageCount;

    public int Count => _memIndex.Count;

    // Sync TryGet only looks up the in-memory index (populated when we put tiles).
    // The renderer uses TryGet during drawing -- it must not block on JS interop.
    public bool TryGet(SKImagePyramidTileId id, out ISKImagePyramidTile? tile)
    {
        if (_memIndex.TryGetValue(id, out var imgTile))
        {
            tile = imgTile;
            return true;
        }
        tile = null;
        return false;
    }

    // TryGetAsync checks browser storage and decodes the image.
    // Called by the controller before hitting the network -- no need to double-fetch.
    public async Task<ISKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
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
            var image = SKImage.FromEncodedData(bytes);
            if (image is null) return null;

            var tile = new SKImagePyramidImageTile(image);
            AddToMemIndex(id, tile);
            return tile;
        }
        catch { return null; }
    }

    public bool Contains(SKImagePyramidTileId id) => _memIndex.ContainsKey(id);

    public void Put(SKImagePyramidTileId id, ISKImagePyramidTile? tile)
    {
        if (tile is not SKImagePyramidImageTile imgTile) return;
        AddToMemIndex(id, imgTile);
        // Fire-and-forget write to storage (best-effort, no await)
        _ = WriteToBrowserAsync(id, imgTile.Image, CancellationToken.None);
    }

    public async Task PutAsync(SKImagePyramidTileId id, ISKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (tile is not SKImagePyramidImageTile imgTile) return;
        bool stored = await WriteToBrowserAsync(id, imgTile.Image, ct).ConfigureAwait(false);
        if (stored)
            AddToMemIndex(id, imgTile);
    }

    /// <summary>
    /// Adds or updates an entry in the in-memory index, evicting an arbitrary entry if at capacity.
    /// Evicted tiles remain in sessionStorage and can be re-hydrated on next TryGetAsync.
    /// </summary>
    private void AddToMemIndex(SKImagePyramidTileId id, SKImagePyramidImageTile tile)
    {
        // If already present, replace and dispose the old tile.
        if (_memIndex.TryGetValue(id, out var existing))
        {
            _memIndex[id] = tile;
            if (!ReferenceEquals(existing, tile))
                existing?.Dispose();
            return;
        }
        // Evict one entry if at capacity to make room.
        if (_memIndex.Count >= MaxMemoryEntries)
        {
            foreach (var key in _memIndex.Keys)
            {
                if (_memIndex.TryRemove(key, out var evicted))
                {
                    evicted?.Dispose();
                    break;
                }
            }
        }
        _memIndex.TryAdd(id, tile);
    }

    private async Task<bool> WriteToBrowserAsync(SKImagePyramidTileId id, SKImage image, CancellationToken ct)
    {
        try
        {
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());
            await _js.InvokeVoidAsync("deepZoomCacheSet", ct, TileKey(id), base64)
                     .ConfigureAwait(false);
            Interlocked.Increment(ref _storageCount);
            return true;
        }
        catch { return false; /* quota exceeded or interop unavailable */ }
    }

    public bool Remove(SKImagePyramidTileId id)
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

    private static string TileKey(SKImagePyramidTileId id) => $"{id.Level}_{id.Col}_{id.Row}";
}
