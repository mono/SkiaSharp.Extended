using Microsoft.Maui.Controls;
using SkiaSharp.Extended.PivotViewer;
using System;

namespace SkiaSharp.Extended.UI.Maui.PivotViewer
{
    /// <summary>
    /// Native MAUI detail pane for PivotViewer.
    /// Shows selected item name, thumbnail, description, facet values, and copyright.
    /// </summary>
    public class PivotViewerDetailPane : ContentView
    {
        private readonly ScrollView _scrollView;
        private readonly VerticalStackLayout _rootStack;
        private readonly Label _nameLabel;
        private readonly Image _thumbnailImage;
        private readonly Label _descriptionLabel;
        private readonly BoxView _separator;
        private readonly VerticalStackLayout _facetsStack;
        private readonly Label _copyrightLabel;

        private DetailPaneModel? _model;

        public static readonly BindableProperty ModelProperty =
            BindableProperty.Create(nameof(Model), typeof(DetailPaneModel), typeof(PivotViewerDetailPane),
                null, propertyChanged: OnModelChanged);

        public DetailPaneModel? Model
        {
            get => (DetailPaneModel?)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        /// <summary>Raised when a hyperlink in the detail pane is clicked.</summary>
        public event EventHandler<PivotViewerLinkEventArgs>? LinkClicked;

        /// <summary>Raised when a filterable value is tapped.</summary>
        public event EventHandler<PivotViewerFilterEventArgs>? FilterRequested;

        public PivotViewerDetailPane()
        {
            WidthRequest = 280;

            _nameLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                Padding = new Thickness(12, 12, 12, 4),
                LineBreakMode = LineBreakMode.WordWrap,
            };

            _thumbnailImage = new Image
            {
                HeightRequest = 160,
                Aspect = Aspect.AspectFit,
                Margin = new Thickness(12, 4),
            };

            _descriptionLabel = new Label
            {
                FontSize = 11,
                TextColor = Colors.Gray,
                Padding = new Thickness(12, 4),
                LineBreakMode = LineBreakMode.WordWrap,
            };

            _separator = new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromRgb(0xE0, 0xE0, 0xE0),
                Margin = new Thickness(12, 8),
            };

            _facetsStack = new VerticalStackLayout { Spacing = 4 };

            _copyrightLabel = new Label
            {
                FontSize = 10,
                TextColor = Colors.LightGray,
                Padding = new Thickness(12, 8),
                LineBreakMode = LineBreakMode.WordWrap,
            };

            _rootStack = new VerticalStackLayout { Spacing = 0 };
            _rootStack.Children.Add(_nameLabel);
            _rootStack.Children.Add(_thumbnailImage);
            _rootStack.Children.Add(_descriptionLabel);
            _rootStack.Children.Add(_separator);
            _rootStack.Children.Add(_facetsStack);
            _rootStack.Children.Add(_copyrightLabel);

            _scrollView = new ScrollView
            {
                Content = _rootStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            };

            Content = _scrollView;
        }

        private void SyncFromModel()
        {
            if (_model == null || !_model.IsShowing)
            {
                IsVisible = false;
                return;
            }

            IsVisible = true;

            _nameLabel.Text = _model.ItemName ?? "";
            _descriptionLabel.Text = _model.Description ?? "";
            _descriptionLabel.IsVisible = !string.IsNullOrEmpty(_model.Description);

            // Thumbnail
            if (_model.Thumbnail != null)
            {
                var stream = _model.Thumbnail.Encode(SKEncodedImageFormat.Png, 90).AsStream();
                _thumbnailImage.Source = ImageSource.FromStream(() => stream);
                _thumbnailImage.IsVisible = true;
            }
            else
            {
                _thumbnailImage.IsVisible = false;
            }

            // Copyright
            _copyrightLabel.Text = _model.CopyrightText ?? "";
            _copyrightLabel.IsVisible = !string.IsNullOrEmpty(_model.CopyrightText);

            // Facets
            _facetsStack.Children.Clear();
            foreach (var facet in _model.FacetValues)
            {
                var propLabel = new Label
                {
                    Text = facet.DisplayName,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DimGray,
                    Padding = new Thickness(12, 4, 12, 0),
                };
                _facetsStack.Children.Add(propLabel);

                foreach (var value in facet.Values)
                {
                    bool isLink = facet.Property.PropertyType == PivotViewerPropertyType.Link;
                    bool canFilter = facet.Property.CanFilter;

                    var valueLabel = new Label
                    {
                        Text = value,
                        FontSize = 12,
                        TextColor = isLink || canFilter ? Colors.CornflowerBlue : Colors.Black,
                        TextDecorations = isLink ? TextDecorations.Underline : TextDecorations.None,
                        Padding = new Thickness(20, 1, 12, 1),
                        LineBreakMode = LineBreakMode.WordWrap,
                    };

                    if (isLink)
                    {
                        var linkValue = value;
                        var tap = new TapGestureRecognizer();
                        tap.Tapped += (s, e) =>
                        {
                            if (Uri.TryCreate(linkValue, UriKind.Absolute, out var uri))
                            {
                                _model?.OnLinkClicked(uri);
                                LinkClicked?.Invoke(this, new PivotViewerLinkEventArgs(uri));
                            }
                        };
                        valueLabel.GestureRecognizers.Add(tap);
                    }
                    else if (canFilter)
                    {
                        var filterValue = value;
                        var tap = new TapGestureRecognizer();
                        tap.Tapped += (s, e) =>
                        {
                            _model?.OnApplyFilter(filterValue);
                            FilterRequested?.Invoke(this, new PivotViewerFilterEventArgs(filterValue));
                        };
                        valueLabel.GestureRecognizers.Add(tap);
                    }

                    _facetsStack.Children.Add(valueLabel);
                }
            }
        }

        private static void OnModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PivotViewerDetailPane pane)
            {
                if (oldValue is DetailPaneModel oldModel)
                    oldModel.PropertyChanged -= pane.OnModelPropertyChanged;

                pane._model = newValue as DetailPaneModel;

                if (pane._model != null)
                    pane._model.PropertyChanged += pane.OnModelPropertyChanged;

                pane.SyncFromModel();
            }
        }

        private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(SyncFromModel);
        }
    }
}
