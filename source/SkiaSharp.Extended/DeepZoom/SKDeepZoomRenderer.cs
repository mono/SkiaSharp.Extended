#nullable enable

using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Renders Deep Zoom tiles onto an SKCanvas. Handles LOD blending and fallback rendering.
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
            SKDeepZoomTileScheduler scheduler)
        {
            // Flush deferred bitmap disposals before rendering
            cache.FlushEvicted();

            canvas.Save();

            var visibleTiles = scheduler.GetVisibleTiles(tileSource, viewport);

            // Pass 1: Draw fallback (lower-resolution) tiles for any missing tiles
            if (EnableLodBlending)
            {
                foreach (var request in visibleTiles)
                {
                    var tileId = request.TileId;
                    if (!cache.Contains(tileId))
                    {
                        var fallback = scheduler.FindBestFallback(tileId, cache);
                        if (fallback.HasValue)
                        {
                            cache.TryGet(fallback.Value, out SKBitmap? parentBitmap);
                            if (parentBitmap != null)
                                DrawFallbackTile(canvas, tileSource, viewport, tileId, fallback.Value, parentBitmap, scheduler);
                        }
                    }
                }
            }

            // Pass 2: Draw high-resolution tiles on top
            foreach (var request in visibleTiles)
            {
                var tileId = request.TileId;
                cache.TryGet(tileId, out SKBitmap? bitmap);

                if (bitmap != null)
                {
                    DrawTile(canvas, tileSource, viewport, tileId, bitmap);
                }
                else if (!EnableLodBlending)
                {
                    var fallback = scheduler.FindBestFallback(tileId, cache);
                    if (fallback.HasValue)
                    {
                        cache.TryGet(fallback.Value, out SKBitmap? parentBitmap);
                        if (parentBitmap != null)
                            DrawFallbackTile(canvas, tileSource, viewport, tileId, fallback.Value, parentBitmap, scheduler);
                    }
                }
            }

            canvas.Restore();
        }

        /// <summary>
        /// Computes the screen-space destination rect for a tile.
        /// Exposed as <see langword="internal"/> so decorator renderers in the same assembly
        /// can reuse the coordinate math without re-implementing it.
        /// </summary>
        public static SKRect GetTileDestRect(
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileId tileId)
        {
            var tileBounds = tileSource.GetTileBounds(tileId.Level, tileId.Col, tileId.Row);
            int levelWidth  = tileSource.GetLevelWidth(tileId.Level);
            int levelHeight = tileSource.GetLevelHeight(tileId.Level);

            double imageLogicalHeight = 1.0 / tileSource.AspectRatio;

            double logicalLeft   = (double)tileBounds.X / levelWidth;
            double logicalTop    = (double)tileBounds.Y / levelHeight * imageLogicalHeight;
            double logicalRight  = (double)(tileBounds.X + tileBounds.Width)  / levelWidth;
            double logicalBottom = (double)(tileBounds.Y + tileBounds.Height) / levelHeight * imageLogicalHeight;

            var topLeft     = viewport.LogicalToElementPoint(logicalLeft, logicalTop);
            var bottomRight = viewport.LogicalToElementPoint(logicalRight, logicalBottom);

            return new SKRect(
                (float)topLeft.X, (float)topLeft.Y,
                (float)bottomRight.X, (float)bottomRight.Y);
        }

        private void DrawTile(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileId tileId,
            SKBitmap bitmap)
        {
            var destRect = GetTileDestRect(tileSource, viewport, tileId);
            var srcRect  = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            canvas.DrawBitmap(bitmap, srcRect, destRect, _tilePaint);
        }

        private void DrawFallbackTile(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileId requested,
            SKDeepZoomTileId parent,
            SKBitmap parentBitmap,
            SKDeepZoomTileScheduler scheduler)
        {
            var (srcX, srcY, srcW, srcH) = scheduler.GetFallbackSourceRect(requested, parent, tileSource);
            var srcRect  = new SKRect(srcX, srcY, srcX + srcW, srcY + srcH);
            var destRect = GetTileDestRect(tileSource, viewport, requested);
            canvas.DrawBitmap(parentBitmap, srcRect, destRect, _tilePaint);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tilePaint.Dispose();
        }
    }
}
