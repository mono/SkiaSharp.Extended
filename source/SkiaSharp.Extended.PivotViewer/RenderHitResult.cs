using System;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Result of a hit-test on the rendered PivotViewer surface.
    /// The UI layer (MAUI/Blazor) dispatches based on the Type.
    /// </summary>
    public class RenderHitResult
    {
        public RenderHitType Type { get; set; } = RenderHitType.None;

        /// <summary>The item that was hit (for Item, ItemDoubleClick types).</summary>
        public PivotViewerItem? Item { get; set; }

        /// <summary>Filter property ID (for FilterCheckbox, FilterHistogramBar types).</summary>
        public string? FilterPropertyId { get; set; }

        /// <summary>Filter value (for FilterCheckbox type).</summary>
        public string? FilterValue { get; set; }

        /// <summary>Numeric range min (for FilterHistogramBar type).</summary>
        public double? RangeMin { get; set; }

        /// <summary>Numeric range max (for FilterHistogramBar type).</summary>
        public double? RangeMax { get; set; }

        /// <summary>DateTime range min (for DateTimeHistogramBar type).</summary>
        public DateTime? DateRangeMin { get; set; }

        /// <summary>DateTime range max (for DateTimeHistogramBar type).</summary>
        public DateTime? DateRangeMax { get; set; }

        /// <summary>Link URI (for DetailLink type).</summary>
        public string? LinkUri { get; set; }

        /// <summary>Sort dropdown row index (for SortDropdownRow type).</summary>
        public int SortRowIndex { get; set; } = -1;

        /// <summary>Toggle category name (for FilterCategoryToggle type).</summary>
        public string? CategoryName { get; set; }

        public static RenderHitResult None => new RenderHitResult { Type = RenderHitType.None };
    }

    /// <summary>
    /// Types of hit targets on the PivotViewer surface.
    /// </summary>
    public enum RenderHitType
    {
        /// <summary>No hit — empty area.</summary>
        None,

        /// <summary>An item in the grid or histogram was tapped.</summary>
        Item,

        /// <summary>Toggle filter pane visibility.</summary>
        FilterToggle,

        /// <summary>Switch to grid view.</summary>
        ViewGrid,

        /// <summary>Switch to graph view.</summary>
        ViewGraph,

        /// <summary>Open sort dropdown.</summary>
        SortDropdown,

        /// <summary>A row in the sort dropdown.</summary>
        SortDropdownRow,

        /// <summary>A string filter checkbox in the filter pane.</summary>
        FilterCheckbox,

        /// <summary>A numeric histogram bar in the filter pane.</summary>
        FilterNumericHistogramBar,

        /// <summary>A datetime histogram bar in the filter pane.</summary>
        FilterDateTimeHistogramBar,

        /// <summary>The "Clear All Filters" button.</summary>
        ClearAllFilters,

        /// <summary>A "Show all N values" / "Show less" toggle.</summary>
        FilterCategoryToggle,

        /// <summary>A link in the detail pane.</summary>
        DetailLink,

        /// <summary>A filterable facet value in the detail pane.</summary>
        DetailFacetFilter,

        /// <summary>A histogram column label in graph view (tap to filter).</summary>
        GraphColumnLabel,
    }
}
