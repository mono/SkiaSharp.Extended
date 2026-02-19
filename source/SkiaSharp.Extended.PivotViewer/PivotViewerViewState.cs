using System.Collections.Generic;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Mutable view state for PivotViewer rendering. Shared between MAUI/Blazor and the renderer.
    /// Contains UI state that is not part of the core controller (visual-only concerns).
    /// </summary>
    public class PivotViewerViewState
    {
        /// <summary>Whether the filter pane is visible.</summary>
        public bool IsFilterPaneVisible { get; set; } = true;

        /// <summary>Vertical scroll offset within the filter pane.</summary>
        public double FilterScrollOffset { get; set; }

        /// <summary>Total content height of the filter pane (set by renderer after each paint).</summary>
        public double FilterContentHeight { get; set; }

        /// <summary>Which filter categories are expanded to show all values.</summary>
        public HashSet<string> ExpandedFilterCategories { get; } = new HashSet<string>();

        /// <summary>Whether the sort dropdown overlay is showing.</summary>
        public bool IsSortDropdownVisible { get; set; }

        /// <summary>Item currently under the pointer (for hover highlight).</summary>
        public PivotViewerItem? HoverItem { get; set; }

        /// <summary>Vertical scroll offset within the detail pane.</summary>
        public double DetailScrollOffset { get; set; }

        /// <summary>Total content height of the detail pane (set by renderer after each paint).</summary>
        public double DetailContentHeight { get; set; }

        /// <summary>Clamps filter scroll offset to valid bounds.</summary>
        public void ClampFilterScroll(double viewportHeight)
        {
            double maxScroll = FilterContentHeight - viewportHeight;
            if (maxScroll < 0) maxScroll = 0;
            if (FilterScrollOffset < 0) FilterScrollOffset = 0;
            if (FilterScrollOffset > maxScroll) FilterScrollOffset = maxScroll;
        }

        /// <summary>Clamps detail pane scroll offset to valid bounds.</summary>
        public void ClampDetailScroll(double viewportHeight)
        {
            double maxScroll = DetailContentHeight - viewportHeight;
            if (maxScroll < 0) maxScroll = 0;
            if (DetailScrollOffset < 0) DetailScrollOffset = 0;
            if (DetailScrollOffset > maxScroll) DetailScrollOffset = maxScroll;
        }
    }
}
