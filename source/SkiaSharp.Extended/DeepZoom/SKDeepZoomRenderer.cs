#nullable enable

using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Renders Deep Zoom tiles onto an SKCanvas. Handles LOD blending and fallback rendering.
    /// Geometry calculations (tile rects, fallback rects) are delegated to <see cref="SKDeepZoomTileLayout"/>.
    /// </summary>
    public class SKDeepZoomRenderer : ISKDeepZoomRenderer
    {
        private readonly SKPaint _tilePaint;

        public SKDeepZoomRenderer()
        {
            _tilePaint = new SKPaint
            {
                IsAntialias = true,
            };
        }

        /// <summary>
        /// Whether to enable LOD (Level-of-Detail) fallback blending.
        /// When <see langword="true"/> (default), lower-resolution parent tiles are drawn as
        /// placeholders while higher-resolution tiles are loading — the view is never blank.
        /// When <see langword="false"/>, missing tiles show as empty space until loaded.
        /// </summary>
        public bool EnableLodBlending { get; set; } = true;

        /// <inheritdoc />
        public void Render(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            ISKDeepZoomTileCache cache,
            SKDeepZoomTileLayout layout)
        {
            // Flush deferred bitmap disposals before rendering
            cache.FlushEvicted();

            canvas.Save();

            var visibleTiles = layout.GetVisibleTiles(tileSource, viewport);

            // Pass 1: Draw fallback (lower-resolution) tiles for any missing tiles
            if (EnableLodBlending)
            {
                foreach (var request in visibleTiles)
                {
                    var tileId = request.TileId;
                    if (!cache.Contains(tileId))
                    {
                        var fallback = layout.FindBestFallback(tileId, cache);
                        if (fallback.HasValue)
                        {
                            cache.TryGet(fallback.Value, out SKBitmap? parentBitmap);
                            if (parentBitmap != null)
                            {
                                var srcRectF = layout.GetFallbackSourceRect(tileId, fallback.Value, tileSource);
                                var srcRect  = new SKRect(srcRectF.X, srcRectF.Y, srcRectF.Right, srcRectF.Bottom);
                                var destRectF = layout.GetTileDestRect(tileSource, viewport, tileId);
                                var destRect  = new SKRect(destRectF.X, destRectF.Y, destRectF.Right, destRectF.Bottom);
                                canvas.DrawBitmap(parentBitmap, srcRect, destRect, _tilePaint);
                            }
                        }
                    }
                }
            }

            // Pass 2: Draw high-resolution tiles on top (missing tiles left blank when LOD blending is off)
            foreach (var request in visibleTiles)
            {
                var tileId = request.TileId;
                cache.TryGet(tileId, out SKBitmap? bitmap);

                if (bitmap != null)
                {
                    var destRectF = layout.GetTileDestRect(tileSource, viewport, tileId);
                    var srcRect  = new SKRect(0, 0, bitmap.Width, bitmap.Height);
                    var destRect = new SKRect(destRectF.X, destRectF.Y, destRectF.Right, destRectF.Bottom);
                    canvas.DrawBitmap(bitmap, srcRect, destRect, _tilePaint);
                }
                // When EnableLodBlending is false, missing tiles intentionally show as blank/white.
            }

            canvas.Restore();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tilePaint.Dispose();
        }
    }
}
