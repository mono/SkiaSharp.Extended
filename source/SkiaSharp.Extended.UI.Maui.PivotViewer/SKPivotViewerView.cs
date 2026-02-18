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
        private bool _disposed;

        public SKPivotViewerView()
        {
            _controller = new PivotViewerController();
            _itemPaint = new SKPaint { IsAntialias = true, Color = SKColors.CornflowerBlue };
            _selectedPaint = new SKPaint { IsAntialias = true, Color = SKColors.Orange, IsStroke = true, StrokeWidth = 3 };
            _textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White, TextSize = 12 };
            _labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.DarkGray, TextSize = 14 };

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

        /// <summary>Accent color for the UI chrome.</summary>
        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        // --- Properties ---

        /// <summary>The underlying PivotViewer controller.</summary>
        public PivotViewerController Controller => _controller;

        /// <summary>All items in the collection.</summary>
        public IReadOnlyList<PivotViewerItem> Items => _controller.Items;

        /// <summary>Items currently in scope after filtering.</summary>
        public IReadOnlyList<PivotViewerItem> InScopeItems => _controller.InScopeItems;

        /// <summary>Property definitions.</summary>
        public IReadOnlyList<PivotViewerProperty> PivotProperties => _controller.Properties;

        /// <summary>Currently selected item.</summary>
        public PivotViewerItem? SelectedItem
        {
            get => _controller.SelectedItem;
            set => _controller.SelectedItem = value;
        }

        /// <summary>Index of the selected item.</summary>
        public int SelectedIndex
        {
            get => _controller.SelectedIndex;
            set => _controller.SelectedIndex = value;
        }

        /// <summary>Current sort property.</summary>
        public PivotViewerProperty? SortPivotProperty
        {
            get => _controller.SortProperty;
            set => _controller.SortProperty = value;
        }

        /// <summary>Current view mode ("grid" or "graph").</summary>
        public string View
        {
            get => _controller.CurrentView;
            set => _controller.CurrentView = value;
        }

        /// <summary>Serialized filter state.</summary>
        public string Filter => _controller.SerializeViewerState();

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
                        _textPaint.TextSize = Math.Min(12, (float)pos.Height / 4);
                        var textBounds = new SKRect();
                        _textPaint.MeasureText(name, ref textBounds);

                        if (textBounds.Width < rect.Width - 4)
                        {
                            canvas.DrawText(name, rect.Left + 4, rect.Bottom - 4, _textPaint);
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
                _labelPaint.TextSize = 12;
                var labelBounds = new SKRect();
                _labelPaint.MeasureText(col.Label, ref labelBounds);

                float labelX = (float)(col.X + (col.Width - labelBounds.Width) / 2);
                canvas.DrawText(col.Label, labelX, info.Height - 4, _labelPaint);

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
            _itemPaint.Dispose();
            _selectedPaint.Dispose();
            _textPaint.Dispose();
            _labelPaint.Dispose();
        }
    }
}
