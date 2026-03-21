#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Returns decoded tiles to the controller. How the tile is obtained
/// (cache, network, filesystem) is an implementation detail.
/// </summary>
/// <remarks>
/// <para>
/// Built-in implementations:
/// <list type="bullet">
///   <item><see cref="SKTieredTileProvider"/> — composes a fetcher and optional persistent cache.</item>
/// </list>
/// </para>
/// <para>
/// Custom implementations are supported by implementing this interface directly.
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
