#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tile data from a source (HTTP, file system, memory, etc.)
/// and returns it as an opaque <see cref="ISKImagePyramidTile"/>.
/// Implement this interface to plug in a custom tile source.
/// </summary>
public interface ISKImagePyramidTileFetcher : IDisposable
{
    /// <summary>
    /// Fetches a tile. Returns <see langword="null"/> when the tile is not available (e.g., 404).
    /// </summary>
    Task<ISKImagePyramidTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default);
}
