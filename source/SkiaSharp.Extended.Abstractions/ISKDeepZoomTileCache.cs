#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Pluggable tile cache for the Deep Zoom rendering pipeline.
    /// Tiles are stored as <see cref="ISKDeepZoomTile"/> so that the cache is
    /// rendering-backend-agnostic.
    /// </summary>
    public interface ISKDeepZoomTileCache : IDisposable
    {
        /// <summary>Number of tiles currently in the cache.</summary>
        int Count { get; }

        /// <summary>Tries to retrieve a cached tile.</summary>
        bool TryGet(SKDeepZoomTileId id, out ISKDeepZoomTile? tile);

        /// <summary>
        /// Asynchronously tries to retrieve a cached tile.
        /// A null return means cache miss — the caller should then fetch from network.
        /// </summary>
        Task<ISKDeepZoomTile?> TryGetAsync(SKDeepZoomTileId id, CancellationToken ct = default);

        /// <summary>Returns <see langword="true"/> if the tile is cached.</summary>
        bool Contains(SKDeepZoomTileId id);

        /// <summary>Stores a tile synchronously.</summary>
        void Put(SKDeepZoomTileId id, ISKDeepZoomTile? tile);

        /// <summary>
        /// Stores a tile. Implementations may apply delays or tiered writes.
        /// </summary>
        Task PutAsync(SKDeepZoomTileId id, ISKDeepZoomTile? tile, CancellationToken ct = default);

        /// <summary>Removes a specific tile from the cache.</summary>
        bool Remove(SKDeepZoomTileId id);

        /// <summary>Clears all cached tiles.</summary>
        void Clear();

        /// <summary>
        /// Disposes tiles evicted since the last call. Call once per render frame
        /// on the UI thread before drawing to safely release resources.
        /// </summary>
        void FlushEvicted();
    }
}
