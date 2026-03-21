#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles over HTTP. Pure network access — no caching.
/// </summary>
public sealed class SKHttpTileFetcher : ISKTileFetcher
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    /// <summary>
    /// Creates an HTTP tile fetcher.
    /// </summary>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/>. If <see langword="null"/>, an internal client
    /// is created and disposed with this instance.
    /// </param>
    public SKHttpTileFetcher(HttpClient? httpClient = null)
    {
        _ownsHttp = httpClient == null;
        _http = httpClient ?? new HttpClient();
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            using var response = await _http
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

#if NETSTANDARD2_0
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#else
            var bytes = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
#endif

            return new SKImagePyramidTileData(bytes);
        }
        catch (HttpRequestException) { return null; }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (OperationCanceledException) { return null; } // HTTP timeout
        catch { return null; }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }
}
