#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Interface for fetching tile image data from various sources (HTTP, file, memory).
    /// </summary>
    public interface ISKDeepZoomTileFetcher : IDisposable
    {
        /// <summary>
        /// Fetches a tile as an SKBitmap. Returns null if the tile is not available (404).
        /// </summary>
        Task<SKBitmap?> FetchTileAsync(string url, CancellationToken cancellationToken = default);
    }
}
