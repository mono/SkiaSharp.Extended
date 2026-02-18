using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SkiaSharp.Extended.UI.Maui.PivotViewer
{
    /// <summary>
    /// A MAUI control that provides the full PivotViewer experience:
    /// grid/graph views, filter pane, detail pane, and search.
    /// Wraps PivotViewerController with BindableProperties and gesture handling.
    /// </summary>
    public class SKPivotViewerView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly PivotViewerController _controller;
        private readonly SKPaint _itemPaint;
        private readonly SKPaint _selectedPaint;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _labelPaint;
        private readonly SKFont _textFont;
        private readonly SKFont _labelFont;
        private IDispatcherTimer? _animationTimer;
        private double _previousPanX;
        private double _previousPanY;
        private bool _disposed;

        public SKPivotViewerView()
        {
            _controller = new PivotViewerController();
            _itemPaint = new SKPaint { IsAntialias = true, Color = SKColors.CornflowerBlue };
            _selectedPaint = new SKPaint { IsAntialias = true, Color = SKColors.Orange, IsStroke = true, StrokeWidth = 3 };
            _textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
            _labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.DarkGray };
            _textFont = new SKFont { Size = 12 };
            _labelFont = new SKFont { Size = 14 };

            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnPaintSurface;
            Content = _canvasView;

            // Wire controller events
            _controller.LayoutUpdated += (s, e) =>
            {
                StartAnimationIfNeeded();
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            };
            _controller.SelectionChanged += (s, e) =>
            {
                SelectionChanged?.Invoke(this, e);
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            };
            _controller.FiltersChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);
            _controller.ViewChanged += (s, e) => ViewChanged?.Invoke(this, EventArgs.Empty);
            _controller.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, EventArgs.Empty);

            SetupGestures();
        }

        // --- BindableProperties ---

        public static readonly BindableProperty AccentColorProperty =
            BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(SKPivotViewerView),
                Colors.CornflowerBlue);

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<PivotViewerItem>), typeof(SKPivotViewerView),
                null, propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty PivotPropertiesProperty =
            BindableProperty.Create(nameof(PivotProperties), typeof(IEnumerable<PivotViewerProperty>), typeof(SKPivotViewerView),
                null, propertyChanged: OnPivotPropertiesChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(PivotViewerItem), typeof(SKPivotViewerView),
                null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SKPivotViewerView),
                -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

        public static readonly BindableProperty SortPivotPropertyProperty =
            BindableProperty.Create(nameof(SortPivotProperty), typeof(PivotViewerProperty), typeof(SKPivotViewerView),
                null, BindingMode.TwoWay, propertyChanged: OnSortPivotPropertyChanged);

        public static readonly BindableProperty ViewProperty =
            BindableProperty.Create(nameof(View), typeof(string), typeof(SKPivotViewerView),
                "grid", BindingMode.TwoWay, propertyChanged: OnViewChanged);

        public static readonly BindableProperty ItemCultureProperty =
            BindableProperty.Create(nameof(ItemCulture), typeof(CultureInfo), typeof(SKPivotViewerView),
                CultureInfo.CurrentCulture);

        /// <summary>Accent color for the UI chrome.</summary>
        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        /// <summary>Bind a collection of PivotViewerItem (SL5 binding model).</summary>
        public IEnumerable<PivotViewerItem>? ItemsSource
        {
            get => (IEnumerable<PivotViewerItem>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>Facet definitions for filtering and display.</summary>
        public new IEnumerable<PivotViewerProperty>? PivotProperties
        {
            get => (IEnumerable<PivotViewerProperty>?)GetValue(PivotPropertiesProperty);
            set => SetValue(PivotPropertiesProperty, value);
        }

        /// <summary>Currently selected item (two-way bindable).</summary>
        public new PivotViewerItem? SelectedItem
        {
            get => (PivotViewerItem?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>Index of the selected item (two-way bindable).</summary>
        public new int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        /// <summary>Active sort facet (two-way bindable).</summary>
        public PivotViewerProperty? SortPivotProperty
        {
            get => (PivotViewerProperty?)GetValue(SortPivotPropertyProperty);
            set => SetValue(SortPivotPropertyProperty, value);
        }

        /// <summary>Current view mode: "grid" or "graph" (two-way bindable).</summary>
        public new string View
        {
            get => (string)GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        /// <summary>Locale for date/number formatting.</summary>
        public CultureInfo ItemCulture
        {
            get => (CultureInfo)GetValue(ItemCultureProperty);
            set => SetValue(ItemCultureProperty, value);
        }

        /// <summary>Serialized filter state. Get to bookmark, set to restore.</summary>
        public string Filter => _controller.SerializeViewerState();

        /// <summary>Items currently in scope after filtering (read-only).</summary>
        public IReadOnlyList<PivotViewerItem> InScopeItems => _controller.InScopeItems;

        /// <summary>The underlying PivotViewer controller.</summary>
        public PivotViewerController Controller => _controller;

        // --- BindableProperty Change Handlers ---

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is IEnumerable<PivotViewerItem> items)
            {
                var props = view.PivotProperties ?? Enumerable.Empty<PivotViewerProperty>();
                view._controller.LoadItems(items, props);
                view._canvasView.InvalidateSurface();
            }
        }

        private static void OnPivotPropertiesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is IEnumerable<PivotViewerProperty> props)
            {
                var items = view.ItemsSource ?? Enumerable.Empty<PivotViewerItem>();
                view._controller.LoadItems(items, props);
                view._canvasView.InvalidateSurface();
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
            {
                view._controller.SelectedItem = newValue as PivotViewerItem;
            }
        }

        private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is int index)
            {
                view._controller.SelectedIndex = index;
            }
        }

        private static void OnSortPivotPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
            {
                view._controller.SortProperty = newValue as PivotViewerProperty;
            }
        }

        private static void OnViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is string viewMode)
            {
                view._controller.CurrentView = viewMode;
            }
        }

        // --- Events ---

        public event EventHandler<SkiaSharp.Extended.PivotViewer.SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? FilterChanged;
        public event EventHandler? ViewChanged;
        public event EventHandler? CollectionChanged;
        public event EventHandler<PivotViewerItem>? ItemDoubleClick;

        // --- Methods ---

        /// <summary>
        /// Loads a CXML collection.
        /// </summary>
        public void LoadCollection(CxmlCollectionSource source)
        {
            _controller.LoadCollection(source);
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Loads items directly (SL5 binding model).
        /// </summary>
        public void LoadItems(
            IEnumerable<PivotViewerItem> items,
            IEnumerable<PivotViewerProperty> properties)
        {
            _controller.LoadItems(items, properties);
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Invalidates the surface to trigger a redraw.
        /// </summary>
        public void InvalidateSurface() => _canvasView.InvalidateSurface();

        /// <summary>
        /// Serializes the current viewer state.
        /// </summary>
        public string SerializeViewerState() => _controller.SerializeViewerState();

        /// <summary>
        /// Restores viewer state from a serialized string.
        /// </summary>
        public void SetViewerState(string state)
        {
            _controller.SetViewerState(state);
            _canvasView.InvalidateSurface();
        }

        // --- Rendering ---

        // Layout constants
        private const float FilterPaneWidth = 220f;
        private const float DetailPaneWidth = 280f;
        private const float ControlBarHeight = 40f;
        private const float Padding = 8f;

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.White);

            // Layout regions
            float filterWidth = FilterPaneWidth;
            float detailWidth = _controller.DetailPane.IsShowing ? DetailPaneWidth : 0;
            float contentLeft = filterWidth;
            float contentWidth = info.Width - filterWidth - detailWidth;
            float contentTop = ControlBarHeight;
            float contentHeight = info.Height - ControlBarHeight;

            // Update controller with content area size
            _controller.SetAvailableSize(contentWidth, contentHeight);

            // Render control bar
            RenderControlBar(canvas, info, ControlBarHeight);

            // Render filter pane
            canvas.Save();
            canvas.ClipRect(new SKRect(0, ControlBarHeight, filterWidth, info.Height));
            RenderFilterPane(canvas, filterWidth, contentHeight, ControlBarHeight);
            canvas.Restore();

            // Render main content
            canvas.Save();
            canvas.ClipRect(new SKRect(contentLeft, contentTop, contentLeft + contentWidth, info.Height));
            canvas.Translate(contentLeft, contentTop);
            if (_controller.CurrentView == "graph" && _controller.HistogramLayout != null)
            {
                RenderHistogramView(canvas, new SKImageInfo((int)contentWidth, (int)contentHeight), _controller.HistogramLayout);
            }
            else if (_controller.GridLayout != null)
            {
                RenderGridView(canvas, new SKImageInfo((int)contentWidth, (int)contentHeight), _controller.GridLayout);
            }
            canvas.Restore();

            // Render detail pane
            if (_controller.DetailPane.IsShowing)
            {
                float detailLeft = info.Width - detailWidth;
                canvas.Save();
                canvas.ClipRect(new SKRect(detailLeft, ControlBarHeight, info.Width, info.Height));
                canvas.Translate(detailLeft, ControlBarHeight);
                RenderDetailPane(canvas, detailWidth, contentHeight);
                canvas.Restore();
            }
        }

        private void RenderGridView(SKCanvas canvas, SKImageInfo info, GridLayout layout)
        {
            var accentColor = ToSKColor(AccentColor);
            _itemPaint.Color = accentColor;

            // Use interpolated positions during animation
            var positions = _controller.LayoutTransition.IsAnimating
                ? _controller.LayoutTransition.GetCurrentPositions()
                : layout.Positions;

            // Apply pan offset
            float panX = (float)_controller.PanOffsetX;
            float panY = (float)_controller.PanOffsetY;
            if (panX != 0 || panY != 0)
            {
                canvas.Save();
                canvas.Translate(panX, panY);
            }

            foreach (var pos in positions)
            {
                var rect = new SKRect(
                    (float)pos.X + 1, (float)pos.Y + 1,
                    (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

                // Try to draw thumbnail from DZC
                bool drewImage = false;
                var imgProvider = _controller.ImageProvider;
                if (imgProvider != null)
                {
                    var thumbnail = imgProvider.GetThumbnailForItem(pos.Item);
                    if (thumbnail != null)
                    {
                        canvas.DrawBitmap(thumbnail, rect);
                        drewImage = true;
                    }
                }

                if (!drewImage)
                {
                    canvas.DrawRect(rect, _itemPaint);
                }

                // Draw item name if space allows
                if (pos.Height > 20)
                {
                    var name = GetItemDisplayName(pos.Item);
                    if (name != null)
                    {
                        _textFont.Size = Math.Min(12, (float)pos.Height / 4);
                        var textWidth = _textFont.MeasureText(name, out _);

                        if (textWidth < rect.Width - 4)
                        {
                            canvas.DrawText(name, rect.Left + 4, rect.Bottom - 4, SKTextAlign.Left, _textFont, _textPaint);
                        }
                    }
                }

                // Highlight selected item with adorner
                if (pos.Item == _controller.SelectedItem)
                {
                    _selectedPaint.Color = SKColors.Orange;
                    _selectedPaint.StrokeWidth = 3;
                    canvas.DrawRect(rect, _selectedPaint);

                    // Draw adorner info bar at bottom of selected item
                    if (pos.Height > 30)
                    {
                        float infoH = Math.Min(24f, (float)pos.Height * 0.3f);
                        var infoRect = new SKRect(rect.Left, rect.Bottom - infoH, rect.Right, rect.Bottom);
                        using var infoBg = new SKPaint { Color = new SKColor(0, 0, 0, 160) };
                        canvas.DrawRect(infoRect, infoBg);

                        var name = GetItemDisplayName(pos.Item);
                        if (name != null)
                        {
                            _textFont.Size = Math.Min(11, infoH - 4);
                            canvas.DrawText(name, infoRect.Left + 4, infoRect.Bottom - 4,
                                SKTextAlign.Left, _textFont, _textPaint);
                        }
                    }
                }
            }

            if (panX != 0 || panY != 0)
                canvas.Restore();
        }

        private void RenderHistogramView(SKCanvas canvas, SKImageInfo info, HistogramLayout layout)
        {
            var accentColor = ToSKColor(AccentColor);
            _itemPaint.Color = accentColor;

            foreach (var col in layout.Columns)
            {
                // Draw column label
                _labelFont.Size = 12;
                var textWidth = _labelFont.MeasureText(col.Label, out _);

                float labelX = (float)(col.X + (col.Width - textWidth) / 2);
                canvas.DrawText(col.Label, labelX, info.Height - 4, SKTextAlign.Left, _labelFont, _labelPaint);

                // Draw item count above column
                if (col.Items.Length > 0)
                {
                    _textFont.Size = 10;
                    string countLabel = col.Items.Length.ToString();
                    float topItemY = col.Items.Min(p => (float)p.Y);
                    using var countPaint = new SKPaint { Color = new SKColor(100, 100, 100) };
                    canvas.DrawText(countLabel,
                        (float)(col.X + col.Width / 2), topItemY - 4,
                        SKTextAlign.Center, _textFont, countPaint);
                }

                // Draw items
                foreach (var pos in col.Items)
                {
                    var rect = new SKRect(
                        (float)pos.X + 1, (float)pos.Y + 1,
                        (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

                    canvas.DrawRect(rect, _itemPaint);

                    if (pos.Item == _controller.SelectedItem)
                    {
                        _selectedPaint.Color = SKColors.Orange;
                        canvas.DrawRect(rect, _selectedPaint);
                    }
                }
            }
        }

        private static string? GetItemDisplayName(PivotViewerItem item)
        {
            var name = item["Name"];
            if (name != null && name.Count > 0)
                return name[0]?.ToString();
            return item.Id;
        }

        private static SKColor ToSKColor(Color color)
        {
            return new SKColor(
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255),
                (byte)(color.Alpha * 255));
        }

        // --- Control Bar ---

        private void RenderControlBar(SKCanvas canvas, SKImageInfo info, float barHeight)
        {
            using var barPaint = new SKPaint { Color = new SKColor(45, 45, 48) };
            canvas.DrawRect(0, 0, info.Width, barHeight, barPaint);

            _textFont.Size = 14;
            using var whitePaint = new SKPaint { Color = SKColors.White };

            // View switcher buttons
            float x = FilterPaneWidth + 10;
            string gridLabel = _controller.CurrentView == "grid" ? "▣ Grid" : "▢ Grid";
            canvas.DrawText(gridLabel, x, barHeight / 2 + 5, SKTextAlign.Left, _textFont, whitePaint);

            x += _textFont.MeasureText(gridLabel, out _) + 20;
            string graphLabel = _controller.CurrentView == "graph" ? "▣ Graph" : "▢ Graph";
            canvas.DrawText(graphLabel, x, barHeight / 2 + 5, SKTextAlign.Left, _textFont, whitePaint);

            // Item count
            float countX = info.Width - 150;
            string countText = $"{_controller.InScopeItems.Count} of {_controller.Items.Count} items";
            canvas.DrawText(countText, countX, barHeight / 2 + 5, SKTextAlign.Left, _textFont, whitePaint);

            // Sort indicator
            if (_controller.SortProperty != null)
            {
                float sortX = info.Width / 2;
                string sortText = $"Sort: {_controller.SortProperty.DisplayName}";
                canvas.DrawText(sortText, sortX, barHeight / 2 + 5, SKTextAlign.Center, _textFont, whitePaint);
            }
        }

        // --- Filter Pane ---

        private void RenderFilterPane(SKCanvas canvas, float width, float height, float topOffset)
        {
            using var bgPaint = new SKPaint { Color = new SKColor(240, 240, 240) };
            canvas.DrawRect(0, topOffset, width, height, bgPaint);

            var filterPane = _controller.FilterPaneModel;
            if (filterPane == null) return;

            var categories = filterPane.GetCategories(_controller.Items);
            float y = topOffset + Padding;
            float lineHeight = 20f;

            // "Clear All" button if any filters active
            if (filterPane.HasActiveFilters)
            {
                using var clearPaint = new SKPaint { Color = SKColors.CornflowerBlue };
                _textFont.Size = 12;
                canvas.DrawText("✕ Clear All Filters", Padding, y + 14, SKTextAlign.Left, _textFont, clearPaint);
                y += lineHeight + 4;
            }

            foreach (var category in categories)
            {
                if (y > topOffset + height) break;

                // Category header
                using var headerPaint = new SKPaint
                {
                    Color = category.IsFiltered ? SKColors.CornflowerBlue : SKColors.Black,
                };
                _textFont.Size = 13;
                canvas.DrawText(category.Property.DisplayName ?? category.Property.Id,
                    Padding, y + 14, SKTextAlign.Left, _textFont, headerPaint);
                y += lineHeight + 2;

                // Value list (for string/text categories)
                if (category.ValueCounts != null && category.Property.PropertyType == PivotViewerPropertyType.Text)
                {
                    _textFont.Size = 11;
                    int shown = 0;
                    foreach (var kv in category.ValueCounts.OrderByDescending(kv => kv.Value).Take(8))
                    {
                        if (y > topOffset + height) break;

                        bool isActive = category.ActiveFilters?.Contains(kv.Key) ?? false;
                        string checkbox = isActive ? "☑" : "☐";
                        string label = $"{checkbox} {kv.Key} ({kv.Value})";

                        using var valuePaint = new SKPaint
                        {
                            Color = isActive ? SKColors.CornflowerBlue : new SKColor(80, 80, 80)
                        };
                        canvas.DrawText(label, Padding + 8, y + 12, SKTextAlign.Left, _textFont, valuePaint);
                        y += lineHeight - 2;
                        shown++;
                    }

                    if (category.ValueCounts.Count > 8)
                    {
                        using var morePaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText($"  +{category.ValueCounts.Count - 8} more...",
                            Padding + 8, y + 12, SKTextAlign.Left, _textFont, morePaint);
                        y += lineHeight - 2;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.Decimal)
                {
                    // Numeric range — render a mini histogram
                    _textFont.Size = 11;
                    var numericValues = new List<double>();
                    foreach (var item in _controller.InScopeItems)
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

                    if (numericValues.Count > 0)
                    {
                        var buckets = HistogramBucketer.CreateNumericBuckets(numericValues);
                        float histH = 40f;
                        float barWidth = (width - Padding * 2 - 16) / Math.Max(1, buckets.Count);
                        int maxCount = buckets.Max(b => b.Count);

                        for (int i = 0; i < buckets.Count; i++)
                        {
                            float barHeight = maxCount > 0 ? (float)buckets[i].Count / maxCount * histH : 0;
                            float barX = Padding + 8 + i * barWidth;
                            float barY = y + histH - barHeight;

                            using var barPaint = new SKPaint { Color = new SKColor(100, 149, 237, 180) };
                            canvas.DrawRect(barX, barY, barWidth - 1, barHeight, barPaint);
                        }
                        y += histH + 4;

                        // Min/max label
                        using var rangePaint = new SKPaint { Color = new SKColor(120, 120, 120) };
                        string rangeLabel = $"{numericValues.Min():F0} – {numericValues.Max():F0}";
                        canvas.DrawText(rangeLabel, Padding + 8, y + 12, SKTextAlign.Left, _textFont, rangePaint);
                        y += lineHeight;
                    }
                    else
                    {
                        using var emptyPaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText("(no numeric data)", Padding + 8, y + 12, SKTextAlign.Left, _textFont, emptyPaint);
                        y += lineHeight;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.DateTime)
                {
                    // DateTime range — render mini histogram like numeric
                    _textFont.Size = 11;
                    var dateValues = new List<DateTime>();
                    foreach (var item in _controller.InScopeItems)
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
                        var buckets = HistogramBucketer.CreateDateTimeBuckets(dateValues);
                        float histH = 40f;
                        float barWidth = (width - Padding * 2 - 16) / Math.Max(1, buckets.Count);
                        int maxCount = buckets.Max(b => b.Count);

                        for (int i = 0; i < buckets.Count; i++)
                        {
                            float barHeight = maxCount > 0 ? (float)buckets[i].Count / maxCount * histH : 0;
                            float barX = Padding + 8 + i * barWidth;
                            float barY = y + histH - barHeight;

                            using var barPaint = new SKPaint { Color = new SKColor(144, 190, 109, 180) };
                            canvas.DrawRect(barX, barY, barWidth - 1, barHeight, barPaint);
                        }
                        y += histH + 4;

                        // Date range label
                        var minDate = dateValues.Min();
                        var maxDate = dateValues.Max();
                        using var rangePaint = new SKPaint { Color = new SKColor(120, 120, 120) };
                        string rangeLabel = $"{minDate:MMM yyyy} – {maxDate:MMM yyyy}";
                        canvas.DrawText(rangeLabel, Padding + 8, y + 12, SKTextAlign.Left, _textFont, rangePaint);
                        y += lineHeight;
                    }
                    else
                    {
                        using var emptyPaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText("(no date data)", Padding + 8, y + 12, SKTextAlign.Left, _textFont, emptyPaint);
                        y += lineHeight;
                    }
                }

                y += 6; // Spacing between categories
            }
        }

        // --- Detail Pane ---

        private void RenderDetailPane(SKCanvas canvas, float width, float height)
        {
            using var bgPaint = new SKPaint { Color = new SKColor(250, 250, 250) };
            canvas.DrawRect(0, 0, width, height, bgPaint);

            // Separator line
            using var sepPaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1 };
            canvas.DrawLine(0, 0, 0, height, sepPaint);

            var detail = _controller.DetailPane;
            var item = detail.SelectedItem;
            if (item == null) return;

            var defaults = _controller.DefaultDetails;
            float y = Padding;

            // Item name (respects DefaultDetails.IsNameHidden)
            if (!defaults.IsNameHidden)
            {
                var name = GetItemDisplayName(item) ?? "Unknown";
                _textFont.Size = 16;
                using var titlePaint = new SKPaint { Color = SKColors.Black };
                canvas.DrawText(name, Padding, y + 16, SKTextAlign.Left, _textFont, titlePaint);
                y += 28;
            }

            // Thumbnail
            var imgProvider = _controller.ImageProvider;
            if (imgProvider != null)
            {
                var thumbnail = imgProvider.GetThumbnailForItem(item);
                if (thumbnail != null)
                {
                    float thumbSize = Math.Min(width - 2 * Padding, 150);
                    float aspectRatio = (float)thumbnail.Width / thumbnail.Height;
                    float thumbW = thumbSize;
                    float thumbH = thumbSize / aspectRatio;
                    canvas.DrawBitmap(thumbnail, new SKRect(Padding, y, Padding + thumbW, y + thumbH));
                    y += thumbH + Padding;
                }
            }

            // Description (respects DefaultDetails.IsDescriptionHidden)
            if (!defaults.IsDescriptionHidden)
            {
                var descValues = item["Description"];
                if (descValues != null && descValues.Count > 0)
                {
                    var desc = descValues[0]?.ToString();
                    if (!string.IsNullOrEmpty(desc))
                    {
                        _textFont.Size = 11;
                        using var descPaint = new SKPaint { Color = new SKColor(80, 80, 80) };
                        // Truncate long descriptions
                        if (desc!.Length > 120) desc = desc.Substring(0, 117) + "...";
                        canvas.DrawText(desc, Padding, y + 12, SKTextAlign.Left, _textFont, descPaint);
                        y += 20;
                    }
                }
            }

            // Separator
            y += 4;
            canvas.DrawLine(Padding, y, width - Padding, y, sepPaint);
            y += 8;

            // Facet values (respects DefaultDetails.IsFacetCategoriesHidden)
            if (!defaults.IsFacetCategoriesHidden)
            {
                var facets = detail.FacetValues;
                _textFont.Size = 11;
                foreach (var facet in facets)
                {
                    if (y > height - 20) break;

                    // Property name
                    using var propPaint = new SKPaint { Color = new SKColor(100, 100, 100) };
                    canvas.DrawText(facet.DisplayName, Padding, y + 12, SKTextAlign.Left, _textFont, propPaint);
                    y += 16;

                    // Values
                    using var valuePaint = new SKPaint { Color = SKColors.Black };
                    foreach (var val in facet.Values.Take(3))
                    {
                        if (y > height - 20) break;
                        string displayVal = val.Length > 40 ? val.Substring(0, 37) + "..." : val;
                        canvas.DrawText(displayVal, Padding + 4, y + 12, SKTextAlign.Left, _textFont, valuePaint);
                        y += 16;
                    }

                    if (facet.Values.Count > 3)
                    {
                        using var morePaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText($"+{facet.Values.Count - 3} more",
                            Padding + 4, y + 12, SKTextAlign.Left, _textFont, morePaint);
                        y += 16;
                    }

                    y += 4;
                }
            }

            // Copyright (respects DefaultDetails.IsCopyrightHidden)
            if (!defaults.IsCopyrightHidden)
            {
                var copyright = _controller.CollectionSource?.Copyright;
                if (copyright != null && y < height - 30)
                {
                    y = height - 24;
                    _textFont.Size = 9;
                    using var copyPaint = new SKPaint { Color = SKColors.Gray };
                    var text = copyright.Text ?? "©";
                    canvas.DrawText(text, Padding, y + 10, SKTextAlign.Left, _textFont, copyPaint);
                }
            }
        }

        // --- Gestures ---

        private void SetupGestures()
        {
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapped;
            GestureRecognizers.Add(tapGesture);

            var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTapGesture.Tapped += OnDoubleTapped;
            GestureRecognizers.Add(doubleTapGesture);

            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinchGesture);

            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }

        private void OnTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (!point.HasValue) return;

            double x = point.Value.X;
            double y = point.Value.Y;

            // Control bar interactions
            if (y < ControlBarHeight)
            {
                HandleControlBarTap(x, y);
                return;
            }

            // Filter pane interactions
            if (x < FilterPaneWidth && y > ControlBarHeight)
            {
                HandleFilterPaneTap(x, y);
                return;
            }

            // Main content area — adjust for content region offset
            double contentX = x - FilterPaneWidth;
            double contentY = y - ControlBarHeight;

            var hit = _controller.HitTest(contentX - _controller.PanOffsetX, contentY - _controller.PanOffsetY);
            _controller.SelectedItem = hit;
            _canvasView.InvalidateSurface();
        }

        private void HandleControlBarTap(double x, double y)
        {
            // View switcher region (after filter pane width)
            if (x > FilterPaneWidth && x < FilterPaneWidth + 200)
            {
                // Toggle between grid and graph
                _textFont.Size = 14;
                string gridLabel = _controller.CurrentView == "grid" ? "▣ Grid" : "▢ Grid";
                float gridWidth = _textFont.MeasureText(gridLabel, out _);

                if (x < FilterPaneWidth + 10 + gridWidth + 10)
                {
                    _controller.CurrentView = "grid";
                }
                else
                {
                    _controller.CurrentView = "graph";
                }
                _canvasView.InvalidateSurface();
            }
        }

        private void HandleFilterPaneTap(double x, double y)
        {
            var filterPane = _controller.FilterPaneModel;
            if (filterPane == null) return;

            // Check "Clear All" button
            if (filterPane.HasActiveFilters && y < ControlBarHeight + Padding + 24)
            {
                filterPane.ClearAllFilters();
                _canvasView.InvalidateSurface();
                return;
            }

            // Calculate which filter value was tapped based on Y position
            var categories = filterPane.GetCategories(_controller.Items);
            float catY = ControlBarHeight + Padding;
            float lineHeight = 20f;

            if (filterPane.HasActiveFilters)
                catY += lineHeight + 4;

            foreach (var category in categories)
            {
                catY += lineHeight + 2; // Header

                if (category.ValueCounts != null && category.Property.PropertyType == PivotViewerPropertyType.Text)
                {
                    foreach (var kv in category.ValueCounts.OrderByDescending(kv => kv.Value).Take(8))
                    {
                        if (y >= catY && y < catY + lineHeight - 2)
                        {
                            filterPane.ToggleStringFilter(category.Property.Id, kv.Key);
                            _canvasView.InvalidateSurface();
                            return;
                        }
                        catY += lineHeight - 2;
                    }

                    if (category.ValueCounts.Count > 8)
                        catY += lineHeight - 2;
                }
                else
                {
                    catY += lineHeight;
                }

                catY += 6;
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.HasValue)
            {
                var hit = _controller.HitTest(point.Value.X, point.Value.Y);
                if (hit != null)
                    ItemDoubleClick?.Invoke(this, hit);
            }
        }

        private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Running:
                    _controller.ZoomAbout(e.Scale, Width / 2, Height / 2);
                    _canvasView.InvalidateSurface();
                    break;
            }
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _previousPanX = 0;
                    _previousPanY = 0;
                    break;
                case GestureStatus.Running:
                    double deltaX = e.TotalX - _previousPanX;
                    double deltaY = e.TotalY - _previousPanY;
                    _previousPanX = e.TotalX;
                    _previousPanY = e.TotalY;
                    _controller.Pan(deltaX, deltaY);
                    _canvasView.InvalidateSurface();
                    break;
            }
        }

        // --- Animation ---

        private void StartAnimationIfNeeded()
        {
            if (!_controller.LayoutTransition.IsAnimating) return;
            if (_animationTimer != null && _animationTimer.IsRunning) return;

            // Clean up old timer if any
            if (_animationTimer != null)
            {
                _animationTimer.Tick -= OnAnimationTick;
                _animationTimer.Stop();
            }

            _animationTimer = Dispatcher.CreateTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
            _animationTimer.Tick += OnAnimationTick;
            _animationTimer.Start();
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            bool needsRedraw = _controller.Update(TimeSpan.FromMilliseconds(16));
            _canvasView.InvalidateSurface();

            if (!needsRedraw && !_controller.LayoutTransition.IsAnimating)
            {
                _animationTimer?.Stop();
            }
        }

        // --- Disposal ---

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Tick -= OnAnimationTick;
            }
            _canvasView.PaintSurface -= OnPaintSurface;
            _controller.Dispose();
            _itemPaint.Dispose();
            _selectedPaint.Dispose();
            _textPaint.Dispose();
            _labelPaint.Dispose();
            _textFont.Dispose();
            _labelFont.Dispose();
        }
    }
}
