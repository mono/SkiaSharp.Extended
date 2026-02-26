using Microsoft.Maui.Controls;
using SkiaSharp.Extended.PivotViewer;
using System;

namespace SkiaSharp.Extended.UI.Maui.PivotViewer
{
    /// <summary>
    /// Native MAUI control bar for the PivotViewer.
    /// Horizontal toolbar: [Filter ☰] [Grid ▦] [Graph 📊] | Sort: [Property ▼] | [Search] | [42 of 298]
    /// </summary>
    public class PivotViewerControlBar : ContentView
    {
        private readonly Button _filterToggleButton;
        private readonly Button _gridViewButton;
        private readonly Button _graphViewButton;
        private readonly Label _sortLabel;
        private readonly Entry _searchEntry;
        private readonly Label _countLabel;
        private readonly Slider _zoomSlider;

        private ControlBarModel? _model;
        private bool _suppressZoomSync;

        public static readonly BindableProperty ModelProperty =
            BindableProperty.Create(nameof(Model), typeof(ControlBarModel), typeof(PivotViewerControlBar),
                null, propertyChanged: OnModelChanged);

        public static readonly BindableProperty BarBackgroundColorProperty =
            BindableProperty.Create(nameof(BarBackgroundColor), typeof(Color), typeof(PivotViewerControlBar),
                Color.FromRgb(0x2D, 0x2D, 0x30), propertyChanged: OnBarBackgroundChanged);

        public ControlBarModel? Model
        {
            get => (ControlBarModel?)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public Color BarBackgroundColor
        {
            get => (Color)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        /// <summary>Raised when the filter pane toggle is tapped.</summary>
        public event EventHandler? FilterToggled;

        /// <summary>Raised when a view mode button is tapped. Arg is "grid" or "graph".</summary>
        public event EventHandler<string>? ViewChanged;

        /// <summary>Raised when the sort label is tapped (to open dropdown).</summary>
        public event EventHandler? SortTapped;

        /// <summary>Raised when the search text changes.</summary>
        public event EventHandler<string>? SearchTextChanged;

        /// <summary>Raised when the zoom slider value changes.</summary>
        public event EventHandler<double>? ZoomChanged;

        public PivotViewerControlBar()
        {
            _filterToggleButton = new Button
            {
                Text = "☰",
                FontSize = 16,
                WidthRequest = 40,
                HeightRequest = 36,
                Padding = new Thickness(0),
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
            };
            AutomationProperties.SetName(_filterToggleButton, "Toggle filter pane");
            _filterToggleButton.Clicked += (s, e) =>
            {
                _model?.ToggleFilterPane();
                FilterToggled?.Invoke(this, EventArgs.Empty);
            };

            _gridViewButton = new Button
            {
                Text = "▦",
                FontSize = 16,
                WidthRequest = 40,
                HeightRequest = 36,
                Padding = new Thickness(0),
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
            };
            AutomationProperties.SetName(_gridViewButton, "Grid view");
            _gridViewButton.Clicked += (s, e) =>
            {
                _model?.SetView("grid");
                ViewChanged?.Invoke(this, "grid");
            };

            _graphViewButton = new Button
            {
                Text = "📊",
                FontSize = 14,
                WidthRequest = 40,
                HeightRequest = 36,
                Padding = new Thickness(0),
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
            };
            AutomationProperties.SetName(_graphViewButton, "Graph view");
            _graphViewButton.Clicked += (s, e) =>
            {
                _model?.SetView("graph");
                ViewChanged?.Invoke(this, "graph");
            };

            _sortLabel = new Label
            {
                Text = "Sort ▼",
                FontSize = 13,
                TextColor = Colors.White,
                VerticalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(8, 0),
            };
            AutomationProperties.SetName(_sortLabel, "Sort property");
            var sortTap = new TapGestureRecognizer();
            sortTap.Tapped += (s, e) => SortTapped?.Invoke(this, EventArgs.Empty);
            _sortLabel.GestureRecognizers.Add(sortTap);

            _searchEntry = new Entry
            {
                Placeholder = "Search...",
                FontSize = 12,
                HeightRequest = 32,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
            };
            AutomationProperties.SetName(_searchEntry, "Search items");
            _searchEntry.TextChanged += (s, e) =>
            {
                if (_model != null)
                    _model.SearchText = e.NewTextValue ?? "";
                SearchTextChanged?.Invoke(this, e.NewTextValue ?? "");
            };

            _zoomSlider = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = 0,
                WidthRequest = 120,
                HeightRequest = 30,
                VerticalOptions = LayoutOptions.Center,
            };
            AutomationProperties.SetName(_zoomSlider, "Zoom level");
            _zoomSlider.ValueChanged += (s, e) =>
            {
                if (!_suppressZoomSync)
                    ZoomChanged?.Invoke(this, e.NewValue);
            };

            _countLabel = new Label
            {
                Text = "0",
                FontSize = 12,
                TextColor = Colors.LightGray,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End,
                Padding = new Thickness(8, 0),
            };
            AutomationProperties.SetName(_countLabel, "Item count");

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),  // filter toggle
                    new ColumnDefinition(GridLength.Auto),  // grid button
                    new ColumnDefinition(GridLength.Auto),  // graph button
                    new ColumnDefinition(GridLength.Auto),  // sort label
                    new ColumnDefinition(GridLength.Star),  // search entry
                    new ColumnDefinition(GridLength.Auto),  // zoom slider
                    new ColumnDefinition(GridLength.Auto),  // count
                },
                ColumnSpacing = 4,
                Padding = new Thickness(4, 2),
                BackgroundColor = BarBackgroundColor,
                HeightRequest = 40,
            };

            grid.Add(_filterToggleButton, 0);
            grid.Add(_gridViewButton, 1);
            grid.Add(_graphViewButton, 2);
            grid.Add(_sortLabel, 3);
            grid.Add(_searchEntry, 4);
            grid.Add(_zoomSlider, 5);
            grid.Add(_countLabel, 6);

            Content = grid;
        }

        /// <summary>
        /// Syncs the zoom slider value from an external source (e.g., pinch zoom).
        /// </summary>
        public void SetZoomValue(double value)
        {
            _suppressZoomSync = true;
            _zoomSlider.Value = Math.Clamp(value, 0, 1);
            _suppressZoomSync = false;
        }

        private static void OnModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PivotViewerControlBar bar)
            {
                if (oldValue is ControlBarModel oldModel)
                    oldModel.PropertyChanged -= bar.OnModelPropertyChanged;

                bar._model = newValue as ControlBarModel;

                if (bar._model != null)
                {
                    bar._model.PropertyChanged += bar.OnModelPropertyChanged;
                    bar.SyncFromModel();
                }
            }
        }

        private static void OnBarBackgroundChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PivotViewerControlBar bar && bar.Content is Grid grid && newValue is Color color)
                grid.BackgroundColor = color;
        }

        private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(SyncFromModel);
        }

        private void SyncFromModel()
        {
            if (_model == null) return;

            _sortLabel.Text = _model.SortPropertyName != null
                ? $"{_model.SortPropertyName} ▼"
                : "Sort ▼";
            _countLabel.Text = _model.CountDisplay;
        }
    }
}
