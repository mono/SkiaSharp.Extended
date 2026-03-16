#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Fetches tiles over HTTP. Thread-safe and reusable.
    /// </summary>
    public class SKDeepZoomHttpTileFetcher : ISKDeepZoomTileFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly bool _ownsClient;

        /// <summary>
        /// Creates a new <see cref="SKDeepZoomHttpTileFetcher"/> with an internal HttpClient.
        /// </summary>
        public SKDeepZoomHttpTileFetcher()
        {
            _httpClient = new HttpClient();
            _ownsClient = true;
        }

        /// <summary>
        /// Creates a new <see cref="SKDeepZoomHttpTileFetcher"/> using an existing HttpClient.
        /// The caller retains ownership of the HttpClient.
        /// </summary>
        public SKDeepZoomHttpTileFetcher(HttpClient httpClient)
        {
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
                var bitmap = SKBitmap.Decode(bytes);
#else
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var bitmap = SKBitmap.Decode(stream);
#endif
                return bitmap != null ? new SKDeepZoomBitmapTile(bitmap) : null;
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

        public void Dispose()
        {
            if (_ownsClient)
                _httpClient.Dispose();
        }
    }
}
