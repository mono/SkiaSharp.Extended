using SkiaSharp.Extended;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharpDemo;

/// <summary>
/// A tile provider decorator that adds an artificial random delay to <see cref="GetTileAsync"/>
/// to simulate slow network tile delivery. Use in sample pages to experiment with progressive
/// loading behaviour.
/// </summary>
public sealed class DelayTileProvider(ISKImagePyramidTileProvider inner) : ISKImagePyramidTileProvider
{
    private readonly ISKImagePyramidTileProvider _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Random _random = new();

    /// <summary>When true, a random delay between <see cref="MinDelayMs"/> and <see cref="MaxDelayMs"/> is applied before returning tiles.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Minimum delay in milliseconds.</summary>
    public int MinDelayMs { get; set; }

    /// <summary>Maximum delay in milliseconds.</summary>
    public int MaxDelayMs { get; set; } = 500;

    /// <inheritdoc/>
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        if (IsEnabled)
        {
            int min = Math.Min(MinDelayMs, MaxDelayMs);
            int max = Math.Max(MinDelayMs, MaxDelayMs);
            int delayMs = min == max ? min : _random.Next(min, max + 1);
            if (delayMs > 0)
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
        }
        return await _inner.GetTileAsync(url, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose() => _inner.Dispose();
}
