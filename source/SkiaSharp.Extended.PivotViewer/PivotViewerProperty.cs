using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Base class for PivotViewer property definitions.
    /// Matches Silverlight SL5 PivotViewerProperty.
    /// </summary>
    public abstract class PivotViewerProperty : IComparable<PivotViewerProperty>, IEquatable<PivotViewerProperty>
    {
        private string _id;
        private string _displayName;
        private string? _format;
        private PivotViewerPropertyOptions _options;
        private bool _isLocked;

        protected PivotViewerProperty(string id, PivotViewerPropertyType propertyType)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            PropertyType = propertyType;
            _displayName = id;
            _options = PivotViewerPropertyOptions.None;
        }

        /// <summary>Unique identifier for the property.</summary>
        public string Id
        {
            get => _id;
            set { EnsureUnlocked(); _id = value ?? throw new ArgumentNullException(nameof(value)); }
        }

        /// <summary>The data type of this property.</summary>
        public PivotViewerPropertyType PropertyType { get; }

        /// <summary>Display name shown in the UI.</summary>
        public string DisplayName
        {
            get => _displayName;
            set { EnsureUnlocked(); _displayName = value ?? throw new ArgumentNullException(nameof(value)); }
        }

        /// <summary>Format string for display (e.g., "#.0 lbs" for numbers).</summary>
        public string? Format
        {
            get => _format;
            set { EnsureUnlocked(); _format = value; }
        }

        /// <summary>Options controlling filter, search, and display behavior.</summary>
        public PivotViewerPropertyOptions Options
        {
            get => _options;
            set { EnsureUnlocked(); _options = value; }
        }

        /// <summary>Whether this property is locked (part of a loaded collection).</summary>
        public bool IsLocked => _isLocked;

        /// <summary>Locks the property, preventing further modifications.</summary>
        public void Lock() => _isLocked = true;

        /// <summary>Throws if the property is locked.</summary>
        protected void EnsureUnlocked()
        {
            if (_isLocked)
                throw new InvalidOperationException("Cannot modify a locked property.");
        }

        /// <summary>Whether this property can be used for filtering.</summary>
        public bool CanFilter => (_options & PivotViewerPropertyOptions.CanFilter) != 0;

        /// <summary>Whether this property is included in search.</summary>
        public bool CanSearchText => (_options & PivotViewerPropertyOptions.CanSearchText) != 0;

        /// <summary>Whether this property is private/hidden.</summary>
        public bool IsPrivate => (_options & PivotViewerPropertyOptions.Private) != 0;

        /// <summary>Whether text should wrap (LongString type).</summary>
        public bool IsWrappingText => (_options & PivotViewerPropertyOptions.WrappingText) != 0;

        public int CompareTo(PivotViewerProperty? other)
        {
            if (other is null) return 1;
            return string.Compare(Id, other.Id, StringComparison.Ordinal);
        }

        public bool Equals(PivotViewerProperty? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as PivotViewerProperty);
        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => $"{DisplayName} ({PropertyType})";
    }

    /// <summary>Text (string) property. CXML type "String" maps here.</summary>
    public class PivotViewerStringProperty : PivotViewerProperty
    {
        public PivotViewerStringProperty(string id) : base(id, PivotViewerPropertyType.Text) { }

        /// <summary>Custom sort orders. Default: cardinality + lexicographic.</summary>
        public List<KeyValuePair<string, IComparer<string>>> Sorts { get; } = new List<KeyValuePair<string, IComparer<string>>>();
    }

    /// <summary>Numeric property. CXML type "Number" maps here.</summary>
    public class PivotViewerNumericProperty : PivotViewerProperty
    {
        public PivotViewerNumericProperty(string id) : base(id, PivotViewerPropertyType.Decimal) { }
    }

    /// <summary>
    /// Delegate for providing custom date range presets. Matches Silverlight's PivotViewerDateTimePropertyRangesProvider.
    /// </summary>
    public delegate IList<IList<DateRange>> PivotViewerDateTimePropertyRangesProvider(
        object viewer,
        PivotViewerDateTimeProperty property,
        IEnumerable<DateTime> values);

    /// <summary>Represents a date range (start to end).</summary>
    public readonly struct DateRange
    {
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; }
        public DateTime End { get; }

        public override string ToString() => $"{Start:d}–{End:d}";
    }

    /// <summary>DateTime property. CXML type "DateTime" maps here.</summary>
    public class PivotViewerDateTimeProperty : PivotViewerProperty
    {
        public PivotViewerDateTimeProperty(string id) : base(id, PivotViewerPropertyType.DateTime)
        {
            PresetProvider = GetStaticAndAutoCalendarDateRangePresets;
        }

        /// <summary>Custom date range presets for the filter pane.</summary>
        public List<DateRange> Presets { get; set; } = new List<DateRange>();

        /// <summary>Delegate for generating date range presets. Default: GetStaticAndAutoCalendarDateRangePresets.</summary>
        public PivotViewerDateTimePropertyRangesProvider? PresetProvider { get; set; }

        /// <summary>
        /// Returns static presets plus auto-generated ranges. Default PresetProvider.
        /// </summary>
        public static IList<IList<DateRange>> GetStaticAndAutoCalendarDateRangePresets(
            object viewer, PivotViewerDateTimeProperty property, IEnumerable<DateTime> values)
        {
            var result = new List<IList<DateRange>>();

            if (property.Presets.Count > 0)
                result.Add(property.Presets);

            result.AddRange(AutogenerateCalendarDateRangePresets(viewer, property, values));
            return result;
        }

        /// <summary>
        /// Returns only static presets (no auto-generation).
        /// </summary>
        public static IList<IList<DateRange>> GetStaticCalendarDateRangePresets(
            object viewer, PivotViewerDateTimeProperty property, IEnumerable<DateTime> values)
        {
            var result = new List<IList<DateRange>>();
            if (property.Presets.Count > 0)
                result.Add(property.Presets);
            return result;
        }

        /// <summary>
        /// Auto-generates calendar date range presets based on the data distribution.
        /// Creates up to 2 levels of granularity (e.g., decades + years).
        /// </summary>
        public static IList<IList<DateRange>> AutogenerateCalendarDateRangePresets(
            object viewer, PivotViewerDateTimeProperty property, IEnumerable<DateTime> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0)
                return new List<IList<DateRange>>();

            var result = new List<IList<DateRange>>();
            var min = sorted[0];
            var max = sorted[sorted.Count - 1];
            var range = max - min;

            // Level 1: coarse granularity
            if (range.TotalDays > 365 * 5)
            {
                // Decades
                result.Add(GenerateRanges(min, max, d => new DateTime(d.Year / 10 * 10, 1, 1), d => d.AddYears(10)));
            }
            else if (range.TotalDays > 365)
            {
                // Years
                result.Add(GenerateRanges(min, max, d => new DateTime(d.Year, 1, 1), d => d.AddYears(1)));
            }

            // Level 2: finer granularity
            if (range.TotalDays > 60)
            {
                // Months
                result.Add(GenerateRanges(min, max, d => new DateTime(d.Year, d.Month, 1), d => d.AddMonths(1)));
            }

            return result;
        }

        private static List<DateRange> GenerateRanges(
            DateTime min, DateTime max, Func<DateTime, DateTime> floor, Func<DateTime, DateTime> advance)
        {
            var ranges = new List<DateRange>();
            var current = floor(min);

            while (current < max && ranges.Count < HistogramBucketer.MaxBuckets)
            {
                var next = advance(current);
                ranges.Add(new DateRange(current, next));
                current = next;
            }

            return ranges;
        }
    }

    /// <summary>Link property. CXML type "Link" maps here.</summary>
    public class PivotViewerLinkProperty : PivotViewerProperty
    {
        public PivotViewerLinkProperty(string id) : base(id, PivotViewerPropertyType.Link) { }
    }
}
