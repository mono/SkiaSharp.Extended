using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Orchestrates the PivotViewer: filtering, sorting, layout, selection, and state.
    /// Platform-agnostic — no MAUI dependency. Can be driven from tests or any UI.
    /// </summary>
    public class PivotViewerController : IDisposable
    {
        private readonly FilterEngine _filterEngine;
        private readonly GridLayoutEngine _layoutEngine;
        private readonly WordWheelIndex _wordWheel;
        private readonly LayoutTransitionManager _layoutTransition;
        private readonly EventHandler _onFiltersChangedHandler;
        private FilterPaneModel? _filterPaneModel;
        private DetailPaneModel? _detailPaneModel;
        private CollectionImageProvider? _imageProvider;
        private CxmlCollectionSource? _collectionSource;
        private PivotViewerItemTemplateCollection _itemTemplates = new PivotViewerItemTemplateCollection();
        private bool _disposed;

        private List<PivotViewerItem> _allItems = new List<PivotViewerItem>();
        private List<PivotViewerProperty> _properties = new List<PivotViewerProperty>();
        private IReadOnlyList<PivotViewerItem> _inScopeItems = Array.Empty<PivotViewerItem>();
        private GridLayout? _currentGridLayout;
        private HistogramLayout? _currentHistogramLayout;

        private PivotViewerItem? _selectedItem;
        private PivotViewerProperty? _sortProperty;
        private bool _sortDescending;
        private string _currentView = "grid";
        private double _availableWidth = 800;
        private double _availableHeight = 600;
        private double _zoomLevel = 0.0; // 0.0 = fit all, 1.0 = single item
        private double _panOffsetX;
        private double _panOffsetY;
        private string _searchText = "";
        private HashSet<string>? _textFilteredItemIds;
        private IReadOnlyList<PivotViewerItem>? _searchFilteredItemsCache;

        public PivotViewerController()
        {
            _filterEngine = new FilterEngine();
            _layoutEngine = new GridLayoutEngine();
            _wordWheel = new WordWheelIndex();
            _layoutTransition = new LayoutTransitionManager();

            _onFiltersChangedHandler = (s, e) => OnFiltersChanged();
            _filterEngine.FiltersChanged += _onFiltersChangedHandler;
        }

        // --- Data ---

        /// <summary>All items in the collection.</summary>
        public IReadOnlyList<PivotViewerItem> Items => _allItems;

        /// <summary>All property definitions.</summary>
        public IReadOnlyList<PivotViewerProperty> Properties => _properties;

        /// <summary>Items filtered by search text only (before facet filtering). Used for filter pane counts.</summary>
        public IReadOnlyList<PivotViewerItem> SearchFilteredItems =>
            _searchFilteredItemsCache ?? _allItems;

        /// <summary>Items currently in scope (after filtering).</summary>
        public IReadOnlyList<PivotViewerItem> InScopeItems => _inScopeItems;

        /// <summary>The filter engine.</summary>
        public FilterEngine FilterEngine => _filterEngine;

        /// <summary>The word wheel search index.</summary>
        public WordWheelIndex WordWheel => _wordWheel;

        /// <summary>The layout transition manager for animated transitions.</summary>
        public LayoutTransitionManager LayoutTransition => _layoutTransition;

        /// <summary>The filter pane model. Created after loading items.</summary>
        public FilterPaneModel? FilterPaneModel => _filterPaneModel;

        /// <summary>The detail pane model.</summary>
        public DetailPaneModel DetailPane
        {
            get
            {
                if (_detailPaneModel == null)
                    _detailPaneModel = new DetailPaneModel();
                return _detailPaneModel;
            }
        }

        /// <summary>
        /// Configuration for the default detail pane appearance.
        /// Matches Silverlight's PivotViewerDefaultDetails for controlling
        /// which sections are visible.
        /// </summary>
        public PivotViewerDefaultDetails DefaultDetails { get; } = new PivotViewerDefaultDetails();

        // --- Selection ---

        /// <summary>Currently selected item.</summary>
        public PivotViewerItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    var old = _selectedItem;
                    _selectedItem = value;
                    DetailPane.SelectedItem = value;
                    SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(old, value));
                }
            }
        }

        /// <summary>Index of the selected item in InScopeItems, or -1.</summary>
        public int SelectedIndex
        {
            get
            {
                if (_selectedItem == null) return -1;
                for (int i = 0; i < _inScopeItems.Count; i++)
                    if (_inScopeItems[i] == _selectedItem) return i;
                return -1;
            }
            set
            {
                if (value >= 0 && value < _inScopeItems.Count)
                    SelectedItem = _inScopeItems[value];
                else
                    SelectedItem = null;
            }
        }

        // --- Sort ---

        /// <summary>Active sort property.</summary>
        public PivotViewerProperty? SortProperty
        {
            get => _sortProperty;
            set
            {
                if (_sortProperty != value)
                {
                    _sortProperty = value;
                    SortPropertyChanged?.Invoke(this, EventArgs.Empty);
                    UpdateInScopeItems();
                }
            }
        }

        /// <summary>When true, sort order is descending. Default is ascending (false).</summary>
        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                if (_sortDescending != value)
                {
                    _sortDescending = value;
                    SortPropertyChanged?.Invoke(this, EventArgs.Empty);
                    UpdateInScopeItems();
                }
            }
        }

        // --- View ---

        /// <summary>Current view mode: "grid" or "graph".</summary>
        public string CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value ?? "grid";
                    ViewChanged?.Invoke(this, EventArgs.Empty);
                    UpdateLayout();
                }
            }
        }

        // --- Layout ---

        /// <summary>Current grid layout (when in grid view).</summary>
        public GridLayout? GridLayout => _currentGridLayout;

        /// <summary>Current histogram layout (when in graph view).</summary>
        public HistogramLayout? HistogramLayout => _currentHistogramLayout;

        /// <summary>Image provider for DZC-backed item thumbnails.</summary>
        public CollectionImageProvider? ImageProvider
        {
            get => _imageProvider;
            set => _imageProvider = value;
        }

        /// <summary>The loaded CXML collection source, if any.</summary>
        public CxmlCollectionSource? CollectionSource => _collectionSource;

        /// <summary>
        /// Item templates for zoom-based template selection. Matches Silverlight's ItemTemplates property.
        /// Templates are selected based on the rendered item width using <see cref="PivotViewerItemTemplateCollection.SelectTemplate"/>.
        /// </summary>
        public PivotViewerItemTemplateCollection ItemTemplates
        {
            get => _itemTemplates;
            set => _itemTemplates = value ?? new PivotViewerItemTemplateCollection();
        }

        /// <summary>
        /// Serialized filter state string. Setting this applies the encoded filters.
        /// Matches Silverlight's Filter property for state persistence.
        /// </summary>
        public string Filter
        {
            get => SerializeViewerState();
            set => SetViewerState(value ?? "");
        }

        /// <summary>
        /// Zoom level: 0.0 = fit all items, 1.0 = single item detail.
        /// Affects the item size in the grid layout.
        /// </summary>
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                var clamped = Math.Max(0.0, Math.Min(1.0, value));
                if (Math.Abs(_zoomLevel - clamped) > 0.001)
                {
                    _zoomLevel = clamped;
                    UpdateLayout();
                }
            }
        }

        /// <summary>Pan offset X (for scrolling through items when zoomed in).</summary>
        public double PanOffsetX => _panOffsetX;

        /// <summary>Pan offset Y.</summary>
        public double PanOffsetY => _panOffsetY;

        /// <summary>
        /// Text filter from the search box. Items not matching the search text
        /// are excluded from InScopeItems.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value ?? "";
                ApplySearchFilter();
            }
        }

        /// <summary>Pans by the given screen-space delta.</summary>
        public void Pan(double deltaX, double deltaY)
        {
            _panOffsetX += deltaX;
            _panOffsetY += deltaY;
            LayoutUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Zooms about a screen point, keeping the point under the cursor stable.
        /// Factor > 1 zooms in, &lt; 1 zooms out. The magnitude controls zoom speed.</summary>
        public void ZoomAbout(double factor, double screenX, double screenY)
        {
            double oldZoom = _zoomLevel;
            // Use factor proportionally: factor=2.0 means double the zoom, factor=0.5 means halve it
            double delta = (factor - 1.0) * 0.5; // Scale sensitivity (0.5 feels natural)
            double newZoom = Math.Max(0.0, Math.Min(1.0, _zoomLevel + delta));

            if (Math.Abs(newZoom - oldZoom) > 0.001)
            {
                // Adjust pan to keep the screen point stable
                // Before zoom, the screen point maps to world coordinate:
                // worldX = screenX - panOffsetX
                // After zoom, the layout changes. We need to adjust pan so
                // the same world point remains at the same screen position.
                double worldX = screenX - _panOffsetX;
                double worldY = screenY - _panOffsetY;

                _zoomLevel = newZoom;
                UpdateLayout();

                // The layout changed (items resized), so the world coordinate
                // at (worldX, worldY) may have shifted. Adjust pan offset.
                double oldScale = 1.0 - oldZoom;
                double newScale = 1.0 - newZoom;
                if (newScale > 1e-6)
                {
                    // World coords scale as 1/(1-zoom), so ratio = oldScale/newScale
                    double zoomRatio = oldScale / newScale;
                    double newWorldX = worldX * zoomRatio;
                    double newWorldY = worldY * zoomRatio;
                    _panOffsetX += worldX - newWorldX;
                    _panOffsetY += worldY - newWorldY;
                }
            }
        }

        /// <summary>
        /// Zooms and pans to center on the specified item, filling the viewport.
        /// The item is also selected.
        /// </summary>
        public void ZoomToItem(PivotViewerItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!_inScopeItems.Contains(item)) return;

            SelectedItem = item;

            // Cancel any active transition so GetItemBounds reads final positions
            _layoutTransition.CancelTransition();

            // Compute item bounds at zoom=0 to get the true "fit all" width
            _zoomLevel = 0;
            UpdateLayout();
            _layoutTransition.CancelTransition(); // snap to final layout immediately

            var fitAllBounds = GetItemBounds(item);

            if (fitAllBounds.Width <= 0 || fitAllBounds.Height <= 0)
                return;

            // How much we need to scale the fit-all size to fill 80% of viewport
            double targetWidth = _availableWidth * 0.8;
            double targetHeight = _availableHeight * 0.8;
            double scaleNeeded = Math.Min(targetWidth / fitAllBounds.Width, targetHeight / fitAllBounds.Height);

            // At zoom=0, item width = fitAllBounds.Width
            // At zoom=1, item width ≈ _availableWidth (one item fills view)
            // Solve for z: scaleNeeded * fitAll = fitAll*(1-z) + available*z
            double denominator = _availableWidth - fitAllBounds.Width;
            if (denominator > 1)
            {
                double desiredZoom = (scaleNeeded - 1) * fitAllBounds.Width / denominator;
                _zoomLevel = Math.Max(0.0, Math.Min(1.0, desiredZoom));
            }
            else
            {
                _zoomLevel = 0;
            }

            UpdateLayout();
            _layoutTransition.CancelTransition(); // snap again for accurate centering

            // Center on the item using final (non-transitioning) positions
            var bounds = GetItemBounds(item);
            _panOffsetX = _availableWidth / 2 - (bounds.X + bounds.Width / 2);
            _panOffsetY = _availableHeight / 2 - (bounds.Y + bounds.Height / 2);
            LayoutUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Selects the next item in the current in-scope list.
        /// If no item is selected, selects the first item.
        /// </summary>
        public void SelectNext()
        {
            if (_inScopeItems.Count == 0) return;
            if (SelectedItem == null)
            {
                SelectedItem = _inScopeItems[0];
                return;
            }
            int idx = IndexOf(_inScopeItems, SelectedItem);
            if (idx >= 0 && idx < _inScopeItems.Count - 1)
                SelectedItem = _inScopeItems[idx + 1];
        }

        /// <summary>
        /// Selects the previous item in the current in-scope list.
        /// If no item is selected, selects the last item.
        /// </summary>
        public void SelectPrevious()
        {
            if (_inScopeItems.Count == 0) return;
            if (SelectedItem == null)
            {
                SelectedItem = _inScopeItems[_inScopeItems.Count - 1];
                return;
            }
            int idx = IndexOf(_inScopeItems, SelectedItem);
            if (idx > 0)
                SelectedItem = _inScopeItems[idx - 1];
        }

        /// <summary>
        /// Selects the item in the row above the current selection in grid view.
        /// Uses the grid layout column count to compute offset.
        /// </summary>
        public void SelectUp()
        {
            if (_inScopeItems.Count == 0 || SelectedItem == null || GridLayout == null) return;
            int idx = IndexOf(_inScopeItems, SelectedItem);
            if (idx < 0) return;

            int cols = Math.Max(1, GridLayout.Columns);
            int newIdx = idx - cols;
            if (newIdx >= 0)
                SelectedItem = _inScopeItems[newIdx];
        }

        /// <summary>
        /// Selects the item in the row below the current selection in grid view.
        /// </summary>
        public void SelectDown()
        {
            if (_inScopeItems.Count == 0 || SelectedItem == null || GridLayout == null) return;
            int idx = IndexOf(_inScopeItems, SelectedItem);
            if (idx < 0) return;

            int cols = Math.Max(1, GridLayout.Columns);
            int newIdx = idx + cols;
            if (newIdx < _inScopeItems.Count)
                SelectedItem = _inScopeItems[newIdx];
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            SelectedItem = null;
        }

        // --- Events ---

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? FiltersChanged;
        public event EventHandler? SortPropertyChanged;
        public event EventHandler? ViewChanged;
        public event EventHandler? CollectionChanged;
        public event EventHandler? LayoutUpdated;
        public event EventHandler? InScopeItemsChanged;
        public event EventHandler<PivotViewerItemDoubleClickEventArgs>? ItemDoubleClicked;

        // --- Methods ---

        /// <summary>
        /// Loads a collection from a CxmlCollectionSource.
        /// </summary>
        public void LoadCollection(CxmlCollectionSource source)
        {
            _collectionSource = source;
            _allItems = new List<PivotViewerItem>(source.Items);
            _properties = new List<PivotViewerProperty>(source.ItemProperties);

            _filterEngine.SetSource(_allItems, _properties);
            _wordWheel.Build(_allItems, _properties);
            ApplySearchFilter(); // Re-apply search text to new data

            _filterPaneModel = new FilterPaneModel(_filterEngine, _properties);

            _selectedItem = null;
            _sortProperty = null;
            _sortDescending = false;

            UpdateInScopeItems();
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads items and properties directly (binding model, like SL5).
        /// </summary>
        public void LoadItems(
            IEnumerable<PivotViewerItem> items,
            IEnumerable<PivotViewerProperty> properties)
        {
            _allItems = new List<PivotViewerItem>(items);
            _properties = new List<PivotViewerProperty>(properties);

            _filterEngine.SetSource(_allItems, _properties);
            _wordWheel.Build(_allItems, _properties);
            ApplySearchFilter(); // Re-apply search text to new data

            _filterPaneModel = new FilterPaneModel(_filterEngine, _properties);

            _selectedItem = null;
            _sortProperty = null;
            _sortDescending = false;

            UpdateInScopeItems();
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Refreshes the current collection by re-processing all items.
        /// Preserves the current sort and view, but clears filters and selection.
        /// If loaded from CXML, re-reads from the stored source.
        /// </summary>
        public void Refresh()
        {
            if (_collectionSource != null)
            {
                _allItems = new List<PivotViewerItem>(_collectionSource.Items);
                _properties = new List<PivotViewerProperty>(_collectionSource.ItemProperties);
            }

            _filterEngine.SetSource(_allItems, _properties);
            _filterEngine.ClearAll();
            _wordWheel.Build(_allItems, _properties);
            ApplySearchFilter();

            _filterPaneModel = new FilterPaneModel(_filterEngine, _properties);
            _selectedItem = null;

            UpdateInScopeItems();
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the available size for layout computation.
        /// </summary>
        public void SetAvailableSize(double width, double height)
        {
            if (Math.Abs(_availableWidth - width) < 0.5 && Math.Abs(_availableHeight - height) < 0.5)
                return;
            _availableWidth = width;
            _availableHeight = height;
            UpdateLayout();
        }

        /// <summary>
        /// Performs a hit test at the given coordinates.
        /// </summary>
        public PivotViewerItem? HitTest(double x, double y)
        {
            if (_currentView == "graph" && _currentHistogramLayout != null)
                return _currentHistogramLayout.HitTest(x, y);

            return _currentGridLayout?.HitTest(x, y);
        }

        /// <summary>
        /// Notifies the controller that an item was double-clicked.
        /// Raises the <see cref="ItemDoubleClicked"/> event.
        /// </summary>
        public void NotifyItemDoubleClicked(PivotViewerItem item)
        {
            if (item == null) return;
            ItemDoubleClicked?.Invoke(this, new PivotViewerItemDoubleClickEventArgs(item));
        }

        /// <summary>
        /// Serializes the current viewer state (filters, sort, view, selection).
        /// </summary>
        public string SerializeViewerState()
        {
            var state = new ViewerState
            {
                ViewId = _currentView,
                SortPropertyId = _sortProperty?.Id,
                SortDescending = _sortDescending,
                SelectedItemId = _selectedItem?.Id,
                Predicates = _filterEngine.Predicates
            };

            return ViewerStateSerializer.Serialize(state);
        }

        /// <summary>
        /// Restores viewer state from a serialized string.
        /// </summary>
        public void SetViewerState(string stateString)
        {
            var state = ViewerStateSerializer.Deserialize(stateString);

            // Clear existing filters
            _filterEngine.ClearAll();

            // Apply deserialized string filters
            foreach (var pred in state.StringPredicates)
            {
                foreach (var v in pred.Values)
                    _filterEngine.AddStringFilter(pred.PropertyId, v);
            }

            // Apply deserialized range filters
            foreach (var (propertyId, expression) in state.RangePredicates)
            {
                TryApplyRangeFilter(propertyId, expression);
            }

            // Apply view
            if (!string.IsNullOrEmpty(state.ViewId))
                CurrentView = state.ViewId;

            // Apply sort
            if (!string.IsNullOrEmpty(state.SortPropertyId))
                SortProperty = _properties.FirstOrDefault(p => p.Id == state.SortPropertyId);
            SortDescending = state.SortDescending;

            // Apply selection
            if (!string.IsNullOrEmpty(state.SelectedItemId))
                SelectedItem = _allItems.FirstOrDefault(i => i.Id == state.SelectedItemId);

            UpdateInScopeItems();
        }

        private void TryApplyRangeFilter(string propertyId, string expression)
        {
            // Parse format: GE(min)AND(LE(max))
            try
            {
                int geStart = expression.IndexOf("GE(") + 3;
                int geEnd = expression.IndexOf(")AND(");
                if (geStart < 3 || geEnd < 0) return;
                string minStr = expression.Substring(geStart, geEnd - geStart);

                int leStart = expression.IndexOf("LE(") + 3;
                int leEnd = expression.LastIndexOf("))");
                if (leStart < 3 || leEnd < 0) return;
                string maxStr = expression.Substring(leStart, leEnd - leStart);

                // Try numeric first, then DateTime
                if (double.TryParse(minStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numMin)
                 && double.TryParse(maxStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numMax))
                {
                    _filterEngine.AddNumericRangeFilter(propertyId, numMin, numMax);
                }
                else if (DateTime.TryParse(minStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dtMin)
                      && DateTime.TryParse(maxStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dtMax))
                {
                    _filterEngine.AddDateTimeRangeFilter(propertyId, dtMin, dtMax);
                }
            }
            catch { /* Ignore malformed expressions */ }
        }

        /// <summary>
        /// Gets in-scope counts for the filter pane (how many items match each value).
        /// </summary>
        public Dictionary<string, int> GetFilterCounts(string propertyId)
        {
            return _filterEngine.ComputeInScopeCounts(propertyId);
        }

        /// <summary>
        /// Search for items by text prefix (word wheel).
        /// </summary>
        public IReadOnlyList<SearchResult> Search(string text)
        {
            return _wordWheel.Search(text);
        }

        /// <summary>
        /// Updates animation state. Call from the render loop.
        /// </summary>
        public bool Update(TimeSpan deltaTime)
        {
            return _layoutTransition.Update(deltaTime.TotalSeconds);
        }

        /// <summary>
        /// Gets the interpolated layout position for an item during transitions.
        /// Returns the final position if no transition is active.
        /// </summary>
        public (double X, double Y, double Width, double Height) GetItemBounds(PivotViewerItem item)
        {
            if (_layoutTransition.IsAnimating && _currentGridLayout != null)
            {
                var positions = _layoutTransition.GetCurrentPositions();
                var pos = positions.FirstOrDefault(p => p.Item == item);
                if (pos.Item != null)
                    return (pos.X, pos.Y, pos.Width, pos.Height);
            }

            if (_currentGridLayout != null)
            {
                var pos = _currentGridLayout.Positions.FirstOrDefault(p => p.Item == item);
                if (pos.Item != null)
                    return (pos.X, pos.Y, pos.Width, pos.Height);
            }

            if (_currentHistogramLayout != null)
            {
                var pos = _currentHistogramLayout.AllPositions.FirstOrDefault(p => p.Item == item);
                if (pos.Item != null)
                    return (pos.X, pos.Y, pos.Width, pos.Height);
            }

            return (0, 0, 0, 0);
        }

        // --- Internal ---

        private void ApplySearchFilter()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                _textFilteredItemIds = null;
                _searchFilteredItemsCache = null; // null = use _allItems
            }
            else
            {
                var matchingItems = _wordWheel.GetMatchingItems(_searchText);
                _textFilteredItemIds = new HashSet<string>(matchingItems.Select(i => i.Id));
                _searchFilteredItemsCache = _allItems.Where(i => _textFilteredItemIds.Contains(i.Id)).ToList();
            }
            UpdateInScopeItems();
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnFiltersChanged()
        {
            UpdateInScopeItems();
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateInScopeItems()
        {
            var filtered = _filterEngine.GetFilteredItems();

            // Apply text search filter if active
            if (_textFilteredItemIds != null)
            {
                filtered = filtered.Where(i => _textFilteredItemIds.Contains(i.Id)).ToList();
            }

            // Apply sort
            if (_sortProperty != null)
            {
                var sortedList = new List<PivotViewerItem>(filtered);
                sortedList.Sort((a, b) =>
                {
                    var va = a[_sortProperty.Id];
                    var vb = b[_sortProperty.Id];

                    if (va == null && vb == null) return 0;
                    if (va == null) return 1;
                    if (vb == null) return -1;

                    var valA = va.Count > 0 ? va[0] : null;
                    var valB = vb.Count > 0 ? vb[0] : null;

                    if (valA == null && valB == null) return 0;
                    if (valA == null) return 1;
                    if (valB == null) return -1;

                    if (valA is IComparable ca)
                    {
                        try { return ca.CompareTo(valB); }
                        catch (ArgumentException) { }
                    }

                    return string.Compare(valA.ToString(), valB.ToString(), StringComparison.OrdinalIgnoreCase);
                });

                if (_sortDescending)
                    sortedList.Reverse();
                _inScopeItems = sortedList;
            }
            else
            {
                _inScopeItems = filtered;
            }

            // Deselect if selected item is no longer in scope
            if (_selectedItem != null && !_inScopeItems.Contains(_selectedItem))
                SelectedItem = null;

            UpdateLayout();
            InScopeItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _updatingLayout;

        private void UpdateLayout()
        {
            if (_updatingLayout) return;
            _updatingLayout = true;
            try
            {
                if (_inScopeItems.Count == 0)
                {
                    _currentGridLayout = null;
                    _currentHistogramLayout = null;
                    LayoutUpdated?.Invoke(this, EventArgs.Empty);
                    return;
                }

                GridLayout? oldGrid = _currentGridLayout;

                if (_currentView == "graph" && _sortProperty != null)
                {
                    _currentHistogramLayout = _layoutEngine.ComputeHistogramLayout(
                        _inScopeItems, _sortProperty.Id, _availableWidth, _availableHeight);
                    _currentGridLayout = null;
                }
                else
                {
                    var newLayout = _zoomLevel > 0.01
                        ? _layoutEngine.ComputeZoomedLayout(
                            _inScopeItems, _availableWidth, _availableHeight, _zoomLevel)
                        : _layoutEngine.ComputeLayout(
                            _inScopeItems, _availableWidth, _availableHeight);

                    // Start transition animation if we had a previous layout
                    if (oldGrid != null && oldGrid.Positions.Length > 0)
                        _layoutTransition.BeginTransition(oldGrid.Positions, newLayout.Positions);

                    _currentGridLayout = newLayout;
                    _currentHistogramLayout = null;
                }

                LayoutUpdated?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _updatingLayout = false;
            }
        }

        /// <summary>Disposes resources held by the controller.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _filterEngine.FiltersChanged -= _onFiltersChangedHandler;
                _layoutTransition.CancelTransition();
                _imageProvider?.Dispose();
                _imageProvider = null;
            }
        }

        private static int IndexOf(IReadOnlyList<PivotViewerItem> list, PivotViewerItem item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], item)) return i;
            }
            return -1;
        }
    }
    /// <summary>Event args for selection changes.</summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(PivotViewerItem? oldItem, PivotViewerItem? newItem)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }

        public PivotViewerItem? OldItem { get; }
        public PivotViewerItem? NewItem { get; }
    }
}
