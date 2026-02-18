using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Extended.PivotViewer;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SkiaSharp.Extended.UI.Maui.PivotViewer
{
    /// <summary>
    /// A MAUI control that provides the full PivotViewer experience:
    /// grid/graph views, filter pane, detail pane, and search.
    /// Wraps PivotViewerController with BindableProperties and gesture handling.
    /// </summary>
    public class SKPivotViewerView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly PivotViewerController _controller;
        private readonly SKPaint _itemPaint;
        private readonly SKPaint _selectedPaint;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _labelPaint;
        private readonly SKFont _textFont;
        private readonly SKFont _labelFont;
        private bool _disposed;

        public SKPivotViewerView()
        {
            _controller = new PivotViewerController();
            _itemPaint = new SKPaint { IsAntialias = true, Color = SKColors.CornflowerBlue };
            _selectedPaint = new SKPaint { IsAntialias = true, Color = SKColors.Orange, IsStroke = true, StrokeWidth = 3 };
            _textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
            _labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.DarkGray };
            _textFont = new SKFont { Size = 12 };
            _labelFont = new SKFont { Size = 14 };

            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnPaintSurface;
            Content = _canvasView;

            // Wire controller events
            _controller.LayoutUpdated += (s, e) =>
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            _controller.SelectionChanged += (s, e) =>
            {
                SelectionChanged?.Invoke(this, e);
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            };
            _controller.FiltersChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);
            _controller.ViewChanged += (s, e) => ViewChanged?.Invoke(this, EventArgs.Empty);
            _controller.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, EventArgs.Empty);

            SetupGestures();
        }

        // --- BindableProperties ---

        public static readonly BindableProperty AccentColorProperty =
            BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(SKPivotViewerView),
                Colors.CornflowerBlue);

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<PivotViewerItem>), typeof(SKPivotViewerView),
                null, propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty PivotPropertiesProperty =
            BindableProperty.Create(nameof(PivotProperties), typeof(IEnumerable<PivotViewerProperty>), typeof(SKPivotViewerView),
                null, propertyChanged: OnPivotPropertiesChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(PivotViewerItem), typeof(SKPivotViewerView),
                null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SKPivotViewerView),
                -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

        public static readonly BindableProperty SortPivotPropertyProperty =
            BindableProperty.Create(nameof(SortPivotProperty), typeof(PivotViewerProperty), typeof(SKPivotViewerView),
                null, BindingMode.TwoWay, propertyChanged: OnSortPivotPropertyChanged);

        public static readonly BindableProperty ViewProperty =
            BindableProperty.Create(nameof(View), typeof(string), typeof(SKPivotViewerView),
                "grid", BindingMode.TwoWay, propertyChanged: OnViewChanged);

        public static readonly BindableProperty ItemCultureProperty =
            BindableProperty.Create(nameof(ItemCulture), typeof(CultureInfo), typeof(SKPivotViewerView),
                CultureInfo.CurrentCulture);

        /// <summary>Accent color for the UI chrome.</summary>
        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        /// <summary>Bind a collection of PivotViewerItem (SL5 binding model).</summary>
        public IEnumerable<PivotViewerItem>? ItemsSource
        {
            get => (IEnumerable<PivotViewerItem>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>Facet definitions for filtering and display.</summary>
        public new IEnumerable<PivotViewerProperty>? PivotProperties
        {
            get => (IEnumerable<PivotViewerProperty>?)GetValue(PivotPropertiesProperty);
            set => SetValue(PivotPropertiesProperty, value);
        }

        /// <summary>Currently selected item (two-way bindable).</summary>
        public new PivotViewerItem? SelectedItem
        {
            get => (PivotViewerItem?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>Index of the selected item (two-way bindable).</summary>
        public new int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        /// <summary>Active sort facet (two-way bindable).</summary>
        public PivotViewerProperty? SortPivotProperty
        {
            get => (PivotViewerProperty?)GetValue(SortPivotPropertyProperty);
            set => SetValue(SortPivotPropertyProperty, value);
        }

        /// <summary>Current view mode: "grid" or "graph" (two-way bindable).</summary>
        public new string View
        {
            get => (string)GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        /// <summary>Locale for date/number formatting.</summary>
        public CultureInfo ItemCulture
        {
            get => (CultureInfo)GetValue(ItemCultureProperty);
            set => SetValue(ItemCultureProperty, value);
        }

        /// <summary>Serialized filter state. Get to bookmark, set to restore.</summary>
        public string Filter => _controller.SerializeViewerState();

        /// <summary>Items currently in scope after filtering (read-only).</summary>
        public IReadOnlyList<PivotViewerItem> InScopeItems => _controller.InScopeItems;

        /// <summary>The underlying PivotViewer controller.</summary>
        public PivotViewerController Controller => _controller;

        // --- BindableProperty Change Handlers ---

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is IEnumerable<PivotViewerItem> items)
            {
                var props = view.PivotProperties ?? Enumerable.Empty<PivotViewerProperty>();
                view._controller.LoadItems(items, props);
                view._canvasView.InvalidateSurface();
            }
        }

        private static void OnPivotPropertiesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is IEnumerable<PivotViewerProperty> props)
            {
                var items = view.ItemsSource ?? Enumerable.Empty<PivotViewerItem>();
                view._controller.LoadItems(items, props);
                view._canvasView.InvalidateSurface();
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
            {
                view._controller.SelectedItem = newValue as PivotViewerItem;
            }
        }

        private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is int index)
            {
                view._controller.SelectedIndex = index;
            }
        }

        private static void OnSortPivotPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view)
            {
                view._controller.SortProperty = newValue as PivotViewerProperty;
            }
        }

        private static void OnViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKPivotViewerView view && newValue is string viewMode)
            {
                view._controller.CurrentView = viewMode;
            }
        }

        // --- Events ---

        public event EventHandler<SkiaSharp.Extended.PivotViewer.SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? FilterChanged;
        public event EventHandler? ViewChanged;
        public event EventHandler? CollectionChanged;
        public event EventHandler<PivotViewerItem>? ItemDoubleClick;

        // --- Methods ---

        /// <summary>
        /// Loads a CXML collection.
        /// </summary>
        public void LoadCollection(CxmlCollectionSource source)
        {
            _controller.LoadCollection(source);
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Loads items directly (SL5 binding model).
        /// </summary>
        public void LoadItems(
            IEnumerable<PivotViewerItem> items,
            IEnumerable<PivotViewerProperty> properties)
        {
            _controller.LoadItems(items, properties);
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Serializes the current viewer state.
        /// </summary>
        public string SerializeViewerState() => _controller.SerializeViewerState();

        /// <summary>
        /// Restores viewer state from a serialized string.
        /// </summary>
        public void SetViewerState(string state)
        {
            _controller.SetViewerState(state);
            _canvasView.InvalidateSurface();
        }

        // --- Rendering ---

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.White);

            _controller.SetAvailableSize(info.Width, info.Height);

            if (_controller.CurrentView == "graph" && _controller.HistogramLayout != null)
            {
                RenderHistogramView(canvas, info, _controller.HistogramLayout);
            }
            else if (_controller.GridLayout != null)
            {
                RenderGridView(canvas, info, _controller.GridLayout);
            }
        }

        private void RenderGridView(SKCanvas canvas, SKImageInfo info, GridLayout layout)
        {
            var accentColor = ToSKColor(AccentColor);
            _itemPaint.Color = accentColor;

            foreach (var pos in layout.Positions)
            {
                var rect = new SKRect(
                    (float)pos.X + 1, (float)pos.Y + 1,
                    (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

                canvas.DrawRect(rect, _itemPaint);

                // Draw item name if space allows
                if (pos.Height > 20)
                {
                    var name = GetItemDisplayName(pos.Item);
                    if (name != null)
                    {
                        _textFont.Size = Math.Min(12, (float)pos.Height / 4);
                        var textWidth = _textFont.MeasureText(name, out _);

                        if (textWidth < rect.Width - 4)
                        {
                            canvas.DrawText(name, rect.Left + 4, rect.Bottom - 4, SKTextAlign.Left, _textFont, _textPaint);
                        }
                    }
                }

                // Highlight selected item
                if (pos.Item == _controller.SelectedItem)
                {
                    _selectedPaint.Color = SKColors.Orange;
                    canvas.DrawRect(rect, _selectedPaint);
                }
            }
        }

        private void RenderHistogramView(SKCanvas canvas, SKImageInfo info, HistogramLayout layout)
        {
            var accentColor = ToSKColor(AccentColor);
            _itemPaint.Color = accentColor;

            foreach (var col in layout.Columns)
            {
                // Draw column label
                var textWidth = _labelFont.MeasureText(col.Label, out _);

                float labelX = (float)(col.X + (col.Width - textWidth) / 2);
                canvas.DrawText(col.Label, labelX, info.Height - 4, SKTextAlign.Left, _labelFont, _labelPaint);

                // Draw items
                foreach (var pos in col.Items)
                {
                    var rect = new SKRect(
                        (float)pos.X + 1, (float)pos.Y + 1,
                        (float)(pos.X + pos.Width) - 1, (float)(pos.Y + pos.Height) - 1);

                    canvas.DrawRect(rect, _itemPaint);

                    if (pos.Item == _controller.SelectedItem)
                    {
                        _selectedPaint.Color = SKColors.Orange;
                        canvas.DrawRect(rect, _selectedPaint);
                    }
                }
            }
        }

        private static string? GetItemDisplayName(PivotViewerItem item)
        {
            var name = item["Name"];
            if (name != null && name.Count > 0)
                return name[0]?.ToString();
            return item.Id;
        }

        private static SKColor ToSKColor(Color color)
        {
            return new SKColor(
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255),
                (byte)(color.Alpha * 255));
        }

        // --- Gestures ---

        private void SetupGestures()
        {
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapped;
            GestureRecognizers.Add(tapGesture);

            var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTapGesture.Tapped += OnDoubleTapped;
            GestureRecognizers.Add(doubleTapGesture);
        }

        private void OnTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.HasValue)
            {
                var hit = _controller.HitTest(point.Value.X, point.Value.Y);
                _controller.SelectedItem = hit;
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.HasValue)
            {
                var hit = _controller.HitTest(point.Value.X, point.Value.Y);
                if (hit != null)
                    ItemDoubleClick?.Invoke(this, hit);
            }
        }

        // --- Disposal ---

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _canvasView.PaintSurface -= OnPaintSurface;
            _controller.Dispose();
            _itemPaint.Dispose();
            _selectedPaint.Dispose();
            _textPaint.Dispose();
            _labelPaint.Dispose();
            _textFont.Dispose();
            _labelFont.Dispose();
        }
    }
}
