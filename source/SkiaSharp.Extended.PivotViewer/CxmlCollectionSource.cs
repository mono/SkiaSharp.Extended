using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Event args for state changes during CXML collection loading.
    /// </summary>
    public class CxmlCollectionStateChangedEventArgs : EventArgs
    {
        public CxmlCollectionStateChangedEventArgs(
            CxmlCollectionState oldState, CxmlCollectionState newState,
            string? message = null, Exception? exception = null)
        {
            OldState = oldState;
            NewState = newState;
            Message = message;
            Exception = exception;
        }

        public CxmlCollectionState OldState { get; }
        public CxmlCollectionState NewState { get; }
        public string? Message { get; }
        public Exception? Exception { get; }
    }

    /// <summary>
    /// Loads and parses a CXML (Collection XML) file, producing a collection of
    /// PivotViewerItems with typed properties. Matches Silverlight SL5 CxmlCollectionSource.
    /// </summary>
    public class CxmlCollectionSource
    {
        private static readonly XNamespace CollectionNs = "http://schemas.microsoft.com/collection/metadata/2009";
        private static readonly XNamespace PivotNs = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";

        private CxmlCollectionState _state;
        private List<PivotViewerItem> _items;
        private List<PivotViewerProperty> _properties;

        public CxmlCollectionSource()
        {
            _state = CxmlCollectionState.Initialized;
            _items = new List<PivotViewerItem>();
            _properties = new List<PivotViewerProperty>();
        }

        /// <summary>Source URI of the CXML file.</summary>
        public Uri? UriSource { get; set; }

        /// <summary>Collection name from CXML.</summary>
        public string? Name { get; private set; }

        /// <summary>Brand image URI from CXML.</summary>
        public Uri? BrandImage { get; private set; }

        /// <summary>Culture/locale from CXML.</summary>
        public CultureInfo? Culture { get; private set; }

        /// <summary>Copyright information from CXML.</summary>
        public PivotViewerHyperlink? Copyright { get; private set; }

        /// <summary>Image base path from CXML Items element.</summary>
        public string? ImageBase { get; private set; }

        /// <summary>Additional search URI for linked collections.</summary>
        public string? AdditionalSearchUri { get; private set; }

        /// <summary>Collection icon URI.</summary>
        public string? Icon { get; private set; }

        /// <summary>Current loading state.</summary>
        public CxmlCollectionState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    var old = _state;
                    _state = value;
                    StateChanged?.Invoke(this, new CxmlCollectionStateChangedEventArgs(old, value));
                }
            }
        }

        /// <summary>All items in the collection.</summary>
        public IReadOnlyList<PivotViewerItem> Items => _items;

        /// <summary>All property definitions in the collection.</summary>
        public IReadOnlyList<PivotViewerProperty> ItemProperties => _properties;

        /// <summary>Fired when loading state changes.</summary>
        public event EventHandler<CxmlCollectionStateChangedEventArgs>? StateChanged;

        /// <summary>Gets a property by its ID (name).</summary>
        public PivotViewerProperty? GetPivotPropertyById(string propertyId)
        {
            return _properties.FirstOrDefault(p => p.Id == propertyId);
        }

        /// <summary>Gets an item by its ID.</summary>
        public PivotViewerItem? GetItemById(string itemId)
        {
            return _items.FirstOrDefault(i => i.Id == itemId);
        }

        /// <summary>Parses a CXML XML string.</summary>
        public static CxmlCollectionSource Parse(string xml)
        {
            var doc = XDocument.Parse(xml);
            return ParseDocument(doc);
        }

        /// <summary>Parses a CXML XML from a stream.</summary>
        public static CxmlCollectionSource Parse(Stream stream)
        {
            var doc = XDocument.Load(stream);
            return ParseDocument(doc);
        }

        private static CxmlCollectionSource ParseDocument(XDocument doc)
        {
            var source = new CxmlCollectionSource();

            try
            {
                source.State = CxmlCollectionState.Loading;

                var collectionElement = doc.Element(CollectionNs + "Collection");
                if (collectionElement == null)
                    throw new FormatException("Invalid CXML: missing <Collection> element with namespace " + CollectionNs);

                // Parse collection-level attributes
                source.Name = collectionElement.Attribute("Name")?.Value;
                source.Icon = collectionElement.Attribute(PivotNs + "Icon")?.Value;

                var supplementAttr = collectionElement.Attribute(PivotNs + "Supplement");
                source.AdditionalSearchUri = collectionElement.Attribute(PivotNs + "AdditionalSearchUri")?.Value;

                // Parse FacetCategories
                var facetCategories = ParseFacetCategories(collectionElement);
                source._properties = new List<PivotViewerProperty>(facetCategories.Values);

                // Parse Copyright
                var copyrightElement = collectionElement.Element(CollectionNs + "Copyright");
                if (copyrightElement != null)
                {
                    string? copyrightName = copyrightElement.Attribute("Name")?.Value;
                    string? copyrightHref = copyrightElement.Attribute("Href")?.Value;
                    if (copyrightName != null && copyrightHref != null)
                    {
                        source.Copyright = new PivotViewerHyperlink(copyrightName,
                            Uri.TryCreate(copyrightHref, UriKind.RelativeOrAbsolute, out var uri) ? uri : new Uri("about:blank"));
                    }
                }

                // Parse BrandImage
                var brandImageElement = collectionElement.Element(CollectionNs + "BrandImage");
                if (brandImageElement != null)
                {
                    string? brandSrc = brandImageElement.Attribute("Source")?.Value;
                    if (brandSrc != null && Uri.TryCreate(brandSrc, UriKind.RelativeOrAbsolute, out var brandUri))
                        source.BrandImage = brandUri;
                }

                // Parse Items
                var itemsElement = collectionElement.Element(CollectionNs + "Items");
                if (itemsElement != null)
                {
                    source.ImageBase = itemsElement.Attribute("ImgBase")?.Value;
                    source._items = ParseItems(itemsElement, facetCategories);
                }

                source.State = CxmlCollectionState.Loaded;
            }
            catch (Exception ex)
            {
                source.State = CxmlCollectionState.Failed;
                source.StateChanged?.Invoke(source, new CxmlCollectionStateChangedEventArgs(
                    CxmlCollectionState.Loading, CxmlCollectionState.Failed, ex.Message, ex));
                throw;
            }

            return source;
        }

        private static Dictionary<string, PivotViewerProperty> ParseFacetCategories(XElement collectionElement)
        {
            var properties = new Dictionary<string, PivotViewerProperty>(StringComparer.Ordinal);

            var facetCategoriesElement = collectionElement.Element(CollectionNs + "FacetCategories");
            if (facetCategoriesElement == null) return properties;

            foreach (var fc in facetCategoriesElement.Elements(CollectionNs + "FacetCategory"))
            {
                string? name = fc.Attribute("Name")?.Value;
                string? type = fc.Attribute("Type")?.Value;
                if (name == null || type == null) continue;

                PivotViewerProperty property = CreateProperty(name, type);

                // Parse format (can be plain attribute or in pivot namespace)
                string? format = fc.Attribute("Format")?.Value ?? fc.Attribute(PivotNs + "Format")?.Value;
                if (format != null) property.Format = format;

                // Parse visibility/search flags
                var options = PivotViewerPropertyOptions.None;

                if (ParseBoolAttribute(fc, PivotNs + "IsFilterVisible"))
                    options |= PivotViewerPropertyOptions.CanFilter;

                if (ParseBoolAttribute(fc, PivotNs + "IsWordWheelVisible"))
                    options |= PivotViewerPropertyOptions.CanSearchText;

                if (!ParseBoolAttribute(fc, PivotNs + "IsMetaDataVisible", true))
                    options |= PivotViewerPropertyOptions.Private;

                property.Options = options;

                // Lock after configuration
                property.Lock();
                properties[name] = property;
            }

            return properties;
        }

        /// <summary>
        /// Creates a typed property from CXML type string, applying the correct mapping:
        /// "String" → Text, "LongString" → Text + WrappingText, "Number" → Decimal,
        /// "DateTime" → DateTime, "Link" → Link
        /// </summary>
        public static PivotViewerProperty CreateProperty(string name, string cxmlType)
        {
            switch (cxmlType)
            {
                case "String":
                    return new PivotViewerStringProperty(name);

                case "LongString":
                    var longStr = new PivotViewerStringProperty(name);
                    longStr.Options |= PivotViewerPropertyOptions.WrappingText;
                    return longStr;

                case "Number":
                    return new PivotViewerNumericProperty(name);

                case "DateTime":
                    return new PivotViewerDateTimeProperty(name);

                case "Link":
                    return new PivotViewerLinkProperty(name);

                default:
                    // Unknown type — default to Text
                    return new PivotViewerStringProperty(name);
            }
        }

        private static List<PivotViewerItem> ParseItems(XElement itemsElement, Dictionary<string, PivotViewerProperty> properties)
        {
            var items = new List<PivotViewerItem>();

            foreach (var itemElement in itemsElement.Elements(CollectionNs + "Item"))
            {
                string? id = itemElement.Attribute("Id")?.Value;
                if (id == null) continue;

                var item = new PivotViewerItem(id);

                // Parse well-known attributes as properties
                string? name = itemElement.Attribute("Name")?.Value;
                if (name != null)
                {
                    var nameProp = GetOrCreateProperty(properties, "Name", "String");
                    item.Add(nameProp, name);
                }

                string? href = itemElement.Attribute("Href")?.Value;
                if (href != null)
                {
                    var hrefProp = GetOrCreateProperty(properties, "Href", "Link");
                    item.Add(hrefProp, href);
                }

                string? img = itemElement.Attribute("Img")?.Value;
                if (img != null)
                {
                    var imgProp = GetOrCreateProperty(properties, "#Image", "String");
                    item.Add(imgProp, img);
                }

                string? description = itemElement.Attribute("Description")?.Value;
                if (description != null)
                {
                    var descProp = GetOrCreateProperty(properties, "Description", "LongString");
                    item.Add(descProp, description);
                }

                // Parse Facets
                var facetsElement = itemElement.Element(CollectionNs + "Facets");
                if (facetsElement != null)
                {
                    foreach (var facetElement in facetsElement.Elements(CollectionNs + "Facet"))
                    {
                        string? facetName = facetElement.Attribute("Name")?.Value;
                        if (facetName == null) continue;

                        if (!properties.TryGetValue(facetName, out var property))
                            continue; // Skip unknown facets

                        var values = ParseFacetValues(facetElement, property);
                        if (values.Count > 0)
                            item.Add(property, values.ToArray());
                    }
                }

                // Parse Description element (child element, not attribute)
                var descElement = itemElement.Element(CollectionNs + "Description");
                if (descElement != null && !item.HasProperty("Description"))
                {
                    var descProp = GetOrCreateProperty(properties, "Description", "LongString");
                    item.Add(descProp, descElement.Value);
                }

                items.Add(item);
            }

            return items;
        }

        private static PivotViewerProperty GetOrCreateProperty(
            Dictionary<string, PivotViewerProperty> properties, string name, string cxmlType)
        {
            if (properties.TryGetValue(name, out var existing))
                return existing;

            var property = CreateProperty(name, cxmlType);
            properties[name] = property;
            return property;
        }

        private static List<object> ParseFacetValues(XElement facetElement, PivotViewerProperty property)
        {
            var values = new List<object>();

            foreach (var child in facetElement.Elements())
            {
                string localName = child.Name.LocalName;

                switch (localName)
                {
                    case "String":
                        string? strVal = child.Attribute("Value")?.Value;
                        if (strVal != null) values.Add(strVal);
                        break;

                    case "Number":
                        string? numStr = child.Attribute("Value")?.Value;
                        if (numStr != null && double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var numVal))
                            values.Add(numVal);
                        break;

                    case "DateTime":
                        string? dateStr = child.Attribute("Value")?.Value;
                        if (dateStr != null && DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateVal))
                            values.Add(dateVal);
                        break;

                    case "Link":
                        string? linkHref = child.Attribute("Href")?.Value;
                        string? linkName = child.Attribute("Name")?.Value;
                        if (linkHref != null && Uri.TryCreate(linkHref, UriKind.RelativeOrAbsolute, out var linkUri))
                            values.Add(new PivotViewerHyperlink(linkName ?? linkHref, linkUri));
                        break;
                }
            }

            return values;
        }

        private static bool ParseBoolAttribute(XElement element, XName attributeName, bool defaultValue = false)
        {
            string? value = element.Attribute(attributeName)?.Value;
            if (value == null) return defaultValue;
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
