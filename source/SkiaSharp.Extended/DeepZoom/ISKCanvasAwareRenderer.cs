#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Optional interface for <see cref="ISKDeepZoomRenderer"/> implementations that render
    /// onto an <see cref="SKCanvas"/>. When a renderer implements this interface,
    /// <see cref="SKDeepZoomController.Render(SKCanvas)"/> will automatically set the canvas
    /// before calling <see cref="ISKDeepZoomRenderer.BeginRender"/>.
    /// </summary>
    public interface ISKCanvasAwareRenderer : ISKDeepZoomRenderer
    {
        /// <summary>The canvas to draw onto. Set before each render frame.</summary>
        SKCanvas? Canvas { get; set; }
    }
}
