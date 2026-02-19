using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Computes grid layout positions for PivotViewer items.
    /// Pure function: given items, size, and aspect ratio → position array.
    /// Matches Silverlight's GridLayoutEngine behavior.
    /// </summary>
    public class GridLayoutEngine
    {
        /// <summary>
        /// Computes a grid layout for the given items within the specified bounds.
        /// </summary>
        public GridLayout ComputeLayout(
            IReadOnlyList<PivotViewerItem> items,
            double availableWidth,
            double availableHeight,
            double itemAspectRatio = 1.0)
        {
            if (items.Count == 0)
                return new GridLayout(Array.Empty<ItemPosition>(), 0, 0, 0, 0);

            if (itemAspectRatio <= 0) itemAspectRatio = 1.0;

            // Find optimal grid dimensions
            int bestCols = 1;
            int bestRows = items.Count;
            double bestItemWidth = 0;
            double bestItemHeight = 0;
            double bestAreaUsed = 0;

            for (int cols = 1; cols <= items.Count; cols++)
            {
                int rows = (int)Math.Ceiling((double)items.Count / cols);

                double itemHeight = availableHeight / rows;
                double itemWidth = itemHeight * itemAspectRatio;

                // Check if items fit width-wise
                if (itemWidth * cols > availableWidth)
                {
                    itemWidth = availableWidth / cols;
                    itemHeight = itemWidth / itemAspectRatio;
                }

                double areaUsed = itemWidth * itemHeight * items.Count;
                if (areaUsed > bestAreaUsed)
                {
                    bestAreaUsed = areaUsed;
                    bestCols = cols;
                    bestRows = rows;
                    bestItemWidth = itemWidth;
                    bestItemHeight = itemHeight;
                }
            }

            // Generate positions
            var positions = new ItemPosition[items.Count];
            double totalGridWidth = bestCols * bestItemWidth;
            double totalGridHeight = bestRows * bestItemHeight;

            // Center the grid
            double offsetX = (availableWidth - totalGridWidth) / 2;
            double offsetY = (availableHeight - totalGridHeight) / 2;

            for (int i = 0; i < items.Count; i++)
            {
                int col = i % bestCols;
                int row = i / bestCols;

                positions[i] = new ItemPosition(
                    items[i],
                    offsetX + col * bestItemWidth,
                    offsetY + row * bestItemHeight,
                    bestItemWidth,
                    bestItemHeight);
            }

            return new GridLayout(positions, bestCols, bestRows, bestItemWidth, bestItemHeight);
        }

        /// <summary>
        /// Computes a grid layout with zoom level support.
        /// zoomLevel 0.0 = fit all items. zoomLevel 1.0 = single item fills the view.
        /// </summary>
        public GridLayout ComputeZoomedLayout(
            IReadOnlyList<PivotViewerItem> items,
            double availableWidth,
            double availableHeight,
            double zoomLevel,
            double itemAspectRatio = 1.0)
        {
            if (items.Count == 0 || zoomLevel < 0.01)
                return ComputeLayout(items, availableWidth, availableHeight, itemAspectRatio);

            // At zoom 1.0, one item fills the view
            // At zoom 0.5, ~4 items fit
            // Interpolate columns between fit-all and 1
            int fitAllCols = (int)Math.Max(1, Math.Sqrt(items.Count * availableWidth / availableHeight));
            int targetCols = Math.Max(1, (int)Math.Round(fitAllCols * (1.0 - zoomLevel) + 1.0 * zoomLevel));

            if (itemAspectRatio <= 0) itemAspectRatio = 1.0;
            int rows = (int)Math.Ceiling((double)items.Count / targetCols);
            double itemWidth = availableWidth / targetCols;
            double itemHeight = itemWidth / itemAspectRatio;

            var positions = new ItemPosition[items.Count];
            double offsetX = 0;
            double offsetY = 0;

            for (int i = 0; i < items.Count; i++)
            {
                int col = i % targetCols;
                int row = i / targetCols;

                positions[i] = new ItemPosition(
                    items[i],
                    offsetX + col * itemWidth,
                    offsetY + row * itemHeight,
                    itemWidth,
                    itemHeight);
            }

            return new GridLayout(positions, targetCols, rows, itemWidth, itemHeight);
        }

        /// <summary>
        /// Computes a grid layout grouped by a facet property (for graph/histogram view).
        /// Each group gets its own column of items.
        /// </summary>
        public HistogramLayout ComputeHistogramLayout(
            IReadOnlyList<PivotViewerItem> items,
            string groupByPropertyId,
            double availableWidth,
            double availableHeight,
            double itemAspectRatio = 1.0)
        {
            if (items.Count == 0)
                return new HistogramLayout(Array.Empty<HistogramColumn>(), Array.Empty<ItemPosition>());

            // Group items by property value
            var groups = new Dictionary<string, List<PivotViewerItem>>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                var values = item[groupByPropertyId];
                if (values == null || values.Count == 0)
                {
                    AddToGroup(groups, "(No value)", item);
                    continue;
                }

                foreach (var val in values)
                {
                    string key;
                    if (val is double d)
                        key = d.ToString(CultureInfo.InvariantCulture);
                    else if (val is int i)
                        key = i.ToString(CultureInfo.InvariantCulture);
                    else if (val is DateTime dt)
                        key = dt.ToString(CultureInfo.InvariantCulture);
                    else
                        key = val?.ToString() ?? "(No value)";
                    AddToGroup(groups, key, item);
                }
            }

            // Sort groups by key — detect numeric/date values for natural ordering
            // Pre-parse keys once for efficiency
            List<KeyValuePair<string, List<PivotViewerItem>>> sortedGroups;
            var keys = groups.Keys.ToList();
            var numericValues = new Dictionary<string, double>(keys.Count);
            bool allNumeric = true;
            foreach (var k in keys)
            {
                if (k == "(No value)") continue;
                if (double.TryParse(k, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    numericValues[k] = d;
                else { allNumeric = false; break; }
            }

            if (allNumeric && numericValues.Count > 0)
            {
                sortedGroups = groups.OrderBy(g =>
                {
                    if (g.Key == "(No value)") return double.MaxValue;
                    return numericValues[g.Key];
                }).ToList();
            }
            else
            {
                var dateValues = new Dictionary<string, DateTime>(keys.Count);
                bool allDates = true;
                foreach (var k in keys)
                {
                    if (k == "(No value)") continue;
                    if (DateTime.TryParse(k, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        dateValues[k] = dt;
                    else { allDates = false; break; }
                }

                if (allDates && dateValues.Count > 0)
                {
                    sortedGroups = groups.OrderBy(g =>
                    {
                        if (g.Key == "(No value)") return DateTime.MaxValue;
                        return dateValues[g.Key];
                    }).ToList();
                }
                else
                {
                    sortedGroups = groups.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase).ToList();
                }
            }
            int groupCount = sortedGroups.Count;
            if (groupCount == 0)
                return new HistogramLayout(Array.Empty<HistogramColumn>(), Array.Empty<ItemPosition>());

            // Layout: each group gets equal width column, items stack vertically
            double colWidth = availableWidth / groupCount;
            double gap = 2; // pixels between items
            int maxItemsInColumn = sortedGroups.Max(g => g.Value.Count);

            double itemHeight = Math.Max(1.0, Math.Min(
                colWidth / itemAspectRatio,
                Math.Max(1.0, (availableHeight - gap * maxItemsInColumn) / maxItemsInColumn)));
            double itemWidth = Math.Max(1.0, itemHeight * itemAspectRatio);

            if (itemWidth > colWidth - gap * 2)
            {
                itemWidth = colWidth - gap * 2;
                itemHeight = itemWidth / itemAspectRatio;
            }

            var allPositions = new List<ItemPosition>();
            var columns = new List<HistogramColumn>();

            for (int gi = 0; gi < sortedGroups.Count; gi++)
            {
                var group = sortedGroups[gi];
                double colX = gi * colWidth;

                // Items stack from bottom up (like a histogram bar)
                double centerX = colX + (colWidth - itemWidth) / 2;
                var colPositions = new List<ItemPosition>();

                for (int ii = 0; ii < group.Value.Count; ii++)
                {
                    double y = Math.Max(0, availableHeight - (ii + 1) * (itemHeight + gap));
                    var pos = new ItemPosition(group.Value[ii], centerX, y, itemWidth, itemHeight);
                    colPositions.Add(pos);
                    allPositions.Add(pos);
                }

                columns.Add(new HistogramColumn(
                    group.Key, colX, colWidth, colPositions.ToArray(), group.Value.Count));
            }

            return new HistogramLayout(columns.ToArray(), allPositions.ToArray(), groupByPropertyId);
        }

        private static void AddToGroup(Dictionary<string, List<PivotViewerItem>> groups, string key, PivotViewerItem item)
        {
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<PivotViewerItem>();
                groups[key] = list;
            }
            if (!list.Contains(item))
                list.Add(item);
        }
    }

    /// <summary>Result of a grid layout computation.</summary>
    public class GridLayout
    {
        public GridLayout(ItemPosition[] positions, int columns, int rows, double itemWidth, double itemHeight)
        {
            Positions = positions;
            Columns = columns;
            Rows = rows;
            ItemWidth = itemWidth;
            ItemHeight = itemHeight;
        }

        public ItemPosition[] Positions { get; }
        public int Columns { get; }
        public int Rows { get; }
        public double ItemWidth { get; }
        public double ItemHeight { get; }

        /// <summary>
        /// Gets the item at the given grid coordinates, or null.
        /// </summary>
        public PivotViewerItem? GetItemAt(int col, int row)
        {
            int index = row * Columns + col;
            if (index >= 0 && index < Positions.Length)
                return Positions[index].Item;
            return null;
        }

        /// <summary>
        /// Gets the item at the given pixel coordinates, or null.
        /// </summary>
        public PivotViewerItem? HitTest(double x, double y)
        {
            foreach (var pos in Positions)
            {
                if (x >= pos.X && x < pos.X + pos.Width &&
                    y >= pos.Y && y < pos.Y + pos.Height)
                    return pos.Item;
            }
            return null;
        }
    }

    /// <summary>Result of a histogram layout computation.</summary>
    public class HistogramLayout
    {
        public HistogramLayout(HistogramColumn[] columns, ItemPosition[] allPositions, string? propertyName = null)
        {
            Columns = columns;
            AllPositions = allPositions;
            PropertyName = propertyName;
        }

        public HistogramColumn[] Columns { get; }
        public ItemPosition[] AllPositions { get; }
        public string? PropertyName { get; }

        public PivotViewerItem? HitTest(double x, double y)
        {
            foreach (var pos in AllPositions)
            {
                if (x >= pos.X && x < pos.X + pos.Width &&
                    y >= pos.Y && y < pos.Y + pos.Height)
                    return pos.Item;
            }
            return null;
        }
    }

    /// <summary>A column in a histogram layout.</summary>
    public class HistogramColumn
    {
        public HistogramColumn(string label, double x, double width, ItemPosition[] items, int count)
        {
            Label = label;
            X = x;
            Width = width;
            Items = items;
            Count = count;
        }

        public string Label { get; }
        public double X { get; }
        public double Width { get; }
        public ItemPosition[] Items { get; }
        public int Count { get; }
    }

    /// <summary>Position and size of an item in a layout.</summary>
    public readonly struct ItemPosition
    {
        public ItemPosition(PivotViewerItem item, double x, double y, double width, double height)
        {
            Item = item;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public PivotViewerItem Item { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
    }
}
