#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Owns the complete fetch-and-persist pipeline for a single tile URL.
/// The controller asks for a tile by URL and receives a decoded tile (or null).
/// How the tile is obtained — from an HTTP server, a local file, a disk cache,
/// or browser storage — is an implementation detail invisible to the controller.
/// </summary>
/// <remarks>
/// <para>
/// Built-in implementations:
/// <list type="bullet">
///   <item><see cref="SKImagePyramidHttpTileProvider"/> — HTTP fetch with optional disk caching.</item>
///   <item><see cref="SKImagePyramidFileTileProvider"/> — reads from the local filesystem; no disk cache needed.</item>
/// </list>
/// </para>
/// <para>
/// Custom implementations (e.g. browser storage for Blazor WebAssembly) are supported by implementing
/// this interface directly.
/// </para>
/// </remarks>
public interface ISKImagePyramidTileProvider : IDisposable
{
    /// <summary>
    /// Returns a tile for the given URL, or <see langword="null"/> if unavailable (e.g. 404, missing file).
    /// </summary>
    /// <param name="url">The full URL or file path of the tile image.</param>
    /// <param name="ct">Cancellation token. Implementations must throw <see cref="OperationCanceledException"/> when cancelled.</param>
    Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default);
}
