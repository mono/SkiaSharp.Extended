#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles over HTTP and decodes them using a provided <see cref="ISKDeepZoomTileDecoder"/>.
/// Thread-safe and reusable.
/// </summary>
public class SKDeepZoomHttpTileFetcher : ISKDeepZoomTileFetcher
{
    private readonly ISKDeepZoomTileDecoder _decoder;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;

    /// <summary>
    /// Creates a new <see cref="SKDeepZoomHttpTileFetcher"/> with the given decoder
    /// and an internally-managed <see cref="HttpClient"/>.
    /// </summary>
    public SKDeepZoomHttpTileFetcher(ISKDeepZoomTileDecoder decoder)
    {
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        _httpClient = new HttpClient();
        _ownsClient = true;
    }

    /// <summary>
    /// Creates a new <see cref="SKDeepZoomHttpTileFetcher"/> using an existing
    /// <see cref="HttpClient"/> (caller retains ownership).
    /// </summary>
    public SKDeepZoomHttpTileFetcher(ISKDeepZoomTileDecoder decoder, HttpClient httpClient)
    {
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsClient = false;
    }

    /// <inheritdoc />
    public async Task<ISKDeepZoomTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

#if NETSTANDARD2_0
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            using var ms = new MemoryStream(bytes);
#else
            using var ms = new MemoryStream();
            using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
#endif
            return _decoder.Decode(ms);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsClient)
            _httpClient.Dispose();
    }
}
