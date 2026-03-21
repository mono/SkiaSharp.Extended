using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Extended;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A tile provider decorator that persists fetched tiles in browser sessionStorage via JS
/// interop, providing a URL-keyed L2 cache that survives page re-renders. Tiles are stored
/// as base64-encoded raw bytes so no re-encoding is needed on a storage hit.
/// </summary>
public sealed class BrowserStorageTileProvider(ISKImagePyramidTileProvider inner, IJSRuntime js)
    : ISKImagePyramidTileProvider
{
    private readonly ISKImagePyramidTileProvider _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly IJSRuntime _js = js;

    /// <inheritdoc/>
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested) return null;

        // 1. Browser sessionStorage hit?
        var cached = await TryReadFromStorageAsync(url, ct).ConfigureAwait(false);
        if (cached != null) return cached;

        // 2. Delegate to inner provider (HTTP fetch)
        var tile = await _inner.GetTileAsync(url, ct).ConfigureAwait(false);
        if (tile == null) return null;

        // 3. Persist to browser storage — fire-and-forget with CancellationToken.None.
        //    Once the tile is in hand, don't let a cancellation skip storage.
        _ = WriteToBrowserAsync(url, tile.RawData);

        return tile;
    }

    /// <inheritdoc/>
    public void Dispose() => _inner.Dispose();

    // ---- Private ----

    private async Task<SKImagePyramidTile?> TryReadFromStorageAsync(string url, CancellationToken ct)
    {
        try
        {
            var base64 = await _js.InvokeAsync<string?>(
                "deepZoomCacheGet", ct, StorageKey(url)).ConfigureAwait(false);

            if (base64 is null) return null;

            var bytes = Convert.FromBase64String(base64);
            var image = SKImage.FromEncodedData(bytes);
            if (image is null) return null;

            return new SKImagePyramidTile(image, bytes);
        }
        catch { return null; }
    }

    private async Task WriteToBrowserAsync(string url, byte[] rawData)
    {
        try
        {
            var base64 = Convert.ToBase64String(rawData);
            await _js.InvokeVoidAsync("deepZoomCacheSet", CancellationToken.None, StorageKey(url), base64)
                     .ConfigureAwait(false);
        }
        catch { }
    }

    private static string StorageKey(string url)
    {
        // Use a short hash of the URL as the sessionStorage key
        ulong h = 14695981039346656037UL;
        foreach (char c in url) { h ^= c; h *= 1099511628211UL; }
        return "skimgpyramid_" + h.ToString("x");
    }
}
