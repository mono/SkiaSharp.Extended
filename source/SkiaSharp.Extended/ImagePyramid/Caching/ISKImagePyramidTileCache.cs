#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Pluggable tile cache for the Deep Zoom rendering pipeline.
/// Tiles are stored as <see cref="SKImagePyramidTile"/> instances (image + raw bytes).
/// </summary>
public interface ISKImagePyramidTileCache : IDisposable
{
    /// <summary>Number of tiles currently in the cache.</summary>
    int Count { get; }

    /// <summary>
    /// The identifier of the currently active image source.
    /// Set by the controller when a new source is loaded via <c>Load()</c>.
    /// I/O-backed caches (e.g. filesystem, browser storage) use this to namespace tiles per source.
    /// In-memory caches may ignore this value.
    /// </summary>
    string? ActiveSourceId { get; set; }

    /// <summary>Tries to retrieve a cached tile synchronously. Only checks in-memory storage.</summary>
    bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile);

    /// <summary>
    /// Asynchronously tries to retrieve a cached tile, including any L2 storage (disk, browser).
    /// A null return means cache miss — the caller should then fetch from network.
    /// </summary>
    Task<SKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default);

    /// <summary>Returns <see langword="true"/> if the tile is in the in-memory cache.</summary>
    bool Contains(SKImagePyramidTileId id);

    /// <summary>Stores a tile synchronously (in-memory only).</summary>
    void Put(SKImagePyramidTileId id, SKImagePyramidTile? tile);

    /// <summary>
    /// Stores a tile, including any L2 storage (disk, browser). Implementations may apply async I/O.
    /// </summary>
    Task PutAsync(SKImagePyramidTileId id, SKImagePyramidTile? tile, CancellationToken ct = default);

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
