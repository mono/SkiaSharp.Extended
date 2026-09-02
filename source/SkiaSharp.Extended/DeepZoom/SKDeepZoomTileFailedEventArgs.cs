#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Event args for when a tile fails to load.
    /// </summary>
    public class SKDeepZoomTileFailedEventArgs : System.EventArgs
    {
        /// <summary>Initializes a new <see cref="SKDeepZoomTileFailedEventArgs"/>.</summary>
        public SKDeepZoomTileFailedEventArgs(SKDeepZoomTileId tileId, System.Exception exception)
        {
            TileId = tileId;
            Exception = exception;
        }

        /// <summary>The tile that failed to load.</summary>
        public SKDeepZoomTileId TileId { get; }

        /// <summary>The exception that caused the failure.</summary>
        public System.Exception Exception { get; }
    }
}
