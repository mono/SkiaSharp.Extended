#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Pluggable renderer for Deep Zoom tiles.
    /// Implement this interface to customize how tiles are drawn onto the canvas
    /// (e.g., to add debug overlays, watermarks, or custom compositing).
    /// </summary>
    /// <remarks>
    /// The default implementation is <see cref="SKDeepZoomRenderer"/>, which supports
    /// LOD fallback blending. Use the decorator pattern to wrap it with additional behaviour.
    /// Tile geometry (dest rects, fallback source rects) is pre-calculated by
    /// <see cref="SKDeepZoomTileLayout"/> and passed to Render.
    /// </remarks>
    public interface ISKDeepZoomRenderer : System.IDisposable
    {
        /// <summary>
        /// Renders the visible Deep Zoom tiles onto <paramref name="canvas"/>.
        /// Called from the UI thread during the canvas paint callback.
        /// </summary>
        void Render(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            ISKDeepZoomTileCache cache,
            SKDeepZoomTileLayout layout);
    }
}
