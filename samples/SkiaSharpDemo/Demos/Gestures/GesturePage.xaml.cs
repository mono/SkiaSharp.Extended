using SkiaSharp;
using SkiaSharp.Extended.Gestures;
using SkiaSharp.Extended.UI.Controls;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

/// <summary>
/// Demo page showcasing all gesture features of SKGestureSurfaceView.
/// </summary>
public partial class GesturePage : ContentPage
{
	// Sticker data for demonstration
	private readonly List<Sticker> _stickers = new();
	private Sticker? _selectedSticker;
	private readonly Queue<string> _eventLog = new();
	private const int MaxLogEntries = 5;

	// Feature toggles (for tap/longpress/drag which are app-level)
	private bool _enableTap = true;
	private bool _enableDoubleTap = true;
	private bool _enableLongPress = true;
	private bool _enableDrag = true;

	// Canvas dimensions for hit testing
	private int _canvasWidth;
	private int _canvasHeight;

	public GesturePage()
	{
		InitializeComponent();

		// Initialize with some stickers
		_stickers.Add(new Sticker { Position = new SKPoint(100, 100), Size = 80, Color = SKColors.Red, Label = "1" });
		_stickers.Add(new Sticker { Position = new SKPoint(200, 200), Size = 60, Color = SKColors.Green, Label = "2" });
		_stickers.Add(new Sticker { Position = new SKPoint(300, 150), Size = 70, Color = SKColors.Blue, Label = "3" });
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		gestureView.Invalidate();
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;

		// Cache canvas size for hit testing
		_canvasWidth = width;
		_canvasHeight = height;

		// Clear background
		canvas.Clear(SKColors.White);

		// Apply transform from the tracker
		canvas.Save();
		canvas.Concat(gestureView.Tracker.Matrix);

		// Draw grid background inside the transform so it pans/zooms/rotates with content
		DrawGrid(canvas, width, height);

		// Draw stickers
		foreach (var sticker in _stickers)
		{
			DrawSticker(canvas, sticker, sticker == _selectedSticker);
		}

		canvas.Restore();

		// Draw crosshair at center for reference
		using var crosshairPaint = new SKPaint
		{
			Color = SKColors.Gray.WithAlpha(100),
			StrokeWidth = 1,
			IsAntialias = true
		};
		canvas.DrawLine(width / 2f, 0, width / 2f, height, crosshairPaint);
		canvas.DrawLine(0, height / 2f, width, height / 2f, crosshairPaint);
	}

	private void DrawGrid(SKCanvas canvas, int width, int height)
	{
		const int gridSize = 40;
		
		using var lightPaint = new SKPaint { Color = new SKColor(240, 240, 240) };
		using var darkPaint = new SKPaint { Color = new SKColor(220, 220, 220) };

		// Expand grid coverage so it fills the view when zoomed out
		var scale = gestureView.Tracker.Scale;
		var extra = (int)(Math.Max(width, height) / scale) + gridSize * 4;
		// Snap start to grid boundary to keep checker pattern correct
		var startX = -(extra / gridSize) * gridSize;
		var startY = startX;
		var endX = width - startX;
		var endY = height - startY;

		for (int y = startY, row = 0; y < endY; y += gridSize, row++)
		{
			for (int x = startX, col = 0; x < endX; x += gridSize, col++)
			{
				var isLight = (col + row) % 2 == 0;
				var rect = new SKRect(x, y, x + gridSize, y + gridSize);
				canvas.DrawRect(rect, isLight ? lightPaint : darkPaint);
			}
		}
	}

	private void DrawSticker(SKCanvas canvas, Sticker sticker, bool isSelected)
	{
		var radius = sticker.Size / 2f;
		
		// Draw shadow
		using var shadowPaint = new SKPaint
		{
			Color = SKColors.Black.WithAlpha(50),
			IsAntialias = true,
			MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
		};
		canvas.DrawCircle(sticker.Position.X + 3, sticker.Position.Y + 3, radius, shadowPaint);

		// Draw sticker fill
		using var fillPaint = new SKPaint
		{
			Color = sticker.Color,
			IsAntialias = true,
			Style = SKPaintStyle.Fill
		};
		canvas.DrawCircle(sticker.Position, radius, fillPaint);

		// Draw selection ring
		if (isSelected)
		{
			using var selectPaint = new SKPaint
			{
				Color = SKColors.Yellow,
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 4
			};
			canvas.DrawCircle(sticker.Position, radius + 4, selectPaint);
		}

		// Draw border
		using var borderPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2
		};
		canvas.DrawCircle(sticker.Position, radius, borderPaint);

		// Draw label
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
			TextSize = sticker.Size * 0.4f,
			TextAlign = SKTextAlign.Center,
			Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
		};
		canvas.DrawText(sticker.Label, sticker.Position.X, sticker.Position.Y + textPaint.TextSize * 0.35f, textPaint);
	}

	private void OnTap(object? sender, SKTapEventArgs e)
	{
		if (!_enableTap) return;
		LogEvent($"Tap at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		// Try to select a sticker
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			statusLabel.Text = $"Selected: Sticker {hitSticker.Label}";
		}
		else
		{
			_selectedSticker = null;
			statusLabel.Text = "No selection";
		}
		
		gestureView.Invalidate();
	}

	private void OnDoubleTap(object? sender, SKTapEventArgs e)
	{
		if (!_enableDoubleTap) return;
		LogEvent($"Double tap ({e.TapCount}x) at ({e.Location.X:F0}, {e.Location.Y:F0})");

		// If a sticker is under the double-tap, select it and suppress zoom
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			statusLabel.Text = $"Selected: Sticker {hitSticker.Label}";
			e.Handled = true;
			gestureView.Invalidate();
		}
	}

	private void OnLongPress(object? sender, SKTapEventArgs e)
	{
		if (!_enableLongPress) return;
		LogEvent($"Long press at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			statusLabel.Text = $"Long press selected: Sticker {hitSticker.Label}";
		}
		
		gestureView.Invalidate();
	}

	private void OnPan(object? sender, SKPanEventArgs e)
	{
		// Transform is handled by the tracker
		statusLabel.Text = $"Pan: Δ({e.Delta.X:F1}, {e.Delta.Y:F1})";
	}

	private void OnPinch(object? sender, SKPinchEventArgs e)
	{
		LogEvent($"Pinch scale: {e.Scale:F2}");
		statusLabel.Text = $"Scale: {gestureView.Tracker.Scale:F2}";
	}

	private void OnRotate(object? sender, SKRotateEventArgs e)
	{
		LogEvent($"Rotate: {e.RotationDelta:F1}°");
		statusLabel.Text = $"Rotation: {gestureView.Tracker.Rotation:F1}°";
	}

	private void OnFling(object? sender, SKFlingEventArgs e)
	{
		LogEvent($"Fling: ({e.VelocityX:F0}, {e.VelocityY:F0}) px/s");
		statusLabel.Text = $"Flinging at {e.Speed:F0} px/s";
	}

	private void OnFlinging(object? sender, SKFlingEventArgs e)
	{
		// Fling transform is handled by the tracker
		statusLabel.Text = $"Flinging... ({e.Speed:F0} px/s)";
	}

	private void OnFlingCompleted(object? sender, EventArgs e)
	{
		statusLabel.Text = "Fling ended";
	}

	private void OnScroll(object? sender, SKScrollEventArgs e)
	{
		// Scroll zoom is handled by the tracker
		statusLabel.Text = $"Scroll zoom: {gestureView.Tracker.Scale:F2}x";
	}

	private void OnHover(object? sender, SKHoverEventArgs e)
	{
		statusLabel.Text = $"Hover: ({e.Location.X:F0}, {e.Location.Y:F0})";
	}

	private void OnDragStarted(object? sender, SKDragEventArgs e)
	{
		if (!_enableDrag) return;
		LogEvent($"Drag started at ({e.StartLocation.X:F0}, {e.StartLocation.Y:F0})");
		
		if (_selectedSticker != null)
		{
			statusLabel.Text = $"Dragging Sticker {_selectedSticker.Label}";
		}
	}

	private void OnDragUpdated(object? sender, SKDragEventArgs e)
	{
		if (!_enableDrag) return;
		// Move selected sticker
		if (_selectedSticker != null)
		{
			_selectedSticker.Position = new SKPoint(
				_selectedSticker.Position.X + e.Delta.X,
				_selectedSticker.Position.Y + e.Delta.Y);
			
			gestureView.Invalidate();
		}
	}

	private void OnDragEnded(object? sender, SKDragEventArgs e)
	{
		if (!_enableDrag) return;
		LogEvent($"Drag ended at ({e.CurrentLocation.X:F0}, {e.CurrentLocation.Y:F0})");
		statusLabel.Text = "Drag completed";
	}

	private Sticker? HitTest(SKPoint location)
	{
		if (_canvasWidth <= 0 || _canvasHeight <= 0)
			return null;

		var matrix = gestureView.Tracker.Matrix;
		if (!matrix.TryInvert(out var inverse))
			return null;

		var transformed = inverse.MapPoint(location);
		
		// Check stickers in reverse order (top to bottom)
		for (int i = _stickers.Count - 1; i >= 0; i--)
		{
			var sticker = _stickers[i];
			var dist = SKPoint.Distance(transformed, sticker.Position);
			if (dist <= sticker.Size / 2)
				return sticker;
		}
		
		return null;
	}

	private async void OnSettingsClicked(object? sender, EventArgs e)
	{
		var page = new ContentPage { Title = "Gesture Settings" };
		var tracker = gestureView.Tracker;

		var touchSlop = gestureView.TouchSlop;
		var longPressDuration = gestureView.LongPressDuration;

		var layout = new VerticalStackLayout { Padding = 20, Spacing = 12 };

		// --- Feature Toggles ---
		layout.Children.Add(new Label { Text = "Feature Toggles", FontAttributes = FontAttributes.Bold, FontSize = 16 });

		var toggles = new (string Label, bool Value, Action<bool> Setter)[]
		{
			("Pan", tracker.IsPanEnabled, v => tracker.IsPanEnabled = v),
			("Pinch (Zoom)", tracker.IsPinchEnabled, v => tracker.IsPinchEnabled = v),
			("Rotate", tracker.IsRotateEnabled, v => tracker.IsRotateEnabled = v),
			("Fling", tracker.IsFlingEnabled, v => tracker.IsFlingEnabled = v),
			("Double Tap Zoom", tracker.IsDoubleTapZoomEnabled, v => tracker.IsDoubleTapZoomEnabled = v),
			("Scroll Zoom", tracker.IsScrollZoomEnabled, v => tracker.IsScrollZoomEnabled = v),
			("Tap (App)", _enableTap, v => _enableTap = v),
			("Double Tap Log (App)", _enableDoubleTap, v => _enableDoubleTap = v),
			("Long Press (App)", _enableLongPress, v => _enableLongPress = v),
			("Drag Sticker (App)", _enableDrag, v => _enableDrag = v),
		};

		foreach (var (label, value, setter) in toggles)
		{
			var sw = new Switch { IsToggled = value };
			var captured = setter;
			sw.Toggled += (_, args) => captured(args.Value);
			layout.Children.Add(new HorizontalStackLayout
			{
				Spacing = 10,
				Children = { sw, new Label { Text = label, VerticalOptions = LayoutOptions.Center } }
			});
		}

		// --- Detection Thresholds ---
		layout.Children.Add(new Label { Text = "Detection Thresholds", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });

		// Touch slop
		var slopLabel = new Label { Text = $"Touch Slop: {touchSlop:F0} px" };
		var slopSlider = new Slider { Minimum = 1, Maximum = 50, Value = touchSlop };
		slopSlider.ValueChanged += (_, args) =>
		{
			gestureView.TouchSlop = (float)args.NewValue;
			slopLabel.Text = $"Touch Slop: {args.NewValue:F0} px";
		};
		layout.Children.Add(slopLabel);
		layout.Children.Add(slopSlider);

		// Long press duration
		var lpLabel = new Label { Text = $"Long Press: {longPressDuration} ms" };
		var lpSlider = new Slider { Minimum = 100, Maximum = 2000, Value = longPressDuration };
		lpSlider.ValueChanged += (_, args) =>
		{
			gestureView.LongPressDuration = (int)args.NewValue;
			lpLabel.Text = $"Long Press: {(int)args.NewValue} ms";
		};
		layout.Children.Add(lpLabel);
		layout.Children.Add(lpSlider);

		// --- Fling Settings ---
		layout.Children.Add(new Label { Text = "Fling Settings", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });

		// Fling friction
		var frictionLabel = new Label { Text = $"Friction: {tracker.FlingFriction:F2}" };
		var frictionSlider = new Slider { Minimum = 0.0, Maximum = 1.0, Value = tracker.FlingFriction };
		frictionSlider.ValueChanged += (_, args) =>
		{
			tracker.FlingFriction = (float)args.NewValue;
			frictionLabel.Text = $"Friction: {args.NewValue:F2}";
		};
		layout.Children.Add(frictionLabel);
		layout.Children.Add(frictionSlider);

		// Fling min velocity
		var minVelLabel = new Label { Text = $"Min Velocity: {tracker.FlingMinVelocity:F0} px/s" };
		var minVelSlider = new Slider { Minimum = 1, Maximum = 50, Value = tracker.FlingMinVelocity };
		minVelSlider.ValueChanged += (_, args) =>
		{
			tracker.FlingMinVelocity = (float)args.NewValue;
			minVelLabel.Text = $"Min Velocity: {args.NewValue:F0} px/s";
		};
		layout.Children.Add(minVelLabel);
		layout.Children.Add(minVelSlider);

		// Fling detection threshold
		var threshLabel = new Label { Text = $"Fling Threshold: {tracker.FlingThreshold:F0} px/s" };
		var threshSlider = new Slider { Minimum = 50, Maximum = 1000, Value = tracker.FlingThreshold };
		threshSlider.ValueChanged += (_, args) =>
		{
			tracker.FlingThreshold = (float)args.NewValue;
			threshLabel.Text = $"Fling Threshold: {args.NewValue:F0} px/s";
		};
		layout.Children.Add(threshLabel);
		layout.Children.Add(threshSlider);

		// Double tap slop
		var dtSlopLabel = new Label { Text = $"Double Tap Slop: {tracker.DoubleTapSlop:F0} px" };
		var dtSlopSlider = new Slider { Minimum = 10, Maximum = 200, Value = tracker.DoubleTapSlop };
		dtSlopSlider.ValueChanged += (_, args) =>
		{
			tracker.DoubleTapSlop = (float)args.NewValue;
			dtSlopLabel.Text = $"Double Tap Slop: {args.NewValue:F0} px";
		};
		layout.Children.Add(dtSlopLabel);
		layout.Children.Add(dtSlopSlider);

		// --- Zoom Settings ---
		layout.Children.Add(new Label { Text = "Zoom Settings", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });

		var zoomFactorLabel = new Label { Text = $"Double Tap Zoom: {tracker.DoubleTapZoomFactor:F1}x" };
		var zoomFactorSlider = new Slider { Minimum = 1.5, Maximum = 5.0, Value = tracker.DoubleTapZoomFactor };
		zoomFactorSlider.ValueChanged += (_, args) =>
		{
			tracker.DoubleTapZoomFactor = (float)args.NewValue;
			zoomFactorLabel.Text = $"Double Tap Zoom: {args.NewValue:F1}x";
		};
		layout.Children.Add(zoomFactorLabel);
		layout.Children.Add(zoomFactorSlider);

		var scrollZoomLabel = new Label { Text = $"Scroll Zoom Factor: {tracker.ScrollZoomFactor:F2}" };
		var scrollZoomSlider = new Slider { Minimum = 0.01, Maximum = 0.5, Value = tracker.ScrollZoomFactor };
		scrollZoomSlider.ValueChanged += (_, args) =>
		{
			tracker.ScrollZoomFactor = (float)args.NewValue;
			scrollZoomLabel.Text = $"Scroll Zoom Factor: {args.NewValue:F2}";
		};
		layout.Children.Add(scrollZoomLabel);
		layout.Children.Add(scrollZoomSlider);

		var minScaleLabel = new Label { Text = $"Min Scale: {tracker.MinScale:F1}x" };
		var minScaleSlider = new Slider { Minimum = 0.1, Maximum = 1.0, Value = tracker.MinScale };
		minScaleSlider.ValueChanged += (_, args) =>
		{
			tracker.MinScale = (float)args.NewValue;
			minScaleLabel.Text = $"Min Scale: {args.NewValue:F1}x";
		};
		layout.Children.Add(minScaleLabel);
		layout.Children.Add(minScaleSlider);

		var maxScaleLabel = new Label { Text = $"Max Scale: {tracker.MaxScale:F1}x" };
		var maxScaleSlider = new Slider { Minimum = 2.0, Maximum = 20.0, Value = tracker.MaxScale };
		maxScaleSlider.ValueChanged += (_, args) =>
		{
			tracker.MaxScale = (float)args.NewValue;
			maxScaleLabel.Text = $"Max Scale: {args.NewValue:F1}x";
		};
		layout.Children.Add(maxScaleLabel);
		layout.Children.Add(maxScaleSlider);

		// --- Current State ---
		layout.Children.Add(new Label { Text = "Current State", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });
		layout.Children.Add(new Label { Text = $"Scale: {tracker.Scale:F2}x" });
		layout.Children.Add(new Label { Text = $"Rotation: {tracker.Rotation:F1}°" });
		layout.Children.Add(new Label { Text = $"Offset: ({tracker.Offset.X:F0}, {tracker.Offset.Y:F0})" });
		layout.Children.Add(new Label { Text = $"Selected: {(_selectedSticker != null ? $"Sticker {_selectedSticker.Label}" : "None")}" });

		// Reset button
		var resetBtn = new Button { Text = "Reset View", Margin = new Thickness(0, 10, 0, 0) };
		resetBtn.Clicked += (_, _) =>
		{
			tracker.Reset();
			_selectedSticker = null;
			gestureView.Invalidate();
		};
		layout.Children.Add(resetBtn);

		page.Content = new ScrollView { Content = layout };
		await Navigation.PushAsync(page);
	}

	private void LogEvent(string message)
	{
		_eventLog.Enqueue($"[{DateTime.Now:HH:mm:ss}] {message}");
		while (_eventLog.Count > MaxLogEntries)
			_eventLog.Dequeue();

		var labels = new[] { eventLog1, eventLog2, eventLog3, eventLog4, eventLog5 };
		var events = _eventLog.ToArray();
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].Text = i < events.Length ? events[i] : "";
		}
	}

	/// <summary>
	/// Represents a draggable sticker.
	/// </summary>
	private class Sticker
	{
		public SKPoint Position { get; set; }
		public float Size { get; set; }
		public SKColor Color { get; set; }
		public string Label { get; set; } = "";
		public float Rotation { get; set; }
		public float Scale { get; set; } = 1f;
	}
}
