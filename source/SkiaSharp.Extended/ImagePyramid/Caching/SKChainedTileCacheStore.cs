#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Tries multiple cache stores in order for reads. Writes go to all writable stores.
/// Use for layered caching (e.g. app-package read-only store + disk cache).
/// </summary>
public sealed class SKChainedTileCacheStore : ISKTileCacheStore
{
    private readonly ISKTileCacheStore[] _stores;

    /// <summary>
    /// Creates a chained cache store. Stores are tried in order for reads.
    /// Writes go to all stores.
    /// </summary>
    public SKChainedTileCacheStore(params ISKTileCacheStore[] stores)
    {
        if (stores == null || stores.Length == 0)
            throw new ArgumentException("At least one store is required.", nameof(stores));
        _stores = stores;
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default)
    {
        foreach (var store in _stores)
        {
            var result = await store.TryGetAsync(key, ct).ConfigureAwait(false);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default)
    {
        foreach (var store in _stores)
        {
            try { await store.SetAsync(key, data, ct).ConfigureAwait(false); }
            catch { }
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        foreach (var store in _stores)
        {
            try { await store.RemoveAsync(key, ct).ConfigureAwait(false); }
            catch { }
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct = default)
    {
        foreach (var store in _stores)
        {
            try { await store.ClearAsync(ct).ConfigureAwait(false); }
            catch { }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var store in _stores)
            store.Dispose();
    }
}
