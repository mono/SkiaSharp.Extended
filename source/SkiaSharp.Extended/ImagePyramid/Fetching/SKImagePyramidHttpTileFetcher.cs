#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles over HTTP and decodes them using SkiaSharp.
/// Raw bytes are buffered before decoding, which handles forward-only streams
/// and allows the bytes to be stored in L2 caches without re-encoding.
/// Thread-safe and reusable.
/// </summary>
public class SKImagePyramidHttpTileFetcher : ISKImagePyramidTileFetcher
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;

    /// <summary>
    /// Creates a new <see cref="SKImagePyramidHttpTileFetcher"/> with an internally-managed <see cref="HttpClient"/>.
    /// </summary>
    public SKImagePyramidHttpTileFetcher()
    {
        _httpClient = new HttpClient();
        _ownsClient = true;
    }

    /// <summary>
    /// Creates a new <see cref="SKImagePyramidHttpTileFetcher"/> using an existing
    /// <see cref="HttpClient"/> (caller retains ownership).
    /// </summary>
    public SKImagePyramidHttpTileFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsClient = false;
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            // ReadAsByteArrayAsync buffers into memory — handles forward-only streams
            // and gives us the raw bytes for L2 cache storage without re-encoding.
#if NETSTANDARD2_0
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#else
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#endif
            var image = SKImage.FromEncodedData(bytes);
            return image != null ? new SKImagePyramidTile(image, bytes) : null;
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
