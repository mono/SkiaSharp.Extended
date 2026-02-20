using System;
using System.Linq;
using SkiaSharp;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Core rendering engine for PivotViewer. Platform-agnostic — depends only on SkiaSharp.
    /// Renders grid view and histogram view (main content area only).
    /// UI chrome (control bar, filter pane, detail pane, sort dropdown) is handled by the host (MAUI/Blazor).
    /// </summary>
    public class PivotViewerRenderer : IDisposable
    {
        // --- Layout constants ---
        public const float ItemPadding = 8f;
        public const float TradingCardMinWidth = 150f;

        // --- Owned paint/font resources ---
        private readonly SKPaint _itemPaint;
        private readonly SKPaint _selectedPaint;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _labelPaint;
        private readonly SKPaint _cardBgPaint;
        private readonly SKPaint _cardBorderPaint;
        private readonly SKPaint _cardSepPaint;
        private readonly SKFont _textFont;
        private readonly SKFont _labelFont;

        private bool _disposed;

        public PivotViewerRenderer()
        {
            _itemPaint = new SKPaint();
            _selectedPaint = new SKPaint { IsStroke = true, StrokeWidth = 3 };
            _textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            _labelPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            _cardBgPaint = new SKPaint { Color = SKColors.White };
            _cardBorderPaint = new SKPaint { IsStroke = true, StrokeWidth = 1, Color = new SKColor(200, 200, 200) };
            _cardSepPaint = new SKPaint { Color = new SKColor(220, 220, 220), StrokeWidth = 1 };
            _textFont = new SKFont { Size = 12 };
            _labelFont = new SKFont { Size = 12 };
        }

        // =====================================================================
        // Main entry points
        // =====================================================================

        /// <summary>
        /// Renders the main content area: grid of items or histogram view.
        /// </summary>
        public void Render(
            SKCanvas canvas,
            SKImageInfo info,
            PivotViewerController controller,
            PivotViewerTheme theme,
            PivotViewerItem? hoverItem = null)
        {
            if (_disposed) return;

            canvas.Clear(SKColors.White);

            // Flush deferred tile disposals on the render thread
            controller.ImageProvider?.FlushEvictedTiles();

            controller.SetAvailableSize(info.Width, info.Height);

            if (controller.CurrentView == "graph" && controller.HistogramLayout != null)
            {
                RenderHistogramView(canvas, info, controller.HistogramLayout, theme, controller);
            }
            else if (controller.GridLayout != null)
            {
                RenderGridView(canvas, info, controller.GridLayout, theme, controller, hoverItem);
            }
            else
            {
                RenderNoResultsMessage(canvas, info.Width, info.Height, theme);
            }
        }

        /// <summary>
        /// Hit-tests a coordinate against the content area (grid items or histogram items).
        /// Returns a <see cref="RenderHitResult"/> the UI layer dispatches.
        /// </summary>
        public RenderHitResult HitTest(
            double viewX,
            double viewY,
            SKImageInfo info,
            PivotViewerController controller)
        {
            if (_disposed) return new RenderHitResult { Type = RenderHitType.None };

            double worldX = viewX - controller.PanOffsetX;
            double worldY = viewY - controller.PanOffsetY;

            // For graph view, reverse-transform from view coords to layout coords
            // (the renderer applies chartLeft/titleHeight offset + scaleX/scaleY)
            if (controller.CurrentView == "graph" && controller.HistogramLayout != null)
            {
                const float yAxisWidth = 40f;
                const float titleHeight = 24f;
                const float xAxisHeight = 20f;
                float chartLeft = yAxisWidth;
                float chartWidth = info.Width - yAxisWidth;
                float chartHeight = info.Height - xAxisHeight - titleHeight;
                float scaleX = chartWidth / Math.Max(1, info.Width);
                float scaleY = chartHeight / Math.Max(1, info.Height);

                double layoutX = (worldX - chartLeft) / scaleX;
                double layoutY = (worldY - titleHeight) / scaleY;

                var hitItem = controller.HitTest(layoutX, layoutY);
                if (hitItem != null)
                    return new RenderHitResult { Type = RenderHitType.Item, Item = hitItem };

                // Graph column label hit test
                if (controller.SortProperty != null && worldY >= info.Height - 30)
                {
                    foreach (var col in controller.HistogramLayout.Columns)
                    {
                        float colViewX = chartLeft + (float)col.X * scaleX;
                        float colViewW = (float)col.Width * scaleX;
                        if (worldX >= colViewX && worldX < colViewX + colViewW && col.Label != "(No value)")
                        {
                            return new RenderHitResult
                            {
                                Type = RenderHitType.GraphColumnLabel,
                                FilterPropertyId = controller.SortProperty.Id,
                                FilterValue = col.Label
                            };
                        }
                    }
                }
            }
            else
            {
                var hitItem = controller.HitTest(worldX, worldY);
                if (hitItem != null)
                    return new RenderHitResult { Type = RenderHitType.Item, Item = hitItem };
            }

            return RenderHitResult.None;
        }

        // =====================================================================
        // Grid View
        // =====================================================================

        private void RenderGridView(
            SKCanvas canvas, SKImageInfo info, GridLayout layout,
            PivotViewerTheme theme, PivotViewerController controller, PivotViewerItem? hoverItem)
        {
            _itemPaint.Color = theme.ItemFallbackColor;

            // Use interpolated positions during animation
            var positions = controller.LayoutTransition.IsAnimating
                ? controller.LayoutTransition.GetCurrentPositions()
                : layout.Positions;

            if (positions.Length == 0)
            {
                RenderNoResultsMessage(canvas, info.Width, info.Height, theme);
                return;
            }

            float panX = (float)controller.PanOffsetX;
            float panY = (float)controller.PanOffsetY;
            if (panX != 0 || panY != 0)
            {
                canvas.Save();
                canvas.Translate(panX, panY);
            }

            // Compute viewport for culling
            var viewportRect = new SKRect(-panX, -panY, info.Width - panX, info.Height - panY);

            foreach (var pos in positions)
            {
                // Cull off-screen items
                if ((float)(pos.X + pos.Width) < viewportRect.Left ||
                    (float)pos.X > viewportRect.Right ||
                    (float)(pos.Y + pos.Height) < viewportRect.Top ||
                    (float)pos.Y > viewportRect.Bottom)
                    continue;

                var rect = new SKRect(
                    (float)pos.X + 1, (float)pos.Y + 1,
                    (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

                // Skip degenerate rects (e.g. items animating out to zero size)
                if (rect.Width < 2 || rect.Height < 2)
                    continue;

                if (pos.Width >= TradingCardMinWidth)
                    RenderTradingCard(canvas, pos, rect, theme, controller);
                else if (TryRenderCustomTemplate(canvas, pos, rect, controller))
                    { /* custom template handled it */ }
                else
                    RenderThumbnailItem(canvas, pos, rect, theme, controller);

                // Selection highlight
                if (pos.Item == controller.SelectedItem)
                {
                    _selectedPaint.Color = theme.SelectionColor;
                    _selectedPaint.StrokeWidth = 3;
                    canvas.DrawRect(rect, _selectedPaint);

                    // Adorner info bar at bottom
                    if (pos.Height > 30)
                    {
                        float infoH = Math.Min(24f, (float)pos.Height * 0.3f);
                        var infoRect = new SKRect(rect.Left, rect.Bottom - infoH, rect.Right, rect.Bottom);
                        using var infoBg = new SKPaint { Color = new SKColor(0, 0, 0, 160) };
                        canvas.DrawRect(infoRect, infoBg);

                        var name = RenderUtils.GetItemDisplayName(pos.Item);
                        _textFont.Size = Math.Min(11, infoH - 4);
                        _textPaint.Color = theme.LightForegroundColor;
                        canvas.DrawText(name, infoRect.Left + 4, infoRect.Bottom - 4,
                            SKTextAlign.Left, _textFont, _textPaint);
                    }
                }

                // Hover highlight
                if (pos.Item == hoverItem && pos.Item != controller.SelectedItem)
                {
                    using var hoverPaint = new SKPaint { Color = theme.HoverColor };
                    canvas.DrawRect(rect, hoverPaint);
                }
            }

            if (panX != 0 || panY != 0)
                canvas.Restore();
        }

        // =====================================================================
        // Thumbnail and Trading Card items
        // =====================================================================

        /// <summary>
        /// Tries to render an item using a custom template from ItemTemplates.
        /// Returns true if a matching template with a RenderAction was found.
        /// </summary>
        private static bool TryRenderCustomTemplate(
            SKCanvas canvas, ItemPosition pos, SKRect rect, PivotViewerController controller)
        {
            var templates = controller.ItemTemplates;
            if (templates == null || templates.Count == 0) return false;

            var template = templates.SelectTemplate((int)pos.Width);
            if (template?.RenderAction == null) return false;

            template.RenderAction(canvas, pos.Item, rect);
            return true;
        }

        private void RenderThumbnailItem(
            SKCanvas canvas, ItemPosition pos, SKRect rect,
            PivotViewerTheme theme, PivotViewerController controller)
        {
            bool drewImage = false;
            var imgProvider = controller.ImageProvider;
            if (imgProvider != null)
            {
                var thumbnail = imgProvider.GetThumbnailForItem(pos.Item);
                if (thumbnail != null)
                {
                    var destRect = RenderUtils.FitUniform(thumbnail.Width, thumbnail.Height, rect);
                    canvas.DrawBitmap(thumbnail, destRect);
                    drewImage = true;
                }
            }

            if (!drewImage)
            {
                _itemPaint.Color = theme.ItemFallbackColor;
                canvas.DrawRect(rect, _itemPaint);
            }

            // Name label for items tall enough to show text
            if (pos.Height > 20)
            {
                var name = RenderUtils.GetItemDisplayName(pos.Item);
                _textFont.Size = Math.Min(12, (float)pos.Height / 4);
                _textPaint.Color = theme.LightForegroundColor;
                var textWidth = _textFont.MeasureText(name, out _);
                if (textWidth < rect.Width - 4)
                {
                    canvas.DrawText(name, rect.Left + 4, rect.Bottom - 4,
                        SKTextAlign.Left, _textFont, _textPaint);
                }
            }
        }

        private void RenderTradingCard(
            SKCanvas canvas, ItemPosition pos, SKRect rect,
            PivotViewerTheme theme, PivotViewerController controller)
        {
            canvas.DrawRect(rect, _cardBgPaint);
            canvas.DrawRect(rect, _cardBorderPaint);

            float padding = 4f;
            float imgHeight = rect.Height * 0.55f;
            var imgRect = new SKRect(
                rect.Left + padding, rect.Top + padding,
                rect.Right - padding, rect.Top + imgHeight);

            // Draw thumbnail
            bool drewImage = false;
            var imgProvider = controller.ImageProvider;
            if (imgProvider != null)
            {
                var thumbnail = imgProvider.GetThumbnailForItem(pos.Item);
                if (thumbnail != null)
                {
                    var destRect = RenderUtils.FitUniform(thumbnail.Width, thumbnail.Height, imgRect);
                    canvas.DrawBitmap(thumbnail, destRect);
                    drewImage = true;
                }
            }
            if (!drewImage)
            {
                _itemPaint.Color = theme.ItemFallbackColor;
                canvas.DrawRect(imgRect, _itemPaint);
            }

            // Facet values below image
            float textY = rect.Top + imgHeight + padding + 2;
            float fontSize = Math.Max(8f, Math.Min(11f, rect.Height * 0.04f));
            float lineHeight = fontSize + 3;
            float maxTextY = rect.Bottom - padding;

            // Item name as title
            var name = RenderUtils.GetItemDisplayName(pos.Item);
            if (textY + lineHeight < maxTextY)
            {
                _labelFont.Size = fontSize + 1;
                _labelPaint.Color = theme.ForegroundColor;
                var truncatedName = RenderUtils.TruncateText(name, _labelFont, rect.Width - padding * 2);
                canvas.DrawText(truncatedName, rect.Left + padding, textY + fontSize,
                    SKTextAlign.Left, _labelFont, _labelPaint);
                textY += lineHeight + 2;
            }

            // Separator line
            if (textY + 2 < maxTextY)
            {
                canvas.DrawLine(rect.Left + padding, textY, rect.Right - padding, textY, _cardSepPaint);
                textY += 4;
            }

            // Facet values — use dark text for readability (fixes white-on-white issue)
            _textFont.Size = fontSize;
            _labelFont.Size = fontSize;
            var properties = controller.Properties;
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    if (textY + lineHeight > maxTextY) break;
                    if (prop.IsPrivate) continue;

                    var values = pos.Item[prop.Id];
                    if (values == null || values.Count == 0) continue;

                    string valueStr = string.Join(", ", values.Take(3));
                    if (values.Count > 3)
                        valueStr += $" +{values.Count - 3} more";
                    string label = prop.DisplayName + ": ";

                    // Label
                    _labelPaint.Color = theme.ForegroundColor;
                    float labelWidth = _labelFont.MeasureText(label, out _);
                    canvas.DrawText(label, rect.Left + padding, textY + fontSize,
                        SKTextAlign.Left, _labelFont, _labelPaint);

                    // Value — use secondary foreground for contrast (not white)
                    float valueX = rect.Left + padding + labelWidth;
                    float availWidth = rect.Right - padding - valueX;
                    if (availWidth > 10)
                    {
                        using var valuePaint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };
                        string truncated = RenderUtils.TruncateText(valueStr, _textFont, availWidth);
                        canvas.DrawText(truncated, valueX, textY + fontSize,
                            SKTextAlign.Left, _textFont, valuePaint);
                    }

                    textY += lineHeight;
                }
            }
        }

        // =====================================================================
        // Histogram View
        // =====================================================================

        private void RenderHistogramView(
            SKCanvas canvas, SKImageInfo info, HistogramLayout layout,
            PivotViewerTheme theme, PivotViewerController controller)
        {
            _itemPaint.Color = theme.AccentColor;

            if (layout.Columns.Length == 0)
            {
                RenderNoResultsMessage(canvas, info.Width, info.Height, theme);
                return;
            }

            // Axis area constants
            const float yAxisWidth = 40f;
            const float xAxisHeight = 20f;
            const float titleHeight = 24f;
            float chartLeft = yAxisWidth;
            float chartBottom = info.Height - xAxisHeight;
            float chartWidth = info.Width - yAxisWidth;
            float chartHeight = info.Height - xAxisHeight - titleHeight;

            // Graph title (property name)
            if (!string.IsNullOrEmpty(layout.PropertyName))
            {
                _labelFont.Size = 13;
                using var titlePaint = new SKPaint { Color = theme.ForegroundColor, IsAntialias = true };
                canvas.DrawText(layout.PropertyName, info.Width / 2, 16,
                    SKTextAlign.Center, _labelFont, titlePaint);
            }

            // Compute max count for Y-axis scale
            int maxCount = 0;
            foreach (var col in layout.Columns)
            {
                if (col.Items.Length > maxCount)
                    maxCount = col.Items.Length;
            }

            // Y-axis labels and gridlines
            if (maxCount > 0)
            {
                int yTicks = Math.Min(5, maxCount);
                int yStep = Math.Max(1, (maxCount + yTicks - 1) / yTicks);
                using var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 };
                _textFont.Size = 9;
                using var yLabelPaint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };

                for (int i = 0; i <= yTicks; i++)
                {
                    int count = i * yStep;
                    if (count > maxCount) count = maxCount;
                    float yFrac = (float)count / maxCount;
                    float y = chartBottom - yFrac * chartHeight;
                    canvas.DrawLine(chartLeft, y, info.Width, y, gridPaint);
                    canvas.DrawText(count.ToString(), yAxisWidth - 4, y + 4,
                        SKTextAlign.Right, _textFont, yLabelPaint);
                    if (count >= maxCount) break;
                }
            }

            // Axis lines
            using (var axisPaint = new SKPaint { Color = new SKColor(180, 180, 180), StrokeWidth = 1 })
            {
                canvas.DrawLine(chartLeft, titleHeight, chartLeft, chartBottom, axisPaint);
                canvas.DrawLine(chartLeft, chartBottom, info.Width, chartBottom, axisPaint);
            }

            // Scale layout positions to chart area
            float scaleX = chartWidth / Math.Max(1, info.Width);
            float scaleY = chartHeight / Math.Max(1, info.Height);

            foreach (var col in layout.Columns)
            {
                float colX = chartLeft + (float)col.X * scaleX;
                float colW = (float)col.Width * scaleX;

                // Column label on X-axis
                _labelFont.Size = 11;
                _labelPaint.Color = theme.ForegroundColor;
                string label = col.Label;
                if (_labelFont.MeasureText(label, out _) > colW - 2)
                {
                    label = RenderUtils.TruncateText(label, _labelFont, colW - 2);
                }
                float labelX = colX + colW / 2;
                canvas.DrawText(label, labelX, info.Height - 4, SKTextAlign.Center, _labelFont, _labelPaint);

                // Item count above column
                if (col.Items.Length > 0)
                {
                    _textFont.Size = 10;
                    string countLabel = col.Items.Length.ToString();
                    float topItemY = titleHeight + col.Items.Min(p => (float)p.Y) * scaleY;
                    using var countPaint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };
                    canvas.DrawText(countLabel, labelX, topItemY - 4,
                        SKTextAlign.Center, _textFont, countPaint);
                }

                // Draw items
                foreach (var pos in col.Items)
                {
                    float px = chartLeft + (float)pos.X * scaleX + 1;
                    float py = titleHeight + (float)pos.Y * scaleY + 1;
                    float pw = (float)pos.Width * scaleX - 2;
                    float ph = (float)pos.Height * scaleY - 2;
                    var rect = new SKRect(px, py, px + pw, py + ph);

                    // Try thumbnail first, fall back to colored rectangle
                    bool drewImage = false;
                    var imgProvider = controller.ImageProvider;
                    if (imgProvider != null && pw > 4 && ph > 4)
                    {
                        var thumbnail = imgProvider.GetThumbnailForItem(pos.Item);
                        if (thumbnail != null)
                        {
                            var destRect = RenderUtils.FitUniform(thumbnail.Width, thumbnail.Height, rect);
                            canvas.DrawBitmap(thumbnail, destRect);
                            drewImage = true;
                        }
                    }
                    if (!drewImage)
                    {
                        canvas.DrawRect(rect, _itemPaint);
                    }

                    if (pos.Item == controller.SelectedItem)
                    {
                        _selectedPaint.Color = theme.SelectionColor;
                        canvas.DrawRect(rect, _selectedPaint);
                    }
                }
            }
        }

        // =====================================================================
        // No Results Message
        // =====================================================================

        private void RenderNoResultsMessage(SKCanvas canvas, float width, float height, PivotViewerTheme theme)
        {
            const string message = "No results found";
            _textFont.Size = 18;
            using var paint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };
            canvas.DrawText(message, width / 2, height / 2, SKTextAlign.Center, _textFont, paint);
        }

        // =====================================================================
        // IDisposable
        // =====================================================================

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _itemPaint.Dispose();
                _selectedPaint.Dispose();
                _textPaint.Dispose();
                _labelPaint.Dispose();
                _cardBgPaint.Dispose();
                _cardBorderPaint.Dispose();
                _cardSepPaint.Dispose();
                _textFont.Dispose();
                _labelFont.Dispose();
            }
            _disposed = true;
        }
    }
}
