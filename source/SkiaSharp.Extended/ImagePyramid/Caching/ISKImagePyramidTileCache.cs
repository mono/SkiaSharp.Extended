#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// The internal sync render buffer used by <see cref="SKImagePyramidController"/>.
/// Stores hot decoded tiles for the render loop.
/// </summary>
/// <remarks>
/// This is a pure in-memory LRU cache — no persistence, no async. Disk caching and
/// other persistent strategies are the responsibility of <see cref="ISKImagePyramidTileProvider"/>
/// implementations, not this interface.
/// </remarks>
public interface ISKImagePyramidTileCache : IDisposable
{
    /// <summary>Number of tiles currently in the cache.</summary>
    int Count { get; }

    /// <summary>Returns <see langword="true"/> if the tile is present in the cache.</summary>
    bool Contains(SKImagePyramidTileId id);

    /// <summary>Tries to retrieve a cached tile. Returns <see langword="false"/> on a miss.</summary>
    bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile);

    /// <summary>Stores a tile in the cache.</summary>
    void Put(SKImagePyramidTileId id, SKImagePyramidTile tile);

    /// <summary>Removes a specific tile from the cache. Returns <see langword="false"/> if not found.</summary>
    bool Remove(SKImagePyramidTileId id);

    /// <summary>Clears all cached tiles, disposing their resources.</summary>
    void Clear();

    /// <summary>
    /// Disposes tiles evicted since the last call.
    /// Call once per render frame on the UI thread before drawing.
    /// </summary>
    void FlushEvicted();
}
