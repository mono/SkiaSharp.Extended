#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Supplies encoded tile bytes for a URL. Everything in the fetch/cache pipeline is a
/// provider: origins produce bytes (HTTP, file) and decorators wrap an inner provider to
/// add behaviour (caching, fallback, latency). Build a pipeline by nesting providers.
/// </summary>
/// <remarks>
/// <para>Built-in implementations:</para>
/// <list type="bullet">
///   <item><see cref="SKHttpTileProvider"/> — HTTP origin.</item>
///   <item><see cref="SKFileTileProvider"/> — local filesystem origin.</item>
///   <item><see cref="SKCompositeTileProvider"/> — tries inner providers in order.</item>
///   <item><see cref="SKCachedTileProvider"/> — base class for persistent cache decorators
///   such as <see cref="SKDiskCacheTileProvider"/>.</item>
/// </list>
/// <para>
/// Providers only ever deal in encoded bytes — decoding happens once, in
/// <see cref="SKImagePyramidController"/>. Return <see langword="null"/> for permanent
/// failures (404, missing file) and throw <see cref="OperationCanceledException"/> when
/// cancelled.
/// </para>
/// </remarks>
public interface ISKImagePyramidTileProvider : IDisposable
{
	/// <summary>
	/// Returns the encoded bytes for a tile, or <see langword="null"/> if the tile is
	/// unavailable (for example a 404 or a missing file).
	/// </summary>
	/// <param name="url">The full URL or file path of the tile image.</param>
	/// <param name="ct">
	/// A token to cancel the operation. Implementations throw
	/// <see cref="OperationCanceledException"/> when cancelled.
	/// </param>
	Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default);
}
