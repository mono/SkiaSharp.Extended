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
        private FilterPaneModel? _filterPaneModel;
        private DetailPaneModel? _detailPaneModel;
        private CollectionImageProvider? _imageProvider;
        private CxmlCollectionSource? _collectionSource;
        private bool _disposed;

        private List<PivotViewerItem> _allItems = new List<PivotViewerItem>();
        private List<PivotViewerProperty> _properties = new List<PivotViewerProperty>();
        private IReadOnlyList<PivotViewerItem> _inScopeItems = Array.Empty<PivotViewerItem>();
        private GridLayout? _currentGridLayout;
        private HistogramLayout? _currentHistogramLayout;

        private PivotViewerItem? _selectedItem;
        private PivotViewerProperty? _sortProperty;
        private string _currentView = "grid";
        private double _availableWidth = 800;
        private double _availableHeight = 600;
        private double _zoomLevel = 0.0; // 0.0 = fit all, 1.0 = single item
        private double _panOffsetX;
        private double _panOffsetY;

        public PivotViewerController()
        {
            _filterEngine = new FilterEngine();
            _layoutEngine = new GridLayoutEngine();
            _wordWheel = new WordWheelIndex();
            _layoutTransition = new LayoutTransitionManager();

            _filterEngine.FiltersChanged += (s, e) => OnFiltersChanged();
        }

        // --- Data ---

        /// <summary>All items in the collection.</summary>
        public IReadOnlyList<PivotViewerItem> Items => _allItems;

        /// <summary>All property definitions.</summary>
        public IReadOnlyList<PivotViewerProperty> Properties => _properties;

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
                    UpdateLayout();
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

        /// <summary>Pans by the given screen-space delta.</summary>
        public void Pan(double deltaX, double deltaY)
        {
            _panOffsetX += deltaX;
            _panOffsetY += deltaY;
            LayoutUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Zooms about a screen point, keeping the point under the cursor stable.</summary>
        public void ZoomAbout(double factor, double screenX, double screenY)
        {
            double oldZoom = _zoomLevel;
            double newZoom = Math.Max(0.0, Math.Min(1.0, _zoomLevel + (factor > 1 ? 0.1 : -0.1)));

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
                double zoomRatio = (1.0 - newZoom) / (1.0 - oldZoom + 0.001);
                double newWorldX = worldX * zoomRatio;
                double newWorldY = worldY * zoomRatio;
                _panOffsetX += worldX - newWorldX;
                _panOffsetY += worldY - newWorldY;
            }
        }

        // --- Events ---

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? FiltersChanged;
        public event EventHandler? SortPropertyChanged;
        public event EventHandler? ViewChanged;
        public event EventHandler? CollectionChanged;
        public event EventHandler? LayoutUpdated;

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

            _filterPaneModel = new FilterPaneModel(_filterEngine, _properties);

            _selectedItem = null;
            _sortProperty = null;

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

            _filterPaneModel = new FilterPaneModel(_filterEngine, _properties);

            _selectedItem = null;
            _sortProperty = null;

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
        /// Serializes the current viewer state (filters, sort, view, selection).
        /// </summary>
        public string SerializeViewerState()
        {
            var state = new ViewerState
            {
                ViewId = _currentView,
                SortPropertyId = _sortProperty?.Id,
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
                _currentView = state.ViewId;

            // Apply sort
            if (!string.IsNullOrEmpty(state.SortPropertyId))
                _sortProperty = _properties.FirstOrDefault(p => p.Id == state.SortPropertyId);

            // Apply selection
            if (!string.IsNullOrEmpty(state.SelectedItemId))
                _selectedItem = _allItems.FirstOrDefault(i => i.Id == state.SelectedItemId);

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

            return (0, 0, 0, 0);
        }

        // --- Internal ---

        private void OnFiltersChanged()
        {
            UpdateInScopeItems();
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateInScopeItems()
        {
            var filtered = _filterEngine.GetFilteredItems();

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
                        return ca.CompareTo(valB);

                    return string.Compare(valA.ToString(), valB.ToString(), StringComparison.OrdinalIgnoreCase);
                });

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
        }

        private void UpdateLayout()
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

        /// <summary>Disposes resources held by the controller.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _layoutTransition.CancelTransition();
                _imageProvider?.Dispose();
                _imageProvider = null;
            }
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
