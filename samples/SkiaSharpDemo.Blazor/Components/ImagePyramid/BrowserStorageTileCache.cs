using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Extended;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A tile cache that persists tile raw bytes in browser sessionStorage via JS interop.
/// Raw bytes are stored as base64 strings, so no re-encoding is needed on cache hit.
/// This demonstrates L2 cache in a tiered memory → storage → network hierarchy.
/// </summary>
public sealed class BrowserStorageTileCache(IJSRuntime js) : ISKImagePyramidTileCache
{
    private readonly IJSRuntime _js = js;
    private const int MaxMemoryEntries = 512;
    private readonly ConcurrentDictionary<SKImagePyramidTileId, SKImagePyramidTile> _memIndex = new();
    private int _storageCount;

    public int Count => _memIndex.Count;

    /// <inheritdoc/>
    /// <remarks>Browser storage cache does not use source namespacing; this property is ignored.</remarks>
    public string? ActiveSourceId { get; set; }

    public bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile)
    {
        if (_memIndex.TryGetValue(id, out var cached))
        {
            tile = cached;
            return true;
        }
        tile = null;
        return false;
    }

    public async Task<SKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
    {
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

            var tile = new SKImagePyramidTile(image, bytes);
            AddToMemIndex(id, tile);
            return tile;
        }
        catch { return null; }
    }

    public bool Contains(SKImagePyramidTileId id) => _memIndex.ContainsKey(id);

    public void Put(SKImagePyramidTileId id, SKImagePyramidTile? tile)
    {
        if (tile is null) return;
        AddToMemIndex(id, tile);
        _ = WriteToBrowserAsync(id, tile, CancellationToken.None);
    }

    public async Task PutAsync(SKImagePyramidTileId id, SKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (tile is null) return;
        bool stored = await WriteToBrowserAsync(id, tile, ct).ConfigureAwait(false);
        if (stored)
            AddToMemIndex(id, tile);
    }

    private void AddToMemIndex(SKImagePyramidTileId id, SKImagePyramidTile tile)
    {
        if (_memIndex.TryGetValue(id, out var existing))
        {
            _memIndex[id] = tile;
            if (!ReferenceEquals(existing, tile))
                existing?.Dispose();
            return;
        }
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

    private async Task<bool> WriteToBrowserAsync(SKImagePyramidTileId id, SKImagePyramidTile tile, CancellationToken ct)
    {
        try
        {
            // Use raw bytes directly — no re-encoding needed!
            var base64 = Convert.ToBase64String(tile.RawData);
            await _js.InvokeVoidAsync("deepZoomCacheSet", ct, TileKey(id), base64)
                     .ConfigureAwait(false);
            Interlocked.Increment(ref _storageCount);
            return true;
        }
        catch { return false; }
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

    public void FlushEvicted() { }

    public void Dispose() => Clear();

    private static string TileKey(SKImagePyramidTileId id) => $"{id.Level}_{id.Col}_{id.Row}";
}
