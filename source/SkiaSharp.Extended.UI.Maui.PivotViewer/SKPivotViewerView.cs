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
    /// Delegates all rendering and hit-testing to <see cref="PivotViewerRenderer"/>.
    /// </summary>
    public class SKPivotViewerView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly PivotViewerController _controller;
        private readonly PivotViewerRenderer _renderer;
        private PivotViewerTheme _theme;
        private readonly PivotViewerViewState _viewState;
        private bool _suppressPropertySync;
        private IDispatcherTimer? _animationTimer;
        private double _previousPanX;
        private double _previousPanY;
        private bool _disposed;
        private bool _isVisible = true;

        private Entry? _searchEntry;
        private bool _isPanningFilterPane;
        private double _lastPointerX = double.NaN;
        private double _lastPivotPinchScale = 1.0;

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

        // Cached last paint info for hit-testing
        private SKImageInfo _lastPaintInfo;

        public SKPivotViewerView()
        {
            _controller = new PivotViewerController();
            _renderer = new PivotViewerRenderer();
            _theme = PivotViewerTheme.Default;
            _viewState = new PivotViewerViewState();

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
                Margin = new Thickness(4, PivotViewerRenderer.ControlBarHeight + 4, 0, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = PivotViewerRenderer.FilterPaneWidth - 8,
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
                Margin = new Thickness(0, 6, PivotViewerRenderer.DetailPaneWidth + 10, 0),
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
                _renderer.InvalidateHistogramCaches();
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
                _renderer.InvalidateHistogramCaches();
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

        /// <summary>
        /// Handles mouse wheel input for zoom in grid/graph views.
        /// Positive delta = zoom in, negative = zoom out.
        /// Call from platform-specific mouse wheel event handler.
        /// </summary>
        public void HandleMouseWheel(double delta)
        {
            if (_disposed) return;

            double step = delta > 0 ? 0.05 : -0.05;
            _controller.ZoomLevel = Math.Clamp(_controller.ZoomLevel + step, 0.0, 1.0);
            SyncZoomSlider();
            _canvasView?.InvalidateSurface();
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

        public static readonly BindableProperty IsFilterPaneVisibleProperty =
            BindableProperty.Create(nameof(IsFilterPaneVisible), typeof(bool), typeof(SKPivotViewerView),
                true, BindingMode.TwoWay, propertyChanged: OnFilterPaneVisibleChanged);

        public static readonly BindableProperty CollectionUriProperty =
            BindableProperty.Create(nameof(CollectionUri), typeof(string), typeof(SKPivotViewerView),
                null, BindingMode.OneWay, propertyChanged: OnCollectionUriChanged);

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

        /// <summary>Controls visibility of the filter pane (two-way bindable).</summary>
        public bool IsFilterPaneVisible
        {
            get => (bool)GetValue(IsFilterPaneVisibleProperty);
            set => SetValue(IsFilterPaneVisibleProperty, value);
        }

        /// <summary>
        /// URI to a .cxml collection file. Setting this downloads the CXML, parses it,
        /// loads the DZC thumbnails, and hydrates the PivotViewer control.
        /// </summary>
        public string? CollectionUri
        {
            get => (string?)GetValue(CollectionUriProperty);
            set => SetValue(CollectionUriProperty, value);
        }

        private CancellationTokenSource? _collectionCts;

        private static void OnCollectionUriChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
                view.LoadFromCollectionUri(newValue as string);
        }

        private async void LoadFromCollectionUri(string? uri)
        {
            _collectionCts?.Cancel();
            _collectionCts?.Dispose();
            _collectionCts = null;

            if (string.IsNullOrWhiteSpace(uri))
                return;

            _collectionCts = new CancellationTokenSource();
            var activeCts = _collectionCts;
            var ct = activeCts.Token;

            try
            {
                using var httpClient = new HttpClient();
                var cxmlUri = new Uri(uri, UriKind.Absolute);

                // Download and parse the CXML
                var source = await CxmlCollectionSource.LoadAsync(cxmlUri, httpClient, ct);

                // Verify this is still the active request before mutating state
                if (_disposed || _collectionCts != activeCts || ct.IsCancellationRequested)
                    return;

                // Dispose previous ImageProvider before replacing
                _controller.ImageProvider?.Dispose();
                _controller.ImageProvider = null;

                // Load the collection into the controller
                LoadCollection(source);

                // If there's an ImageBase, try to load thumbnails via DZC
                if (source.ImageBase != null)
                {
                    var dzcUri = new Uri(cxmlUri, source.ImageBase);
                    var dzcPath = dzcUri.GetLeftPart(UriPartial.Path);
                    if (dzcPath.EndsWith(".dzc", StringComparison.OrdinalIgnoreCase))
                    {
                        var dzcXml = await httpClient.GetStringAsync(dzcUri, ct);

                        // Re-check after await
                        if (_disposed || _collectionCts != activeCts || ct.IsCancellationRequested)
                            return;

                        using var dzcStream = new System.IO.MemoryStream(
                            System.Text.Encoding.UTF8.GetBytes(dzcXml));
                        var dzc = DzcTileSource.Parse(dzcStream);
                        var fetcher = new HttpTileFetcher();
                        var queryString = dzcUri.ToString().Substring(dzcPath.Length);
                        var basePath = dzcPath.Substring(0, dzcPath.Length - 4) + "_files";

                        var imageProvider = new CollectionImageProvider(dzc, fetcher, basePath, queryString);
                        _controller.ImageProvider = imageProvider;

                        // Kick off thumbnail loading for all items
                        _ = LoadThumbnailsInBackground(imageProvider, source, ct);

                        _canvasView.InvalidateSurface();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load collection: {ex.Message}");
            }
        }

        private async Task LoadThumbnailsInBackground(
            CollectionImageProvider provider, CxmlCollectionSource source, CancellationToken ct)
        {
            try
            {
                var itemIds = source.Items
                    .Select(i => CollectionImageProvider.GetItemImageIndex(i))
                    .Where(id => id.HasValue && id.Value >= 0)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                foreach (var id in itemIds)
                {
                    ct.ThrowIfCancellationRequested();
                    if (_disposed || _controller.ImageProvider != provider) return;

                    await provider.LoadThumbnailAsync(id, 128, ct);

                    // Invalidate periodically to show thumbnails as they load
                    if (!_disposed && _controller.ImageProvider == provider)
                        MainThread.BeginInvokeOnMainThread(() => _canvasView?.InvalidateSurface());
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Thumbnail load error: {ex.Message}");
            }
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
            {
                view.SyncTheme();
                view._canvasView?.InvalidateSurface();
            }
        }

        private static void OnFilterPaneVisibleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is bool visible)
            {
                view._viewState.IsFilterPaneVisible = visible;
                if (view._searchEntry != null)
                    view._searchEntry.IsVisible = visible;
                view._canvasView?.InvalidateSurface();
            }
        }

        private void SyncTheme()
        {
            _theme = new PivotViewerTheme
            {
                AccentColor = ToSKColor(AccentColor),
                ControlBackground = ToSKColor(ControlBackground),
                SecondaryBackground = ToSKColor(SecondaryBackground),
                SecondaryForeground = ToSKColor(SecondaryForeground),
            };
        }

        private static SKColor ToSKColor(Color color)
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

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isVisible) return;

            // Flush deferred tile bitmap disposals on the UI thread
            _controller.ImageProvider?.FlushEvictedTiles();

            _lastPaintInfo = e.Info;
            _renderer.Render(e.Surface.Canvas, e.Info, _controller, _theme, _viewState);
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            _controller.SearchText = e.NewTextValue ?? "";
            _canvasView.InvalidateSurface();
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

            var hit = _renderer.HitTest(x, y, _lastPaintInfo, _controller, _viewState);

            switch (hit.Type)
            {
                case RenderHitType.SortDropdownRow:
                    {
                        var properties = _controller.Properties;
                        int index = hit.SortRowIndex;
                        if (index >= 0 && index < properties.Count)
                        {
                            if (_controller.SortProperty?.Id == properties[index].Id)
                                _controller.SortDescending = !_controller.SortDescending;
                            else
                                _controller.SortProperty = properties[index];
                        }
                        _viewState.IsSortDropdownVisible = false;
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.FilterToggle:
                    _viewState.IsFilterPaneVisible = !_viewState.IsFilterPaneVisible;
                    SetValue(IsFilterPaneVisibleProperty, _viewState.IsFilterPaneVisible);
                    _canvasView.InvalidateSurface();
                    return;

                case RenderHitType.ViewGrid:
                    _controller.CurrentView = "grid";
                    _canvasView.InvalidateSurface();
                    return;

                case RenderHitType.ViewGraph:
                    _controller.CurrentView = "graph";
                    _canvasView.InvalidateSurface();
                    return;

                case RenderHitType.SortDropdown:
                    _viewState.IsSortDropdownVisible = !_viewState.IsSortDropdownVisible;
                    _canvasView.InvalidateSurface();
                    return;

                case RenderHitType.ClearAllFilters:
                    _controller.FilterPaneModel?.ClearAllFilters();
                    _canvasView.InvalidateSurface();
                    return;

                case RenderHitType.FilterCheckbox:
                    if (hit.FilterPropertyId != null && hit.FilterValue != null)
                    {
                        _controller.FilterPaneModel?.ToggleStringFilter(hit.FilterPropertyId, hit.FilterValue);
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.FilterNumericHistogramBar:
                    if (hit.FilterPropertyId != null && hit.RangeMin.HasValue && hit.RangeMax.HasValue)
                    {
                        _controller.FilterPaneModel?.SetNumericRangeFilter(hit.FilterPropertyId, hit.RangeMin.Value, hit.RangeMax.Value);
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.FilterDateTimeHistogramBar:
                    if (hit.FilterPropertyId != null && hit.DateRangeMin.HasValue && hit.DateRangeMax.HasValue)
                    {
                        _controller.FilterPaneModel?.SetDateTimeRangeFilter(hit.FilterPropertyId, hit.DateRangeMin.Value, hit.DateRangeMax.Value);
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.FilterCategoryToggle:
                    if (hit.CategoryName != null)
                    {
                        if (_viewState.ExpandedFilterCategories.Contains(hit.CategoryName))
                            _viewState.ExpandedFilterCategories.Remove(hit.CategoryName);
                        else
                            _viewState.ExpandedFilterCategories.Add(hit.CategoryName);
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.DetailLink:
                    if (hit.LinkUri != null && Uri.TryCreate(hit.LinkUri, UriKind.Absolute, out var linkUri))
                    {
                        _controller.DetailPane.OnLinkClicked(linkUri);
                        LinkClicked?.Invoke(this, new PivotViewerLinkEventArgs(linkUri));
                    }
                    return;

                case RenderHitType.DetailFacetFilter:
                    if (hit.FilterPropertyId != null && hit.FilterValue != null)
                    {
                        _controller.FilterPaneModel?.ToggleStringFilter(hit.FilterPropertyId, hit.FilterValue);
                        _controller.DetailPane.OnApplyFilter(hit.FilterValue);
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.GraphColumnLabel:
                    if (hit.FilterPropertyId != null && hit.FilterValue != null)
                    {
                        _controller.FilterPaneModel?.ToggleStringFilter(hit.FilterPropertyId, hit.FilterValue);
                        _canvasView.InvalidateSurface();
                    }
                    return;

                case RenderHitType.Item:
                    _controller.SelectedItem = hit.Item;
                    _canvasView.InvalidateSurface();
                    return;

                case RenderHitType.None:
                default:
                    // Close sort dropdown if open, or deselect
                    if (_viewState.IsSortDropdownVisible)
                    {
                        _viewState.IsSortDropdownVisible = false;
                        _canvasView.InvalidateSurface();
                        return;
                    }
                    _controller.SelectedItem = null;
                    _canvasView.InvalidateSurface();
                    return;
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (!point.HasValue) return;

            var hit = _renderer.HitTest(point.Value.X, point.Value.Y, _lastPaintInfo, _controller, _viewState);
            if (hit.Type == RenderHitType.Item && hit.Item != null)
            {
                _controller.ZoomToItem(hit.Item);
                _controller.NotifyItemDoubleClicked(hit.Item);
                ItemDoubleClick?.Invoke(this, hit.Item);

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
                    _isPanningFilterPane = _viewState.IsFilterPaneVisible && !double.IsNaN(_lastPointerX) && _lastPointerX < PivotViewerRenderer.FilterPaneWidth;
                    break;
                case GestureStatus.Running:
                    double deltaX = e.TotalX - _previousPanX;
                    double deltaY = e.TotalY - _previousPanY;
                    _previousPanX = e.TotalX;
                    _previousPanY = e.TotalY;

                    if (_isPanningFilterPane)
                    {
                        double contentHeight = Height - PivotViewerRenderer.ControlBarHeight;
                        _viewState.FilterScrollOffset -= deltaY;
                        _viewState.ClampFilterScroll(contentHeight);
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

            _collectionCts?.Cancel();
            _collectionCts?.Dispose();
            _collectionCts = null;

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
            _renderer.Dispose();
        }
    }
}
