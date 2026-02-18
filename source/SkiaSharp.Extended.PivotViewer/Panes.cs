using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Model for the filter pane that shows facet categories with checkboxes,
    /// histograms, and range sliders. This is the core model — MAUI/Blazor
    /// renders it as appropriate.
    /// </summary>
    public class FilterPaneModel : INotifyPropertyChanged
    {
        private readonly FilterEngine _filterEngine;
        private readonly IReadOnlyList<PivotViewerProperty> _properties;

        public FilterPaneModel(FilterEngine filterEngine, IReadOnlyList<PivotViewerProperty> properties)
        {
            _filterEngine = filterEngine ?? throw new ArgumentNullException(nameof(filterEngine));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// Gets the filterable categories (properties with CanFilter flag).
        /// </summary>
        public IReadOnlyList<FilterCategory> GetCategories(IReadOnlyList<PivotViewerItem> allItems)
        {
            var categories = new List<FilterCategory>();

            foreach (var prop in _properties)
            {
                if (!prop.Options.HasFlag(PivotViewerPropertyOptions.CanFilter))
                    continue;

                var category = new FilterCategory(prop);

                // Get in-scope counts
                var counts = _filterEngine.ComputeInScopeCounts(prop.Id, allItems);
                category.ValueCounts = counts;

                // Get active filters for this property
                category.ActiveFilters = _filterEngine.GetActiveFilters(prop.Id);

                categories.Add(category);
            }

            return categories;
        }

        /// <summary>Toggle a string filter value.</summary>
        public void ToggleStringFilter(string propertyId, string value)
        {
            if (_filterEngine.HasStringFilter(propertyId, value))
                _filterEngine.RemoveStringFilter(propertyId, value);
            else
                _filterEngine.AddStringFilter(propertyId, value);

            OnPropertyChanged(nameof(GetCategories));
        }

        /// <summary>Set a numeric range filter.</summary>
        public void SetNumericRangeFilter(string propertyId, double min, double max)
        {
            _filterEngine.RemoveAllFilters(propertyId);
            _filterEngine.AddNumericRangeFilter(propertyId, min, max);
            OnPropertyChanged(nameof(GetCategories));
        }

        /// <summary>Set a datetime range filter.</summary>
        public void SetDateTimeRangeFilter(string propertyId, DateTime start, DateTime end)
        {
            _filterEngine.RemoveAllFilters(propertyId);
            _filterEngine.AddDateTimeRangeFilter(propertyId, start, end);
            OnPropertyChanged(nameof(GetCategories));
        }

        /// <summary>Clear all filters for a specific property.</summary>
        public void ClearPropertyFilters(string propertyId)
        {
            _filterEngine.RemoveAllFilters(propertyId);
            OnPropertyChanged(nameof(GetCategories));
        }

        /// <summary>Clear all filters.</summary>
        public void ClearAllFilters()
        {
            _filterEngine.ClearAll();
            OnPropertyChanged(nameof(GetCategories));
        }

        /// <summary>Whether any filters are active.</summary>
        public bool HasActiveFilters => _filterEngine.HasActiveFilters;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// A single filter category in the filter pane.
    /// </summary>
    public class FilterCategory
    {
        public FilterCategory(PivotViewerProperty property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        /// <summary>The property this category represents.</summary>
        public PivotViewerProperty Property { get; }

        /// <summary>Value → in-scope count pairs.</summary>
        public IDictionary<string, int>? ValueCounts { get; set; }

        /// <summary>Currently active filter values for this property.</summary>
        public IReadOnlyList<string>? ActiveFilters { get; set; }

        /// <summary>Whether this category has active filters.</summary>
        public bool IsFiltered => ActiveFilters != null && ActiveFilters.Count > 0;
    }

    /// <summary>
    /// Model for the detail pane that shows information about the selected item.
    /// </summary>
    public class DetailPaneModel : INotifyPropertyChanged
    {
        private PivotViewerItem? _selectedItem;
        private bool _isExpanded = true;
        private bool _isShowing;

        /// <summary>The currently selected item.</summary>
        public PivotViewerItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    _isShowing = value != null;
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(IsShowing));
                    OnPropertyChanged(nameof(FacetValues));
                }
            }
        }

        /// <summary>Whether the detail pane is expanded.</summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        /// <summary>Whether the detail pane is showing (has a selected item).</summary>
        public bool IsShowing
        {
            get => _isShowing;
            set { _isShowing = value; OnPropertyChanged(nameof(IsShowing)); }
        }

        /// <summary>
        /// Get the facet values for the selected item, suitable for display.
        /// Returns (propertyDisplayName, formattedValues) pairs.
        /// </summary>
        public IReadOnlyList<FacetDisplay> FacetValues
        {
            get
            {
                if (_selectedItem == null)
                    return Array.Empty<FacetDisplay>();

                var results = new List<FacetDisplay>();
                foreach (var prop in _selectedItem.Properties)
                {
                    if (prop.Options.HasFlag(PivotViewerPropertyOptions.Private))
                        continue;

                    var values = _selectedItem[prop];
                    if (values == null || values.Count == 0)
                        continue;

                    var formatted = new List<string>();
                    foreach (var val in values)
                    {
                        if (val is PivotViewerHyperlink link)
                            formatted.Add(link.Text);
                        else if (val != null)
                            formatted.Add(FormatValue(val, prop));
                        else
                            formatted.Add("");
                    }

                    results.Add(new FacetDisplay(prop.DisplayName ?? prop.Id, formatted, prop));
                }

                return results;
            }
        }

        /// <summary>Raised when a link in the detail pane is clicked.</summary>
        public event EventHandler<PivotViewerLinkEventArgs>? LinkClicked;

        /// <summary>Raised when a filter should be applied from the detail pane.</summary>
        public event EventHandler<PivotViewerFilterEventArgs>? ApplyFilter;

        public void OnLinkClicked(Uri uri)
        {
            var args = new PivotViewerLinkEventArgs(uri);
            LinkClicked?.Invoke(this, args);
        }

        public void OnApplyFilter(string filter)
        {
            var args = new PivotViewerFilterEventArgs(filter);
            ApplyFilter?.Invoke(this, args);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static string FormatValue(object value, PivotViewerProperty property)
        {
            if (property.Format != null && value is IFormattable formattable)
            {
                try { return formattable.ToString(property.Format, null); }
                catch { /* fallback */ }
            }
            return value.ToString() ?? "";
        }
    }

    /// <summary>
    /// A facet display entry for the detail pane.
    /// </summary>
    public class FacetDisplay
    {
        public FacetDisplay(string displayName, IReadOnlyList<string> values, PivotViewerProperty property)
        {
            DisplayName = displayName;
            Values = values;
            Property = property;
        }

        public string DisplayName { get; }
        public IReadOnlyList<string> Values { get; }
        public PivotViewerProperty Property { get; }
    }

    /// <summary>
    /// Configuration for the default detail pane appearance.
    /// Matches Silverlight's PivotViewerDefaultDetails class for controlling
    /// which sections are visible in the item detail view.
    /// </summary>
    public class PivotViewerDefaultDetails : INotifyPropertyChanged
    {
        private bool _isNameHidden;
        private bool _isDescriptionHidden;
        private bool _isFacetCategoriesHidden;
        private bool _isRelatedCollectionsHidden;
        private bool _isCopyrightHidden;

        /// <summary>Whether the item name is hidden in the detail pane.</summary>
        public bool IsNameHidden
        {
            get => _isNameHidden;
            set { _isNameHidden = value; OnPropertyChanged(nameof(IsNameHidden)); }
        }

        /// <summary>Whether the item description is hidden.</summary>
        public bool IsDescriptionHidden
        {
            get => _isDescriptionHidden;
            set { _isDescriptionHidden = value; OnPropertyChanged(nameof(IsDescriptionHidden)); }
        }

        /// <summary>Whether the facet categories section is hidden.</summary>
        public bool IsFacetCategoriesHidden
        {
            get => _isFacetCategoriesHidden;
            set { _isFacetCategoriesHidden = value; OnPropertyChanged(nameof(IsFacetCategoriesHidden)); }
        }

        /// <summary>Whether the related collections section is hidden.</summary>
        public bool IsRelatedCollectionsHidden
        {
            get => _isRelatedCollectionsHidden;
            set { _isRelatedCollectionsHidden = value; OnPropertyChanged(nameof(IsRelatedCollectionsHidden)); }
        }

        /// <summary>Whether the copyright section is hidden.</summary>
        public bool IsCopyrightHidden
        {
            get => _isCopyrightHidden;
            set { _isCopyrightHidden = value; OnPropertyChanged(nameof(IsCopyrightHidden)); }
        }

        /// <summary>Raised when a link in the detail pane is clicked.</summary>
        public event EventHandler<PivotViewerLinkEventArgs>? LinkClicked;

        /// <summary>Raised when a filter action is triggered from the detail pane.</summary>
        public event EventHandler<PivotViewerFilterEventArgs>? ApplyFilter;

        /// <summary>Triggers the LinkClicked event.</summary>
        public void OnLinkClicked(Uri uri)
        {
            var args = new PivotViewerLinkEventArgs(uri);
            LinkClicked?.Invoke(this, args);
        }

        /// <summary>Triggers the ApplyFilter event.</summary>
        public void OnApplyFilter(string filter)
        {
            var args = new PivotViewerFilterEventArgs(filter);
            ApplyFilter?.Invoke(this, args);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
