#nullable enable

using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Renders Deep Zoom tiles onto an SKCanvas. Handles LOD blending and fallback rendering.
    /// </summary>
    public class SKDeepZoomRenderer : IDisposable
    {
        private readonly SKPaint _tilePaint;
        private readonly SKPaint _debugPaint;

        public SKDeepZoomRenderer()
        {
            _tilePaint = new SKPaint
            {
                IsAntialias = true,
            };
            _debugPaint = new SKPaint
            {
                Color = new SKColor(128, 128, 128, 80),
                IsStroke = true,
                StrokeWidth = 1,
                IsAntialias = false,
            };
        }

        /// <summary>
        /// Whether to draw tile boundaries for debugging.
        /// </summary>
        public bool ShowTileBorders { get; set; }

        /// <summary>
        /// Whether to enable LOD cross-fade blending when transitioning between tile levels.
        /// When enabled, fallback (lower-resolution) tiles are drawn underneath with fading opacity.
        /// Default is true (matches Silverlight behavior).
        /// </summary>
        public bool EnableLodBlending { get; set; } = true;

        /// <summary>
        /// Whether to render a debug statistics overlay showing viewport, level, cache, and tile info.
        /// </summary>
        public bool ShowDebugStats { get; set; }

        // Last render stats for the overlay
        private int _lastRenderedTiles;
        private int _lastVisibleTiles;
        private int _lastFallbackTiles;
        private int _lastOptimalLevel;

        // Reusable paints for stats overlay
        private SKPaint? _statsBgPaint;
        private SKPaint? _statsTextPaint;
        private SKFont? _statsFont;

        /// <summary>
        /// Renders visible tiles onto the canvas using the current viewport state.
        /// </summary>
        public void Render(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileCache cache,
            SKDeepZoomTileScheduler scheduler)
        {
            // Flush deferred bitmap disposals before rendering
            cache.FlushEvicted();

            canvas.Save();

            var visibleTiles = scheduler.GetVisibleTiles(tileSource, viewport);
            _lastVisibleTiles = visibleTiles.Count;
            _lastOptimalLevel = tileSource.GetOptimalLevel(viewport.ViewportWidth, viewport.ControlWidth);
            int rendered = 0;
            int fallbacks = 0;

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
                            {
                                DrawFallbackTile(canvas, tileSource, viewport, tileId,
                                    fallback.Value, parentBitmap, scheduler);
                                fallbacks++;
                            }
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
                    rendered++;
                }
                else if (!EnableLodBlending)
                {
                    // Single-pass fallback when blending is disabled
                    var fallback = scheduler.FindBestFallback(tileId, cache);
                    if (fallback.HasValue)
                    {
                        cache.TryGet(fallback.Value, out SKBitmap? parentBitmap);
                        if (parentBitmap != null)
                        {
                            DrawFallbackTile(canvas, tileSource, viewport, tileId,
                                fallback.Value, parentBitmap, scheduler);
                            fallbacks++;
                        }
                    }
                }
            }

            _lastRenderedTiles = rendered;
            _lastFallbackTiles = fallbacks;

            canvas.Restore();

            if (ShowDebugStats)
            {
                RenderStatsOverlay(canvas, tileSource, viewport, cache);
            }
        }

        /// <summary>
        /// Draws a single tile at its correct position on screen.
        /// </summary>
        private void DrawTile(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileId tileId,
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
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileId requested,
            SKDeepZoomTileId parent,
            SKBitmap parentBitmap,
            SKDeepZoomTileScheduler scheduler)
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

        /// <summary>
        /// Renders a semi-transparent debug overlay with viewport and tile statistics.
        /// </summary>
        private void RenderStatsOverlay(
            SKCanvas canvas,
            SKDeepZoomImageSource tileSource,
            SKDeepZoomViewport viewport,
            SKDeepZoomTileCache cache)
        {
            _statsBgPaint ??= new SKPaint { Color = new SKColor(0, 0, 0, 200) };
            _statsTextPaint ??= new SKPaint { Color = SKColors.White, IsAntialias = true };
            _statsFont ??= new SKFont { Size = 24 };

            double zoom = viewport.ViewportWidth > 0 ? 1.0 / viewport.ViewportWidth : 0;
            int levelW = tileSource.GetLevelWidth(_lastOptimalLevel);
            int levelH = tileSource.GetLevelHeight(_lastOptimalLevel);
            int tilesX = tileSource.GetTileCountX(_lastOptimalLevel);
            int tilesY = tileSource.GetTileCountY(_lastOptimalLevel);
            var (l, t, r, b) = viewport.GetLogicalBounds();

            var lines = new[]
            {
                $"VP Width: {viewport.ViewportWidth:F4}   Zoom: {zoom:F1}x",
                $"Origin: ({viewport.ViewportOriginX:F4}, {viewport.ViewportOriginY:F4})",
                $"Bounds: ({l:F3},{t:F3}) → ({r:F3},{b:F3})",
                $"Level: L{_lastOptimalLevel}  {levelW}×{levelH}  Grid: {tilesX}×{tilesY}",
                $"Tiles: {_lastRenderedTiles}/{_lastVisibleTiles} rendered   {_lastFallbackTiles} fallback",
                $"Cache: {cache.Count}   Image: {tileSource.ImageWidth}×{tileSource.ImageHeight}",
                $"Scale: {viewport.Scale:F0} px/lu   Control: {viewport.ControlWidth:F0}×{viewport.ControlHeight:F0}",
            };

            float lineHeight = 30;
            float padding = 10;
            float boxHeight = lines.Length * lineHeight + padding * 2;
            float boxWidth = 620;
            float x = (float)viewport.ControlWidth - boxWidth - 12;
            float y = 12;

            canvas.DrawRoundRect(x, y, boxWidth, boxHeight, 6, 6, _statsBgPaint);

            float textY = y + padding + 22;
            foreach (var line in lines)
            {
                canvas.DrawText(line, x + padding, textY, _statsFont, _statsTextPaint);
                textY += lineHeight;
            }
        }

        public void Dispose()
        {
            _tilePaint.Dispose();
            _debugPaint.Dispose();
            _statsBgPaint?.Dispose();
            _statsTextPaint?.Dispose();
            _statsFont?.Dispose();
        }
    }
}
