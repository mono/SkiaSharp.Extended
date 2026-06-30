using Microsoft.JSInterop;
using SkiaSharp.Extended;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A persistent cache decorator that stores fetched tiles in browser <c>sessionStorage</c>
/// via JS interop, giving the pyramid a URL-keyed L2 cache that survives page re-renders.
/// </summary>
/// <remarks>
/// This is the browser equivalent of the built-in <see cref="SKDiskCacheTileProvider"/>:
/// it derives from <see cref="SKCachedTileProvider"/> and implements just the storage
/// backend. Tiles are stored as base64-encoded encoded bytes, so a cache hit needs no
/// re-encoding and the decode still happens once, in the controller.
/// </remarks>
public sealed class BrowserCacheTileProvider : SKCachedTileProvider
{
    private readonly IJSRuntime _js;

    public BrowserCacheTileProvider(ISKImagePyramidTileProvider inner, IJSRuntime js)
        : base(inner)
    {
        _js = js ?? throw new ArgumentNullException(nameof(js));
    }

    /// <inheritdoc/>
    protected override async Task<SKImagePyramidTileData?> ReadAsync(string key, CancellationToken ct)
    {
        var base64 = await _js.InvokeAsync<string?>("imagePyramidCacheGet", ct, key).ConfigureAwait(false);
        if (base64 is null)
            return null;

        var bytes = Convert.FromBase64String(base64);
        return new SKImagePyramidTileData(bytes);
    }

    /// <inheritdoc/>
    protected override async Task WriteAsync(string key, SKImagePyramidTileData data, CancellationToken ct)
    {
        var base64 = Convert.ToBase64String(data.Data);
        await _js.InvokeVoidAsync("imagePyramidCacheSet", ct, key, base64).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes every tile this provider has cached in browser storage.
    /// </summary>
    public async Task ClearAsync()
    {
        try { await _js.InvokeVoidAsync("imagePyramidCacheClear").ConfigureAwait(false); }
        catch { /* best effort */ }
    }
}
