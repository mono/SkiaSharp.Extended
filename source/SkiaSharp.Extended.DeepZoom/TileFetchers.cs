using SkiaSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Fetches tiles over HTTP. Thread-safe and reusable.
    /// </summary>
    public class HttpTileFetcher : ITileFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly bool _ownsClient;

        /// <summary>
        /// Creates a new HttpTileFetcher with an internal HttpClient.
        /// </summary>
        public HttpTileFetcher()
        {
            _httpClient = new HttpClient();
            _ownsClient = true;
        }

        /// <summary>
        /// Creates a new HttpTileFetcher using an existing HttpClient.
        /// The caller retains ownership of the HttpClient.
        /// </summary>
        public HttpTileFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ownsClient = false;
        }

        public async Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return null; // 404 is expected for missing tiles

#if NETSTANDARD2_0
                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return SKBitmap.Decode(bytes);
#else
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return SKBitmap.Decode(stream);
#endif
            }
            catch (HttpRequestException)
            {
                return null; // Network errors → null (tile not available)
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

    /// <summary>
    /// Fetches tiles from the local file system.
    /// </summary>
    public class FileTileFetcher : ITileFetcher
    {
        public Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                // Handle both file:// URIs and plain paths
                string path = url;
                if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    path = new Uri(url).LocalPath;
                }

                if (!File.Exists(path))
                    return Task.FromResult<SKBitmap?>(null);

                var bitmap = SKBitmap.Decode(path);
                return Task.FromResult<SKBitmap?>(bitmap);
            }
            catch
            {
                return Task.FromResult<SKBitmap?>(null);
            }
        }

        public void Dispose() { }
    }
}
