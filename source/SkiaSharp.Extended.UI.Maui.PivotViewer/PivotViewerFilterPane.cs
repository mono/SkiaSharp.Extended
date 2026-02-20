using Microsoft.Maui.Controls;
using SkiaSharp.Extended.PivotViewer;
using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.UI.Maui.PivotViewer
{
    /// <summary>
    /// Native MAUI filter pane for PivotViewer.
    /// Scrollable left panel with filter categories, checkboxes, and clear buttons.
    /// </summary>
    public class PivotViewerFilterPane : ContentView
    {
        private readonly ScrollView _scrollView;
        private readonly VerticalStackLayout _rootStack;
        private readonly Button _clearAllButton;

        private FilterPaneModel? _model;
        private IReadOnlyList<PivotViewerItem>? _items;
        private const int DefaultMaxVisible = 8;

        public static readonly BindableProperty ModelProperty =
            BindableProperty.Create(nameof(Model), typeof(FilterPaneModel), typeof(PivotViewerFilterPane),
                null, propertyChanged: OnModelChanged);

        public static readonly BindableProperty ItemsProperty =
            BindableProperty.Create(nameof(Items), typeof(IReadOnlyList<PivotViewerItem>), typeof(PivotViewerFilterPane),
                null, propertyChanged: OnItemsChanged);

        public FilterPaneModel? Model
        {
            get => (FilterPaneModel?)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public IReadOnlyList<PivotViewerItem>? Items
        {
            get => (IReadOnlyList<PivotViewerItem>?)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        /// <summary>Raised when a filter value changes (for parent to invalidate canvas).</summary>
        public event EventHandler? FilterChanged;

        public PivotViewerFilterPane()
        {
            WidthRequest = 220;

            _clearAllButton = new Button
            {
                Text = "Clear All Filters",
                FontSize = 12,
                HeightRequest = 32,
                Padding = new Thickness(8, 4),
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.CornflowerBlue,
                IsVisible = false,
            };
            AutomationProperties.SetName(_clearAllButton, "Clear all filters");
            _clearAllButton.Clicked += (s, e) =>
            {
                _model?.ClearAllFilters();
                Refresh();
                FilterChanged?.Invoke(this, EventArgs.Empty);
            };

            _rootStack = new VerticalStackLayout { Spacing = 2 };
            _rootStack.Children.Add(_clearAllButton);

            _scrollView = new ScrollView
            {
                Content = _rootStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            };

            Content = _scrollView;
        }

        /// <summary>
        /// Rebuilds the filter pane UI from the current model and items.
        /// </summary>
        public void Refresh()
        {
            // Clear everything after the clear-all button
            while (_rootStack.Children.Count > 1)
                _rootStack.Children.RemoveAt(_rootStack.Children.Count - 1);

            if (_model == null || _items == null || _items.Count == 0)
            {
                _clearAllButton.IsVisible = false;
                return;
            }

            _clearAllButton.IsVisible = _model.HasActiveFilters;

            var categories = _model.GetCategories(_items);
            foreach (var category in categories)
            {
                BuildCategorySection(category);
            }
        }

        private void BuildCategorySection(FilterCategory category)
        {
            // Header row: category name + optional clear (✕)
            var headerGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                Padding = new Thickness(8, 6, 8, 2),
            };

            var headerLabel = new Label
            {
                Text = category.Property.DisplayName,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                VerticalTextAlignment = TextAlignment.Center,
            };
            headerGrid.Add(headerLabel, 0);

            if (category.IsFiltered)
            {
                var clearButton = new Button
                {
                    Text = "✕",
                    FontSize = 11,
                    WidthRequest = 28,
                    HeightRequest = 28,
                    Padding = new Thickness(0),
                    BackgroundColor = Colors.Transparent,
                    TextColor = Colors.Gray,
                };
                AutomationProperties.SetName(clearButton, $"Clear {category.Property.DisplayName} filters");
                var propId = category.Property.Id;
                clearButton.Clicked += (s, e) =>
                {
                    _model?.ClearPropertyFilters(propId);
                    Refresh();
                    FilterChanged?.Invoke(this, EventArgs.Empty);
                };
                headerGrid.Add(clearButton, 1);
            }

            _rootStack.Children.Add(headerGrid);

            // Values
            if (category.ValueCounts == null || category.ValueCounts.Count == 0)
            {
                if (category.Property.PropertyType == PivotViewerPropertyType.Decimal ||
                    category.Property.PropertyType == PivotViewerPropertyType.DateTime)
                {
                    var numericLabel = new Label
                    {
                        Text = category.Property.PropertyType == PivotViewerPropertyType.DateTime
                            ? "Date/time filter"
                            : "Numeric filter",
                        FontSize = 11,
                        TextColor = Colors.Gray,
                        Padding = new Thickness(12, 2),
                    };
                    _rootStack.Children.Add(numericLabel);
                }
                return;
            }

            var activeSet = new HashSet<string>(StringComparer.Ordinal);
            if (category.ActiveFilters != null)
            {
                foreach (var f in category.ActiveFilters)
                    activeSet.Add(f);
            }

            int count = 0;
            int total = category.ValueCounts.Count;
            bool needsToggle = total > DefaultMaxVisible;
            bool isExpanded = category.IsExpanded;

            var valuesStack = new VerticalStackLayout { Spacing = 1 };

            foreach (var kvp in category.ValueCounts)
            {
                count++;
                if (!isExpanded && count > DefaultMaxVisible)
                    break;

                var row = new HorizontalStackLayout
                {
                    Spacing = 4,
                    Padding = new Thickness(12, 1),
                };

                var checkBox = new CheckBox
                {
                    IsChecked = activeSet.Contains(kvp.Key),
                    WidthRequest = 20,
                    HeightRequest = 20,
                    VerticalOptions = LayoutOptions.Center,
                };
                var propId = category.Property.Id;
                var filterValue = kvp.Key;
                checkBox.CheckedChanged += (s, e) =>
                {
                    _model?.ToggleStringFilter(propId, filterValue);
                    Refresh();
                    FilterChanged?.Invoke(this, EventArgs.Empty);
                };

                var valueLabel = new Label
                {
                    Text = $"{kvp.Key} ({kvp.Value})",
                    FontSize = 12,
                    TextColor = Colors.Black,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.TailTruncation,
                };

                row.Children.Add(checkBox);
                row.Children.Add(valueLabel);
                valuesStack.Children.Add(row);
            }

            _rootStack.Children.Add(valuesStack);

            // Show all / Show less toggle
            if (needsToggle)
            {
                var toggleButton = new Button
                {
                    Text = isExpanded ? "Show less" : $"Show all {total} values",
                    FontSize = 11,
                    HeightRequest = 28,
                    Padding = new Thickness(12, 2),
                    BackgroundColor = Colors.Transparent,
                    TextColor = Colors.CornflowerBlue,
                };
                var cat = category;
                toggleButton.Clicked += (s, e) =>
                {
                    cat.ToggleExpanded();
                    Refresh();
                };
                _rootStack.Children.Add(toggleButton);
            }

            // Separator
            _rootStack.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromRgb(0xE0, 0xE0, 0xE0),
                Margin = new Thickness(8, 4),
            });
        }

        private static void OnModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PivotViewerFilterPane pane)
            {
                if (oldValue is FilterPaneModel oldModel)
                    oldModel.PropertyChanged -= pane.OnModelPropertyChanged;

                pane._model = newValue as FilterPaneModel;

                if (pane._model != null)
                    pane._model.PropertyChanged += pane.OnModelPropertyChanged;

                pane.Refresh();
            }
        }

        private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PivotViewerFilterPane pane)
            {
                pane._items = newValue as IReadOnlyList<PivotViewerItem>;
                pane.Refresh();
            }
        }

        private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(Refresh);
        }
    }
}
