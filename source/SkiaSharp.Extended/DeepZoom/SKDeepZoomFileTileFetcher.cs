#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Fetches tiles from the local file system.
    /// Accepts both plain file paths and <c>file://</c> URIs.
    /// </summary>
    public class SKDeepZoomFileTileFetcher : ISKDeepZoomTileFetcher
    {
        /// <inheritdoc />
        public Task<ISKDeepZoomTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string path = url;
                if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                    path = new Uri(url).LocalPath;

                if (!File.Exists(path))
                    return Task.FromResult<ISKDeepZoomTile?>(null);

                cancellationToken.ThrowIfCancellationRequested();
                var bitmap = SKBitmap.Decode(path);
                ISKDeepZoomTile? tile = bitmap != null ? new SKDeepZoomBitmapTile(bitmap) : null;
                return Task.FromResult(tile);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return Task.FromResult<ISKDeepZoomTile?>(null);
            }
        }

        public void Dispose() { }
    }
}
