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
    public class CxmlCollectionSource : System.ComponentModel.INotifyPropertyChanged
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

        /// <summary>Raised when a property value changes. Matches Silverlight INotifyPropertyChanged.</summary>
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));

        /// <summary>Source URI of the CXML file.</summary>
        public Uri? UriSource { get; set; }

        /// <summary>Collection name from CXML.</summary>
        public string? Name { get; private set; }

        /// <summary>CXML schema version (e.g., "1.0").</summary>
        public string? SchemaVersion { get; private set; }

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

        /// <summary>Related collections parsed from CXML.</summary>
        public IReadOnlyList<(string Name, string Href)> RelatedCollections { get; private set; } = Array.Empty<(string, string)>();

        /// <summary>URI of supplemental CXML file that provides additional item data.</summary>
        public string? SupplementUri { get; private set; }

        /// <summary>Base URI for resolving relative Href values on items.</summary>
        public string? HrefBase { get; private set; }

        /// <summary>Extra data types parsed from CXML ExtraData/Type elements (e.g., device type groupings).</summary>
        public IReadOnlyList<CxmlExtraDataType> ExtraDataTypes { get; private set; } = Array.Empty<CxmlExtraDataType>();

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
                    OnPropertyChanged(nameof(State));
                }
            }
        }

        /// <summary>All items in the collection.</summary>
        public IReadOnlyList<PivotViewerItem> Items => _items;

        /// <summary>All property definitions in the collection.</summary>
        public IReadOnlyList<PivotViewerProperty> ItemProperties => _properties;

        /// <summary>Item templates for zoom-based template selection. Matches Silverlight API.</summary>
        public PivotViewerItemTemplateCollection ItemTemplates { get; } = new PivotViewerItemTemplateCollection();

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

        /// <summary>
        /// Merges supplemental data from another parsed CXML into this collection.
        /// Items are matched by Id; only new property values are added (existing values are not duplicated).
        /// </summary>
        public void MergeSupplementalData(CxmlCollectionSource supplement)
        {
            if (supplement == null)
                throw new ArgumentNullException(nameof(supplement));

            // Build lookup for canonical property instances
            var propLookup = new Dictionary<string, PivotViewerProperty>(StringComparer.Ordinal);
            foreach (var p in _properties) propLookup[p.Id] = p;

            // Add any new properties from the supplement
            foreach (var prop in supplement.ItemProperties)
            {
                if (!propLookup.ContainsKey(prop.Id))
                {
                    _properties.Add(prop);
                    propLookup[prop.Id] = prop;
                }
            }

            // Merge item data — only add values for properties the item doesn't already have
            foreach (var suppItem in supplement.Items)
            {
                var target = GetItemById(suppItem.Id);
                if (target == null) continue;

                foreach (var prop in suppItem.Properties)
                {
                    // Skip if the target already has values for this property
                    if (target.HasProperty(prop.Id))
                        continue;

                    var values = suppItem[prop];
                    if (values != null && values.Count > 0)
                    {
                        // Use canonical property instance from main collection
                        var canonicalProp = propLookup.TryGetValue(prop.Id, out var cp) ? cp : prop;
                        target.Add(canonicalProp, values.ToArray());
                    }
                }
            }
        }

        /// <summary>Parses a CXML XML string.</summary>
        public static CxmlCollectionSource Parse(string xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            var doc = XDocument.Parse(xml);
            return ParseDocument(doc);
        }

        /// <summary>Parses a CXML XML from a stream.</summary>
        public static CxmlCollectionSource Parse(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            var doc = XDocument.Load(stream);
            return ParseDocument(doc);
        }

        /// <summary>
        /// Asynchronously loads and parses a CXML from a URI using the provided HttpClient.
        /// Matches Silverlight's async loading pattern with state machine.
        /// </summary>
        public static async System.Threading.Tasks.Task<CxmlCollectionSource> LoadAsync(
            Uri uri, System.Net.Http.HttpClient httpClient,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var source = new CxmlCollectionSource { UriSource = uri };

            try
            {
                source.State = CxmlCollectionState.Loading;
                var xml = await httpClient.GetStringAsync(uri
#if NET5_0_OR_GREATER
                    , cancellationToken
#endif
                    ).ConfigureAwait(false);

                var doc = XDocument.Parse(xml);
                ParseInto(source, doc);
                source.State = CxmlCollectionState.Loaded;

                // Auto-load supplemental CXML if SupplementUri is set
                if (source.SupplementUri != null)
                {
                    try
                    {
                        var supplementUrl = new Uri(uri, source.SupplementUri);
                        var suppXml = await httpClient.GetStringAsync(supplementUrl
#if NET5_0_OR_GREATER
                            , cancellationToken
#endif
                            ).ConfigureAwait(false);
                        var suppDoc = XDocument.Parse(suppXml);
                        var supplement = new CxmlCollectionSource();
                        ParseInto(supplement, suppDoc);
                        source.MergeSupplementalData(supplement);
                    }
                    catch (Exception)
                    {
                        // Supplemental data is optional — failure is not fatal
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                source.State = CxmlCollectionState.Failed;
                source.StateChanged?.Invoke(source,
                    new CxmlCollectionStateChangedEventArgs(CxmlCollectionState.Loading, CxmlCollectionState.Failed,
                        ex.Message, ex));
            }

            return source;
        }

        /// <summary>
        /// Asynchronously loads a CXML from a stream.
        /// </summary>
        public static async System.Threading.Tasks.Task<CxmlCollectionSource> LoadAsync(
            Stream stream, System.Threading.CancellationToken cancellationToken = default)
        {
            var source = new CxmlCollectionSource();

            try
            {
                source.State = CxmlCollectionState.Loading;
                var doc = await System.Threading.Tasks.Task.Run(
                    () => XDocument.Load(stream), cancellationToken);
                ParseInto(source, doc);
                source.State = CxmlCollectionState.Loaded;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                source.State = CxmlCollectionState.Failed;
                source.StateChanged?.Invoke(source,
                    new CxmlCollectionStateChangedEventArgs(CxmlCollectionState.Loading, CxmlCollectionState.Failed,
                        ex.Message, ex));
            }

            return source;
        }

        private static CxmlCollectionSource ParseDocument(XDocument doc)
        {
            var source = new CxmlCollectionSource();
            ParseInto(source, doc);
            return source;
        }

        private static void ParseInto(CxmlCollectionSource source, XDocument doc)
        {
            try
            {
                source.State = CxmlCollectionState.Loading;

                var collectionElement = doc.Element(CollectionNs + "Collection");
                if (collectionElement == null)
                    throw new FormatException("Invalid CXML: missing <Collection> element with namespace " + CollectionNs);

                // Parse collection-level attributes
                source.Name = collectionElement.Attribute("Name")?.Value;
                source.SchemaVersion = collectionElement.Attribute("SchemaVersion")?.Value;
                source.Icon = collectionElement.Attribute(PivotNs + "Icon")?.Value;

                var supplementAttr = collectionElement.Attribute(PivotNs + "Supplement");
                source.SupplementUri = supplementAttr?.Value;
                source.AdditionalSearchUri = collectionElement.Attribute(PivotNs + "AdditionalSearchUri")?.Value;

                // Parse FacetCategories
                var facetCategories = ParseFacetCategories(collectionElement);
                source._properties = new List<PivotViewerProperty>(facetCategories.Values);

                // Parse Copyright (may be in collection ns or pivot ns)
                var copyrightElement = collectionElement.Element(CollectionNs + "Copyright")
                    ?? collectionElement.Element(PivotNs + "Copyright");
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
                    source.HrefBase = itemsElement.Attribute("HrefBase")?.Value;
                    source._items = ParseItems(itemsElement, facetCategories, source.HrefBase);

                    // Sync implicit properties (Name, Href, Description, #Image) back to _properties
                    var existingIds = new HashSet<string>(source._properties.Select(p => p.Id), StringComparer.Ordinal);
                    foreach (var prop in facetCategories.Values)
                    {
                        if (!existingIds.Contains(prop.Id))
                            source._properties.Add(prop);
                    }
                }

                // Parse ExtraData (Type elements for UI groupings, e.g., buxton.cxml)
                var extraDataElement = collectionElement.Element(PivotNs + "ExtraData")
                    ?? collectionElement.Element(CollectionNs + "ExtraData");
                if (extraDataElement == null)
                    extraDataElement = collectionElement.Element("ExtraData");
                if (extraDataElement != null)
                {
                    var types = new List<CxmlExtraDataType>();
                    // Look inside a Types wrapper element (e.g., buxton.cxml), or directly under ExtraData
                    var typesContainer = extraDataElement.Element("Types")
                        ?? extraDataElement.Element(PivotNs + "Types")
                        ?? extraDataElement.Element(CollectionNs + "Types")
                        ?? extraDataElement;
                    foreach (var typeEl in typesContainer.Elements("Type")
                        .Concat(typesContainer.Elements(PivotNs + "Type"))
                        .Concat(typesContainer.Elements(CollectionNs + "Type")))
                    {
                        string? typeName = typeEl.Attribute("Name")?.Value;
                        string? typeImage = typeEl.Attribute("Image")?.Value;
                        string? sortedIdsStr = typeEl.Attribute("SortedIds")?.Value;
                        var sortedIds = !string.IsNullOrEmpty(sortedIdsStr)
                            ? sortedIdsStr.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray()
                            : Array.Empty<string>();
                        types.Add(new CxmlExtraDataType(typeName ?? "", typeImage, sortedIds));
                    }
                    source.ExtraDataTypes = types;
                }

                // Parse RelatedCollections
                var relatedElement = collectionElement.Element(PivotNs + "RelatedCollections")
                    ?? collectionElement.Element(CollectionNs + "RelatedCollections");
                if (relatedElement != null)
                {
                    var related = new List<(string Name, string Href)>();
                    foreach (var rc in relatedElement.Elements(PivotNs + "RelatedCollection")
                        .Concat(relatedElement.Elements(CollectionNs + "RelatedCollection")))
                    {
                        string? rcName = rc.Attribute("Name")?.Value;
                        string? rcHref = rc.Attribute("Href")?.Value;
                        if (rcName != null && rcHref != null)
                            related.Add((rcName, rcHref));
                    }
                    source.RelatedCollections = related;
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

                property.Options |= options;

                // Parse SortOrder extension for String properties
                if (property is PivotViewerStringProperty strProp)
                {
                    var extension = fc.Element(CollectionNs + "Extension");
                    if (extension != null)
                    {
                        foreach (var sortOrder in extension.Elements(PivotNs + "SortOrder"))
                        {
                            string? sortName = sortOrder.Attribute("Name")?.Value;
                            if (sortName == null) continue;

                            var sortValues = new List<string>();
                            foreach (var sv in sortOrder.Elements(PivotNs + "SortValue"))
                            {
                                string? val = sv.Attribute("Value")?.Value;
                                if (val != null) sortValues.Add(val);
                            }

                            if (sortValues.Count > 0)
                            {
                                var comparer = new CustomSortOrderComparer(sortValues);
                                strProp.Sorts.Add(new KeyValuePair<string, IComparer<string>>(sortName, comparer));
                            }
                        }
                    }
                }

                // Parse DecimalPlaces for Numeric properties
                if (property is PivotViewerNumericProperty numProp)
                {
                    var dp = fc.Attribute(PivotNs + "DecimalPlaces")?.Value;
                    if (dp != null && int.TryParse(dp, out int decimalPlaces))
                        numProp.DecimalPlaces = decimalPlaces;
                }

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

        private static List<PivotViewerItem> ParseItems(XElement itemsElement, Dictionary<string, PivotViewerProperty> properties, string? hrefBase = null)
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
                    // Resolve relative Href against HrefBase if provided
                    if (hrefBase != null && !Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        var baseUri = new Uri(hrefBase, UriKind.RelativeOrAbsolute);
                        if (baseUri.IsAbsoluteUri)
                            href = new Uri(baseUri, href).ToString();
                        else
                            href = hrefBase.TrimEnd('/') + "/" + href;
                    }
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

                // Parse AdditionalSearchText — spec defines it as an attribute on <Item>
                var additionalSearch = itemElement.Attribute(PivotNs + "AdditionalSearchText")?.Value;
                if (!string.IsNullOrWhiteSpace(additionalSearch))
                {
                    item.AdditionalSearchText = additionalSearch.Trim();
                }
                else
                {
                    // Fallback: check Extension element (non-standard but some tools emit this)
                    var extElement = itemElement.Element(CollectionNs + "Extension");
                    if (extElement != null)
                    {
                        var searchTextElement = extElement.Element(PivotNs + "AdditionalSearchText");
                        if (searchTextElement != null && !string.IsNullOrWhiteSpace(searchTextElement.Value))
                        {
                            item.AdditionalSearchText = searchTextElement.Value.Trim();
                        }
                    }
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
                        if (strVal != null)
                        {
                            // Coerce to correct type if property defines differently
                            if (property.PropertyType == PivotViewerPropertyType.Decimal &&
                                double.TryParse(strVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var coercedNum))
                                values.Add(coercedNum);
                            else if (property.PropertyType == PivotViewerPropertyType.DateTime &&
                                DateTime.TryParse(strVal, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var coercedDate))
                                values.Add(coercedDate);
                            else
                                values.Add(strVal);
                        }
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

    /// <summary>
    /// Represents a Type element from CXML ExtraData, used for UI groupings (e.g., device types in Buxton collection).
    /// </summary>
    public class CxmlExtraDataType
    {
        public CxmlExtraDataType(string name, string? image, string[] sortedIds)
        {
            Name = name;
            Image = image;
            SortedIds = sortedIds;
        }

        /// <summary>Display name of the type category.</summary>
        public string Name { get; }

        /// <summary>Image filename for the type icon.</summary>
        public string? Image { get; }

        /// <summary>Item IDs belonging to this type, in display order.</summary>
        public string[] SortedIds { get; }
    }
}
