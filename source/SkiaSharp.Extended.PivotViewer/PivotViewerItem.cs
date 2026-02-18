using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Event args for property value changes on a PivotViewerItem.
    /// </summary>
    public class PivotViewerPropertyChangedEventArgs : EventArgs
    {
        public PivotViewerPropertyChangedEventArgs(PivotViewerProperty property, object? oldValue, object? newValue)
        {
            PivotProperty = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public PivotViewerProperty PivotProperty { get; }
        public object? OldValue { get; }
        public object? NewValue { get; }
    }

    /// <summary>
    /// Represents a single item in a PivotViewer collection.
    /// Matches Silverlight SL5 PivotViewerItem — no built-in Name/Description/Href properties.
    /// All data is accessed via the string indexer with well-known keys.
    /// </summary>
    public class PivotViewerItem
    {
        private readonly string _id;
        private readonly Dictionary<string, List<object>> _values;
        private readonly Dictionary<string, PivotViewerProperty> _properties;
        private readonly ObservableCollection<PivotViewerProperty> _propertyList;

        public PivotViewerItem(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _values = new Dictionary<string, List<object>>(StringComparer.Ordinal);
            _properties = new Dictionary<string, PivotViewerProperty>(StringComparer.Ordinal);
            _propertyList = new ObservableCollection<PivotViewerProperty>();
        }

        /// <summary>Unique item identifier.</summary>
        public string Id => _id;

        /// <summary>All properties registered on this item.</summary>
        public ReadOnlyObservableCollection<PivotViewerProperty> Properties =>
            new ReadOnlyObservableCollection<PivotViewerProperty>(_propertyList);

        /// <summary>
        /// Gets values for a property by its ID string.
        /// Returns null if the property has no values on this item.
        /// </summary>
        public IList<object>? this[string propertyId]
        {
            get
            {
                if (_values.TryGetValue(propertyId, out var list) && list.Count > 0)
                    return list.AsReadOnly();
                return null;
            }
        }

        /// <summary>
        /// Gets values for a property by its property object.
        /// </summary>
        public IList<object>? this[PivotViewerProperty property]
        {
            get => this[property?.Id ?? throw new ArgumentNullException(nameof(property))];
        }

        /// <summary>Adds values for a property.</summary>
        public void Add(PivotViewerProperty property, params object[] values)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (!_properties.ContainsKey(property.Id))
            {
                _properties[property.Id] = property;
                _propertyList.Add(property);
            }

            if (!_values.TryGetValue(property.Id, out var list))
            {
                list = new List<object>();
                _values[property.Id] = list;
            }

            list.AddRange(values);
            PropertyChanged?.Invoke(this, new PivotViewerPropertyChangedEventArgs(property, null, values));
        }

        /// <summary>Sets values for a property (replacing existing).</summary>
        public void Set(PivotViewerProperty property, params object[] values)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (!_properties.ContainsKey(property.Id))
            {
                _properties[property.Id] = property;
                _propertyList.Add(property);
            }

            var oldValues = _values.TryGetValue(property.Id, out var existing) ? existing.ToArray() : null;
            _values[property.Id] = new List<object>(values);
            PropertyChanged?.Invoke(this, new PivotViewerPropertyChangedEventArgs(property, oldValues, values));
        }

        /// <summary>Removes all values for a property.</summary>
        public bool Remove(PivotViewerProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            var oldValues = _values.TryGetValue(property.Id, out var existing) ? existing.ToArray() : null;
            bool removed = _values.Remove(property.Id);
            if (removed)
            {
                _properties.Remove(property.Id);
                _propertyList.Remove(property);
                PropertyChanged?.Invoke(this, new PivotViewerPropertyChangedEventArgs(property, oldValues, null));
            }
            return removed;
        }

        /// <summary>Gets the property value as a typed list, or null.</summary>
        public IList<T>? GetValues<T>(string propertyId)
        {
            var values = this[propertyId];
            if (values == null) return null;

            var result = new List<T>();
            foreach (var val in values)
            {
                if (val is T typed)
                    result.Add(typed);
            }
            return result.Count > 0 ? result : null;
        }

        /// <summary>Tries to get a single typed value for a property.</summary>
        public bool TryGetSingleValue<T>(string propertyId, out T? value)
        {
            var values = this[propertyId];
            if (values != null && values.Count > 0 && values[0] is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>Gets a property definition by its ID.</summary>
        public PivotViewerProperty? GetPivotPropertyById(string propertyId)
        {
            _properties.TryGetValue(propertyId, out var property);
            return property;
        }

        /// <summary>Checks if this item has any values for a property.</summary>
        public bool HasProperty(string propertyId) =>
            _values.TryGetValue(propertyId, out var list) && list.Count > 0;

        /// <summary>Fired when property values change.</summary>
        public event EventHandler<PivotViewerPropertyChangedEventArgs>? PropertyChanged;

        public override string ToString()
        {
            // Try to use the "Name" well-known key if available
            if (TryGetSingleValue<string>("Name", out var name) && name != null)
                return $"{name} ({Id})";
            return Id;
        }
    }
}
