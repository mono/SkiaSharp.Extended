using System.ComponentModel;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Bindable model for the PivotViewer control bar (toolbar).
    /// Exposes all state needed by native UI controls: view mode, sort, search, counts.
    /// </summary>
    public class ControlBarModel : INotifyPropertyChanged
    {
        private readonly PivotViewerController _controller;
        private bool _isFilterPaneVisible = true;

        public ControlBarModel(PivotViewerController controller)
        {
            _controller = controller ?? throw new System.ArgumentNullException(nameof(controller));
            _controller.ViewChanged += (_, _) => OnPropertyChanged(nameof(CurrentView));
            _controller.SortPropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(SortPropertyName));
                OnPropertyChanged(nameof(SortDescending));
            };
            _controller.InScopeItemsChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(InScopeCount));
                OnPropertyChanged(nameof(CountDisplay));
            };
            _controller.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(CountDisplay));
            };
            _controller.FiltersChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(InScopeCount));
                OnPropertyChanged(nameof(CountDisplay));
            };
        }

        /// <summary>Whether the filter pane is visible.</summary>
        public bool IsFilterPaneVisible
        {
            get => _isFilterPaneVisible;
            set
            {
                if (_isFilterPaneVisible == value) return;
                _isFilterPaneVisible = value;
                OnPropertyChanged(nameof(IsFilterPaneVisible));
            }
        }

        /// <summary>Current view mode ("grid" or "graph").</summary>
        public string CurrentView => _controller.CurrentView;

        /// <summary>Display name of the active sort property, or null.</summary>
        public string? SortPropertyName => _controller.SortProperty?.DisplayName;

        /// <summary>Whether sorting is descending.</summary>
        public bool SortDescending => _controller.SortDescending;

        /// <summary>Text entered in the search box.</summary>
        public string SearchText
        {
            get => _controller.SearchText;
            set
            {
                if (_controller.SearchText == value) return;
                _controller.SearchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        /// <summary>Number of items currently in scope (after filtering).</summary>
        public int InScopeCount => _controller.InScopeItems.Count;

        /// <summary>Total number of items in the collection.</summary>
        public int TotalCount => _controller.Items.Count;

        /// <summary>Formatted count string, e.g. "42 of 298".</summary>
        public string CountDisplay =>
            InScopeCount == TotalCount
                ? $"{TotalCount}"
                : $"{InScopeCount} of {TotalCount}";

        /// <summary>Toggle filter pane visibility.</summary>
        public void ToggleFilterPane() => IsFilterPaneVisible = !IsFilterPaneVisible;

        /// <summary>Switch to the specified view mode.</summary>
        public void SetView(string view) => _controller.CurrentView = view;

        /// <summary>Toggle sort direction.</summary>
        public void ToggleSortDirection()
        {
            _controller.SortDescending = !_controller.SortDescending;
        }

        /// <summary>Clear search text and all filters.</summary>
        public void ClearAll()
        {
            SearchText = "";
            _controller.FilterPaneModel?.ClearAllFilters();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
