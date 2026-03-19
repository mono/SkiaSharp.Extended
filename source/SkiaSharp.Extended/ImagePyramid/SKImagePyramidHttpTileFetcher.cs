#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles over HTTP and decodes them using SkiaSharp.
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
    public async Task<SKImage?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
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
            return SKImage.FromEncodedData(ms);
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
