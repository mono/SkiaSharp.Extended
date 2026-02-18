using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Serializes and deserializes PivotViewer state (filters, sort, view, selection)
    /// as a URL-safe string. Matches Silverlight's CollectionFragmentBuilder format.
    /// </summary>
    public static class ViewerStateSerializer
    {
        /// <summary>
        /// Serializes the current viewer state to a URL-safe string.
        /// Format: filter1=value1&amp;filter2=value2&amp;$view=gridview&amp;$sort=PropertyId&amp;$select=itemId
        /// </summary>
        public static string Serialize(ViewerState state)
        {
            var parts = new List<string>();

            // Serialize filter predicates
            if (state.Predicates != null)
            {
                foreach (var pred in state.Predicates)
                {
                    if (pred is StringFilterPredicate sfp && sfp.Values.Count > 0)
                    {
                        foreach (var v in sfp.Values)
                            parts.Add($"{Encode(pred.PropertyId)}={Encode(v)}");
                    }
                    else if (pred is NumericRangeFilterPredicate nrp)
                    {
                        parts.Add($"{Encode(pred.PropertyId)}=GE({nrp.Min})AND(LE({nrp.Max}))");
                    }
                    else if (pred is DateTimeRangeFilterPredicate drp)
                    {
                        parts.Add($"{Encode(pred.PropertyId)}=GE({drp.Min:O})AND(LE({drp.Max:O}))");
                    }
                }
            }

            // Serialize view
            if (!string.IsNullOrEmpty(state.ViewId))
                parts.Add($"$view={Encode(state.ViewId)}");

            // Serialize sort
            if (!string.IsNullOrEmpty(state.SortPropertyId))
                parts.Add($"$sort={Encode(state.SortPropertyId)}");

            // Serialize selection
            if (!string.IsNullOrEmpty(state.SelectedItemId))
                parts.Add($"$select={Encode(state.SelectedItemId)}");

            return string.Join("&", parts);
        }

        /// <summary>
        /// Deserializes a viewer state string back into a ViewerState object.
        /// </summary>
        public static ViewerState Deserialize(string stateString)
        {
            var state = new ViewerState();
            if (string.IsNullOrEmpty(stateString))
                return state;

            // Remove leading # if present
            if (stateString.StartsWith("#"))
                stateString = stateString.Substring(1);

            var stringFilters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            var pairs = stateString.Split('&');
            foreach (var pair in pairs)
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0) continue;

                string key = Decode(pair.Substring(0, eq));
                string value = eq < pair.Length - 1 ? pair.Substring(eq + 1) : "";

                if (key == "$view")
                {
                    state.ViewId = Decode(value);
                }
                else if (key == "$sort")
                {
                    state.SortPropertyId = Decode(value);
                }
                else if (key == "$select")
                {
                    state.SelectedItemId = Decode(value);
                }
                else
                {
                    // Filter predicate
                    string decodedValue = Decode(value);

                    if (decodedValue.StartsWith("GE("))
                    {
                        state.RangePredicates.Add((key, decodedValue));
                    }
                    else
                    {
                        if (!stringFilters.TryGetValue(key, out var vals))
                        {
                            vals = new List<string>();
                            stringFilters[key] = vals;
                        }
                        vals.Add(decodedValue);
                    }
                }
            }

            foreach (var kv in stringFilters)
            {
                var pred = new StringFilterPredicate(kv.Key);
                foreach (var v in kv.Value)
                    pred.AddValue(v);
                state.StringPredicates.Add(pred);
            }

            return state;
        }

        private static string Encode(string value) => Uri.EscapeDataString(value);
        private static string Decode(string value) => Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Represents the serializable state of a PivotViewer.
    /// </summary>
    public class ViewerState
    {
        public string? ViewId { get; set; }
        public string? SortPropertyId { get; set; }
        public string? SelectedItemId { get; set; }
        public IReadOnlyList<FilterPredicate>? Predicates { get; set; }

        // Used during deserialization
        public List<StringFilterPredicate> StringPredicates { get; } = new List<StringFilterPredicate>();
        public List<(string PropertyId, string Expression)> RangePredicates { get; } = new List<(string, string)>();
    }
}
