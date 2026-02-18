using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Computes histogram buckets for numeric and datetime values.
    /// Matches Silverlight's HistogramBucketer (max 15 buckets, auto-scaling).
    /// </summary>
    public static class HistogramBucketer
    {
        public const int MaxBuckets = 15;

        /// <summary>
        /// Create numeric histogram buckets from a set of values.
        /// Returns up to MaxBuckets evenly-spaced ranges.
        /// </summary>
        public static List<HistogramBucket<double>> CreateNumericBuckets(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0)
                return new List<HistogramBucket<double>>();

            double min = sorted[0];
            double max = sorted[sorted.Count - 1];

            if (Math.Abs(max - min) < double.Epsilon)
            {
                return new List<HistogramBucket<double>>
                {
                    new HistogramBucket<double>(min, max, sorted.Count)
                };
            }

            int bucketCount = Math.Min(MaxBuckets, sorted.Count);
            double step = (max - min) / bucketCount;

            // Use "nice" step sizes
            step = NiceStep(step);
            double niceMin = Math.Floor(min / step) * step;
            double niceMax = Math.Ceiling(max / step) * step;
            bucketCount = (int)Math.Ceiling((niceMax - niceMin) / step);
            bucketCount = Math.Min(MaxBuckets, Math.Max(1, bucketCount));

            // Single pass counting using sorted data and binary search
            var counts = new int[bucketCount];
            int idx = 0;
            for (int i = 0; i < bucketCount && idx < sorted.Count; i++)
            {
                double hi = niceMin + (i + 1) * step;
                bool isLast = (i == bucketCount - 1);
                while (idx < sorted.Count && (isLast ? sorted[idx] <= hi : sorted[idx] < hi))
                {
                    counts[i]++;
                    idx++;
                }
            }

            var buckets = new List<HistogramBucket<double>>(bucketCount);
            for (int i = 0; i < bucketCount; i++)
            {
                double lo = niceMin + i * step;
                double hi = niceMin + (i + 1) * step;
                buckets.Add(new HistogramBucket<double>(lo, hi, counts[i]));
            }

            return buckets;
        }

        /// <summary>
        /// Create datetime histogram buckets from a set of values.
        /// Auto-selects appropriate time granularity (years, months, days).
        /// </summary>
        public static List<HistogramBucket<DateTime>> CreateDateTimeBuckets(IEnumerable<DateTime> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0)
                return new List<HistogramBucket<DateTime>>();

            DateTime min = sorted[0];
            DateTime max = sorted[sorted.Count - 1];
            TimeSpan range = max - min;

            if (range.TotalDays < 1)
            {
                return new List<HistogramBucket<DateTime>>
                {
                    new HistogramBucket<DateTime>(min, max, sorted.Count)
                };
            }

            // Choose granularity
            DateTimeGranularity granularity;
            if (range.TotalDays > 365 * 10)
                granularity = DateTimeGranularity.Decade;
            else if (range.TotalDays > 365 * 2)
                granularity = DateTimeGranularity.Year;
            else if (range.TotalDays > 60)
                granularity = DateTimeGranularity.Month;
            else
                granularity = DateTimeGranularity.Day;

            var buckets = new List<HistogramBucket<DateTime>>();
            DateTime current = FloorDate(min, granularity);

            // Single-pass counting using sorted data
            int idx = 0;
            while (current <= max)
            {
                DateTime next = AdvanceDate(current, granularity);
                int count = 0;
                while (idx < sorted.Count && sorted[idx] < next)
                {
                    if (sorted[idx] >= current)
                        count++;
                    idx++;
                }
                buckets.Add(new HistogramBucket<DateTime>(current, next, count));
                current = next;
            }

            // If we exceeded MaxBuckets, consolidate
            if (buckets.Count > MaxBuckets)
            {
                var consolidated = new List<HistogramBucket<DateTime>>();
                int groupSize = (int)Math.Ceiling((double)buckets.Count / MaxBuckets);
                for (int i = 0; i < buckets.Count; i += groupSize)
                {
                    var group = buckets.Skip(i).Take(groupSize).ToList();
                    consolidated.Add(new HistogramBucket<DateTime>(
                        group[0].Min,
                        group[group.Count - 1].Max,
                        group.Sum(b => b.Count)));
                }
                return consolidated;
            }

            return buckets;
        }

        /// <summary>
        /// Create string histogram buckets (value + count pairs).
        /// </summary>
        public static List<HistogramBucket<string>> CreateStringBuckets(IEnumerable<string> values)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in values)
            {
                if (counts.ContainsKey(v))
                    counts[v]++;
                else
                    counts[v] = 1;
            }

            return counts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => new HistogramBucket<string>(kv.Key, kv.Key, kv.Value))
                .ToList();
        }

        private static double NiceStep(double rawStep)
        {
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double residual = rawStep / magnitude;

            if (residual <= 1.0) return magnitude;
            if (residual <= 2.0) return 2 * magnitude;
            if (residual <= 5.0) return 5 * magnitude;
            return 10 * magnitude;
        }

        private static DateTime FloorDate(DateTime date, DateTimeGranularity granularity)
        {
            switch (granularity)
            {
                case DateTimeGranularity.Decade:
                    return new DateTime(date.Year / 10 * 10, 1, 1);
                case DateTimeGranularity.Year:
                    return new DateTime(date.Year, 1, 1);
                case DateTimeGranularity.Month:
                    return new DateTime(date.Year, date.Month, 1);
                case DateTimeGranularity.Day:
                    return date.Date;
                default:
                    return date;
            }
        }

        private static DateTime AdvanceDate(DateTime date, DateTimeGranularity granularity)
        {
            switch (granularity)
            {
                case DateTimeGranularity.Decade:
                    return date.AddYears(10);
                case DateTimeGranularity.Year:
                    return date.AddYears(1);
                case DateTimeGranularity.Month:
                    return date.AddMonths(1);
                case DateTimeGranularity.Day:
                    return date.AddDays(1);
                default:
                    return date.AddDays(1);
            }
        }

        private enum DateTimeGranularity { Day, Month, Year, Decade }
    }

    /// <summary>
    /// A single histogram bucket with a range and count.
    /// </summary>
    public class HistogramBucket<T>
    {
        public HistogramBucket(T min, T max, int count)
        {
            Min = min;
            Max = max;
            Count = count;
        }

        public T Min { get; }
        public T Max { get; }
        public int Count { get; set; }

        /// <summary>Label for display purposes.</summary>
        public string Label { get; set; } = "";

        public override string ToString() => $"[{Min}–{Max}]: {Count}";
    }
}
