#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Represents a Deep Zoom Collection (DZC) — a composite tiled pyramid of multiple images.
    /// Parses the DZC XML descriptor and provides access to sub-images and composite tile math.
    /// </summary>
    public class SKDeepZoomCollectionSource
    {
        private const string DeepZoomNamespace2008 = "http://schemas.microsoft.com/deepzoom/2008";
        private const string DeepZoomNamespace2009 = "http://schemas.microsoft.com/deepzoom/2009";

        private readonly List<SKDeepZoomCollectionSubImage> _items;

        public SKDeepZoomCollectionSource(int maxLevel, int tileSize, string format, IReadOnlyList<SKDeepZoomCollectionSubImage> items)
        {
            if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));
            if (string.IsNullOrEmpty(format)) throw new ArgumentException("Format cannot be null or empty.", nameof(format));

            MaxLevel = maxLevel;
            TileSize = tileSize;
            Format = format;
            _items = new List<SKDeepZoomCollectionSubImage>(items);
        }

        /// <summary>Maximum composite pyramid level.</summary>
        public int MaxLevel { get; }

        /// <summary>Tile size in pixels.</summary>
        public int TileSize { get; }

        /// <summary>Image format (e.g., "jpg", "png").</summary>
        public string Format { get; }

        /// <summary>Number of items in the collection.</summary>
        public int ItemCount => _items.Count;

        /// <summary>Read-only list of all sub-images.</summary>
        public IReadOnlyList<SKDeepZoomCollectionSubImage> Items => _items;

        /// <summary>Next item ID (from DZC NextItemId attribute).</summary>
        public int NextItemId { get; set; }

        /// <summary>Base URI for composite mosaic tiles.</summary>
        public string? TilesBaseUri { get; set; }

        /// <summary>Gets the relative composite tile URL.</summary>
        public string GetCompositeTileUrl(int level, int col, int row)
        {
            return $"{level}/{col}_{row}.{Format}";
        }

        /// <summary>
        /// Computes the Morton (Z-order) grid dimensions for this collection.
        /// Items are placed on a 2^N × 2^N grid.
        /// </summary>
        public int GetMortonGridSize()
        {
            if (_items.Count == 0) return 0;
            int n = (int)Math.Ceiling(Math.Log(Math.Ceiling(Math.Sqrt(_items.Count))) / Math.Log(2));
            return 1 << n;
        }

        /// <summary>
        /// Converts a Morton (Z-order) index to (column, row) coordinates.
        /// </summary>
        public static (int Col, int Row) MortonToGrid(int mortonIndex)
        {
            int col = 0, row = 0;
            for (int bit = 0; bit < 16; bit++)
            {
                col |= ((mortonIndex >> (2 * bit)) & 1) << bit;
                row |= ((mortonIndex >> (2 * bit + 1)) & 1) << bit;
            }
            return (col, row);
        }

        /// <summary>Converts (column, row) to a Morton index.</summary>
        public static int GridToMorton(int col, int row)
        {
            int morton = 0;
            for (int bit = 0; bit < 16; bit++)
            {
                morton |= ((col >> bit) & 1) << (2 * bit);
                morton |= ((row >> bit) & 1) << (2 * bit + 1);
            }
            return morton;
        }

        /// <summary>Parses a DZC XML string.</summary>
        public static SKDeepZoomCollectionSource Parse(string xml)
        {
            var doc = XDocument.Parse(xml);
            return ParseDocument(doc, null);
        }

        /// <summary>Parses a DZC XML from a stream.</summary>
        public static SKDeepZoomCollectionSource Parse(Stream stream)
        {
            var doc = XDocument.Load(stream);
            return ParseDocument(doc, null);
        }

        /// <summary>Parses a DZC XML from a stream with a base URI.</summary>
        public static SKDeepZoomCollectionSource Parse(Stream stream, string? baseUri)
        {
            var doc = XDocument.Load(stream);
            return ParseDocument(doc, baseUri);
        }

        private static SKDeepZoomCollectionSource ParseDocument(XDocument doc, string? baseUri)
        {
            // Try both 2008 and 2009 namespaces
            var ns = XNamespace.Get(DeepZoomNamespace2008);
            var collectionElement = doc.Element(ns + "Collection");
            if (collectionElement == null)
            {
                ns = XNamespace.Get(DeepZoomNamespace2009);
                collectionElement = doc.Element(ns + "Collection");
            }
            if (collectionElement == null)
                throw new FormatException("Invalid DZC: missing <Collection> element.");

            int maxLevel = int.Parse(collectionElement.Attribute("MaxLevel")?.Value ?? "0");
            int tileSize = int.Parse(collectionElement.Attribute("TileSize")?.Value ?? "256");
            string format = collectionElement.Attribute("Format")?.Value ?? "jpg";
            int nextItemId = int.Parse(collectionElement.Attribute("NextItemId")?.Value ?? "0");

            var items = new List<SKDeepZoomCollectionSubImage>();
            var itemsElement = collectionElement.Element(ns + "Items");
            if (itemsElement != null)
            {
                foreach (var iElement in itemsElement.Elements(ns + "I"))
                {
                    int id = int.Parse(iElement.Attribute("Id")?.Value ?? "0");
                    int n = int.Parse(iElement.Attribute("N")?.Value ?? "0");
                    string? source = iElement.Attribute("Source")?.Value;

                    var sizeElement = iElement.Element(ns + "Size");
                    int width = int.Parse(sizeElement?.Attribute("Width")?.Value ?? "0");
                    int height = int.Parse(sizeElement?.Attribute("Height")?.Value ?? "0");

                    var subImage = new SKDeepZoomCollectionSubImage(id, n, width, height, source);

                    // Parse viewport if present
                    var viewportElement = iElement.Element(ns + "Viewport");
                    if (viewportElement != null)
                    {
                        subImage.ViewportWidth = double.Parse(viewportElement.Attribute("Width")?.Value ?? "0", CultureInfo.InvariantCulture);
                        subImage.ViewportX = double.Parse(viewportElement.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture);
                        subImage.ViewportY = double.Parse(viewportElement.Attribute("Y")?.Value ?? "0", CultureInfo.InvariantCulture);
                    }

                    items.Add(subImage);
                }
            }

            var result = new SKDeepZoomCollectionSource(maxLevel, tileSize, format, items)
            {
                NextItemId = nextItemId,
                TilesBaseUri = baseUri
            };
            return result;
        }
    }
}
