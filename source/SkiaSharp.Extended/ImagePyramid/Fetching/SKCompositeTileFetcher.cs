#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Tries multiple fetchers in order, returning the first non-null result.
/// Use for hybrid scenarios (e.g. app package first, HTTP fallback).
/// </summary>
public sealed class SKCompositeTileFetcher : ISKTileFetcher
{
    private readonly ISKTileFetcher[] _fetchers;

    /// <summary>
    /// Creates a composite fetcher that tries each fetcher in order.
    /// </summary>
    public SKCompositeTileFetcher(params ISKTileFetcher[] fetchers)
    {
        if (fetchers == null || fetchers.Length == 0)
            throw new ArgumentException("At least one fetcher is required.", nameof(fetchers));
        _fetchers = fetchers;
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        foreach (var fetcher in _fetchers)
        {
            var result = await fetcher.FetchAsync(url, ct).ConfigureAwait(false);
            if (result != null)
                return result;
        }

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var fetcher in _fetchers)
            fetcher.Dispose();
    }
}
