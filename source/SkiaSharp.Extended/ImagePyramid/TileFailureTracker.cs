#nullable enable

using System;
using System.Collections.Concurrent;

namespace SkiaSharp.Extended;

/// <summary>
/// Tracks tile fetch failures with exponential backoff.
/// Replaces permanent blacklisting — transient failures are retried
/// after increasing delays, while tiles that exceed the retry limit
/// are treated as permanent failures until manually reset.
/// </summary>
public sealed class TileFailureTracker
{
    private readonly ConcurrentDictionary<SKImagePyramidTileId, FailureEntry> _failures = new();
    private readonly TimeSpan _baseDelay;
    private readonly int _maxRetries;

    /// <summary>
    /// Creates a failure tracker.
    /// </summary>
    /// <param name="baseDelay">Base delay before first retry. Doubles on each subsequent failure. Default: 5 seconds.</param>
    /// <param name="maxRetries">Maximum retry attempts before treating as permanent. Default: 3.</param>
    public TileFailureTracker(TimeSpan? baseDelay = null, int maxRetries = 3)
    {
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(5);
        _maxRetries = maxRetries > 0 ? maxRetries : 3;
    }

    /// <summary>Records a fetch failure for a tile.</summary>
    public void RecordFailure(SKImagePyramidTileId id)
    {
        _failures.AddOrUpdate(
            id,
            _ => new FailureEntry(1, DateTime.UtcNow),
            (_, existing) => new FailureEntry(existing.Count + 1, DateTime.UtcNow));
    }

    /// <summary>
    /// Returns <see langword="true"/> if the tile should be skipped
    /// (either permanent failure or still within backoff window).
    /// </summary>
    public bool ShouldSkip(SKImagePyramidTileId id)
    {
        if (!_failures.TryGetValue(id, out var entry))
            return false;

        if (entry.Count >= _maxRetries)
            return true;

        // Exponential backoff: baseDelay * 2^(attempts-1)
        var delay = TimeSpan.FromTicks(_baseDelay.Ticks * (1L << (entry.Count - 1)));
        return DateTime.UtcNow - entry.LastAttempt < delay;
    }

    /// <summary>Clears failure state for a specific tile.</summary>
    public void Reset(SKImagePyramidTileId id) => _failures.TryRemove(id, out _);

    /// <summary>Clears all failure state, allowing all tiles to be re-fetched.</summary>
    public void ResetAll() => _failures.Clear();

    /// <summary>Number of tiles currently tracked as failed.</summary>
    public int Count => _failures.Count;

    private readonly record struct FailureEntry(int Count, DateTime LastAttempt);
}
