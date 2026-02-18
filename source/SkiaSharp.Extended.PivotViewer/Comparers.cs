using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Sorts string values by their frequency (cardinality) in descending order,
    /// then alphabetically for ties. Used in Silverlight's PivotViewerStringProperty.Sorts.
    /// </summary>
    public class PivotViewerPropertyCardinalityComparer : IComparer<string>
    {
        private readonly Dictionary<string, int> _counts;

        /// <summary>
        /// Create a comparer from a set of values (counts occurrences).
        /// </summary>
        public PivotViewerPropertyCardinalityComparer(IEnumerable<string> values)
        {
            _counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in values)
            {
                if (_counts.ContainsKey(value))
                    _counts[value]++;
                else
                    _counts[value] = 1;
            }
        }

        /// <summary>
        /// Create a comparer with pre-computed counts.
        /// </summary>
        public PivotViewerPropertyCardinalityComparer(IDictionary<string, int> counts)
        {
            _counts = new Dictionary<string, int>(counts, StringComparer.OrdinalIgnoreCase);
        }

        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int countX = _counts.ContainsKey(x) ? _counts[x] : 0;
            int countY = _counts.ContainsKey(y) ? _counts[y] : 0;

            // Higher count first
            int result = countY.CompareTo(countX);
            if (result != 0) return result;

            // Alphabetical for ties
            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Sorts string values by a predefined order from CXML SortOrder extension.
    /// Values not in the predefined order sort after all known values, alphabetically.
    /// </summary>
    public class CustomSortOrderComparer : IComparer<string>
    {
        private readonly Dictionary<string, int> _order;

        /// <summary>
        /// Creates a comparer from an ordered list of values.
        /// The first value in the list sorts first.
        /// </summary>
        public CustomSortOrderComparer(IList<string> orderedValues)
        {
            _order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < orderedValues.Count; i++)
            {
                // First occurrence wins — ignore duplicates
                if (!_order.ContainsKey(orderedValues[i]))
                    _order[orderedValues[i]] = i;
            }
        }

        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            bool hasX = _order.TryGetValue(x, out int indexX);
            bool hasY = _order.TryGetValue(y, out int indexY);

            if (hasX && hasY) return indexX.CompareTo(indexY);
            if (hasX) return -1; // Known values sort before unknown
            if (hasY) return 1;

            // Both unknown: alphabetical
            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }
}
