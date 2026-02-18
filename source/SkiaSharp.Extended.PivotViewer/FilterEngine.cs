using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Manages filtering for a PivotViewer collection. Filters items by facet values
    /// with AND across categories and OR within a category (matching Silverlight behavior).
    /// </summary>
    public class FilterEngine
    {
        private readonly List<FilterPredicate> _predicates = new List<FilterPredicate>();
        private IReadOnlyList<PivotViewerItem> _allItems = Array.Empty<PivotViewerItem>();
        private IReadOnlyList<PivotViewerProperty> _properties = Array.Empty<PivotViewerProperty>();

        /// <summary>Active filter predicates.</summary>
        public IReadOnlyList<FilterPredicate> Predicates => _predicates;

        /// <summary>Fired when filters change.</summary>
        public event EventHandler? FiltersChanged;

        /// <summary>
        /// Sets the source data for filtering.
        /// </summary>
        public void SetSource(IReadOnlyList<PivotViewerItem> items, IReadOnlyList<PivotViewerProperty> properties)
        {
            _allItems = items ?? throw new ArgumentNullException(nameof(items));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// Adds a filter predicate for a string/text property.
        /// Multiple values for the same property use OR logic.
        /// Multiple properties use AND logic.
        /// </summary>
        public void AddStringFilter(string propertyId, string value)
        {
            var existing = _predicates.FirstOrDefault(p =>
                p.PropertyId == propertyId && p is StringFilterPredicate);

            if (existing is StringFilterPredicate sfp)
            {
                sfp.AddValue(value);
            }
            else
            {
                var pred = new StringFilterPredicate(propertyId);
                pred.AddValue(value);
                _predicates.Add(pred);
            }

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Removes a specific string filter value. If the last value is removed, removes the predicate.
        /// </summary>
        public void RemoveStringFilter(string propertyId, string value)
        {
            var existing = _predicates.FirstOrDefault(p =>
                p.PropertyId == propertyId && p is StringFilterPredicate) as StringFilterPredicate;

            if (existing != null)
            {
                existing.RemoveValue(value);
                if (existing.Values.Count == 0)
                    _predicates.Remove(existing);
            }

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a numeric range filter.
        /// </summary>
        public void AddNumericRangeFilter(string propertyId, double min, double max)
        {
            _predicates.RemoveAll(p => p.PropertyId == propertyId && p is NumericRangeFilterPredicate);
            _predicates.Add(new NumericRangeFilterPredicate(propertyId, min, max));
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a DateTime range filter.
        /// </summary>
        public void AddDateTimeRangeFilter(string propertyId, DateTime min, DateTime max)
        {
            _predicates.RemoveAll(p => p.PropertyId == propertyId && p is DateTimeRangeFilterPredicate);
            _predicates.Add(new DateTimeRangeFilterPredicate(propertyId, min, max));
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Removes all filters for a given property.
        /// </summary>
        public void RemoveFilter(string propertyId)
        {
            _predicates.RemoveAll(p => p.PropertyId == propertyId);
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        public void ClearAll()
        {
            _predicates.Clear();
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns the items that match all active filter predicates (AND across properties).
        /// </summary>
        public IReadOnlyList<PivotViewerItem> GetFilteredItems()
        {
            if (_predicates.Count == 0)
                return _allItems;

            var result = new List<PivotViewerItem>();

            foreach (var item in _allItems)
            {
                bool matchesAll = true;
                foreach (var predicate in _predicates)
                {
                    if (!predicate.Matches(item))
                    {
                        matchesAll = false;
                        break;
                    }
                }

                if (matchesAll)
                    result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Computes in-scope counts for a given property, considering all OTHER active filters.
        /// This matches Silverlight's FacetManager.ComputeInScopeCounts behavior:
        /// for each possible value of a property, count how many items would match if 
        /// the user selected that value (with all other filters still applied).
        /// </summary>
        public Dictionary<string, int> ComputeInScopeCounts(string propertyId)
        {
            // Get items filtered by ALL other predicates (excluding this property)
            var otherPredicates = _predicates.Where(p => p.PropertyId != propertyId).ToList();
            var inScopeItems = _allItems.Where(item =>
                otherPredicates.All(p => p.Matches(item))).ToList();

            var counts = new Dictionary<string, int>();

            foreach (var item in inScopeItems)
            {
                var values = item[propertyId];
                if (values == null) continue;

                foreach (var value in values)
                {
                    string key = value?.ToString() ?? "";
                    if (counts.ContainsKey(key))
                        counts[key]++;
                    else
                        counts[key] = 1;
                }
            }

            return counts;
        }

        /// <summary>
        /// Computes histogram buckets for a numeric property.
        /// </summary>
        public IReadOnlyList<HistogramBucket> ComputeNumericHistogram(
            string propertyId, IReadOnlyList<PivotViewerItem> items, int maxBuckets = 15)
        {
            var values = new List<double>();
            foreach (var item in items)
            {
                var vals = item[propertyId];
                if (vals == null) continue;
                foreach (var v in vals)
                {
                    if (v is double d) values.Add(d);
                    else if (v is int i) values.Add(i);
                    else if (v is float f) values.Add(f);
                    else if (v is decimal dec) values.Add((double)dec);
                }
            }

            if (values.Count == 0)
                return Array.Empty<HistogramBucket>();

            values.Sort();
            double min = values[0];
            double max = values[values.Count - 1];

            if (Math.Abs(max - min) < double.Epsilon)
            {
                return new[] { new HistogramBucket(min.ToString(), min, max, values.Count) };
            }

            double range = max - min;
            double bucketSize = range / maxBuckets;

            // Round bucket size to a nice number
            bucketSize = NiceNumber(bucketSize);
            double niceMin = Math.Floor(min / bucketSize) * bucketSize;
            double niceMax = Math.Ceiling(max / bucketSize) * bucketSize;

            var buckets = new List<HistogramBucket>();
            for (double start = niceMin; start < niceMax; start += bucketSize)
            {
                double end = start + bucketSize;
                int count = values.Count(v => v >= start && v < end);
                if (Math.Abs(end - niceMax) < double.Epsilon)
                    count = values.Count(v => v >= start && v <= end);

                if (count > 0 || buckets.Count > 0) // Include empty buckets after first non-empty
                {
                    buckets.Add(new HistogramBucket(
                        FormatBucketLabel(start, end), start, end, count));
                }
            }

            return buckets;
        }

        private static double NiceNumber(double value)
        {
            double exp = Math.Floor(Math.Log10(value));
            double frac = value / Math.Pow(10, exp);

            double nice;
            if (frac <= 1.5) nice = 1;
            else if (frac <= 3) nice = 2;
            else if (frac <= 7) nice = 5;
            else nice = 10;

            return nice * Math.Pow(10, exp);
        }

        private static string FormatBucketLabel(double start, double end)
        {
            if (Math.Abs(end - start - 1) < double.Epsilon && start == Math.Floor(start))
                return start.ToString("G");
            return $"{start:G}–{end:G}";
        }
    }

    /// <summary>A histogram bucket with label, range, and item count.</summary>
    public readonly struct HistogramBucket
    {
        public HistogramBucket(string label, double min, double max, int count)
        {
            Label = label;
            Min = min;
            Max = max;
            Count = count;
        }

        public string Label { get; }
        public double Min { get; }
        public double Max { get; }
        public int Count { get; }
    }

    /// <summary>Base class for filter predicates.</summary>
    public abstract class FilterPredicate
    {
        protected FilterPredicate(string propertyId) => PropertyId = propertyId;
        public string PropertyId { get; }
        public abstract bool Matches(PivotViewerItem item);
    }

    /// <summary>Filters items by matching any of the selected string values (OR logic).</summary>
    public class StringFilterPredicate : FilterPredicate
    {
        private readonly HashSet<string> _values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public StringFilterPredicate(string propertyId) : base(propertyId) { }

        public IReadOnlyCollection<string> Values => _values;

        public void AddValue(string value) => _values.Add(value);
        public void RemoveValue(string value) => _values.Remove(value);

        public override bool Matches(PivotViewerItem item)
        {
            if (_values.Count == 0) return true;

            var itemValues = item[PropertyId];
            if (itemValues == null) return false;

            foreach (var v in itemValues)
            {
                if (v != null && _values.Contains(v.ToString()!))
                    return true;
            }

            return false;
        }
    }

    /// <summary>Filters items by numeric range (inclusive).</summary>
    public class NumericRangeFilterPredicate : FilterPredicate
    {
        public NumericRangeFilterPredicate(string propertyId, double min, double max) : base(propertyId)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; }
        public double Max { get; }

        public override bool Matches(PivotViewerItem item)
        {
            var values = item[PropertyId];
            if (values == null) return false;

            foreach (var v in values)
            {
                double d;
                if (v is double dd) d = dd;
                else if (v is int ii) d = ii;
                else if (v is float ff) d = ff;
                else if (v is decimal dec) d = (double)dec;
                else continue;

                if (d >= Min && d <= Max)
                    return true;
            }

            return false;
        }
    }

    /// <summary>Filters items by DateTime range (inclusive).</summary>
    public class DateTimeRangeFilterPredicate : FilterPredicate
    {
        public DateTimeRangeFilterPredicate(string propertyId, DateTime min, DateTime max) : base(propertyId)
        {
            Min = min;
            Max = max;
        }

        public DateTime Min { get; }
        public DateTime Max { get; }

        public override bool Matches(PivotViewerItem item)
        {
            var values = item[PropertyId];
            if (values == null) return false;

            foreach (var v in values)
            {
                if (v is DateTime dt)
                {
                    if (dt >= Min && dt <= Max)
                        return true;
                }
            }

            return false;
        }
    }
}
