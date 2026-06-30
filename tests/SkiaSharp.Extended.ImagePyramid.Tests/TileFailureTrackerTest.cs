#nullable enable

using System;
using System.Threading;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class TileFailureTrackerTest
{
    private static SKImagePyramidTileId Id(int level = 0, int col = 0, int row = 0) =>
        new SKImagePyramidTileId(level, col, row);

    [Fact]
    public void NewTracker_HasZeroCount()
    {
        var tracker = new TileFailureTracker();
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public void ShouldSkip_UnknownTile_ReturnsFalse()
    {
        var tracker = new TileFailureTracker();
        Assert.False(tracker.ShouldSkip(Id()));
    }

    [Fact]
    public void RecordFailure_IncreasesCount()
    {
        var tracker = new TileFailureTracker();
        tracker.RecordFailure(Id(0, 0, 0));
        tracker.RecordFailure(Id(0, 0, 1));
        Assert.Equal(2, tracker.Count);
    }

    [Fact]
    public void ShouldSkip_FirstFailure_TrueWithinBackoffWindow()
    {
        // baseDelay=5s means first backoff window is 5s — should skip immediately after failure
        var tracker = new TileFailureTracker(baseDelay: TimeSpan.FromSeconds(5));
        var id = Id();
        tracker.RecordFailure(id);
        Assert.True(tracker.ShouldSkip(id));
    }

    [Fact]
    public void ShouldSkip_AfterBackoffExpires_ReturnsFalse()
    {
        // Very short backoff — should not skip after delay
        var tracker = new TileFailureTracker(baseDelay: TimeSpan.FromMilliseconds(1));
        var id = Id();
        tracker.RecordFailure(id);
        Thread.Sleep(20); // wait for backoff window to expire
        Assert.False(tracker.ShouldSkip(id));
    }

    [Fact]
    public void ShouldSkip_AtMaxRetries_ReturnsTruePermanently()
    {
        var tracker = new TileFailureTracker(
            baseDelay: TimeSpan.FromMilliseconds(1),
            maxRetries: 3);
        var id = Id();

        // Record enough failures to exceed maxRetries, waiting between each to clear backoff
        for (int i = 0; i < 3; i++)
        {
            tracker.RecordFailure(id);
            Thread.Sleep(10);
        }

        // At maxRetries, should skip even after the window expires
        Thread.Sleep(50);
        Assert.True(tracker.ShouldSkip(id));
    }

    [Fact]
    public void Reset_ClearsSingleTile()
    {
        var tracker = new TileFailureTracker();
        var id = Id();
        tracker.RecordFailure(id);
        Assert.True(tracker.ShouldSkip(id));

        tracker.Reset(id);
        Assert.False(tracker.ShouldSkip(id));
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public void ResetAll_ClearsAll()
    {
        var tracker = new TileFailureTracker();
        tracker.RecordFailure(Id(0, 0, 0));
        tracker.RecordFailure(Id(0, 0, 1));
        tracker.RecordFailure(Id(0, 0, 2));
        Assert.Equal(3, tracker.Count);

        tracker.ResetAll();
        Assert.Equal(0, tracker.Count);
        Assert.False(tracker.ShouldSkip(Id(0, 0, 0)));
    }

    [Fact]
    public void ExponentialBackoff_SecondFailureHasLongerDelay()
    {
        // baseDelay=50ms → first backoff=50ms, second=100ms
        var tracker = new TileFailureTracker(baseDelay: TimeSpan.FromMilliseconds(50), maxRetries: 5);
        var id = Id();

        // First failure
        tracker.RecordFailure(id);
        Thread.Sleep(60); // wait past first backoff (50ms)
        Assert.False(tracker.ShouldSkip(id)); // backoff expired

        // Second failure — now backoff is 100ms
        tracker.RecordFailure(id);
        Thread.Sleep(60); // only 60ms, still within 100ms window
        Assert.True(tracker.ShouldSkip(id)); // still in backoff
    }
}
