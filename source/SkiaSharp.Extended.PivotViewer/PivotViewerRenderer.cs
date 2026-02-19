using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Core rendering engine for PivotViewer. Platform-agnostic — depends only on SkiaSharp.
    /// Renders grid view, histogram view, filter pane, detail pane, control bar, and sort dropdown.
    /// Used by MAUI, Blazor, or any SkiaSharp host.
    /// </summary>
    public class PivotViewerRenderer : IDisposable
    {
        // --- Layout constants ---
        public const float FilterPaneWidth = 220f;
        public const float DetailPaneWidth = 280f;
        public const float ControlBarHeight = 40f;
        public const float ItemPadding = 8f;
        public const float TradingCardMinWidth = 150f;

        private const float LineHeight = 20f;
        private const float HistogramHeight = 40f;

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

        // --- Cached histogram data (invalidated when filters change) ---
        private readonly Dictionary<string, List<HistogramBucket<double>>> _numericHistogramCache = new();
        private readonly Dictionary<string, (List<HistogramBucket<DateTime>> Buckets, DateTime Min, DateTime Max)> _dateHistogramCache = new();

        // --- Detail pane hit rects (populated during RenderDetailPane, consumed by HitTest) ---
        private readonly List<(SKRect Bounds, Uri Href)> _detailLinkHitRects = new();
        private readonly List<(SKRect Bounds, string PropertyId, string Value)> _detailFacetHitRects = new();

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
        /// Renders the full PivotViewer surface: control bar, filter pane, content area, detail pane, and sort dropdown.
        /// </summary>
        public void Render(
            SKCanvas canvas,
            SKImageInfo info,
            PivotViewerController controller,
            PivotViewerTheme theme,
            PivotViewerViewState viewState)
        {
            if (_disposed) return;

            canvas.Clear(SKColors.White);

            // Flush deferred tile disposals on the render thread
            controller.ImageProvider?.FlushEvictedTiles();

            // Compute layout regions
            float filterWidth = viewState.IsFilterPaneVisible ? FilterPaneWidth : 0;
            float detailWidth = controller.DetailPane.IsShowing ? DetailPaneWidth : 0;
            float contentLeft = filterWidth;
            float contentWidth = Math.Max(0, info.Width - filterWidth - detailWidth);
            float contentTop = ControlBarHeight;
            float contentHeight = Math.Max(0, info.Height - ControlBarHeight);

            controller.SetAvailableSize(contentWidth, contentHeight);

            // Control bar
            RenderControlBar(canvas, info, theme, controller, viewState);

            // Filter pane
            if (viewState.IsFilterPaneVisible && filterWidth > 0)
            {
                canvas.Save();
                canvas.ClipRect(new SKRect(0, ControlBarHeight, filterWidth, info.Height));
                RenderFilterPane(canvas, filterWidth, contentHeight, ControlBarHeight, theme, controller, viewState);
                canvas.Restore();
            }

            // Main content area
            canvas.Save();
            canvas.ClipRect(new SKRect(contentLeft, contentTop, contentLeft + contentWidth, info.Height));
            canvas.Translate(contentLeft, contentTop);
            if (controller.CurrentView == "graph" && controller.HistogramLayout != null)
            {
                RenderHistogramView(canvas, new SKImageInfo((int)contentWidth, (int)contentHeight),
                    controller.HistogramLayout, theme, controller);
            }
            else if (controller.GridLayout != null)
            {
                RenderGridView(canvas, new SKImageInfo((int)contentWidth, (int)contentHeight),
                    controller.GridLayout, theme, controller, viewState);
            }
            canvas.Restore();

            // Detail pane
            if (controller.DetailPane.IsShowing)
            {
                float detailLeft = info.Width - detailWidth;
                canvas.Save();
                canvas.ClipRect(new SKRect(detailLeft, ControlBarHeight, info.Width, info.Height));
                canvas.Translate(detailLeft, ControlBarHeight);
                RenderDetailPane(canvas, detailWidth, contentHeight, theme, controller);
                canvas.Restore();
            }

            // Sort dropdown overlay (last so it draws on top)
            RenderSortDropdown(canvas, info, theme, controller, viewState);
        }

        /// <summary>
        /// Hit-tests a view-space coordinate against the rendered layout regions.
        /// Returns a <see cref="RenderHitResult"/> the UI layer dispatches.
        /// </summary>
        public RenderHitResult HitTest(
            double viewX,
            double viewY,
            SKImageInfo info,
            PivotViewerController controller,
            PivotViewerViewState viewState)
        {
            if (_disposed) return new RenderHitResult { Type = RenderHitType.None };

            float filterWidth = viewState.IsFilterPaneVisible ? FilterPaneWidth : 0;
            float detailWidth = controller.DetailPane.IsShowing ? DetailPaneWidth : 0;
            float contentWidth = Math.Max(0, info.Width - filterWidth - detailWidth);
            float contentHeight = Math.Max(0, info.Height - ControlBarHeight);

            // Sort dropdown overlay (highest priority)
            if (viewState.IsSortDropdownVisible)
            {
                var sortResult = HitTestSortDropdown(viewX, viewY, info, controller);
                if (sortResult.Type != RenderHitType.None)
                    return sortResult;
            }

            // Control bar
            if (viewY < ControlBarHeight)
                return HitTestControlBar(viewX, viewY, info, controller, viewState);

            // Filter pane
            if (viewState.IsFilterPaneVisible && viewX < filterWidth && viewY > ControlBarHeight)
                return HitTestFilterPane(viewX, viewY, controller, viewState);

            // Detail pane
            if (detailWidth > 0 && viewX > info.Width - detailWidth)
            {
                double localX = viewX - (info.Width - detailWidth);
                double localY = viewY - ControlBarHeight;
                return HitTestDetailPane(localX, localY);
            }

            // Main content area
            double contentX = viewX - filterWidth;
            double contentY = viewY - ControlBarHeight;
            double worldX = contentX - controller.PanOffsetX;
            double worldY = contentY - controller.PanOffsetY;

            var hitItem = controller.HitTest(worldX, worldY);
            if (hitItem != null)
                return new RenderHitResult { Type = RenderHitType.Item, Item = hitItem };

            // Graph column label hit test
            if (controller.CurrentView == "graph" && controller.HistogramLayout != null && controller.SortProperty != null)
            {
                if (worldY >= contentHeight - 30)
                {
                    foreach (var col in controller.HistogramLayout.Columns)
                    {
                        if (worldX >= col.X && worldX < col.X + col.Width && col.Label != "(No value)")
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

            return RenderHitResult.None;
        }

        /// <summary>
        /// Invalidates cached histogram data. Call when filters or in-scope items change.
        /// </summary>
        public void InvalidateHistogramCaches()
        {
            _numericHistogramCache.Clear();
            _dateHistogramCache.Clear();
        }

        // =====================================================================
        // Grid View
        // =====================================================================

        private void RenderGridView(
            SKCanvas canvas, SKImageInfo info, GridLayout layout,
            PivotViewerTheme theme, PivotViewerController controller, PivotViewerViewState viewState)
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

                if (pos.Width >= TradingCardMinWidth)
                    RenderTradingCard(canvas, pos, rect, theme, controller);
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
                if (pos.Item == viewState.HoverItem && pos.Item != controller.SelectedItem)
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
        // Control Bar
        // =====================================================================

        private void RenderControlBar(
            SKCanvas canvas, SKImageInfo info,
            PivotViewerTheme theme, PivotViewerController controller, PivotViewerViewState viewState)
        {
            using var barPaint = new SKPaint { Color = theme.ControlBackground };
            canvas.DrawRect(0, 0, info.Width, ControlBarHeight, barPaint);

            _textFont.Size = 14;
            using var lightPaint = new SKPaint { Color = theme.LightForegroundColor, IsAntialias = true };

            // Filter pane toggle
            float toggleWidth = 24;
            float x = (viewState.IsFilterPaneVisible ? FilterPaneWidth : toggleWidth) + 10;
            string toggleLabel = viewState.IsFilterPaneVisible ? "◀" : "▶";
            canvas.DrawText(toggleLabel, 6, ControlBarHeight / 2 + 5, SKTextAlign.Left, _textFont, lightPaint);

            // View switcher
            string gridLabel = controller.CurrentView == "grid" ? "▣ Grid" : "▢ Grid";
            canvas.DrawText(gridLabel, x, ControlBarHeight / 2 + 5, SKTextAlign.Left, _textFont, lightPaint);
            x += _textFont.MeasureText(gridLabel, out _) + 20;

            string graphLabel = controller.CurrentView == "graph" ? "▣ Graph" : "▢ Graph";
            canvas.DrawText(graphLabel, x, ControlBarHeight / 2 + 5, SKTextAlign.Left, _textFont, lightPaint);

            // Item count
            float countX = info.Width - 150;
            string countText = $"{controller.InScopeItems.Count} of {controller.Items.Count} items";
            canvas.DrawText(countText, countX, ControlBarHeight / 2 + 5, SKTextAlign.Left, _textFont, lightPaint);

            // Active filter breadcrumbs
            {
                var activeFilters = new List<string>();
                if (!string.IsNullOrEmpty(controller.SearchText))
                    activeFilters.Add($"\"{controller.SearchText}\"");

                var predicates = controller.FilterEngine.Predicates;
                foreach (var prop in controller.Properties)
                {
                    bool hasFilter = false;
                    for (int i = 0; i < predicates.Count; i++)
                    {
                        if (predicates[i].PropertyId == prop.Id)
                        {
                            hasFilter = true;
                            break;
                        }
                    }
                    if (hasFilter)
                        activeFilters.Add(prop.DisplayName ?? prop.Id);
                }

                if (activeFilters.Count > 0)
                {
                    float graphLabelWidth = _textFont.MeasureText(graphLabel, out _);
                    _textFont.Size = 11;
                    using var breadcrumbPaint = new SKPaint { Color = new SKColor(180, 200, 255), IsAntialias = true };
                    string breadcrumb = string.Join(" › ", activeFilters);
                    float bx = x + graphLabelWidth + 20;
                    float maxWidth = info.Width / 2 - bx - 60;
                    if (maxWidth > 40)
                    {
                        breadcrumb = RenderUtils.TruncateText(breadcrumb, _textFont, maxWidth);
                        canvas.DrawText(breadcrumb, bx, ControlBarHeight / 2 + 4,
                            SKTextAlign.Left, _textFont, breadcrumbPaint);
                    }
                    _textFont.Size = 14;
                }
            }

            // Sort indicator
            {
                float sortX = info.Width / 2;
                string arrow = controller.SortDescending ? "▲" : "▼";
                string sortText = controller.SortProperty != null
                    ? $"Sort: {controller.SortProperty.DisplayName} {arrow}"
                    : $"Sort {arrow}";
                canvas.DrawText(sortText, sortX, ControlBarHeight / 2 + 5, SKTextAlign.Center, _textFont, lightPaint);
            }
        }

        // =====================================================================
        // Sort Dropdown
        // =====================================================================

        private void RenderSortDropdown(
            SKCanvas canvas, SKImageInfo info,
            PivotViewerTheme theme, PivotViewerController controller, PivotViewerViewState viewState)
        {
            if (!viewState.IsSortDropdownVisible) return;

            var properties = controller.Properties;
            if (properties.Count == 0) return;

            float dropdownWidth = 200;
            float rowHeight = 28;
            float dropdownHeight = properties.Count * rowHeight + 8;
            float dropdownX = info.Width / 2 - dropdownWidth / 2;
            float dropdownY = ControlBarHeight;

            var renderRect = new SKRect(dropdownX, dropdownY, dropdownX + dropdownWidth, dropdownY + dropdownHeight);

            // Shadow + background
            using var shadowPaint = new SKPaint { Color = new SKColor(0, 0, 0, 60) };
            canvas.DrawRect(renderRect.Left + 2, renderRect.Top + 2, dropdownWidth, dropdownHeight, shadowPaint);
            using var bgPaint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(renderRect, bgPaint);
            using var borderPaint = new SKPaint { Color = new SKColor(180, 180, 180), IsStroke = true, StrokeWidth = 1 };
            canvas.DrawRect(renderRect, borderPaint);

            // Rows
            _textFont.Size = 13;
            using var textPaint = new SKPaint { Color = theme.ForegroundColor, IsAntialias = true };
            using var selectedTextPaint = new SKPaint { Color = theme.AccentColor, IsAntialias = true };
            using var hoverBgPaint = new SKPaint { Color = new SKColor(230, 240, 255) };

            float y = dropdownY + 4;
            foreach (var prop in properties)
            {
                bool isSelected = controller.SortProperty?.Id == prop.Id;
                if (isSelected)
                    canvas.DrawRect(dropdownX, y, dropdownWidth, rowHeight, hoverBgPaint);

                canvas.DrawText(prop.DisplayName ?? prop.Id, dropdownX + 10, y + rowHeight / 2 + 5,
                    SKTextAlign.Left, _textFont, isSelected ? selectedTextPaint : textPaint);
                y += rowHeight;
            }
        }

        // =====================================================================
        // Filter Pane
        // =====================================================================

        private void RenderFilterPane(
            SKCanvas canvas, float width, float height, float topOffset,
            PivotViewerTheme theme, PivotViewerController controller, PivotViewerViewState viewState)
        {
            using var bgPaint = new SKPaint { Color = theme.ControlBackground };
            canvas.DrawRect(0, topOffset, width, height, bgPaint);

            var filterPane = controller.FilterPaneModel;
            if (filterPane == null) return;

            // Apply scroll offset
            canvas.Save();
            canvas.Translate(0, -(float)viewState.FilterScrollOffset);

            var categories = filterPane.GetCategories(controller.SearchFilteredItems);
            float y = topOffset + ItemPadding;

            using var lightPaint = new SKPaint { Color = theme.LightForegroundColor, IsAntialias = true };
            using var accentPaint = new SKPaint { Color = theme.AccentColor, IsAntialias = true };
            using var dimPaint = new SKPaint { Color = new SKColor(80, 80, 80), IsAntialias = true };
            using var linkPaint = new SKPaint { Color = new SKColor(0, 102, 204), IsAntialias = true };
            using var rangePaint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };
            using var emptyPaint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };

            // "Clear All" button
            if (filterPane.HasActiveFilters)
            {
                _textFont.Size = 12;
                canvas.DrawText("✕ Clear All Filters", ItemPadding, y + 14, SKTextAlign.Left, _textFont, accentPaint);
                y += LineHeight + 4;
            }

            float scrollOffset = (float)viewState.FilterScrollOffset;
            foreach (var category in categories)
            {
                bool visible = y < topOffset + height + scrollOffset && y + LineHeight > topOffset;

                // Category header
                if (visible)
                {
                    _textFont.Size = 13;
                    var headerPaint = category.IsFiltered ? accentPaint : lightPaint;
                    canvas.DrawText(category.Property.DisplayName ?? category.Property.Id,
                        ItemPadding, y + 14, SKTextAlign.Left, _textFont, headerPaint);
                }
                y += LineHeight + 2;

                // String/Text/Link values — checkbox UI
                if (category.ValueCounts != null && (
                    category.Property.PropertyType == PivotViewerPropertyType.Text ||
                    category.Property.PropertyType == PivotViewerPropertyType.Link))
                {
                    _textFont.Size = 11;
                    bool isExpanded = viewState.ExpandedFilterCategories.Contains(category.Property.Id);
                    int maxVisible = isExpanded ? category.ValueCounts.Count : 8;

                    foreach (var kv in category.ValueCounts.OrderByDescending(kv => kv.Value).Take(maxVisible))
                    {
                        visible = y < topOffset + height + scrollOffset && y + LineHeight > topOffset;
                        if (visible)
                        {
                            bool isActive = category.ActiveFilters?.Contains(kv.Key) ?? false;
                            string checkbox = isActive ? "☑" : "☐";
                            string label = $"{checkbox} {kv.Key} ({kv.Value})";
                            var valuePaint = isActive ? accentPaint : dimPaint;
                            canvas.DrawText(label, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, valuePaint);
                        }
                        y += LineHeight - 2;
                    }

                    // "Show all N values..." / "Show less" toggle
                    if (category.ValueCounts.Count > 8)
                    {
                        visible = y < topOffset + height + scrollOffset && y + LineHeight > topOffset;
                        if (visible)
                        {
                            string toggleText = isExpanded
                                ? "  ▾ Show less"
                                : $"  ▸ Show all {category.ValueCounts.Count} values...";
                            canvas.DrawText(toggleText, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, linkPaint);
                        }
                        y += LineHeight - 2;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.Decimal)
                {
                    // Numeric mini histogram
                    _textFont.Size = 11;
                    if (!_numericHistogramCache.TryGetValue(category.Property.Id, out var buckets))
                    {
                        var numericValues = new List<double>();
                        foreach (var item in controller.InScopeItems)
                        {
                            var vals = item[category.Property.Id];
                            if (vals != null)
                            {
                                foreach (var v in vals)
                                {
                                    if (v is double d) numericValues.Add(d);
                                    else if (v is IConvertible c)
                                    {
                                        try { numericValues.Add(c.ToDouble(null)); } catch { }
                                    }
                                }
                            }
                        }
                        buckets = numericValues.Count > 0
                            ? HistogramBucketer.CreateNumericBuckets(numericValues)
                            : new List<HistogramBucket<double>>();
                        _numericHistogramCache[category.Property.Id] = buckets;
                    }

                    if (buckets.Count > 0)
                    {
                        float barWidth = (width - ItemPadding * 2 - 16) / Math.Max(1, buckets.Count);
                        int maxBucketCount = buckets.Max(b => b.Count);

                        for (int i = 0; i < buckets.Count; i++)
                        {
                            float barHeight = maxBucketCount > 0 ? (float)buckets[i].Count / maxBucketCount * HistogramHeight : 0;
                            float barX = ItemPadding + 8 + i * barWidth;
                            float barY = y + HistogramHeight - barHeight;
                            using var barPaint = new SKPaint { Color = new SKColor(theme.AccentColor.Red, theme.AccentColor.Green, theme.AccentColor.Blue, 180) };
                            canvas.DrawRect(barX, barY, barWidth - 1, barHeight, barPaint);
                        }
                        y += HistogramHeight + 4;

                        string rangeLabel = $"{buckets[0].Label} – {buckets[buckets.Count - 1].Label}";
                        canvas.DrawText(rangeLabel, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, rangePaint);
                        y += LineHeight;
                    }
                    else
                    {
                        canvas.DrawText("(no numeric data)", ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, emptyPaint);
                        y += LineHeight;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.DateTime)
                {
                    // DateTime mini histogram
                    _textFont.Size = 11;
                    if (!_dateHistogramCache.TryGetValue(category.Property.Id, out var dateData))
                    {
                        var dateValues = new List<DateTime>();
                        foreach (var item in controller.InScopeItems)
                        {
                            var vals = item[category.Property.Id];
                            if (vals != null)
                            {
                                foreach (var v in vals)
                                {
                                    if (v is DateTime dt) dateValues.Add(dt);
                                    else if (v is string s && DateTime.TryParse(s, out var parsed))
                                        dateValues.Add(parsed);
                                }
                            }
                        }

                        if (dateValues.Count > 0)
                        {
                            var dtBuckets = HistogramBucketer.CreateDateTimeBuckets(dateValues);
                            dateData = (dtBuckets, dateValues.Min(), dateValues.Max());
                        }
                        else
                        {
                            dateData = (new List<HistogramBucket<DateTime>>(), DateTime.MinValue, DateTime.MaxValue);
                        }
                        _dateHistogramCache[category.Property.Id] = dateData;
                    }

                    if (dateData.Buckets.Count > 0)
                    {
                        var dtBuckets = dateData.Buckets;
                        float barWidth = (width - ItemPadding * 2 - 16) / Math.Max(1, dtBuckets.Count);
                        int maxBucketCount = dtBuckets.Max(b => b.Count);

                        for (int i = 0; i < dtBuckets.Count; i++)
                        {
                            float barHeight = maxBucketCount > 0 ? (float)dtBuckets[i].Count / maxBucketCount * HistogramHeight : 0;
                            float barX = ItemPadding + 8 + i * barWidth;
                            float barY = y + HistogramHeight - barHeight;
                            using var barPaint = new SKPaint { Color = new SKColor(144, 190, 109, 180) };
                            canvas.DrawRect(barX, barY, barWidth - 1, barHeight, barPaint);
                        }
                        y += HistogramHeight + 4;

                        string rangeLabel = $"{dateData.Min:MMM yyyy} – {dateData.Max:MMM yyyy}";
                        canvas.DrawText(rangeLabel, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, rangePaint);
                        y += LineHeight;
                    }
                    else
                    {
                        canvas.DrawText("(no date data)", ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, emptyPaint);
                        y += LineHeight;
                    }
                }

                y += 6; // Spacing between categories
            }

            // Track total content height for scroll clamping
            viewState.FilterContentHeight = y - topOffset;
            canvas.Restore();
        }

        // =====================================================================
        // Detail Pane
        // =====================================================================

        private void RenderDetailPane(
            SKCanvas canvas, float width, float height,
            PivotViewerTheme theme, PivotViewerController controller)
        {
            _detailLinkHitRects.Clear();
            _detailFacetHitRects.Clear();

            using var bgPaint = new SKPaint { Color = theme.SecondaryBackground };
            canvas.DrawRect(0, 0, width, height, bgPaint);

            using var sepPaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1 };
            canvas.DrawLine(0, 0, 0, height, sepPaint);

            var detail = controller.DetailPane;
            var item = detail.SelectedItem;
            if (item == null) return;

            var defaults = controller.DefaultDetails;
            float y = ItemPadding;

            // Item name
            if (!defaults.IsNameHidden)
            {
                var name = RenderUtils.GetItemDisplayName(item);
                _textFont.Size = 16;
                using var titlePaint = new SKPaint { Color = theme.ForegroundColor, IsAntialias = true };
                canvas.DrawText(name, ItemPadding, y + 16, SKTextAlign.Left, _textFont, titlePaint);
                y += 28;
            }

            // Thumbnail
            var imgProvider = controller.ImageProvider;
            if (imgProvider != null)
            {
                var thumbnail = imgProvider.GetThumbnailForItem(item);
                if (thumbnail != null)
                {
                    float thumbSize = Math.Min(width - 2 * ItemPadding, 150);
                    float aspectRatio = (float)thumbnail.Width / thumbnail.Height;
                    float thumbW = thumbSize;
                    float thumbH = thumbSize / aspectRatio;
                    canvas.DrawBitmap(thumbnail, new SKRect(ItemPadding, y, ItemPadding + thumbW, y + thumbH));
                    y += thumbH + ItemPadding;
                }
            }

            // Description
            if (!defaults.IsDescriptionHidden)
            {
                var descValues = item["Description"];
                if (descValues != null && descValues.Count > 0)
                {
                    var desc = descValues[0]?.ToString();
                    if (!string.IsNullOrEmpty(desc))
                    {
                        _textFont.Size = 11;
                        using var descPaint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };
                        if (desc!.Length > 120) desc = desc.Substring(0, 117) + "...";
                        canvas.DrawText(desc, ItemPadding, y + 12, SKTextAlign.Left, _textFont, descPaint);
                        y += 20;
                    }
                }
            }

            // Separator
            y += 4;
            canvas.DrawLine(ItemPadding, y, width - ItemPadding, y, sepPaint);
            y += 8;

            // Facet values
            if (!defaults.IsFacetCategoriesHidden)
            {
                var facets = detail.FacetValues;
                _textFont.Size = 11;

                foreach (var facet in facets)
                {
                    if (y > height - 20) break;

                    // Property name
                    using var propPaint = new SKPaint { Color = theme.SecondaryForeground, IsAntialias = true };
                    canvas.DrawText(facet.DisplayName, ItemPadding, y + 12, SKTextAlign.Left, _textFont, propPaint);
                    y += 16;

                    // Values
                    bool isLinkType = facet.Property is PivotViewerLinkProperty;
                    bool isFilterable = facet.Property.Options.HasFlag(PivotViewerPropertyOptions.CanFilter);
                    var valueColor = isLinkType || isFilterable
                        ? new SKColor(0, 102, 204)
                        : theme.ForegroundColor;
                    using var valuePaint = new SKPaint { Color = valueColor, IsAntialias = true };

                    var rawValues = isLinkType ? item[facet.Property] : null;
                    int valIdx = 0;
                    foreach (var val in facet.Values.Take(3))
                    {
                        if (y > height - 20) break;
                        string displayVal = val.Length > 40 ? val.Substring(0, 37) + "..." : val;
                        float textWidth = _textFont.MeasureText(displayVal, out _);
                        canvas.DrawText(displayVal, ItemPadding + 4, y + 12, SKTextAlign.Left, _textFont, valuePaint);

                        if (isLinkType)
                        {
                            // Underline for links
                            canvas.DrawLine(ItemPadding + 4, y + 14, ItemPadding + 4 + textWidth, y + 14, valuePaint);
                            if (rawValues != null && valIdx < rawValues.Count && rawValues[valIdx] is PivotViewerHyperlink hl)
                            {
                                _detailLinkHitRects.Add((new SKRect(ItemPadding + 4, y, ItemPadding + 4 + textWidth, y + 16), hl.Uri));
                            }
                        }
                        else if (isFilterable && facet.Property.PropertyType == PivotViewerPropertyType.Text)
                        {
                            _detailFacetHitRects.Add((new SKRect(ItemPadding + 4, y, ItemPadding + 4 + textWidth, y + 16), facet.Property.Id, val));
                        }

                        y += 16;
                        valIdx++;
                    }

                    if (facet.Values.Count > 3)
                    {
                        using var morePaint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
                        canvas.DrawText($"+{facet.Values.Count - 3} more",
                            ItemPadding + 4, y + 12, SKTextAlign.Left, _textFont, morePaint);
                        y += 16;
                    }

                    y += 4;
                }
            }

            // Copyright
            if (!defaults.IsCopyrightHidden)
            {
                var copyright = controller.CollectionSource?.Copyright;
                if (copyright != null && y < height - 30)
                {
                    y = height - 24;
                    _textFont.Size = 9;
                    using var copyPaint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
                    canvas.DrawText(copyright.Text ?? "©", ItemPadding, y + 10, SKTextAlign.Left, _textFont, copyPaint);
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
        // Hit Test Helpers
        // =====================================================================

        private RenderHitResult HitTestControlBar(
            double x, double y, SKImageInfo info,
            PivotViewerController controller, PivotViewerViewState viewState)
        {
            // Filter pane toggle (left edge)
            if (x < 20)
                return new RenderHitResult { Type = RenderHitType.FilterToggle };

            // Sort dropdown region (center)
            float sortCenter = info.Width / 2f;
            if (x > sortCenter - 100 && x < sortCenter + 100)
                return new RenderHitResult { Type = RenderHitType.SortDropdown };

            // View switcher
            float cbFilterW = viewState.IsFilterPaneVisible ? FilterPaneWidth : 24;
            if (x > cbFilterW && x < cbFilterW + 200)
            {
                _textFont.Size = 14;
                string gridLabel = controller.CurrentView == "grid" ? "▣ Grid" : "▢ Grid";
                float gridWidth = _textFont.MeasureText(gridLabel, out _);

                if (x < cbFilterW + 10 + gridWidth + 10)
                    return new RenderHitResult { Type = RenderHitType.ViewGrid };
                else
                    return new RenderHitResult { Type = RenderHitType.ViewGraph };
            }

            return RenderHitResult.None;
        }

        private RenderHitResult HitTestSortDropdown(
            double x, double y, SKImageInfo info, PivotViewerController controller)
        {
            var properties = controller.Properties;
            if (properties.Count == 0) return RenderHitResult.None;

            float dropdownWidth = 200;
            float rowHeight = 28;
            float dropdownHeight = properties.Count * rowHeight + 8;
            float dropdownX = info.Width / 2 - dropdownWidth / 2;
            float dropdownY = ControlBarHeight;

            if (x >= dropdownX && x <= dropdownX + dropdownWidth &&
                y >= dropdownY && y <= dropdownY + dropdownHeight)
            {
                int index = (int)((y - dropdownY - 4) / rowHeight);
                if (index >= 0 && index < properties.Count)
                {
                    return new RenderHitResult
                    {
                        Type = RenderHitType.SortDropdownRow,
                        SortRowIndex = index
                    };
                }
            }

            return RenderHitResult.None;
        }

        private RenderHitResult HitTestFilterPane(
            double x, double y,
            PivotViewerController controller, PivotViewerViewState viewState)
        {
            var filterPane = controller.FilterPaneModel;
            if (filterPane == null) return RenderHitResult.None;

            double adjustedY = y + viewState.FilterScrollOffset;

            // "Clear All" button
            float clearAllStart = ControlBarHeight + ItemPadding;
            float clearAllEnd = clearAllStart + 24;
            if (filterPane.HasActiveFilters && adjustedY >= clearAllStart && adjustedY < clearAllEnd)
                return new RenderHitResult { Type = RenderHitType.ClearAllFilters };

            // Walk category layout to find hit target
            var categories = filterPane.GetCategories(controller.SearchFilteredItems);
            float catY = ControlBarHeight + ItemPadding;

            if (filterPane.HasActiveFilters)
                catY += LineHeight + 4;

            foreach (var category in categories)
            {
                catY += LineHeight + 2; // Header

                if (category.ValueCounts != null && (
                    category.Property.PropertyType == PivotViewerPropertyType.Text ||
                    category.Property.PropertyType == PivotViewerPropertyType.Link))
                {
                    bool isExpanded = viewState.ExpandedFilterCategories.Contains(category.Property.Id);
                    int maxVisible = isExpanded ? category.ValueCounts.Count : 8;

                    foreach (var kv in category.ValueCounts.OrderByDescending(kv => kv.Value).Take(maxVisible))
                    {
                        if (adjustedY >= catY && adjustedY < catY + LineHeight - 2)
                        {
                            return new RenderHitResult
                            {
                                Type = RenderHitType.FilterCheckbox,
                                FilterPropertyId = category.Property.Id,
                                FilterValue = kv.Key
                            };
                        }
                        catY += LineHeight - 2;
                    }

                    // "Show all" / "Show less" toggle
                    if (category.ValueCounts.Count > 8)
                    {
                        if (adjustedY >= catY && adjustedY < catY + LineHeight - 2)
                        {
                            return new RenderHitResult
                            {
                                Type = RenderHitType.FilterCategoryToggle,
                                CategoryName = category.Property.Id
                            };
                        }
                        catY += LineHeight - 2;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.Decimal)
                {
                    if (_numericHistogramCache.TryGetValue(category.Property.Id, out var buckets) && buckets.Count > 0)
                    {
                        float barWidth = (FilterPaneWidth - ItemPadding * 2 - 16) / Math.Max(1, buckets.Count);

                        if (adjustedY >= catY && adjustedY < catY + HistogramHeight)
                        {
                            int barIndex = (int)((x - ItemPadding - 8) / barWidth);
                            if (barIndex >= 0 && barIndex < buckets.Count)
                            {
                                return new RenderHitResult
                                {
                                    Type = RenderHitType.FilterNumericHistogramBar,
                                    FilterPropertyId = category.Property.Id,
                                    RangeMin = buckets[barIndex].Min,
                                    RangeMax = barIndex < buckets.Count - 1
                                        ? buckets[barIndex].Max - 1e-10
                                        : buckets[barIndex].Max
                                };
                            }
                        }
                        catY += HistogramHeight + 4;
                        catY += LineHeight; // range label
                    }
                    else
                    {
                        catY += LineHeight;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.DateTime)
                {
                    if (_dateHistogramCache.TryGetValue(category.Property.Id, out var dateData) && dateData.Buckets.Count > 0)
                    {
                        var dtBuckets = dateData.Buckets;
                        float barWidth = (FilterPaneWidth - ItemPadding * 2 - 16) / Math.Max(1, dtBuckets.Count);

                        if (adjustedY >= catY && adjustedY < catY + HistogramHeight)
                        {
                            int barIndex = (int)((x - ItemPadding - 8) / barWidth);
                            if (barIndex >= 0 && barIndex < dtBuckets.Count)
                            {
                                return new RenderHitResult
                                {
                                    Type = RenderHitType.FilterDateTimeHistogramBar,
                                    FilterPropertyId = category.Property.Id,
                                    DateRangeMin = dtBuckets[barIndex].Min,
                                    DateRangeMax = barIndex < dtBuckets.Count - 1
                                        ? dtBuckets[barIndex].Max.AddTicks(-1)
                                        : dtBuckets[barIndex].Max
                                };
                            }
                        }
                        catY += HistogramHeight + 4;
                        catY += LineHeight;
                    }
                    else
                    {
                        catY += LineHeight;
                    }
                }
                else
                {
                    catY += LineHeight;
                }

                catY += 6;
            }

            return RenderHitResult.None;
        }

        private RenderHitResult HitTestDetailPane(double localX, double localY)
        {
            // Check link hit rects (populated during RenderDetailPane)
            foreach (var (bounds, href) in _detailLinkHitRects)
            {
                if (localX >= bounds.Left && localX <= bounds.Right &&
                    localY >= bounds.Top && localY <= bounds.Bottom)
                {
                    return new RenderHitResult
                    {
                        Type = RenderHitType.DetailLink,
                        LinkUri = href.ToString()
                    };
                }
            }

            // Check facet value hit rects for tap-to-filter
            foreach (var (bounds, propertyId, value) in _detailFacetHitRects)
            {
                if (localX >= bounds.Left && localX <= bounds.Right &&
                    localY >= bounds.Top && localY <= bounds.Bottom)
                {
                    return new RenderHitResult
                    {
                        Type = RenderHitType.DetailFacetFilter,
                        FilterPropertyId = propertyId,
                        FilterValue = value
                    };
                }
            }

            return RenderHitResult.None;
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
