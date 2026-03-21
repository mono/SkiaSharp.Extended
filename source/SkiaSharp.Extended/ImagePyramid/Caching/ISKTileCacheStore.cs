#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Platform-agnostic persistent tile storage keyed by string.
/// Implementations provide disk, browser, or database-backed stores.
/// </summary>
/// <remarks>
/// <para>
/// Built-in implementations:
/// <list type="bullet">
///   <item><see cref="SKDiskTileCacheStore"/> — filesystem with hashed filenames and expiry.</item>
///   <item><see cref="SKNullTileCacheStore"/> — no-op (disabled cache).</item>
///   <item><see cref="SKChainedTileCacheStore"/> — tries multiple stores in order.</item>
/// </list>
/// </para>
/// <para>
/// Custom implementations (e.g. IndexedDB for Blazor WASM, SQLite for mobile)
/// are supported by implementing this interface directly.
/// </para>
/// </remarks>
public interface ISKTileCacheStore : IDisposable
{
    /// <summary>
    /// Returns cached tile data, or <see langword="null"/> on a miss or expiry.
    /// </summary>
    Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Stores tile data. Implementations should be safe to call fire-and-forget.
    /// </summary>
    Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default);

    /// <summary>Removes a specific cache entry.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Clears all cached entries.</summary>
    Task ClearAsync(CancellationToken ct = default);
}
