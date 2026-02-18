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
}
