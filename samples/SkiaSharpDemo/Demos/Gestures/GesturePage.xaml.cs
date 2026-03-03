using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

/// <summary>
/// Demo page showcasing gesture features using SKGestureTracker directly with SKCanvasView.
/// This demonstrates the recommended pattern: apps use the tracker directly rather than
/// a wrapper view, giving full control over which gestures to handle.
/// </summary>
public partial class GesturePage : ContentPage
{
	// Gesture tracker - the core gesture recognition component
	private SKGestureTracker _tracker = null!;

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

		CreateTracker();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		canvasView.InvalidateSurface();
	}

	// Dispose on handler detach, recreate on handler attach.
	// This survives push/pop navigation (which doesn't change handler)
	// but properly cleans up when the page is removed from the tree.
	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();

		if (Handler != null)
		{
			// Page attached to visual tree — ensure tracker exists
			if (_tracker == null!)
				CreateTracker();
		}
		else
		{
			// Page removed from visual tree — dispose to release timers
			if (_tracker != null!)
			{
				UnsubscribeTrackerEvents();
				_tracker.Dispose();
				_tracker = null!;
			}
		}
	}

	private void CreateTracker()
	{
		_tracker = new SKGestureTracker();
		SubscribeTrackerEvents();
	}

	private void SubscribeTrackerEvents()
	{
		_tracker.TapDetected += OnTap;
		_tracker.DoubleTapDetected += OnDoubleTap;
		_tracker.LongPressDetected += OnLongPress;
		_tracker.PanDetected += OnPan;
		_tracker.PinchDetected += OnPinch;
		_tracker.RotateDetected += OnRotate;
		_tracker.FlingDetected += OnFling;
		_tracker.FlingUpdated += OnFlingUpdated;
		_tracker.FlingCompleted += OnFlingCompleted;
		_tracker.ScrollDetected += OnScroll;
		_tracker.HoverDetected += OnHover;
		_tracker.DragStarted += OnDragStarted;
		_tracker.DragUpdated += OnDragUpdated;
		_tracker.DragEnded += OnDragEnded;
		_tracker.TransformChanged += OnTransformChanged;
	}

	private void UnsubscribeTrackerEvents()
	{
		_tracker.TapDetected -= OnTap;
		_tracker.DoubleTapDetected -= OnDoubleTap;
		_tracker.LongPressDetected -= OnLongPress;
		_tracker.PanDetected -= OnPan;
		_tracker.PinchDetected -= OnPinch;
		_tracker.RotateDetected -= OnRotate;
		_tracker.FlingDetected -= OnFling;
		_tracker.FlingUpdated -= OnFlingUpdated;
		_tracker.FlingCompleted -= OnFlingCompleted;
		_tracker.ScrollDetected -= OnScroll;
		_tracker.HoverDetected -= OnHover;
		_tracker.DragStarted -= OnDragStarted;
		_tracker.DragUpdated -= OnDragUpdated;
		_tracker.DragEnded -= OnDragEnded;
		_tracker.TransformChanged -= OnTransformChanged;
	}

	/// <summary>
	/// Handle touch events from SKCanvasView and forward to the tracker.
	/// </summary>
	private void OnTouch(object? sender, SKTouchEventArgs e)
	{
		// Convert MAUI touch event to tracker input
		e.Handled = ProcessTouch(_tracker, e);

		if (e.Handled)
			canvasView.InvalidateSurface();
	}

	/// <summary>
	/// Processes a MAUI SKTouchEventArgs through the gesture tracker.
	/// SKTouchEventArgs.Location is already in device-pixel coordinates (same as the canvas),
	/// so no coordinate conversion is needed.
	/// </summary>
	private static bool ProcessTouch(SKGestureTracker tracker, SKTouchEventArgs e)
	{
		var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;
		var location = e.Location;

		return e.ActionType switch
		{
			SKTouchAction.Pressed => tracker.ProcessTouchDown(e.Id, location, isMouse),
			SKTouchAction.Moved => tracker.ProcessTouchMove(e.Id, location, e.InContact),
			SKTouchAction.Released => tracker.ProcessTouchUp(e.Id, location, isMouse),
			SKTouchAction.Cancelled => tracker.ProcessTouchCancel(e.Id),
			SKTouchAction.WheelChanged => tracker.ProcessMouseWheel(location, 0, e.WheelDelta),
			_ => true, // Entered/Exited — accept to keep receiving events
		};
	}

	/// <summary>
	/// Invalidate canvas when transform changes (pan, zoom, rotate, fling).
	/// </summary>
	private void OnTransformChanged(object? sender, EventArgs e)
	{
		canvasView.InvalidateSurface();
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
		canvas.Concat(_tracker.Matrix);

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
		var scale = _tracker.Scale;
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
		using var textFont = new SKFont
		{
			Size = sticker.Size * 0.4f,
			Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
		};
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true
		};
		canvas.DrawText(sticker.Label, sticker.Position.X, sticker.Position.Y + textFont.Size * 0.35f, SKTextAlign.Center, textFont, textPaint);
	}

	private void OnTap(object? sender, SKTapGestureEventArgs e)
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
		
		canvasView.InvalidateSurface();
	}

	private void OnDoubleTap(object? sender, SKTapGestureEventArgs e)
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
			canvasView.InvalidateSurface();
		}
	}

	private void OnLongPress(object? sender, SKLongPressGestureEventArgs e)
	{
		if (!_enableLongPress) return;
		LogEvent($"Long press at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			statusLabel.Text = $"Long press selected: Sticker {hitSticker.Label}";
		}
		
		canvasView.InvalidateSurface();
	}

	private void OnPan(object? sender, SKPanGestureEventArgs e)
	{
		// Transform is handled by the tracker
		statusLabel.Text = $"Pan: Δ({e.Delta.X:F1}, {e.Delta.Y:F1})";
	}

	private void OnPinch(object? sender, SKPinchGestureEventArgs e)
	{
		LogEvent($"Pinch scale: {e.ScaleDelta:F2}");
		statusLabel.Text = $"Scale: {_tracker.Scale:F2}";
	}

	private void OnRotate(object? sender, SKRotateGestureEventArgs e)
	{
		LogEvent($"Rotate: {e.RotationDelta:F1}°");
		statusLabel.Text = $"Rotation: {_tracker.Rotation:F1}°";
	}

	private void OnFling(object? sender, SKFlingGestureEventArgs e)
	{
		LogEvent($"Fling: ({e.Velocity.X:F0}, {e.Velocity.Y:F0}) px/s");
		statusLabel.Text = $"Flinging at {e.Speed:F0} px/s";
	}

	private void OnFlingUpdated(object? sender, SKFlingGestureEventArgs e)
	{
		// Fling transform is handled by the tracker
		statusLabel.Text = $"Flinging... ({e.Speed:F0} px/s)";
	}

	private void OnFlingCompleted(object? sender, EventArgs e)
	{
		statusLabel.Text = "Fling ended";
	}

	private void OnScroll(object? sender, SKScrollGestureEventArgs e)
	{
		// Scroll zoom is handled by the tracker
		statusLabel.Text = $"Scroll zoom: {_tracker.Scale:F2}x";
	}

	private void OnHover(object? sender, SKHoverGestureEventArgs e)
	{
		statusLabel.Text = $"Hover: ({e.Location.X:F0}, {e.Location.Y:F0})";
	}

	private void OnDragStarted(object? sender, SKDragGestureEventArgs e)
	{
		if (!_enableDrag) return;
		LogEvent($"Drag started at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		if (_selectedSticker != null)
		{
			statusLabel.Text = $"Dragging Sticker {_selectedSticker.Label}";
			e.Handled = true; // suppress canvas pan
		}
	}

	private void OnDragUpdated(object? sender, SKDragGestureEventArgs e)
	{
		if (!_enableDrag) return;
		// Move selected sticker in content-space
		if (_selectedSticker != null)
		{
			// Convert screen-space delta to content-space via inverse matrix
			var matrix = _tracker.Matrix;
			if (matrix.TryInvert(out var inverse))
			{
				var contentDelta = inverse.MapVector(e.Delta.X, e.Delta.Y);
				_selectedSticker.Position = new SKPoint(
					_selectedSticker.Position.X + contentDelta.X,
					_selectedSticker.Position.Y + contentDelta.Y);
			}

			e.Handled = true; // suppress canvas pan
			canvasView.InvalidateSurface();
		}
	}

	private void OnDragEnded(object? sender, SKDragGestureEventArgs e)
	{
		if (!_enableDrag) return;
		LogEvent($"Drag ended at ({e.Location.X:F0}, {e.Location.Y:F0})");
		statusLabel.Text = "Drag completed";
	}

	private Sticker? HitTest(SKPoint location)
	{
		if (_canvasWidth <= 0 || _canvasHeight <= 0)
			return null;

		var matrix = _tracker.Matrix;
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

		var layout = new VerticalStackLayout { Padding = 20, Spacing = 12 };

		// --- Feature Toggles (Tracker-level) ---
		layout.Children.Add(new Label { Text = "Tracker Feature Toggles", FontAttributes = FontAttributes.Bold, FontSize = 16 });

		var trackerToggles = new (string Label, bool Value, Action<bool> Setter)[]
		{
			("Tap", _tracker.IsTapEnabled, v => _tracker.IsTapEnabled = v),
			("Double Tap", _tracker.IsDoubleTapEnabled, v => _tracker.IsDoubleTapEnabled = v),
			("Long Press", _tracker.IsLongPressEnabled, v => _tracker.IsLongPressEnabled = v),
			("Pan", _tracker.IsPanEnabled, v => _tracker.IsPanEnabled = v),
			("Pinch (Zoom)", _tracker.IsPinchEnabled, v => _tracker.IsPinchEnabled = v),
			("Rotate", _tracker.IsRotateEnabled, v => _tracker.IsRotateEnabled = v),
			("Fling", _tracker.IsFlingEnabled, v => _tracker.IsFlingEnabled = v),
			("Double Tap Zoom", _tracker.IsDoubleTapZoomEnabled, v => _tracker.IsDoubleTapZoomEnabled = v),
			("Scroll Zoom", _tracker.IsScrollZoomEnabled, v => _tracker.IsScrollZoomEnabled = v),
			("Hover", _tracker.IsHoverEnabled, v => _tracker.IsHoverEnabled = v),
		};

		foreach (var (label, value, setter) in trackerToggles)
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

		// --- App-level Toggles ---
		layout.Children.Add(new Label { Text = "App Toggles", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });

		var appToggles = new (string Label, bool Value, Action<bool> Setter)[]
		{
			("Tap Selection (App)", _enableTap, v => _enableTap = v),
			("Double Tap Log (App)", _enableDoubleTap, v => _enableDoubleTap = v),
			("Long Press Info (App)", _enableLongPress, v => _enableLongPress = v),
			("Drag Sticker (App)", _enableDrag, v => _enableDrag = v),
		};

		foreach (var (label, value, setter) in appToggles)
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

		AddSlider(layout, "Touch Slop", "px", 1, 50, _tracker.Options.TouchSlop, v => _tracker.Options.TouchSlop = v);
		AddSlider(layout, "Double Tap Slop", "px", 10, 200, _tracker.Options.DoubleTapSlop, v => _tracker.Options.DoubleTapSlop = v);
		AddSlider(layout, "Fling Threshold", "px/s", 50, 1000, _tracker.Options.FlingThreshold, v => _tracker.Options.FlingThreshold = v);
		AddSliderInt(layout, "Long Press Duration", "ms", 100, 2000, _tracker.Options.LongPressDuration, v => _tracker.Options.LongPressDuration = v);

		// --- Fling Settings ---
		layout.Children.Add(new Label { Text = "Fling Settings", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });

		AddSlider(layout, "Friction", "", 0.01f, 0.5f, _tracker.Options.FlingFriction, v => _tracker.Options.FlingFriction = v, "F3");
		AddSlider(layout, "Min Velocity", "px/s", 1, 50, _tracker.Options.FlingMinVelocity, v => _tracker.Options.FlingMinVelocity = v);
		AddSliderInt(layout, "Frame Interval", "ms", 8, 50, _tracker.Options.FlingFrameInterval, v => _tracker.Options.FlingFrameInterval = v);

		// --- Zoom Settings ---
		layout.Children.Add(new Label { Text = "Zoom Settings", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });

		AddSlider(layout, "Double Tap Zoom", "x", 1.5f, 5, _tracker.Options.DoubleTapZoomFactor, v => _tracker.Options.DoubleTapZoomFactor = v, "F1");
		AddSliderInt(layout, "Zoom Animation", "ms", 50, 1000, _tracker.Options.ZoomAnimationDuration, v => _tracker.Options.ZoomAnimationDuration = v);
		AddSlider(layout, "Scroll Zoom Factor", "", 0.01f, 0.5f, _tracker.Options.ScrollZoomFactor, v => _tracker.Options.ScrollZoomFactor = v, "F3");
		AddSlider(layout, "Min Scale", "x", 0.1f, 1, _tracker.Options.MinScale, v => _tracker.Options.MinScale = v, "F2");
		AddSlider(layout, "Max Scale", "x", 2, 20, _tracker.Options.MaxScale, v => _tracker.Options.MaxScale = v, "F1");

		// --- Current State ---
		layout.Children.Add(new Label { Text = "Current State", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });
		layout.Children.Add(new Label { Text = $"Scale: {_tracker.Scale:F2}x" });
		layout.Children.Add(new Label { Text = $"Rotation: {_tracker.Rotation:F1}°" });
		layout.Children.Add(new Label { Text = $"Offset: ({_tracker.Offset.X:F0}, {_tracker.Offset.Y:F0})" });
		layout.Children.Add(new Label { Text = $"Selected: {(_selectedSticker != null ? $"Sticker {_selectedSticker.Label}" : "None")}" });

		// Reset button
		var resetBtn = new Button { Text = "Reset View", Margin = new Thickness(0, 10, 0, 0) };
		resetBtn.Clicked += (_, _) =>
		{
			_tracker.Reset();
			_selectedSticker = null;
			canvasView.InvalidateSurface();
		};
		layout.Children.Add(resetBtn);

		page.Content = new ScrollView { Content = layout };
		await Navigation.PushAsync(page);
	}

	private static void AddSlider(VerticalStackLayout layout, string name, string unit, float min, float max, float value, Action<float> setter, string format = "F0")
	{
		var suffix = string.IsNullOrEmpty(unit) ? "" : $" {unit}";
		var label = new Label { Text = $"{name}: {value.ToString(format)}{suffix}" };
		var slider = new Slider { Minimum = min, Maximum = max, Value = value };
		slider.ValueChanged += (_, args) =>
		{
			setter((float)args.NewValue);
			label.Text = $"{name}: {((float)args.NewValue).ToString(format)}{suffix}";
		};
		layout.Children.Add(label);
		layout.Children.Add(slider);
	}

	private static void AddSliderInt(VerticalStackLayout layout, string name, string unit, int min, int max, int value, Action<int> setter)
	{
		var label = new Label { Text = $"{name}: {value} {unit}" };
		var slider = new Slider { Minimum = min, Maximum = max, Value = value };
		slider.ValueChanged += (_, args) =>
		{
			setter((int)args.NewValue);
			label.Text = $"{name}: {(int)args.NewValue} {unit}";
		};
		layout.Children.Add(label);
		layout.Children.Add(slider);
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
