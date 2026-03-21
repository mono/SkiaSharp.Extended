#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Composes a fetcher and optional persistent cache into a tile provider.
/// Flow: persistent cache → origin fetch → persist → decode → return.
/// </summary>
/// <remarks>
/// <para>
/// The tiered provider handles only persistent caching and origin fetching.
/// The in-memory render buffer is managed by <see cref="SKImagePyramidController"/>,
/// which checks it before calling this provider.
/// </para>
/// </remarks>
public sealed class SKTieredTileProvider : ISKImagePyramidTileProvider
{
    private readonly ISKTileFetcher _fetcher;
    private readonly ISKTileCacheStore? _persistentCache;

    /// <summary>
    /// Creates a tiered tile provider.
    /// </summary>
    /// <param name="fetcher">The origin tile fetcher (HTTP, file, composite, etc.).</param>
    /// <param name="persistentCache">
    /// Optional persistent cache. Pass <see langword="null"/> to disable persistent caching.
    /// </param>
    public SKTieredTileProvider(
        ISKTileFetcher fetcher,
        ISKTileCacheStore? persistentCache = null)
    {
        _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
        _persistentCache = persistentCache;
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        string key = ComputeKey(url);
        SKImagePyramidTileData? tileData = null;

        // 1. Persistent cache
        if (_persistentCache != null)
        {
            try { tileData = await _persistentCache.TryGetAsync(key, ct).ConfigureAwait(false); }
            catch { }
        }

        // 2. Origin fetch
        if (tileData == null)
        {
            tileData = await _fetcher.FetchAsync(url, ct).ConfigureAwait(false);
            if (tileData == null) return null;

            // Persist (fire-and-forget, don't let cancellation skip storage)
            if (_persistentCache != null)
                _ = PersistSafeAsync(key, tileData);
        }

        // 3. Decode
        var image = SKImage.FromEncodedData(tileData.Data);
        return image == null ? null : new SKImagePyramidTile(image, tileData.Data);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _fetcher.Dispose();
        _persistentCache?.Dispose();
    }

    // ---- Private ----

    private async Task PersistSafeAsync(string key, SKImagePyramidTileData data)
    {
        try { await _persistentCache!.SetAsync(key, data, CancellationToken.None).ConfigureAwait(false); }
        catch { }
    }

    private static string ComputeKey(string url)
    {
        // FNV-1a 64-bit hash for filesystem-safe, short keys
        const ulong OffsetBasis = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        ulong hash = OffsetBasis;
        foreach (char c in url)
        {
            hash ^= (byte)(c & 0xFF);
            hash *= Prime;
            hash ^= (byte)(c >> 8);
            hash *= Prime;
        }
        return hash.ToString("x16");
    }
}
