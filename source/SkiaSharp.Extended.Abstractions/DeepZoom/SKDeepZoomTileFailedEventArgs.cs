#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// Event args for when a tile fails to load.
/// </summary>
public class SKDeepZoomTileFailedEventArgs(SKDeepZoomTileId tileId, System.Exception exception) : System.EventArgs
{
    /// <summary>The tile that failed to load.</summary>
    public SKDeepZoomTileId TileId { get; } = tileId;

    /// <summary>The exception that caused the failure.</summary>
    public System.Exception Exception { get; } = exception;
}
