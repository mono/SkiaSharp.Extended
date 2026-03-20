using SkiaSharp;
using SkiaSharp.Extended;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A tile cache decorator that adds artificial random delays to <see cref="PutAsync"/>
/// to simulate slow network or disk tile delivery. Use in sample pages to experiment
/// with progressive loading behaviour.
/// </summary>
public sealed class DelayTileCache(ISKImagePyramidTileCache inner) : ISKImagePyramidTileCache
{
    private readonly ISKImagePyramidTileCache _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Random _random = new();

    /// <summary>When true, a random delay between <see cref="MinDelayMs"/> and <see cref="MaxDelayMs"/> is applied before tiles are stored.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Minimum delay in milliseconds (0–2500).</summary>
    public int MinDelayMs { get; set; }

    /// <summary>Maximum delay in milliseconds (0–2500).</summary>
    public int MaxDelayMs { get; set; } = 500;

    public int Count => _inner.Count;

    public bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile) => _inner.TryGet(id, out tile);

    public Task<SKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
        => _inner.TryGetAsync(id, ct);

    public bool Contains(SKImagePyramidTileId id) => _inner.Contains(id);

    public void Put(SKImagePyramidTileId id, SKImagePyramidTile? tile) => _inner.Put(id, tile);

    public async Task PutAsync(SKImagePyramidTileId id, SKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (IsEnabled)
        {
            int min = Math.Min(MinDelayMs, MaxDelayMs);
            int max = Math.Max(MinDelayMs, MaxDelayMs);
            int delayMs = min == max ? min : _random.Next(min, max + 1);
            if (delayMs > 0)
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
        }
        await _inner.PutAsync(id, tile, ct).ConfigureAwait(false);
    }

    public bool Remove(SKImagePyramidTileId id) => _inner.Remove(id);

    public void Clear() => _inner.Clear();

    public void FlushEvicted() => _inner.FlushEvicted();

    public void Dispose() => _inner.Dispose();
}
