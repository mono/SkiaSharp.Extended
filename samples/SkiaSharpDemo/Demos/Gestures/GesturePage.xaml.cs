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

	// Feature toggles
	private bool _enablePan = true;
	private bool _enablePinch = true;
	private bool _enableRotate = true;
	private bool _enableFling = true;
	private bool _enableTap = true;
	private bool _enableDoubleTap = true;
	private bool _enableLongPress = true;
	private bool _enableDrag = true;

	// Transform state
	private float _canvasScale = 1f;
	private float _canvasRotation;
	private SKPoint _canvasOffset = SKPoint.Empty;
	private int _canvasWidth;
	private int _canvasHeight;

	// Fling animation state
	private IDispatcherTimer? _flingTimer;
	private float _flingVelocityX;
	private float _flingVelocityY;
	private const float FlingFriction = 0.92f; // Deceleration factor per frame (higher = smoother/slower stop)
	private const float FlingMinVelocity = 5f; // Minimum velocity to continue animation
	private const int FlingFrameMs = 16; // ~60 FPS

	public GesturePage()
	{
		InitializeComponent();

		// Initialize with some stickers
		_stickers.Add(new Sticker { Position = new SKPoint(100, 100), Size = 80, Color = SKColors.Red, Label = "1" });
		_stickers.Add(new Sticker { Position = new SKPoint(200, 200), Size = 60, Color = SKColors.Green, Label = "2" });
		_stickers.Add(new Sticker { Position = new SKPoint(300, 150), Size = 70, Color = SKColors.Blue, Label = "3" });

		// Create fling animation timer
		_flingTimer = Dispatcher.CreateTimer();
		_flingTimer.Interval = TimeSpan.FromMilliseconds(FlingFrameMs);
		_flingTimer.Tick += OnFlingTimerTick;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		gestureView.Invalidate();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		StopFlingAnimation();
	}

	private void OnFlingTimerTick(object? sender, EventArgs e)
	{
		// Convert screen-space fling velocity to content-space offset delta
		var d = ScreenToContentDelta(
			_flingVelocityX * (FlingFrameMs / 1000f),
			_flingVelocityY * (FlingFrameMs / 1000f));
		_canvasOffset = new SKPoint(_canvasOffset.X + d.X, _canvasOffset.Y + d.Y);

		// Apply friction (deceleration)
		_flingVelocityX *= FlingFriction;
		_flingVelocityY *= FlingFriction;

		// Update display
		gestureView.Invalidate();

		// Calculate current speed
		var speed = MathF.Sqrt(_flingVelocityX * _flingVelocityX + _flingVelocityY * _flingVelocityY);

		// Stop when velocity is too low
		if (speed < FlingMinVelocity)
		{
			StopFlingAnimation();
			statusLabel.Text = "Fling ended";
		}
		else
		{
			statusLabel.Text = $"Flinging... ({speed:F0} px/s)";
		}
	}

	private void StartFlingAnimation(float velocityX, float velocityY)
	{
		_flingVelocityX = velocityX;
		_flingVelocityY = velocityY;
		_flingTimer?.Start();
	}

	private void StopFlingAnimation()
	{
		_flingTimer?.Stop();
		_flingVelocityX = 0;
		_flingVelocityY = 0;
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

		// Apply canvas transforms: rotate/scale around view center, then pan
		canvas.Save();
		canvas.Translate(width / 2f, height / 2f);
		canvas.Scale(_canvasScale);
		canvas.RotateDegrees(_canvasRotation);
		canvas.Translate(_canvasOffset.X, _canvasOffset.Y);
		canvas.Translate(-width / 2f, -height / 2f);

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
		var extra = (int)(Math.Max(width, height) / _canvasScale) + gridSize * 4;
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
		StopFlingAnimation(); // Stop any ongoing fling
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
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Double tap ({e.TapCount}x) at ({e.Location.X:F0}, {e.Location.Y:F0})");

		const float zoomIn = 2f;
		const float maxScale = 3f;

		if (_canvasScale >= maxScale - 0.01f)
		{
			// Already at max zoom — reset to 1x centered
			_canvasScale = 1f;
			_canvasRotation = 0f;
			_canvasOffset = SKPoint.Empty;
			statusLabel.Text = "View reset";
		}
		else
		{
			var newScale = Math.Min(_canvasScale * zoomIn, maxScale);
			AdjustOffsetForPivot(e.Location, _canvasScale, newScale, _canvasRotation, _canvasRotation);
			_canvasScale = newScale;
			statusLabel.Text = $"Zoom: {_canvasScale:F2}x";
		}
		
		gestureView.Invalidate();
	}

	private void OnLongPress(object? sender, SKTapEventArgs e)
	{
		if (!_enableLongPress) return;
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Long press at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			statusLabel.Text = $"Long press selected: Sticker {hitSticker.Label}";
		}
		
		gestureView.Invalidate();
	}

	/// <summary>
	/// Converts a screen-space delta to content-space offset delta,
	/// accounting for current rotation and scale.
	/// </summary>
	private SKPoint ScreenToContentDelta(float dx, float dy)
	{
		var inv = SKMatrix.CreateRotationDegrees(-_canvasRotation);
		var mapped = inv.MapVector(dx, dy);
		return new SKPoint(mapped.X / _canvasScale, mapped.Y / _canvasScale);
	}

	private void OnPan(object? sender, SKPanEventArgs e)
	{
		if (!_enablePan) return;
		StopFlingAnimation(); // Stop any ongoing fling when starting a new pan
		
		// Only pan canvas when no sticker is selected
		if (_selectedSticker == null)
		{
			var d = ScreenToContentDelta(e.Delta.X, e.Delta.Y);
			_canvasOffset = new SKPoint(_canvasOffset.X + d.X, _canvasOffset.Y + d.Y);
			statusLabel.Text = $"Pan: Δ({e.Delta.X:F1}, {e.Delta.Y:F1})";
		}
		
		gestureView.Invalidate();
	}

	private void OnPinch(object? sender, SKPinchEventArgs e)
	{
		if (!_enablePinch) return;
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Pinch scale: {e.Scale:F2}");
		
		// Apply center movement as pan so content follows the fingers
		if (_enablePan)
		{
			var panDelta = ScreenToContentDelta(
				e.Center.X - e.PreviousCenter.X,
				e.Center.Y - e.PreviousCenter.Y);
			_canvasOffset = new SKPoint(_canvasOffset.X + panDelta.X, _canvasOffset.Y + panDelta.Y);
		}

		var newScale = Math.Clamp(_canvasScale * e.Scale, 0.5f, 3f);
		AdjustOffsetForPivot(e.Center, _canvasScale, newScale, _canvasRotation, _canvasRotation);
		_canvasScale = newScale;
		statusLabel.Text = $"Scale: {_canvasScale:F2}";
		
		gestureView.Invalidate();
	}

	private void OnRotate(object? sender, SKRotateEventArgs e)
	{
		if (!_enableRotate) return;
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Rotate: {e.RotationDelta:F1}°");
		
		// Center pan is handled in OnPinch (both fire on the same touch move)
		var newRotation = _canvasRotation + e.RotationDelta;
		AdjustOffsetForPivot(e.Center, _canvasScale, _canvasScale, _canvasRotation, newRotation);
		_canvasRotation = newRotation;
		statusLabel.Text = $"Rotation: {_canvasRotation:F1}°";
		
		gestureView.Invalidate();
	}

	private void OnFling(object? sender, SKFlingEventArgs e)
	{
		if (!_enableFling) return;
		LogEvent($"Fling: ({e.VelocityX:F0}, {e.VelocityY:F0}) px/s");
		
		// Start smooth fling animation
		StartFlingAnimation(e.VelocityX, e.VelocityY);
		statusLabel.Text = $"Flinging at {MathF.Sqrt(e.VelocityX * e.VelocityX + e.VelocityY * e.VelocityY):F0} px/s";
	}

	private void OnDragStarted(object? sender, SKDragEventArgs e)
	{
		if (!_enableDrag) return;
		StopFlingAnimation(); // Stop any ongoing fling
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

	/// <summary>
	/// Adjusts _canvasOffset so that the content under screenPivot stays fixed
	/// when scale or rotation changes.
	/// </summary>
	private void AdjustOffsetForPivot(SKPoint screenPivot, float oldScale, float newScale, float oldRotDeg, float newRotDeg)
	{
		var w2 = (float)_canvasWidth / 2f;
		var h2 = (float)_canvasHeight / 2f;
		var d = new SKPoint(screenPivot.X - w2, screenPivot.Y - h2);

		// Q = Rot(-θ) · d / scale  (content-space vector from center to pivot)
		var rotOld = SKMatrix.CreateRotationDegrees(-oldRotDeg);
		var qOld = rotOld.MapVector(d.X, d.Y);
		qOld = new SKPoint(qOld.X / oldScale, qOld.Y / oldScale);

		var rotNew = SKMatrix.CreateRotationDegrees(-newRotDeg);
		var qNew = rotNew.MapVector(d.X, d.Y);
		qNew = new SKPoint(qNew.X / newScale, qNew.Y / newScale);

		_canvasOffset = new SKPoint(
			_canvasOffset.X + qNew.X - qOld.X,
			_canvasOffset.Y + qNew.Y - qOld.Y);
	}

	private SKMatrix BuildCanvasTransform()
	{
		var w = (float)_canvasWidth;
		var h = (float)_canvasHeight;

		// Must match the canvas call order in OnPaintSurface.
		// Canvas calls are pre-concat, so the effective matrix is
		// (last call) · ... · (first call), i.e.:
		// T(-w/2,-h/2) · T(ox,oy) · R(θ) · S(s) · T(w/2,h/2)
		var matrix = SKMatrix.CreateTranslation(-w / 2f, -h / 2f);
		matrix = matrix.PreConcat(SKMatrix.CreateTranslation(_canvasOffset.X, _canvasOffset.Y));
		matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees(_canvasRotation));
		matrix = matrix.PreConcat(SKMatrix.CreateScale(_canvasScale, _canvasScale));
		matrix = matrix.PreConcat(SKMatrix.CreateTranslation(w / 2f, h / 2f));
		return matrix;
	}

	private Sticker? HitTest(SKPoint location)
	{
		if (_canvasWidth <= 0 || _canvasHeight <= 0)
			return null;

		var matrix = BuildCanvasTransform();
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

		var touchSlop = gestureView.TouchSlop;
		var longPressDuration = gestureView.LongPressDuration;

		var layout = new VerticalStackLayout { Padding = 20, Spacing = 12 };

		// --- Gesture Toggles ---
		layout.Children.Add(new Label { Text = "Gesture Toggles", FontAttributes = FontAttributes.Bold, FontSize = 16 });

		var toggles = new (string Label, bool Value, Action<bool> Setter)[]
		{
			("Tap", _enableTap, v => _enableTap = v),
			("Double Tap", _enableDoubleTap, v => _enableDoubleTap = v),
			("Long Press", _enableLongPress, v => _enableLongPress = v),
			("Pan", _enablePan, v => _enablePan = v),
			("Pinch (Zoom)", _enablePinch, v => _enablePinch = v),
			("Rotate", _enableRotate, v => _enableRotate = v),
			("Fling", _enableFling, v => _enableFling = v),
			("Drag (Sticker)", _enableDrag, v => _enableDrag = v),
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

		// --- Thresholds ---
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

		// --- Current State ---
		layout.Children.Add(new Label { Text = "Current State", FontAttributes = FontAttributes.Bold, FontSize = 16, Margin = new Thickness(0, 10, 0, 0) });
		layout.Children.Add(new Label { Text = $"Scale: {_canvasScale:F2}x" });
		layout.Children.Add(new Label { Text = $"Rotation: {_canvasRotation:F1}°" });
		layout.Children.Add(new Label { Text = $"Offset: ({_canvasOffset.X:F0}, {_canvasOffset.Y:F0})" });
		layout.Children.Add(new Label { Text = $"Selected: {(_selectedSticker != null ? $"Sticker {_selectedSticker.Label}" : "None")}" });

		// Reset button
		var resetBtn = new Button { Text = "Reset View", Margin = new Thickness(0, 10, 0, 0) };
		resetBtn.Clicked += (_, _) =>
		{
			_canvasScale = 1f;
			_canvasRotation = 0f;
			_canvasOffset = SKPoint.Empty;
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
