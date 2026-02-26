using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Fluent builder for constructing PivotViewer collections programmatically.
    /// Alternative to loading from CXML files.
    /// </summary>
    public class PivotViewerCollectionBuilder
    {
        private string _name = "Collection";
        private PivotViewerHyperlink? _copyright;
        private readonly List<PivotViewerProperty> _properties = new List<PivotViewerProperty>();
        private readonly List<PivotViewerItem> _items = new List<PivotViewerItem>();

        /// <summary>Set the collection name.</summary>
        public PivotViewerCollectionBuilder WithName(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        /// <summary>Set the copyright info.</summary>
        public PivotViewerCollectionBuilder WithCopyright(string text, string uri)
        {
            _copyright = new PivotViewerHyperlink(text, new Uri(uri, UriKind.RelativeOrAbsolute));
            return this;
        }

        /// <summary>Add a string property.</summary>
        public PivotViewerCollectionBuilder AddStringProperty(
            string id,
            string displayName,
            PivotViewerPropertyOptions options = PivotViewerPropertyOptions.CanFilter)
        {
            var prop = new PivotViewerStringProperty(id)
            {
                DisplayName = displayName,
                Options = options
            };
            _properties.Add(prop);
            return this;
        }

        /// <summary>Add a numeric property.</summary>
        public PivotViewerCollectionBuilder AddNumericProperty(
            string id,
            string displayName,
            string? format = null,
            PivotViewerPropertyOptions options = PivotViewerPropertyOptions.CanFilter)
        {
            var prop = new PivotViewerNumericProperty(id)
            {
                DisplayName = displayName,
                Format = format,
                Options = options
            };
            _properties.Add(prop);
            return this;
        }

        /// <summary>Add a DateTime property.</summary>
        public PivotViewerCollectionBuilder AddDateTimeProperty(
            string id,
            string displayName,
            PivotViewerPropertyOptions options = PivotViewerPropertyOptions.CanFilter)
        {
            var prop = new PivotViewerDateTimeProperty(id)
            {
                DisplayName = displayName,
                Options = options
            };
            _properties.Add(prop);
            return this;
        }

        /// <summary>Add a link property.</summary>
        public PivotViewerCollectionBuilder AddLinkProperty(string id, string displayName)
        {
            var prop = new PivotViewerLinkProperty(id)
            {
                DisplayName = displayName
            };
            _properties.Add(prop);
            return this;
        }

        /// <summary>Add a pre-configured property.</summary>
        public PivotViewerCollectionBuilder AddProperty(PivotViewerProperty property)
        {
            _properties.Add(property ?? throw new ArgumentNullException(nameof(property)));
            return this;
        }

        /// <summary>Add an item with facet values.</summary>
        public PivotViewerCollectionBuilder AddItem(
            string id,
            Action<ItemBuilder> configure)
        {
            var item = new PivotViewerItem(id);
            var builder = new ItemBuilder(item, _properties);
            configure(builder);
            _items.Add(item);
            return this;
        }

        /// <summary>Add a pre-configured item.</summary>
        public PivotViewerCollectionBuilder AddItem(PivotViewerItem item)
        {
            _items.Add(item ?? throw new ArgumentNullException(nameof(item)));
            return this;
        }

        /// <summary>Build the collection source.</summary>
        public (IReadOnlyList<PivotViewerItem> Items, IReadOnlyList<PivotViewerProperty> Properties) Build()
        {
            // Lock all properties
            foreach (var prop in _properties)
                prop.Lock();

            return (_items.AsReadOnly(), _properties.AsReadOnly());
        }

        /// <summary>
        /// Item builder for fluent facet value assignment.
        /// </summary>
        public class ItemBuilder
        {
            private readonly PivotViewerItem _item;
            private readonly IReadOnlyList<PivotViewerProperty> _properties;

            internal ItemBuilder(PivotViewerItem item, IReadOnlyList<PivotViewerProperty> properties)
            {
                _item = item;
                _properties = properties;
            }

            /// <summary>Set a facet value by property ID.</summary>
            public ItemBuilder Set(string propertyId, params object[] values)
            {
                var prop = _properties.FirstOrDefault(p => p.Id == propertyId);
                if (prop != null)
                    _item.Add(prop, values);
                return this;
            }

            /// <summary>Set a facet value directly on a property.</summary>
            public ItemBuilder Set(PivotViewerProperty property, params object[] values)
            {
                _item.Add(property, values);
                return this;
            }
        }
    }
}
