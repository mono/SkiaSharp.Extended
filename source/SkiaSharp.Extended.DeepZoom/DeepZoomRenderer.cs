using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Renders Deep Zoom tiles onto an SKCanvas. Handles LOD blending and fallback rendering.
    /// Pure SkiaSharp — no MAUI dependency.
    /// </summary>
    public class DeepZoomRenderer : IDisposable
    {
        private readonly SKPaint _tilePaint;
        private readonly SKPaint _debugPaint;

        public DeepZoomRenderer()
        {
            _tilePaint = new SKPaint
            {
                FilterQuality = SKFilterQuality.High,
                IsAntialias = true,
            };
            _debugPaint = new SKPaint
            {
                Color = SKColors.Red.WithAlpha(128),
                IsStroke = true,
                StrokeWidth = 1,
                IsAntialias = true,
            };
        }

        /// <summary>
        /// Whether to draw tile boundaries for debugging.
        /// </summary>
        public bool ShowTileBorders { get; set; }

        /// <summary>
        /// Renders visible tiles onto the canvas using the current viewport state.
        /// </summary>
        public void Render(
            SKCanvas canvas,
            DziTileSource tileSource,
            Viewport viewport,
            TileCache cache,
            TileScheduler scheduler)
        {
            canvas.Save();

            var visibleTiles = scheduler.GetVisibleTiles(tileSource, viewport);

            foreach (var request in visibleTiles)
            {
                var tileId = request.TileId;
                cache.TryGet(tileId, out SKBitmap? bitmap);

                if (bitmap != null)
                {
                    DrawTile(canvas, tileSource, viewport, tileId, bitmap);
                }
                else
                {
                    // Try fallback: find the best parent tile
                    var fallback = scheduler.FindBestFallback(tileId, cache);
                    if (fallback.HasValue)
                    {
                        cache.TryGet(fallback.Value, out SKBitmap? parentBitmap);
                        if (parentBitmap != null)
                        {
                            DrawFallbackTile(canvas, tileSource, viewport, tileId,
                                fallback.Value, parentBitmap, scheduler);
                        }
                    }
                }
            }

            canvas.Restore();
        }

        /// <summary>
        /// Draws a single tile at its correct position on screen.
        /// </summary>
        private void DrawTile(
            SKCanvas canvas,
            DziTileSource tileSource,
            Viewport viewport,
            TileId tileId,
            SKBitmap bitmap)
        {
            var tileBounds = tileSource.GetTileBounds(tileId.Level, tileId.Col, tileId.Row);
            int levelWidth = tileSource.GetLevelWidth(tileId.Level);
            int levelHeight = tileSource.GetLevelHeight(tileId.Level);

            double imageLogicalHeight = 1.0 / tileSource.AspectRatio;

            // Tile bounds in logical coords
            double logicalLeft = (double)tileBounds.X / levelWidth;
            double logicalTop = (double)tileBounds.Y / levelHeight * imageLogicalHeight;
            double logicalRight = (double)(tileBounds.X + tileBounds.Width) / levelWidth;
            double logicalBottom = (double)(tileBounds.Y + tileBounds.Height) / levelHeight * imageLogicalHeight;

            // Logical to screen
            var topLeft = viewport.LogicalToElementPoint(logicalLeft, logicalTop);
            var bottomRight = viewport.LogicalToElementPoint(logicalRight, logicalBottom);

            var destRect = new SKRect(
                (float)topLeft.X, (float)topLeft.Y,
                (float)bottomRight.X, (float)bottomRight.Y);

            var srcRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);

            canvas.DrawBitmap(bitmap, srcRect, destRect, _tilePaint);

            if (ShowTileBorders)
            {
                canvas.DrawRect(destRect, _debugPaint);
            }
        }

        /// <summary>
        /// Draws a portion of a parent tile as a fallback for a missing tile.
        /// </summary>
        private void DrawFallbackTile(
            SKCanvas canvas,
            DziTileSource tileSource,
            Viewport viewport,
            TileId requested,
            TileId parent,
            SKBitmap parentBitmap,
            TileScheduler scheduler)
        {
            var (srcX, srcY, srcW, srcH) = scheduler.GetFallbackSourceRect(requested, parent, tileSource);
            var srcRect = new SKRect(srcX, srcY, srcX + srcW, srcY + srcH);

            // Get destination rect (same as where the requested tile would go)
            var tileBounds = tileSource.GetTileBounds(requested.Level, requested.Col, requested.Row);
            int levelWidth = tileSource.GetLevelWidth(requested.Level);
            int levelHeight = tileSource.GetLevelHeight(requested.Level);

            double imageLogicalHeight = 1.0 / tileSource.AspectRatio;

            double logicalLeft = (double)tileBounds.X / levelWidth;
            double logicalTop = (double)tileBounds.Y / levelHeight * imageLogicalHeight;
            double logicalRight = (double)(tileBounds.X + tileBounds.Width) / levelWidth;
            double logicalBottom = (double)(tileBounds.Y + tileBounds.Height) / levelHeight * imageLogicalHeight;

            var topLeft = viewport.LogicalToElementPoint(logicalLeft, logicalTop);
            var bottomRight = viewport.LogicalToElementPoint(logicalRight, logicalBottom);

            var destRect = new SKRect(
                (float)topLeft.X, (float)topLeft.Y,
                (float)bottomRight.X, (float)bottomRight.Y);

            canvas.DrawBitmap(parentBitmap, srcRect, destRect, _tilePaint);

            if (ShowTileBorders)
            {
                canvas.DrawRect(destRect, _debugPaint);
            }
        }

        public void Dispose()
        {
            _tilePaint.Dispose();
            _debugPaint.Dispose();
        }
    }
}
