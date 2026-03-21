#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches raw encoded tile data from an origin source.
/// No caching logic — pure network, filesystem, or resource access.
/// </summary>
/// <remarks>
/// <para>
/// Built-in implementations:
/// <list type="bullet">
///   <item><see cref="SKHttpTileFetcher"/> — HTTP GET.</item>
///   <item><see cref="SKFileTileFetcher"/> — reads from the local filesystem.</item>
///   <item><see cref="SKCompositeTileFetcher"/> — tries multiple fetchers in order.</item>
/// </list>
/// </para>
/// <para>
/// Return <see langword="null"/> for permanent failures (404, missing file).
/// Throw <see cref="OperationCanceledException"/> when cancelled.
/// </para>
/// </remarks>
public interface ISKTileFetcher : IDisposable
{
    /// <summary>
    /// Fetches tile data for the given URL. Returns <see langword="null"/> if unavailable.
    /// </summary>
    /// <param name="url">The full URL or path of the tile image.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default);
}
