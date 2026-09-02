#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Fetches tiles from the local file system.
    /// </summary>
    public class SKDeepZoomFileTileFetcher : ISKDeepZoomTileFetcher
    {
        public Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

                cancellationToken.ThrowIfCancellationRequested();
                var bitmap = SKBitmap.Decode(path);
                return Task.FromResult<SKBitmap?>(bitmap);
            }
            catch (OperationCanceledException)
            {
                throw; // Propagate cancellation
            }
            catch
            {
                return Task.FromResult<SKBitmap?>(null);
            }
        }

        public void Dispose() { }
    }
}
