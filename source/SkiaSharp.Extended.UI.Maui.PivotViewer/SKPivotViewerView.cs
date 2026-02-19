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
        private readonly SKPaint _cardBgPaint;
        private readonly SKPaint _cardBorderPaint;
        private readonly SKPaint _cardSepPaint;
        private readonly SKFont _textFont;
        private readonly SKFont _labelFont;
        private bool _suppressPropertySync;
        private IDispatcherTimer? _animationTimer;
        private double _previousPanX;
        private double _previousPanY;
        private bool _disposed;
        private bool _isVisible = true;

        // Cached histogram data — invalidated when filters change
        private readonly Dictionary<string, List<HistogramBucket<double>>> _numericHistogramCache = new();
        private readonly Dictionary<string, (List<HistogramBucket<DateTime>> Buckets, DateTime Min, DateTime Max)> _dateHistogramCache = new();

        private Entry? _searchEntry;
        private float _filterScrollOffset;
        private float _filterContentHeight;
        private bool _isPanningFilterPane;
        private double _lastPointerX = double.NaN; // Track last known pointer X for pan origin detection
        private double _lastPivotPinchScale = 1.0;

        // Sort dropdown state
        private bool _showSortDropdown;
        private SKRect _sortDropdownRect;

        // Detail pane link hit regions (populated during RenderDetailPane)
        private List<(SKRect Bounds, Uri Href)> _detailLinkHitRects = new();
        // Detail pane facet value hit regions for tap-to-filter
        private List<(SKRect Bounds, string PropertyId, string Value)> _detailFacetHitRects = new();

        // Filter pane expanded categories (show all values instead of top 8)
        private readonly HashSet<string> _expandedFilterCategories = new();

        // Zoom slider
        private Slider? _zoomSlider;
        private bool _suppressZoomSliderUpdate;

        // Named event handlers for proper cleanup
        private EventHandler? _onLayoutUpdated;
        private EventHandler<SkiaSharp.Extended.PivotViewer.SelectionChangedEventArgs>? _onSelectionChanged;
        private EventHandler? _onFiltersChanged;
        private EventHandler? _onViewChanged;
        private EventHandler? _onSortPropertyChanged;
        private EventHandler? _onCollectionChanged;
        private EventHandler<ValueChangedEventArgs>? _onZoomSliderValueChanged;

        // Gesture recognizer references for disposal
        private TapGestureRecognizer? _tapGesture;
        private TapGestureRecognizer? _doubleTapGesture;
        private PinchGestureRecognizer? _pinchGesture;
        private PanGestureRecognizer? _panGesture;
        private PointerGestureRecognizer? _pointerGesture;
        private EventHandler<PointerEventArgs>? _onPointerPressed;

        public SKPivotViewerView()
        {
            _controller = new PivotViewerController();
            _itemPaint = new SKPaint { IsAntialias = true, Color = SKColors.CornflowerBlue };
            _selectedPaint = new SKPaint { IsAntialias = true, Color = SKColors.Orange, IsStroke = true, StrokeWidth = 3 };
            _textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
            _labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.DarkGray };
            _cardBgPaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill };
            _cardBorderPaint = new SKPaint { Color = new SKColor(200, 200, 200), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            _cardSepPaint = new SKPaint { Color = new SKColor(220, 220, 220), StrokeWidth = 1 };
            _textFont = new SKFont { Size = 12 };
            _labelFont = new SKFont { Size = 14 };

            _canvasView = new SKCanvasView();
            _canvasView.IgnorePixelScaling = true;
            _canvasView.PaintSurface += OnPaintSurface;
            _canvasView.EnableTouchEvents = true;
            _canvasView.Touch += OnCanvasTouch;

            // Search entry overlay for the filter pane
            _searchEntry = new Entry
            {
                Placeholder = "Search...",
                FontSize = 12,
                BackgroundColor = Colors.White,
                Margin = new Thickness(4, ControlBarHeight + 4, 0, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = FilterPaneWidth - 8,
                HeightRequest = 32,
            };
            _searchEntry.TextChanged += OnSearchTextChanged;

            _zoomSlider = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = 0,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = 150,
                HeightRequest = 30,
                Margin = new Thickness(0, 6, DetailPaneWidth + 10, 0),
            };
            _onZoomSliderValueChanged = (s, e) =>
            {
                if (_suppressZoomSliderUpdate) return;
                _controller.ZoomLevel = e.NewValue;
                _canvasView.InvalidateSurface();
            };
            _zoomSlider.ValueChanged += _onZoomSliderValueChanged;

            var grid = new Grid();
            grid.Children.Add(_canvasView);
            grid.Children.Add(_searchEntry);
            grid.Children.Add(_zoomSlider);
            Content = grid;

            // Wire controller events using named handlers for proper cleanup
            _onLayoutUpdated = (s, e) =>
            {
                StartAnimationIfNeeded();
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            };
            _controller.LayoutUpdated += _onLayoutUpdated;

            _onSelectionChanged = (s, e) =>
            {
                // Sync two-way bindable properties back from controller
                _suppressPropertySync = true;
                SetValue(SelectedItemProperty, _controller.SelectedItem);
                SetValue(SelectedIndexProperty, _controller.SelectedIndex);
                _suppressPropertySync = false;
                SelectionChanged?.Invoke(this, e);
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            };
            _controller.SelectionChanged += _onSelectionChanged;

            _onFiltersChanged = (s, e) =>
            {
                InvalidateHistogramCaches();
                FilterChanged?.Invoke(this, EventArgs.Empty);
            };
            _controller.FiltersChanged += _onFiltersChanged;

            _onViewChanged = (s, e) =>
            {
                _suppressPropertySync = true;
                SetValue(ViewProperty, _controller.CurrentView);
                _suppressPropertySync = false;
                ViewChanged?.Invoke(this, EventArgs.Empty);
            };
            _controller.ViewChanged += _onViewChanged;

            _onSortPropertyChanged = (s, e) =>
            {
                _suppressPropertySync = true;
                SetValue(SortPivotPropertyProperty, _controller.SortProperty);
                SetValue(SortDescendingProperty, _controller.SortDescending);
                _suppressPropertySync = false;
                SortPivotPropertyChanged?.Invoke(this, EventArgs.Empty);
            };
            _controller.SortPropertyChanged += _onSortPropertyChanged;

            _onCollectionChanged = (s, e) =>
            {
                InvalidateHistogramCaches();
                CollectionChanged?.Invoke(this, EventArgs.Empty);
            };
            _controller.CollectionChanged += _onCollectionChanged;

            SetupGestures();

            // Suppress animation when not visible
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        private void OnViewLoaded(object? sender, EventArgs e)
        {
            _isVisible = true;
        }
        private void OnViewUnloaded(object? sender, EventArgs e) { _isVisible = false; _animationTimer?.Stop(); }

        /// <summary>
        /// Handles keyboard input for navigation. Call from platform-specific key handlers.
        /// </summary>
        public void HandleKeyPress(string key)
        {
            switch (key)
            {
                case "Left":
                    _controller.SelectPrevious();
                    _canvasView.InvalidateSurface();
                    break;
                case "Right":
                    _controller.SelectNext();
                    _canvasView.InvalidateSurface();
                    break;
                case "Up":
                    _controller.SelectUp();
                    _canvasView.InvalidateSurface();
                    break;
                case "Down":
                    _controller.SelectDown();
                    _canvasView.InvalidateSurface();
                    break;
                case "Enter":
                case "Return":
                    if (_controller.SelectedItem != null)
                    {
                        var bounds = _controller.GetItemBounds(_controller.SelectedItem);
                        if (bounds.Width > 0)
                        {
                            // Add pan offsets to convert layout coords to screen coords
                            _controller.ZoomAbout(1.6,
                                bounds.X + bounds.Width / 2 + _controller.PanOffsetX,
                                bounds.Y + bounds.Height / 2 + _controller.PanOffsetY);
                            SyncZoomSlider();
                            _canvasView.InvalidateSurface();
                        }
                    }
                    break;
                case "Escape":
                    if (_controller.SelectedItem != null)
                        _controller.ClearSelection();
                    else
                        _controller.ZoomLevel = 0;
                    SyncZoomSlider();
                    _canvasView.InvalidateSurface();
                    break;
            }
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

        public static readonly BindableProperty SortDescendingProperty =
            BindableProperty.Create(nameof(SortDescending), typeof(bool), typeof(SKPivotViewerView),
                false, BindingMode.TwoWay, propertyChanged: OnSortDescendingChanged);

        public static readonly BindableProperty ViewProperty =
            BindableProperty.Create(nameof(View), typeof(string), typeof(SKPivotViewerView),
                "grid", BindingMode.TwoWay, propertyChanged: OnViewChanged);

        public static readonly BindableProperty ItemCultureProperty =
            BindableProperty.Create(nameof(ItemCulture), typeof(CultureInfo), typeof(SKPivotViewerView),
                CultureInfo.CurrentCulture);

        public static readonly BindableProperty ControlBackgroundProperty =
            BindableProperty.Create(nameof(ControlBackground), typeof(Color), typeof(SKPivotViewerView),
                Color.FromRgb(0xF0, 0xF0, 0xF0), propertyChanged: OnThemePropertyChanged);

        public static readonly BindableProperty SecondaryBackgroundProperty =
            BindableProperty.Create(nameof(SecondaryBackground), typeof(Color), typeof(SKPivotViewerView),
                Colors.White, propertyChanged: OnThemePropertyChanged);

        public static readonly BindableProperty SecondaryForegroundProperty =
            BindableProperty.Create(nameof(SecondaryForeground), typeof(Color), typeof(SKPivotViewerView),
                Color.FromRgb(0x66, 0x66, 0x66), propertyChanged: OnThemePropertyChanged);

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
        public IEnumerable<PivotViewerProperty>? PivotProperties
        {
            get => (IEnumerable<PivotViewerProperty>?)GetValue(PivotPropertiesProperty);
            set => SetValue(PivotPropertiesProperty, value);
        }

        /// <summary>Currently selected item (two-way bindable).</summary>
        public PivotViewerItem? SelectedItem
        {
            get => (PivotViewerItem?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>Index of the selected item (two-way bindable).</summary>
        public int SelectedIndex
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

        /// <summary>Sort in descending order (two-way bindable).</summary>
        public bool SortDescending
        {
            get => (bool)GetValue(SortDescendingProperty);
            set => SetValue(SortDescendingProperty, value);
        }

        /// <summary>Current view mode: "grid" or "graph" (two-way bindable).</summary>
        public string View
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

        /// <summary>Background color for the control chrome (filter pane, control bar).</summary>
        public Color ControlBackground
        {
            get => (Color)GetValue(ControlBackgroundProperty);
            set => SetValue(ControlBackgroundProperty, value);
        }

        /// <summary>Secondary background color (detail pane, dropdown).</summary>
        public Color SecondaryBackground
        {
            get => (Color)GetValue(SecondaryBackgroundProperty);
            set => SetValue(SecondaryBackgroundProperty, value);
        }

        /// <summary>Secondary foreground color (labels, hints).</summary>
        public Color SecondaryForeground
        {
            get => (Color)GetValue(SecondaryForegroundProperty);
            set => SetValue(SecondaryForegroundProperty, value);
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
            if (bindable is SKPivotViewerView view)
            {
                var items = newValue as IEnumerable<PivotViewerItem> ?? Enumerable.Empty<PivotViewerItem>();
                var props = view.PivotProperties ?? Enumerable.Empty<PivotViewerProperty>();
                view._controller.LoadItems(items, props);
                view._canvasView.InvalidateSurface();
            }
        }

        private static void OnPivotPropertiesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
            {
                var items = view.ItemsSource ?? Enumerable.Empty<PivotViewerItem>();
                var props = newValue as IEnumerable<PivotViewerProperty> ?? Enumerable.Empty<PivotViewerProperty>();
                view._controller.LoadItems(items, props);
                view._canvasView.InvalidateSurface();
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && !view._suppressPropertySync)
            {
                view._controller.SelectedItem = newValue as PivotViewerItem;
            }
        }

        private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && !view._suppressPropertySync && newValue is int index)
            {
                view._controller.SelectedIndex = index;
            }
        }

        private static void OnSortPivotPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && !view._suppressPropertySync)
            {
                view._controller.SortProperty = newValue as PivotViewerProperty;
            }
        }

        private static void OnSortDescendingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && !view._suppressPropertySync && newValue is bool desc)
            {
                view._controller.SortDescending = desc;
            }
        }

        private static void OnViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && !view._suppressPropertySync && newValue is string viewMode)
            {
                view._controller.CurrentView = viewMode;
            }
        }

        private static void OnThemePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
                view._canvasView?.InvalidateSurface();
        }

        private SKColor ToSkColor(Color color)
        {
            return new SKColor(
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255),
                (byte)(color.Alpha * 255));
        }

        // --- Events ---

        public event EventHandler<SkiaSharp.Extended.PivotViewer.SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? FilterChanged;
        public event EventHandler? ViewChanged;
        public event EventHandler? CollectionChanged;
        public event EventHandler? SortPivotPropertyChanged;
        public event EventHandler<PivotViewerItem>? ItemDoubleClick;

        /// <summary>Fired when a hyperlink in the detail pane is clicked.</summary>
        public event EventHandler<PivotViewerLinkEventArgs>? LinkClicked;

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
        private const float ItemPadding = 8f;
        private const float TradingCardMinWidth = 150f;

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isVisible) return;

            // Flush deferred tile bitmap disposals on the UI thread
            _controller.ImageProvider?.FlushEvictedTiles();

            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.White);

            // Layout regions — clamp to zero for narrow screens
            float filterWidth = FilterPaneWidth;
            float detailWidth = _controller.DetailPane.IsShowing ? DetailPaneWidth : 0;
            float contentLeft = filterWidth;
            float contentWidth = Math.Max(0, info.Width - filterWidth - detailWidth);
            float contentTop = ControlBarHeight;
            float contentHeight = Math.Max(0, info.Height - ControlBarHeight);

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

            // Sort dropdown overlay (rendered last so it draws on top)
            RenderSortDropdown(canvas, info);
        }

        private void RenderGridView(SKCanvas canvas, SKImageInfo info, GridLayout layout)
        {
            var accentColor = ToSKColor(AccentColor);
            _itemPaint.Color = accentColor;

            // Use interpolated positions during animation
            var positions = _controller.LayoutTransition.IsAnimating
                ? _controller.LayoutTransition.GetCurrentPositions()
                : layout.Positions;

            // Show "no results" when filters produce 0 items
            if (positions.Length == 0)
            {
                RenderNoResultsMessage(canvas, info.Width, info.Height);
                return;
            }

            // Apply pan offset
            float panX = (float)_controller.PanOffsetX;
            float panY = (float)_controller.PanOffsetY;
            if (panX != 0 || panY != 0)
            {
                canvas.Save();
                canvas.Translate(panX, panY);
            }

            // Compute visible viewport for culling off-screen items
            var viewportRect = new SKRect(-panX, -panY, info.Width - panX, info.Height - panY);

            foreach (var pos in positions)
            {
                // Skip items that are entirely outside the visible viewport
                if ((float)(pos.X + pos.Width) < viewportRect.Left ||
                    (float)pos.X > viewportRect.Right ||
                    (float)(pos.Y + pos.Height) < viewportRect.Top ||
                    (float)pos.Y > viewportRect.Bottom)
                    continue;

                var rect = new SKRect(
                    (float)pos.X + 1, (float)pos.Y + 1,
                    (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

                // Use trading card layout when items are large enough
                if (pos.Width >= TradingCardMinWidth)
                {
                    RenderTradingCard(canvas, pos, rect);
                }
                else
                {
                    RenderThumbnailItem(canvas, pos, rect);
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

        private void RenderThumbnailItem(SKCanvas canvas, ItemPosition pos, SKRect rect)
        {
            bool drewImage = false;
            var imgProvider = _controller.ImageProvider;
            if (imgProvider != null)
            {
                var thumbnail = imgProvider.GetThumbnailForItem(pos.Item);
                if (thumbnail != null)
                {
                    var destRect = FitUniform(thumbnail.Width, thumbnail.Height, rect);
                    canvas.DrawBitmap(thumbnail, destRect);
                    drewImage = true;
                }
            }

            if (!drewImage)
            {
                canvas.DrawRect(rect, _itemPaint);
            }

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
        }

        private void RenderTradingCard(SKCanvas canvas, ItemPosition pos, SKRect rect)
        {
            canvas.DrawRect(rect, _cardBgPaint);
            canvas.DrawRect(rect, _cardBorderPaint);

            float padding = 4f;
            float imgHeight = rect.Height * 0.55f; // Thumbnail gets ~55% of card
            var imgRect = new SKRect(
                rect.Left + padding, rect.Top + padding,
                rect.Right - padding, rect.Top + imgHeight);

            // Draw thumbnail
            bool drewImage = false;
            var imgProvider = _controller.ImageProvider;
            if (imgProvider != null)
            {
                var thumbnail = imgProvider.GetThumbnailForItem(pos.Item);
                if (thumbnail != null)
                {
                    var destRect = FitUniform(thumbnail.Width, thumbnail.Height, imgRect);
                    canvas.DrawBitmap(thumbnail, destRect);
                    drewImage = true;
                }
            }
            if (!drewImage)
            {
                canvas.DrawRect(imgRect, _itemPaint);
            }

            // Draw facet values below the image
            float textY = rect.Top + imgHeight + padding + 2;
            float fontSize = Math.Max(8f, Math.Min(11f, rect.Height * 0.04f));
            float lineHeight = fontSize + 3;
            float maxTextY = rect.Bottom - padding;

            // Item name as title
            var name = GetItemDisplayName(pos.Item);
            if (name != null && textY + lineHeight < maxTextY)
            {
                _labelFont.Size = fontSize + 1;
                var truncatedName = TruncateText(name, _labelFont, rect.Width - padding * 2);
                canvas.DrawText(truncatedName, rect.Left + padding, textY + fontSize,
                    SKTextAlign.Left, _labelFont, _labelPaint);
                textY += lineHeight + 2;
            }

            // Draw separator line
            if (textY + 2 < maxTextY)
            {
                canvas.DrawLine(rect.Left + padding, textY, rect.Right - padding, textY, _cardSepPaint);
                textY += 4;
            }

            // Render facet values
            _textFont.Size = fontSize;
            _labelFont.Size = fontSize;
            var properties = _controller.Properties;
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    if (textY + lineHeight > maxTextY) break;
                    if (prop.IsPrivate) continue;

                    var values = pos.Item[prop.Id];
                    if (values == null || values.Count == 0) continue;

                    string valueStr = string.Join(", ", values.Take(3));
                    string label = prop.DisplayName + ": ";

                    // Label in bold-like color
                    float labelWidth = _labelFont.MeasureText(label, out _);
                    canvas.DrawText(label, rect.Left + padding, textY + fontSize,
                        SKTextAlign.Left, _labelFont, _labelPaint);

                    // Value text
                    float valueX = rect.Left + padding + labelWidth;
                    float availWidth = rect.Right - padding - valueX;
                    if (availWidth > 10)
                    {
                        string truncated = TruncateText(valueStr, _textFont, availWidth);
                        canvas.DrawText(truncated, valueX, textY + fontSize,
                            SKTextAlign.Left, _textFont, _textPaint);
                    }

                    textY += lineHeight;
                }
            }
        }

        private static string TruncateText(string text, SKFont font, float maxWidth)
        {
            if (font.MeasureText(text, out _) <= maxWidth)
                return text;

            for (int len = text.Length - 1; len > 0; len--)
            {
                string candidate = text.Substring(0, len) + "…";
                if (font.MeasureText(candidate, out _) <= maxWidth)
                    return candidate;
            }
            return "…";
        }

        private void RenderHistogramView(SKCanvas canvas, SKImageInfo info, HistogramLayout layout)
        {
            var accentColor = ToSKColor(AccentColor);
            _itemPaint.Color = accentColor;

            // Show "no results" when filters produce 0 items
            if (layout.Columns.Length == 0)
            {
                RenderNoResultsMessage(canvas, info.Width, info.Height);
                return;
            }

            // Layout constants for axis area
            const float yAxisWidth = 40f;
            const float xAxisHeight = 20f;
            const float titleHeight = 24f;
            float chartLeft = yAxisWidth;
            float chartBottom = info.Height - xAxisHeight;
            float chartWidth = info.Width - yAxisWidth;
            float chartHeight = info.Height - xAxisHeight - titleHeight;

            // Draw graph title (property name)
            if (!string.IsNullOrEmpty(layout.PropertyName))
            {
                _labelFont.Size = 13;
                using var titlePaint = new SKPaint { Color = new SKColor(60, 60, 60) };
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

            // Draw Y-axis labels and gridlines
            if (maxCount > 0)
            {
                int yTicks = Math.Min(5, maxCount);
                int yStep = Math.Max(1, (maxCount + yTicks - 1) / yTicks);
                using var gridPaint = new SKPaint { Color = new SKColor(230, 230, 230), StrokeWidth = 1 };
                _textFont.Size = 9;
                using var yLabelPaint = new SKPaint { Color = new SKColor(120, 120, 120) };

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

            // Draw axis lines
            using (var axisPaint = new SKPaint { Color = new SKColor(180, 180, 180), StrokeWidth = 1 })
            {
                canvas.DrawLine(chartLeft, titleHeight, chartLeft, chartBottom, axisPaint);
                canvas.DrawLine(chartLeft, chartBottom, info.Width, chartBottom, axisPaint);
            }

            // Scale layout positions to fit within the chart area
            // Layout engine computed positions in [0, availableWidth] x [0, availableHeight]
            // We need to map them into [chartLeft, chartLeft+chartWidth] x [titleHeight, chartBottom]
            float scaleX = chartWidth / Math.Max(1, info.Width);
            float scaleY = chartHeight / Math.Max(1, info.Height);

            foreach (var col in layout.Columns)
            {
                // Map column X from layout space to chart space
                float colX = chartLeft + (float)col.X * scaleX;
                float colW = (float)col.Width * scaleX;

                // Draw column label on X-axis
                _labelFont.Size = 11;
                string label = col.Label;
                var textWidth = _labelFont.MeasureText(label, out _);

                if (textWidth > colW - 2)
                {
                    while (label.Length > 3 && _labelFont.MeasureText(label + "…", out _) > colW - 2)
                        label = label.Substring(0, label.Length - 1);
                    label += "…";
                }

                float labelX = colX + colW / 2;
                canvas.DrawText(label, labelX, info.Height - 4, SKTextAlign.Center, _labelFont, _labelPaint);

                // Draw item count above column
                if (col.Items.Length > 0)
                {
                    _textFont.Size = 10;
                    string countLabel = col.Items.Length.ToString();
                    float topItemY = titleHeight + col.Items.Min(p => (float)p.Y) * scaleY;
                    using var countPaint = new SKPaint { Color = new SKColor(100, 100, 100) };
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

        private void RenderNoResultsMessage(SKCanvas canvas, float width, float height)
        {
            const string message = "No results found";
            _textFont.Size = 18;
            using var paint = new SKPaint { Color = new SKColor(128, 128, 128) };
            canvas.DrawText(message, width / 2, height / 2, SKTextAlign.Center, _textFont, paint);
        }

        /// <summary>Fit source dimensions into dest rect preserving aspect ratio (Uniform stretch).</summary>
        private static SKRect FitUniform(int srcW, int srcH, SKRect dest)
        {
            if (srcW <= 0 || srcH <= 0) return dest;
            float srcAspect = (float)srcW / srcH;
            float destAspect = dest.Width / dest.Height;
            float w, h;
            if (srcAspect > destAspect)
            {
                w = dest.Width;
                h = w / srcAspect;
            }
            else
            {
                h = dest.Height;
                w = h * srcAspect;
            }
            float x = dest.Left + (dest.Width - w) / 2;
            float y = dest.Top + (dest.Height - h) / 2;
            return new SKRect(x, y, x + w, y + h);
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

            // Active filter breadcrumbs (between view switcher and sort)
            {
                var activeFilters = new List<string>();
                if (!string.IsNullOrEmpty(_controller.SearchText))
                    activeFilters.Add($"\"{_controller.SearchText}\"");

                // Check each property for any active predicate (string, numeric, or datetime)
                var predicates = _controller.FilterEngine.Predicates;
                foreach (var prop in _controller.Properties)
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
                    // Measure graphLabel at original size (14) before changing font
                    float graphLabelWidth = _textFont.MeasureText(graphLabel, out _);
                    _textFont.Size = 11;
                    using var breadcrumbPaint = new SKPaint { Color = new SKColor(180, 200, 255) };
                    string breadcrumb = string.Join(" › ", activeFilters);
                    float bx = x + graphLabelWidth + 20;
                    float maxWidth = info.Width / 2 - bx - 60; // leave room for sort label
                    if (maxWidth > 40)
                    {
                        // Truncate based on pixel width
                        while (_textFont.MeasureText(breadcrumb, out _) > maxWidth && breadcrumb.Length > 4)
                            breadcrumb = breadcrumb.Substring(0, breadcrumb.Length - 4) + "...";
                        canvas.DrawText(breadcrumb, bx, barHeight / 2 + 4, SKTextAlign.Left, _textFont, breadcrumbPaint);
                    }
                    _textFont.Size = 14;
                }
            }

            // Sort indicator (clickable)
            {
                float sortX = info.Width / 2;
                string arrow = _controller.SortDescending ? "▲" : "▼";
                string sortText = _controller.SortProperty != null
                    ? $"Sort: {_controller.SortProperty.DisplayName} {arrow}"
                    : $"Sort {arrow}";
                canvas.DrawText(sortText, sortX, barHeight / 2 + 5, SKTextAlign.Center, _textFont, whitePaint);
            }
        }

        private void RenderSortDropdown(SKCanvas canvas, SKImageInfo info)
        {
            if (!_showSortDropdown) return;

            var properties = _controller.Properties;
            if (properties.Count == 0) return;

            // Render using pixel-space coordinates (info.Width/Height)
            float scale = info.Width / (float)Math.Max(1, Width);
            float dropdownWidth = 200 * scale;
            float rowHeight = 28 * scale;
            float dropdownHeight = properties.Count * rowHeight + 8 * scale;
            float dropdownX = info.Width / 2 - dropdownWidth / 2;
            float dropdownY = ControlBarHeight * scale;

            var renderRect = new SKRect(dropdownX, dropdownY, dropdownX + dropdownWidth, dropdownY + dropdownHeight);

            // Background
            using var bgPaint = new SKPaint { Color = SKColors.White };
            using var shadowPaint = new SKPaint { Color = new SKColor(0, 0, 0, 60) };
            canvas.DrawRect(renderRect.Left + 2 * scale, renderRect.Top + 2 * scale,
                dropdownWidth, dropdownHeight, shadowPaint);
            canvas.DrawRect(renderRect, bgPaint);

            // Border
            using var borderPaint = new SKPaint { Color = new SKColor(180, 180, 180), IsStroke = true, StrokeWidth = 1 };
            canvas.DrawRect(renderRect, borderPaint);

            // Rows
            _textFont.Size = 13 * scale;
            using var textPaint = new SKPaint { Color = new SKColor(30, 30, 30) };
            using var selectedTextPaint = new SKPaint { Color = SKColors.CornflowerBlue };
            using var hoverPaint = new SKPaint { Color = new SKColor(230, 240, 255) };

            float y = dropdownY + 4 * scale;
            foreach (var prop in properties)
            {
                bool isSelected = _controller.SortProperty?.Id == prop.Id;
                if (isSelected)
                    canvas.DrawRect(dropdownX, y, dropdownWidth, rowHeight, hoverPaint);

                canvas.DrawText(prop.DisplayName ?? prop.Id, dropdownX + 10 * scale, y + rowHeight / 2 + 5 * scale,
                    SKTextAlign.Left, _textFont, isSelected ? selectedTextPaint : textPaint);
                y += rowHeight;
            }
        }

        // --- Filter Pane ---

        private void RenderFilterPane(SKCanvas canvas, float width, float height, float topOffset)
        {
            using var bgPaint = new SKPaint { Color = ToSkColor(ControlBackground) };
            canvas.DrawRect(0, topOffset, width, height, bgPaint);

            var filterPane = _controller.FilterPaneModel;
            if (filterPane == null) return;

            // Apply scroll offset
            canvas.Save();
            canvas.Translate(0, -_filterScrollOffset);

            var categories = filterPane.GetCategories(_controller.SearchFilteredItems);
            float searchBoxHeight = _searchEntry != null ? 40f : 0f;
            float y = topOffset + ItemPadding + searchBoxHeight;
            float lineHeight = 20f;

            // "Clear All" button if any filters active
            if (filterPane.HasActiveFilters)
            {
                using var clearPaint = new SKPaint { Color = SKColors.CornflowerBlue };
                _textFont.Size = 12;
                canvas.DrawText("✕ Clear All Filters", ItemPadding, y + 14, SKTextAlign.Left, _textFont, clearPaint);
                y += lineHeight + 4;
            }

            foreach (var category in categories)
            {
                bool visible = y < topOffset + height + _filterScrollOffset && y + lineHeight > topOffset;

                // Category header
                if (visible)
                {
                    using var headerPaint = new SKPaint
                    {
                        Color = category.IsFiltered ? SKColors.CornflowerBlue : SKColors.Black,
                    };
                    _textFont.Size = 13;
                    canvas.DrawText(category.Property.DisplayName ?? category.Property.Id,
                        ItemPadding, y + 14, SKTextAlign.Left, _textFont, headerPaint);
                }
                y += lineHeight + 2;

                // Value list (for string/text/link categories — all use checkbox UI)
                if (category.ValueCounts != null && (
                    category.Property.PropertyType == PivotViewerPropertyType.Text ||
                    category.Property.PropertyType == PivotViewerPropertyType.Link))
                {
                    _textFont.Size = 11;
                    int shown = 0;
                    bool isExpanded = _expandedFilterCategories.Contains(category.Property.Id);
                    int maxVisible = isExpanded ? category.ValueCounts.Count : 8;
                    foreach (var kv in category.ValueCounts.OrderByDescending(kv => kv.Value).Take(maxVisible))
                    {
                        visible = y < topOffset + height + _filterScrollOffset && y + lineHeight > topOffset;

                        if (visible)
                        {
                            bool isActive = category.ActiveFilters?.Contains(kv.Key) ?? false;
                            string checkbox = isActive ? "☑" : "☐";
                            string label = $"{checkbox} {kv.Key} ({kv.Value})";

                            using var valuePaint = new SKPaint
                            {
                                Color = isActive ? SKColors.CornflowerBlue : new SKColor(80, 80, 80)
                            };
                            canvas.DrawText(label, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, valuePaint);
                        }
                        y += lineHeight - 2;
                        shown++;
                    }

                    if (!isExpanded && category.ValueCounts.Count > 8)
                    {
                        visible = y < topOffset + height + _filterScrollOffset && y + lineHeight > topOffset;
                        if (visible)
                        {
                            using var morePaint = new SKPaint { Color = new SKColor(0, 102, 204) };
                            canvas.DrawText($"  ▸ Show all {category.ValueCounts.Count} values...",
                                ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, morePaint);
                        }
                        y += lineHeight - 2;
                    }
                    else if (isExpanded && category.ValueCounts.Count > 8)
                    {
                        visible = y < topOffset + height + _filterScrollOffset && y + lineHeight > topOffset;
                        if (visible)
                        {
                            using var morePaint = new SKPaint { Color = new SKColor(0, 102, 204) };
                            canvas.DrawText("  ▾ Show less",
                                ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, morePaint);
                        }
                        y += lineHeight - 2;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.Decimal)
                {
                    // Numeric range — render a cached mini histogram
                    _textFont.Size = 11;
                    if (!_numericHistogramCache.TryGetValue(category.Property.Id, out var buckets))
                    {
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
                        buckets = numericValues.Count > 0
                            ? HistogramBucketer.CreateNumericBuckets(numericValues)
                            : new List<HistogramBucket<double>>();
                        _numericHistogramCache[category.Property.Id] = buckets;
                    }

                    if (buckets.Count > 0)
                    {
                        float histH = 40f;
                        float barWidth = (width - ItemPadding * 2 - 16) / Math.Max(1, buckets.Count);
                        int maxCount = buckets.Max(b => b.Count);

                        for (int i = 0; i < buckets.Count; i++)
                        {
                            float barHeight = maxCount > 0 ? (float)buckets[i].Count / maxCount * histH : 0;
                            float barX = ItemPadding + 8 + i * barWidth;
                            float barY = y + histH - barHeight;

                            using var barPaint = new SKPaint { Color = new SKColor(100, 149, 237, 180) };
                            canvas.DrawRect(barX, barY, barWidth - 1, barHeight, barPaint);
                        }
                        y += histH + 4;

                        // Min/max label
                        using var rangePaint = new SKPaint { Color = new SKColor(120, 120, 120) };
                        string rangeLabel = $"{buckets[0].Label} – {buckets[buckets.Count - 1].Label}";
                        canvas.DrawText(rangeLabel, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, rangePaint);
                        y += lineHeight;
                    }
                    else
                    {
                        using var emptyPaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText("(no numeric data)", ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, emptyPaint);
                        y += lineHeight;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.DateTime)
                {
                    // DateTime range — render cached mini histogram
                    _textFont.Size = 11;
                    if (!_dateHistogramCache.TryGetValue(category.Property.Id, out var dateData))
                    {
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
                        float histH = 40f;
                        float barWidth = (width - ItemPadding * 2 - 16) / Math.Max(1, dtBuckets.Count);
                        int maxCount = dtBuckets.Max(b => b.Count);

                        for (int i = 0; i < dtBuckets.Count; i++)
                        {
                            float barHeight = maxCount > 0 ? (float)dtBuckets[i].Count / maxCount * histH : 0;
                            float barX = ItemPadding + 8 + i * barWidth;
                            float barY = y + histH - barHeight;

                            using var barPaint = new SKPaint { Color = new SKColor(144, 190, 109, 180) };
                            canvas.DrawRect(barX, barY, barWidth - 1, barHeight, barPaint);
                        }
                        y += histH + 4;

                        using var rangePaint = new SKPaint { Color = new SKColor(120, 120, 120) };
                        string rangeLabel = $"{dateData.Min:MMM yyyy} – {dateData.Max:MMM yyyy}";
                        canvas.DrawText(rangeLabel, ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, rangePaint);
                        y += lineHeight;
                    }
                    else
                    {
                        using var emptyPaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText("(no date data)", ItemPadding + 8, y + 12, SKTextAlign.Left, _textFont, emptyPaint);
                        y += lineHeight;
                    }
                }

                y += 6; // Spacing between categories
            }

            // Track total content height for scroll clamping
            _filterContentHeight = y - topOffset;
            canvas.Restore();
        }

        // --- Detail Pane ---

        private void RenderDetailPane(SKCanvas canvas, float width, float height)
        {
            _detailLinkHitRects.Clear();
            _detailFacetHitRects.Clear();

            using var bgPaint = new SKPaint { Color = ToSkColor(SecondaryBackground) };
            canvas.DrawRect(0, 0, width, height, bgPaint);

            // Separator line
            using var sepPaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1 };
            canvas.DrawLine(0, 0, 0, height, sepPaint);

            var detail = _controller.DetailPane;
            var item = detail.SelectedItem;
            if (item == null) return;

            var defaults = _controller.DefaultDetails;
            float y = ItemPadding;

            // Item name (respects DefaultDetails.IsNameHidden)
            if (!defaults.IsNameHidden)
            {
                var name = GetItemDisplayName(item) ?? "Unknown";
                _textFont.Size = 16;
                using var titlePaint = new SKPaint { Color = SKColors.Black };
                canvas.DrawText(name, ItemPadding, y + 16, SKTextAlign.Left, _textFont, titlePaint);
                y += 28;
            }

            // Thumbnail
            var imgProvider = _controller.ImageProvider;
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
                        canvas.DrawText(desc, ItemPadding, y + 12, SKTextAlign.Left, _textFont, descPaint);
                        y += 20;
                    }
                }
            }

            // Separator
            y += 4;
            canvas.DrawLine(ItemPadding, y, width - ItemPadding, y, sepPaint);
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
                    canvas.DrawText(facet.DisplayName, ItemPadding, y + 12, SKTextAlign.Left, _textFont, propPaint);
                    y += 16;

                    // Values — use blue + underline for Link-type properties
                    bool isLinkType = facet.Property is PivotViewerLinkProperty;
                    bool isFilterable = facet.Property.Options.HasFlag(PivotViewerPropertyOptions.CanFilter);
                    using var valuePaint = new SKPaint { Color = isLinkType ? new SKColor(0, 102, 204) : (isFilterable ? new SKColor(0, 90, 180) : SKColors.Black) };
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
                            // Draw underline for link values
                            canvas.DrawLine(ItemPadding + 4, y + 14, ItemPadding + 4 + textWidth, y + 14, valuePaint);

                            // Record hit rect for link tap handling
                            if (rawValues != null && valIdx < rawValues.Count && rawValues[valIdx] is PivotViewerHyperlink hl)
                            {
                                _detailLinkHitRects.Add((new SKRect(ItemPadding + 4, y, ItemPadding + 4 + textWidth, y + 16), hl.Uri));
                            }
                        }
                        else if (isFilterable && facet.Property.PropertyType == PivotViewerPropertyType.Text)
                        {
                            // Record hit rect for tap-to-filter on filterable text values
                            _detailFacetHitRects.Add((new SKRect(ItemPadding + 4, y, ItemPadding + 4 + textWidth, y + 16), facet.Property.Id, val));
                        }
                        y += 16;
                        valIdx++;
                    }

                    if (facet.Values.Count > 3)
                    {
                        using var morePaint = new SKPaint { Color = SKColors.Gray };
                        canvas.DrawText($"+{facet.Values.Count - 3} more",
                            ItemPadding + 4, y + 12, SKTextAlign.Left, _textFont, morePaint);
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
                    canvas.DrawText(text, ItemPadding, y + 10, SKTextAlign.Left, _textFont, copyPaint);
                }
            }
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            _controller.SearchText = e.NewTextValue ?? "";
            _canvasView.InvalidateSurface();
        }

        private void InvalidateHistogramCaches()
        {
            _numericHistogramCache.Clear();
            _dateHistogramCache.Clear();
        }

        // --- Gestures ---

        private void SetupGestures()
        {
            _tapGesture = new TapGestureRecognizer();
            _tapGesture.Tapped += OnTapped;
            GestureRecognizers.Add(_tapGesture);

            _doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            _doubleTapGesture.Tapped += OnDoubleTapped;
            GestureRecognizers.Add(_doubleTapGesture);

            _pinchGesture = new PinchGestureRecognizer();
            _pinchGesture.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(_pinchGesture);

            _panGesture = new PanGestureRecognizer();
            _panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(_panGesture);

            // Track pointer position to determine pan origin (filter pane vs content)
            _pointerGesture = new PointerGestureRecognizer();
            _onPointerPressed = (s, e) =>
            {
                var pos = e.GetPosition(this);
                if (pos.HasValue)
                    _lastPointerX = pos.Value.X;
            };
            _pointerGesture.PointerPressed += _onPointerPressed;
            GestureRecognizers.Add(_pointerGesture);
        }

        private void OnTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (!point.HasValue) return;

            double x = point.Value.X;
            double y = point.Value.Y;

            // Handle sort dropdown interactions
            if (_showSortDropdown)
            {
                if (x >= _sortDropdownRect.Left && x <= _sortDropdownRect.Right &&
                    y >= _sortDropdownRect.Top && y <= _sortDropdownRect.Bottom)
                {
                    // Determine which property was tapped
                    float rowHeight = 28;
                    int index = (int)((y - _sortDropdownRect.Top - 4) / rowHeight);
                    var properties = _controller.Properties;
                    if (index >= 0 && index < properties.Count)
                    {
                        if (_controller.SortProperty?.Id == properties[index].Id)
                        {
                            // Tapping the already-selected sort property toggles direction
                            _controller.SortDescending = !_controller.SortDescending;
                        }
                        else
                        {
                            _controller.SortProperty = properties[index];
                        }
                    }
                }
                _showSortDropdown = false;
                _canvasView.InvalidateSurface();
                return;
            }

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

            // Detail pane interactions (right side when showing)
            float totalWidth = (float)Width;
            float detailWidth = _controller.DetailPane.IsShowing ? DetailPaneWidth : 0;
            if (detailWidth > 0 && x > totalWidth - detailWidth)
            {
                HandleDetailPaneTap(x - (totalWidth - detailWidth), y - ControlBarHeight);
                return;
            }

            // Main content area — adjust for content region offset
            double contentX = x - FilterPaneWidth;
            double contentY = y - ControlBarHeight;

            // Try item hit test first (works for both grid and graph views)
            var hit = _controller.HitTest(contentX - _controller.PanOffsetX, contentY - _controller.PanOffsetY);
            if (hit != null)
            {
                _controller.SelectedItem = hit;
                _canvasView.InvalidateSurface();
                return;
            }

            // Graph view: if no item was hit, check if a histogram column label area was tapped
            // Only trigger if the tap is in the bottom label area (last 30 logical pixels)
            if (_controller.CurrentView == "graph" && _controller.HistogramLayout != null && _controller.SortProperty != null)
            {
                var layout = _controller.HistogramLayout;
                double hitX = contentX - _controller.PanOffsetX;
                double hitY = contentY - _controller.PanOffsetY;
                double contentHeight = Height - ControlBarHeight;
                if (hitY >= contentHeight - 30)
                {
                    foreach (var col in layout.Columns)
                    {
                        if (hitX >= col.X && hitX < col.X + col.Width)
                        {
                            // Don't filter on synthesized "(No value)" label
                            if (col.Label != "(No value)")
                            {
                                _controller.FilterPaneModel?.ToggleStringFilter(_controller.SortProperty.Id, col.Label);
                            }
                            _canvasView.InvalidateSurface();
                            return;
                        }
                    }
                }
            }

            // No item or column hit — deselect
            _controller.SelectedItem = null;
            _canvasView.InvalidateSurface();
        }

        private void HandleDetailPaneTap(double localX, double localY)
        {
            // Check link hit rects first (populated during RenderDetailPane)
            foreach (var (bounds, href) in _detailLinkHitRects)
            {
                if (localX >= bounds.Left && localX <= bounds.Right &&
                    localY >= bounds.Top && localY <= bounds.Bottom)
                {
                    _controller.DetailPane.OnLinkClicked(href);
                    LinkClicked?.Invoke(this, new PivotViewerLinkEventArgs(href));
                    return;
                }
            }

            // Check facet value hit rects for tap-to-filter
            foreach (var (bounds, propertyId, value) in _detailFacetHitRects)
            {
                if (localX >= bounds.Left && localX <= bounds.Right &&
                    localY >= bounds.Top && localY <= bounds.Bottom)
                {
                    _controller.FilterPaneModel?.ToggleStringFilter(propertyId, value);
                    _controller.DetailPane.OnApplyFilter(value);
                    _canvasView.InvalidateSurface();
                    return;
                }
            }

            // Fallback: if the selected item has an Href, treat taps as link clicks
            var item = _controller.DetailPane.SelectedItem;
            if (item == null) return;

            var hrefValues = item["Href"];
            if (hrefValues != null && hrefValues.Count > 0)
            {
                var href = hrefValues[0]?.ToString();
                if (!string.IsNullOrEmpty(href) && Uri.TryCreate(href, UriKind.Absolute, out var uri))
                {
                    _controller.DefaultDetails.OnLinkClicked(uri);
                    LinkClicked?.Invoke(this, new PivotViewerLinkEventArgs(uri));
                }
            }
        }

        private void HandleControlBarTap(double x, double y)
        {
            // Sort dropdown region (center of control bar)
            float totalWidth = (float)Width;
            float sortCenter = totalWidth / 2;
            if (x > sortCenter - 100 && x < sortCenter + 100)
            {
                _showSortDropdown = !_showSortDropdown;
                if (_showSortDropdown)
                {
                    // Pre-calculate dropdown rect in logical coordinates for hit-testing
                    var properties = _controller.Properties;
                    float dropdownWidth = 200;
                    float rowHeight = 28;
                    float dropdownHeight = properties.Count * rowHeight + 8;
                    float dropdownX = totalWidth / 2 - dropdownWidth / 2;
                    float dropdownY = ControlBarHeight;
                    _sortDropdownRect = new SKRect(dropdownX, dropdownY,
                        dropdownX + dropdownWidth, dropdownY + dropdownHeight);
                }
                _canvasView.InvalidateSurface();
                return;
            }

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

            // Adjust tap Y for scroll offset
            double adjustedY = y + _filterScrollOffset;

            // Check "Clear All" button (only responds when tap is within its rendered bounds)
            float searchBoxOffset = 40f;
            float clearAllStart = ControlBarHeight + searchBoxOffset + ItemPadding;
            float clearAllEnd = clearAllStart + 24;
            if (filterPane.HasActiveFilters && adjustedY >= clearAllStart && adjustedY < clearAllEnd)
            {
                filterPane.ClearAllFilters();
                _canvasView.InvalidateSurface();
                return;
            }

            // Calculate which filter value was tapped based on Y position
            var categories = filterPane.GetCategories(_controller.SearchFilteredItems);
            float searchBoxHeight = _searchEntry != null ? 40f : 0f;
            float catY = ControlBarHeight + ItemPadding + searchBoxHeight;
            float lineHeight = 20f;

            if (filterPane.HasActiveFilters)
                catY += lineHeight + 4;

            foreach (var category in categories)
            {
                catY += lineHeight + 2; // Header

                if (category.ValueCounts != null && (
                    category.Property.PropertyType == PivotViewerPropertyType.Text ||
                    category.Property.PropertyType == PivotViewerPropertyType.Link))
                {
                    bool isExpanded = _expandedFilterCategories.Contains(category.Property.Id);
                    int maxVisible = isExpanded ? category.ValueCounts.Count : 8;
                    foreach (var kv in category.ValueCounts.OrderByDescending(kv => kv.Value).Take(maxVisible))
                    {
                        if (adjustedY >= catY && adjustedY < catY + lineHeight - 2)
                        {
                            filterPane.ToggleStringFilter(category.Property.Id, kv.Key);
                            _canvasView.InvalidateSurface();
                            return;
                        }
                        catY += lineHeight - 2;
                    }

                    if (category.ValueCounts.Count > 8)
                    {
                        // "Show all" / "Show less" toggle
                        if (adjustedY >= catY && adjustedY < catY + lineHeight - 2)
                        {
                            if (isExpanded)
                                _expandedFilterCategories.Remove(category.Property.Id);
                            else
                                _expandedFilterCategories.Add(category.Property.Id);
                            _canvasView.InvalidateSurface();
                            return;
                        }
                        catY += lineHeight - 2;
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.Decimal)
                {
                    // Numeric histogram tap handling
                    if (_numericHistogramCache.TryGetValue(category.Property.Id, out var buckets) && buckets.Count > 0)
                    {
                        float histH = 40f;
                        float barWidth = (FilterPaneWidth - ItemPadding * 2 - 16) / Math.Max(1, buckets.Count);

                        if (adjustedY >= catY && adjustedY < catY + histH)
                        {
                            int barIndex = (int)((x - ItemPadding - 8) / barWidth);
                            if (barIndex >= 0 && barIndex < buckets.Count)
                            {
                                double min = buckets[barIndex].Min;
                                // Use exclusive upper bound for non-last buckets to match histogram grouping
                                double max = barIndex < buckets.Count - 1
                                    ? buckets[barIndex].Max - 1e-10
                                    : buckets[barIndex].Max;
                                filterPane.SetNumericRangeFilter(category.Property.Id, min, max);
                                _canvasView.InvalidateSurface();
                                return;
                            }
                        }
                        catY += histH + 4;
                        catY += lineHeight; // range label
                    }
                    else
                    {
                        catY += lineHeight; // "(no numeric data)"
                    }
                }
                else if (category.Property.PropertyType == PivotViewerPropertyType.DateTime)
                {
                    // DateTime histogram tap handling
                    if (_dateHistogramCache.TryGetValue(category.Property.Id, out var dateData) && dateData.Buckets.Count > 0)
                    {
                        var dtBuckets = dateData.Buckets;
                        float histH = 40f;
                        float barWidth = (FilterPaneWidth - ItemPadding * 2 - 16) / Math.Max(1, dtBuckets.Count);

                        if (adjustedY >= catY && adjustedY < catY + histH)
                        {
                            int barIndex = (int)((x - ItemPadding - 8) / barWidth);
                            if (barIndex >= 0 && barIndex < dtBuckets.Count)
                            {
                                var dtMin = dtBuckets[barIndex].Min;
                                // Use exclusive upper bound for non-last buckets
                                var dtMax = barIndex < dtBuckets.Count - 1
                                    ? dtBuckets[barIndex].Max.AddTicks(-1)
                                    : dtBuckets[barIndex].Max;
                                filterPane.SetDateTimeRangeFilter(category.Property.Id, dtMin, dtMax);
                                _canvasView.InvalidateSurface();
                                return;
                            }
                        }
                        catY += histH + 4;
                        catY += lineHeight; // range label
                    }
                    else
                    {
                        catY += lineHeight; // "(no date data)"
                    }
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
            if (!point.HasValue) return;

            double contentX = point.Value.X - FilterPaneWidth;
            double contentY = point.Value.Y - ControlBarHeight;
            var hit = _controller.HitTest(
                contentX - _controller.PanOffsetX,
                contentY - _controller.PanOffsetY);
            if (hit != null)
            {
                _controller.ZoomToItem(hit);
                _controller.NotifyItemDoubleClicked(hit);
                ItemDoubleClick?.Invoke(this, hit);

                SyncZoomSlider();
                _canvasView.InvalidateSurface();
            }
        }

        private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _lastPivotPinchScale = 1.0;
                    break;
                case GestureStatus.Running:
                    double scaleChange = e.Scale / _lastPivotPinchScale;
                    _lastPivotPinchScale = e.Scale;
                    _controller.ZoomAbout(scaleChange, Width / 2, Height / 2);
                    SyncZoomSlider();
                    _canvasView.InvalidateSurface();
                    break;
            }
        }

        private void OnCanvasTouch(object? sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
        {
            // Capture touch position for filter pane scroll detection on touch platforms
            // (PointerGestureRecognizer only fires for mouse/pen, not touch)
            if (e.ActionType == SkiaSharp.Views.Maui.SKTouchAction.Pressed)
            {
                _lastPointerX = e.Location.X;
            }
            else if (e.ActionType == SkiaSharp.Views.Maui.SKTouchAction.Released ||
                     e.ActionType == SkiaSharp.Views.Maui.SKTouchAction.Cancelled)
            {
                _lastPointerX = double.NaN;
            }
            // Don't set e.Handled — let MAUI gesture recognizers handle the rest
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _previousPanX = 0;
                    _previousPanY = 0;
                    // Use last pointer position to determine if pan started in filter pane
                    _isPanningFilterPane = !double.IsNaN(_lastPointerX) && _lastPointerX < FilterPaneWidth;
                    break;
                case GestureStatus.Running:
                    double deltaX = e.TotalX - _previousPanX;
                    double deltaY = e.TotalY - _previousPanY;
                    _previousPanX = e.TotalX;
                    _previousPanY = e.TotalY;

                    if (_isPanningFilterPane)
                    {
                        float contentHeight = (float)(Height - ControlBarHeight);
                        _filterScrollOffset = Math.Clamp(
                            _filterScrollOffset - (float)deltaY,
                            0,
                            Math.Max(0, _filterContentHeight - contentHeight));
                        _canvasView.InvalidateSurface();
                    }
                    else
                    {
                        _controller.Pan(deltaX, deltaY);
                        _canvasView.InvalidateSurface();
                    }
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _isPanningFilterPane = false;
                    _lastPointerX = double.NaN;
                    break;
            }
        }

        // --- Animation ---

        private void SyncZoomSlider()
        {
            if (_zoomSlider == null) return;
            _suppressZoomSliderUpdate = true;
            _zoomSlider.Value = _controller.ZoomLevel;
            _suppressZoomSliderUpdate = false;
        }

        private void StartAnimationIfNeeded()
        {
            if (_disposed) return;
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
            if (_disposed || !_isVisible)
            {
                _animationTimer?.Stop();
                return;
            }

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
                _animationTimer = null;
            }
            _canvasView.PaintSurface -= OnPaintSurface;
            _canvasView.Touch -= OnCanvasTouch;

            // Unsubscribe all controller events
            if (_onLayoutUpdated != null) _controller.LayoutUpdated -= _onLayoutUpdated;
            if (_onSelectionChanged != null) _controller.SelectionChanged -= _onSelectionChanged;
            if (_onFiltersChanged != null) _controller.FiltersChanged -= _onFiltersChanged;
            if (_onViewChanged != null) _controller.ViewChanged -= _onViewChanged;
            if (_onSortPropertyChanged != null) _controller.SortPropertyChanged -= _onSortPropertyChanged;
            if (_onCollectionChanged != null) _controller.CollectionChanged -= _onCollectionChanged;

            // Unsubscribe UI control events
            if (_searchEntry != null) _searchEntry.TextChanged -= OnSearchTextChanged;
            if (_zoomSlider != null && _onZoomSliderValueChanged != null)
                _zoomSlider.ValueChanged -= _onZoomSliderValueChanged;

            // Unsubscribe gesture recognizer events
            if (_tapGesture != null) _tapGesture.Tapped -= OnTapped;
            if (_doubleTapGesture != null) _doubleTapGesture.Tapped -= OnDoubleTapped;
            if (_pinchGesture != null) _pinchGesture.PinchUpdated -= OnPinchUpdated;
            if (_panGesture != null) _panGesture.PanUpdated -= OnPanUpdated;
            if (_pointerGesture != null && _onPointerPressed != null)
                _pointerGesture.PointerPressed -= _onPointerPressed;

            // Unsubscribe lifecycle events
            Loaded -= OnViewLoaded;
            Unloaded -= OnViewUnloaded;

            _controller.Dispose();
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
    }
}
