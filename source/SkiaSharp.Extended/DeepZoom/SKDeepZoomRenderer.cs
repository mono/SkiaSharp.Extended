#nullable enable

using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// SkiaSharp implementation of <see cref="ISKDeepZoomRenderer"/>.
    /// Draws Deep Zoom tiles onto an <see cref="SKCanvas"/>.
    /// </summary>
    /// <remarks>
    /// Set <see cref="Canvas"/> before calling <see cref="SKDeepZoomController.Render()"/>.
    /// The rendering pipeline (two-pass LOD blending) is orchestrated by
    /// <see cref="SKDeepZoomController"/>; this class handles only the actual draw calls.
    /// </remarks>
    public class SKDeepZoomRenderer : ISKCanvasAwareRenderer
    {
        private readonly SKPaint _tilePaint;

        public SKDeepZoomRenderer()
        {
            _tilePaint = new SKPaint { IsAntialias = true };
        }

        /// <summary>
        /// The canvas to draw onto. Must be set before each render frame.
        /// </summary>
        public SKCanvas? Canvas { get; set; }

        /// <summary>
        /// Whether to enable LOD (Level-of-Detail) fallback blending.
        /// When <see langword="true"/> (default), lower-resolution parent tiles are drawn as
        /// placeholders while higher-resolution tiles are loading — the view is never blank.
        /// When <see langword="false"/>, missing tiles show as empty space until loaded.
        /// </summary>
        /// <remarks>
        /// <para><strong>LOD blending ON (default):</strong> as you zoom in, blurry ancestor
        /// tiles fill the screen immediately; they sharpen as hi-res tiles arrive. Ideal for
        /// interactive exploration and smooth UX.</para>
        /// <para><strong>LOD blending OFF:</strong> only tiles that are already cached at the
        /// exact requested level are drawn; everything else is blank. Better for
        /// scientific/medical imaging where placeholder blur could be misleading, or when
        /// you need to see precisely what has and has not loaded.</para>
        /// </remarks>
        public bool EnableLodBlending { get; set; } = true;

        // ---- ISKDeepZoomRenderer ----

        /// <inheritdoc />
        public void BeginRender()
        {
            Canvas?.Save();
        }

        /// <inheritdoc />
        public void DrawTile(SKDeepZoomRectF destRect, ISKDeepZoomTile tile)
        {
            if (Canvas == null) return;
            var bitmap = ((SKDeepZoomBitmapTile)tile).Bitmap;
            var src = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
            Canvas.DrawBitmap(bitmap, src, dest, _tilePaint);
        }

        /// <inheritdoc />
        public void DrawFallbackTile(SKDeepZoomRectF destRect, SKDeepZoomRectF sourceRect, ISKDeepZoomTile tile)
        {
            if (Canvas == null) return;
            var bitmap = ((SKDeepZoomBitmapTile)tile).Bitmap;
            var src  = new SKRect(sourceRect.X, sourceRect.Y, sourceRect.Right, sourceRect.Bottom);
            var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
            Canvas.DrawBitmap(bitmap, src, dest, _tilePaint);
        }

        /// <inheritdoc />
        public void EndRender()
        {
            Canvas?.Restore();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tilePaint.Dispose();
        }
    }
}
