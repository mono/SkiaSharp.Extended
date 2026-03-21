#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SkiaSharp.Extended;

/// <summary>
/// A tile cache that persists tile bytes in browser sessionStorage via JS interop.
/// The original encoded bytes are stored as base64 strings — no re-encoding needed.
/// This is the recommended L2 tier for Blazor WebAssembly (no real filesystem available).
/// </summary>
/// <remarks>
/// <para>
/// Requires the following JavaScript functions in the host page:
/// <c>deepZoomCacheGet(key)</c>, <c>deepZoomCacheSet(key, base64)</c>, <c>deepZoomCacheClear()</c>.
/// </para>
/// <para>
/// An in-memory index provides synchronous <see cref="TryGet"/> and <see cref="Contains"/>
/// for the renderer's hot path. The JS storage provides persistence across page interactions.
/// </para>
/// </remarks>
public sealed class SKImagePyramidBrowserStorageTileCache : ISKImagePyramidTileCache
{
    private readonly IJSRuntime _js;
    private const int MaxMemoryEntries = 512;
    private readonly ConcurrentDictionary<SKImagePyramidTileId, SKImagePyramidTile> _memIndex = new();
    private int _storageCount;

    /// <summary>Creates a new browser storage tile cache.</summary>
    /// <param name="js">The JS runtime for browser storage interop.</param>
    public SKImagePyramidBrowserStorageTileCache(IJSRuntime js)
    {
        _js = js ?? throw new ArgumentNullException(nameof(js));
    }

    /// <inheritdoc />
    public int Count => _memIndex.Count;

    /// <inheritdoc />
    public bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile)
    {
        if (_memIndex.TryGetValue(id, out var t))
        {
            tile = t;
            return true;
        }
        tile = null;
        return false;
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
    {
        if (_memIndex.TryGetValue(id, out var cached))
            return cached;

        try
        {
            var base64 = await _js.InvokeAsync<string?>("deepZoomCacheGet", ct, TileKey(id)).ConfigureAwait(false);
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

    /// <inheritdoc />
    public bool Contains(SKImagePyramidTileId id) => _memIndex.ContainsKey(id);

    /// <inheritdoc />
    public void Put(SKImagePyramidTileId id, SKImagePyramidTile? tile)
    {
        if (tile is null) return;
        AddToMemIndex(id, tile);
        _ = WriteToBrowserAsync(id, tile.RawData, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task PutAsync(SKImagePyramidTileId id, SKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (tile is null) return;
        bool stored = await WriteToBrowserAsync(id, tile.RawData, ct).ConfigureAwait(false);
        if (stored)
            AddToMemIndex(id, tile);
    }

    /// <inheritdoc />
    public bool Remove(SKImagePyramidTileId id)
    {
        bool removed = _memIndex.TryRemove(id, out var tile);
        tile?.Dispose();
        return removed;
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var tile in _memIndex.Values) tile?.Dispose();
        _memIndex.Clear();
        _storageCount = 0;
        _ = _js.InvokeVoidAsync("deepZoomCacheClear");
    }

    /// <inheritdoc />
    /// <remarks>No deferred disposal needed for browser tiles (no GPU-bound resources).</remarks>
    public void FlushEvicted() { }

    /// <inheritdoc />
    public void Dispose() => Clear();

    // ---- Private ----

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

    private async Task<bool> WriteToBrowserAsync(SKImagePyramidTileId id, byte[] bytes, CancellationToken ct)
    {
        try
        {
            var base64 = Convert.ToBase64String(bytes);
            await _js.InvokeVoidAsync("deepZoomCacheSet", ct, TileKey(id), base64).ConfigureAwait(false);
            Interlocked.Increment(ref _storageCount);
            return true;
        }
        catch { return false; }
    }

    private static string TileKey(SKImagePyramidTileId id) => $"{id.Level}_{id.Col}_{id.Row}";
}
