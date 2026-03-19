#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Pluggable tile cache for the Deep Zoom rendering pipeline.
/// Tiles are stored as <see cref="SKImage"/> instances.
/// </summary>
public interface ISKImagePyramidTileCache : IDisposable
{
    /// <summary>Number of tiles currently in the cache.</summary>
    int Count { get; }

    /// <summary>Tries to retrieve a cached tile.</summary>
    bool TryGet(SKImagePyramidTileId id, out SKImage? tile);

    /// <summary>
    /// Asynchronously tries to retrieve a cached tile.
    /// A null return means cache miss — the caller should then fetch from network.
    /// </summary>
    Task<SKImage?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default);

    /// <summary>Returns <see langword="true"/> if the tile is cached.</summary>
    bool Contains(SKImagePyramidTileId id);

    /// <summary>Stores a tile synchronously.</summary>
    void Put(SKImagePyramidTileId id, SKImage? tile);

    /// <summary>
    /// Stores a tile. Implementations may apply delays or tiered writes.
    /// </summary>
    Task PutAsync(SKImagePyramidTileId id, SKImage? tile, CancellationToken ct = default);

    /// <summary>Removes a specific tile from the cache.</summary>
    bool Remove(SKImagePyramidTileId id);

    /// <summary>Clears all cached tiles.</summary>
    void Clear();

    /// <summary>
    /// Disposes tiles evicted since the last call. Call once per render frame
    /// on the UI thread before drawing to safely release resources.
    /// </summary>
    void FlushEvicted();
}
