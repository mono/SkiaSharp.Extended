using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Word wheel search index for PivotViewer. Indexes searchable facet values
    /// and provides prefix-based autocomplete matching.
    /// Matches Silverlight's CharBucket/CharBucketGroup pattern.
    /// </summary>
    public class WordWheelIndex
    {
        private readonly List<WordEntry> _entries = new List<WordEntry>();
        private readonly Dictionary<string, HashSet<PivotViewerItem>> _valueToItems =
            new Dictionary<string, HashSet<PivotViewerItem>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Maximum number of search results to return.</summary>
        public int ResultLimit { get; set; } = 50;

        /// <summary>
        /// Builds the search index from items and their searchable properties.
        /// Properties must have CanSearchText option.
        /// </summary>
        public void Build(
            IReadOnlyList<PivotViewerItem> items,
            IReadOnlyList<PivotViewerProperty> properties)
        {
            _entries.Clear();
            _valueToItems.Clear();

            var searchableProperties = properties
                .Where(p => (p.Options & PivotViewerPropertyOptions.CanSearchText) != 0)
                .ToList();

            foreach (var item in items)
            {
                foreach (var prop in searchableProperties)
                {
                    var values = item[prop.Id];
                    if (values == null) continue;

                    foreach (var val in values)
                    {
                        if (val == null) continue;
                        string text = val.ToString()!;
                        if (string.IsNullOrWhiteSpace(text)) continue;

                        // Index the full value
                        AddEntry(text, item);

                        // Also index individual words for multi-word search
                        var words = text.Split(new[] { ' ', '-', ',', '/', '\\', '(', ')' },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in words)
                        {
                            if (word.Length > 1 && word != text)
                                AddEntry(word, item);
                        }
                    }
                }

                // Index AdditionalSearchText if present
                if (!string.IsNullOrWhiteSpace(item.AdditionalSearchText))
                {
                    var words = item.AdditionalSearchText.Split(new[] { ' ', '-', ',', '/', '\\', '(', ')' },
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        if (word.Length > 0)
                            AddEntry(word, item);
                    }
                }
            }

            // Sort entries for binary search
            _entries.Sort((a, b) => string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase));
        }

        private void AddEntry(string text, PivotViewerItem item)
        {
            string normalizedText = text.ToLowerInvariant();

            if (!_valueToItems.TryGetValue(normalizedText, out var itemSet))
            {
                itemSet = new HashSet<PivotViewerItem>();
                _valueToItems[normalizedText] = itemSet;
                _entries.Add(new WordEntry(text, normalizedText));
            }

            itemSet.Add(item);
        }

        /// <summary>
        /// Searches for entries matching the given prefix. Returns matching text values.
        /// </summary>
        public IReadOnlyList<SearchResult> Search(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return Array.Empty<SearchResult>();

            string normalizedPrefix = prefix.ToLowerInvariant();
            var results = new List<SearchResult>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in _entries)
            {
                if (entry.NormalizedText.StartsWith(normalizedPrefix, StringComparison.Ordinal))
                {
                    if (seen.Add(entry.NormalizedText))
                    {
                        if (_valueToItems.TryGetValue(entry.NormalizedText, out var items))
                        {
                            results.Add(new SearchResult(entry.Text, items.Count));
                        }
                    }

                    if (results.Count >= ResultLimit)
                        break;
                }
            }

            return results;
        }

        /// <summary>
        /// Returns items matching the search text (prefix match on any indexed value).
        /// </summary>
        public IReadOnlyList<PivotViewerItem> GetMatchingItems(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return Array.Empty<PivotViewerItem>();

            string normalizedSearch = searchText.ToLowerInvariant();
            var matchingItems = new HashSet<PivotViewerItem>();

            foreach (var entry in _entries)
            {
                if (entry.NormalizedText.StartsWith(normalizedSearch, StringComparison.Ordinal))
                {
                    if (_valueToItems.TryGetValue(entry.NormalizedText, out var items))
                    {
                        foreach (var item in items)
                            matchingItems.Add(item);
                    }
                }
            }

            return matchingItems.ToList();
        }

        /// <summary>
        /// Gets character buckets for the current search prefix, showing available
        /// next characters for progressive narrowing (Silverlight CharBucket behavior).
        /// </summary>
        public IReadOnlyList<CharBucket> GetCharBuckets(string prefix = "")
        {
            string normalizedPrefix = (prefix ?? "").ToLowerInvariant();
            int prefixLen = normalizedPrefix.Length;

            var charGroups = new Dictionary<char, CharBucketBuilder>();

            foreach (var entry in _entries)
            {
                if (!entry.NormalizedText.StartsWith(normalizedPrefix, StringComparison.Ordinal))
                    continue;

                if (entry.NormalizedText.Length <= prefixLen)
                    continue;

                char nextChar = entry.NormalizedText[prefixLen];

                if (!charGroups.TryGetValue(nextChar, out var builder))
                {
                    builder = new CharBucketBuilder(nextChar);
                    charGroups[nextChar] = builder;
                }

                builder.AddEntry(entry.Text);
                if (_valueToItems.TryGetValue(entry.NormalizedText, out var items))
                    builder.ItemCount += items.Count;
            }

            return charGroups
                .OrderBy(kv => kv.Key)
                .Select(kv => new CharBucket(
                    kv.Value.Character, kv.Value.EntryCount, kv.Value.ItemCount))
                .ToList();
        }

        private class CharBucketBuilder
        {
            public CharBucketBuilder(char c)
            {
                Character = c;
            }

            public char Character { get; }
            public int EntryCount { get; private set; }
            public int ItemCount { get; set; }

            public void AddEntry(string text) => EntryCount++;
        }
    }

    /// <summary>An entry in the word wheel index.</summary>
    internal readonly struct WordEntry
    {
        public WordEntry(string text, string normalizedText)
        {
            Text = text;
            NormalizedText = normalizedText;
        }

        public string Text { get; }
        public string NormalizedText { get; }
    }

    /// <summary>A search result with the matched text and item count.</summary>
    public readonly struct SearchResult
    {
        public SearchResult(string text, int itemCount)
        {
            Text = text;
            ItemCount = itemCount;
        }

        public string Text { get; }
        public int ItemCount { get; }
    }

    /// <summary>
    /// A character bucket showing available next characters for progressive search narrowing.
    /// </summary>
    public readonly struct CharBucket
    {
        public CharBucket(char character, int entryCount, int itemCount)
        {
            Character = character;
            EntryCount = entryCount;
            ItemCount = itemCount;
        }

        public char Character { get; }
        public int EntryCount { get; }
        public int ItemCount { get; }
    }
}
