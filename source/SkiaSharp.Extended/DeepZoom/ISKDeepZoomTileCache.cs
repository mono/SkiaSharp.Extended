#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Pluggable tile bitmap cache for Deep Zoom rendering.
    /// Implement this interface to provide custom caching strategies
    /// (in-memory, disk, tiered, etc.).
    /// </summary>
    public interface ISKDeepZoomTileCache : IDisposable
    {
        /// <summary>Number of tiles currently in the cache.</summary>
        int Count { get; }

        /// <summary>Tries to retrieve a cached tile bitmap.</summary>
        bool TryGet(SKDeepZoomTileId id, out SKBitmap? bitmap);

        /// <summary>
        /// Asynchronously tries to retrieve a cached tile bitmap.
        /// Use this for cache tiers that involve I/O (browser storage, disk, etc.).
        /// A null return means cache miss -- the caller should then fetch from network.
        /// </summary>
        Task<SKBitmap?> TryGetAsync(SKDeepZoomTileId id, CancellationToken ct = default);

        /// <summary>Returns <see langword="true"/> if the tile is cached.</summary>
        bool Contains(SKDeepZoomTileId id);

        /// <summary>
        /// Stores a tile bitmap synchronously. Use this for simple in-memory caches.
        /// </summary>
        void Put(SKDeepZoomTileId id, SKBitmap? bitmap);

        /// <summary>
        /// Stores a tile bitmap. Implementations may apply delays or tiered writes.
        /// Called from a background thread; the <paramref name="ct"/> should be respected.
        /// </summary>
        Task PutAsync(SKDeepZoomTileId id, SKBitmap? bitmap, CancellationToken ct = default);

        /// <summary>Removes a specific tile from the cache.</summary>
        bool Remove(SKDeepZoomTileId id);

        /// <summary>Clears all cached tiles.</summary>
        void Clear();

        /// <summary>
        /// Disposes bitmaps evicted since the last call. Call once per render frame
        /// on the UI thread before drawing to safely release GPU-bound bitmaps.
        /// </summary>
        void FlushEvicted();
    }
}
