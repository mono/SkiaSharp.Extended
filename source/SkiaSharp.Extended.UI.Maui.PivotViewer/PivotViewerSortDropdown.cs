using Microsoft.Maui.Controls;
using SkiaSharp.Extended.PivotViewer;
using System;

namespace SkiaSharp.Extended.UI.Maui.PivotViewer
{
    /// <summary>
    /// Native MAUI sort dropdown overlay for PivotViewer.
    /// Shows available sort properties; tapping one selects it and closes.
    /// </summary>
    public class PivotViewerSortDropdown : ContentView
    {
        private readonly Border _border;
        private readonly VerticalStackLayout _stack;

        private SortDropdownModel? _model;

        public static readonly BindableProperty ModelProperty =
            BindableProperty.Create(nameof(Model), typeof(SortDropdownModel), typeof(PivotViewerSortDropdown),
                null, propertyChanged: OnModelChanged);

        public SortDropdownModel? Model
        {
            get => (SortDropdownModel?)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        /// <summary>Raised when a sort property is selected.</summary>
        public event EventHandler<PivotViewerProperty>? PropertySelected;

        public PivotViewerSortDropdown()
        {
            IsVisible = false;

            _stack = new VerticalStackLayout { Spacing = 0 };

            _border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
                Stroke = Colors.Gray,
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Colors.Black),
                    Offset = new Point(2, 2),
                    Radius = 6,
                    Opacity = 0.3f,
                },
                Padding = new Thickness(0),
                Content = _stack,
            };

            Content = _border;
        }

        private void Rebuild()
        {
            _stack.Children.Clear();

            if (_model == null)
            {
                IsVisible = false;
                return;
            }

            IsVisible = _model.IsVisible;
            if (!_model.IsVisible) return;

            var properties = _model.AvailableProperties;
            var selected = _model.SelectedProperty;

            foreach (var prop in properties)
            {
                bool isSelected = selected != null && selected.Id == prop.Id;

                var label = new Label
                {
                    Text = prop.DisplayName,
                    FontSize = 13,
                    FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
                    TextColor = isSelected ? Colors.CornflowerBlue : Colors.Black,
                    BackgroundColor = isSelected ? Color.FromRgba(0x64, 0x95, 0xED, 0x20) : Colors.Transparent,
                    Padding = new Thickness(16, 8),
                };
                AutomationProperties.SetName(label, $"Sort by {prop.DisplayName}");

                var sortProp = prop;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    _model?.SelectProperty(sortProp);
                    PropertySelected?.Invoke(this, sortProp);
                    Rebuild();
                };
                label.GestureRecognizers.Add(tap);

                _stack.Children.Add(label);
            }
        }

        private static void OnModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PivotViewerSortDropdown dropdown)
            {
                if (oldValue is SortDropdownModel oldModel)
                    oldModel.PropertyChanged -= dropdown.OnModelPropertyChanged;

                dropdown._model = newValue as SortDropdownModel;

                if (dropdown._model != null)
                    dropdown._model.PropertyChanged += dropdown.OnModelPropertyChanged;

                dropdown.Rebuild();
            }
        }

        private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(Rebuild);
        }
    }
}
