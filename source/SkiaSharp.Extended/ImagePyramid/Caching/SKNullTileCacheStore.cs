#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// A no-op cache store that never stores or returns anything.
/// Use for testing or when persistent caching is disabled.
/// </summary>
public sealed class SKNullTileCacheStore : ISKTileCacheStore
{
    /// <inheritdoc />
    public Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default)
        => Task.FromResult<SKImagePyramidTileData?>(null);

    /// <inheritdoc />
    public Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken ct = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public void Dispose() { }
}
