using System.Collections.Generic;
using System.ComponentModel;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Bindable model for the sort dropdown overlay.
    /// Exposes available sort properties and selection state.
    /// </summary>
    public class SortDropdownModel : INotifyPropertyChanged
    {
        private readonly PivotViewerController _controller;
        private bool _isVisible;

        public SortDropdownModel(PivotViewerController controller)
        {
            _controller = controller ?? throw new System.ArgumentNullException(nameof(controller));
            _controller.SortPropertyChanged += (_, _) => OnPropertyChanged(nameof(SelectedProperty));
        }

        /// <summary>Whether the sort dropdown is visible.</summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        /// <summary>Toggle dropdown visibility.</summary>
        public void Toggle() => IsVisible = !IsVisible;

        /// <summary>Close the dropdown.</summary>
        public void Close() => IsVisible = false;

        /// <summary>Properties available for sorting (non-private properties).</summary>
        public IReadOnlyList<PivotViewerProperty> AvailableProperties
        {
            get
            {
                var sortable = new List<PivotViewerProperty>();
                foreach (var prop in _controller.Properties)
                {
                    if (!prop.Options.HasFlag(PivotViewerPropertyOptions.Private))
                        sortable.Add(prop);
                }
                return sortable;
            }
        }

        /// <summary>Currently selected sort property.</summary>
        public PivotViewerProperty? SelectedProperty
        {
            get => _controller.SortProperty;
            set
            {
                _controller.SortProperty = value;
                IsVisible = false;
                OnPropertyChanged(nameof(SelectedProperty));
            }
        }

        /// <summary>Select a property and close the dropdown.</summary>
        public void SelectProperty(PivotViewerProperty property)
        {
            SelectedProperty = property;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
