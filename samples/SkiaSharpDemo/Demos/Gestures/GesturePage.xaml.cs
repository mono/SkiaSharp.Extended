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

	// Transform state
	private float _canvasScale = 1f;
	private float _canvasRotation;
	private SKPoint _canvasOffset = SKPoint.Empty;

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
		// Apply velocity to canvas offset
		_canvasOffset = new SKPoint(
			_canvasOffset.X + _flingVelocityX * (FlingFrameMs / 1000f),
			_canvasOffset.Y + _flingVelocityY * (FlingFrameMs / 1000f));

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

		// Clear background
		canvas.Clear(SKColors.White);

		// Draw grid background so scrolling is visible
		DrawGrid(canvas, width, height);

		// Apply canvas transforms (pan, scale, rotate)
		canvas.Save();
		canvas.Translate(width / 2f + _canvasOffset.X, height / 2f + _canvasOffset.Y);
		canvas.Scale(_canvasScale);
		canvas.RotateDegrees(_canvasRotation);
		canvas.Translate(-width / 2f, -height / 2f);

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

		// Calculate grid offset based on canvas pan
		var offsetX = (int)_canvasOffset.X % (gridSize * 2);
		var offsetY = (int)_canvasOffset.Y % (gridSize * 2);

		for (int y = -gridSize * 2; y < height + gridSize * 2; y += gridSize)
		{
			for (int x = -gridSize * 2; x < width + gridSize * 2; x += gridSize)
			{
				var isLight = ((x / gridSize) + (y / gridSize)) % 2 == 0;
				var rect = new SKRect(x + offsetX, y + offsetY, x + offsetX + gridSize, y + offsetY + gridSize);
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
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Double tap ({e.TapCount}x) at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		// Reset view transform on double tap
		_canvasScale = 1f;
		_canvasRotation = 0f;
		_canvasOffset = SKPoint.Empty;
		statusLabel.Text = "View reset";
		
		gestureView.Invalidate();
	}

	private void OnLongPress(object? sender, SKTapEventArgs e)
	{
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

	private void OnPan(object? sender, SKPanEventArgs e)
	{
		StopFlingAnimation(); // Stop any ongoing fling when starting a new pan
		
		// Pan the canvas
		_canvasOffset = new SKPoint(_canvasOffset.X + e.Delta.X, _canvasOffset.Y + e.Delta.Y);
		statusLabel.Text = $"Pan: Δ({e.Delta.X:F1}, {e.Delta.Y:F1})";
		
		gestureView.Invalidate();
	}

	private void OnPinch(object? sender, SKPinchEventArgs e)
	{
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Pinch scale: {e.Scale:F2}");
		
		// Scale the canvas
		_canvasScale *= e.Scale;
		_canvasScale = Math.Clamp(_canvasScale, 0.5f, 3f);
		statusLabel.Text = $"Scale: {_canvasScale:F2}";
		
		gestureView.Invalidate();
	}

	private void OnRotate(object? sender, SKRotateEventArgs e)
	{
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Rotate: {e.RotationDelta:F1}°");
		
		// Rotate the canvas
		_canvasRotation += e.RotationDelta;
		statusLabel.Text = $"Rotation: {_canvasRotation:F1}°";
		
		gestureView.Invalidate();
	}

	private void OnFling(object? sender, SKFlingEventArgs e)
	{
		LogEvent($"Fling: ({e.VelocityX:F0}, {e.VelocityY:F0}) px/s");
		
		// Start smooth fling animation
		StartFlingAnimation(e.VelocityX, e.VelocityY);
		statusLabel.Text = $"Flinging at {MathF.Sqrt(e.VelocityX * e.VelocityX + e.VelocityY * e.VelocityY):F0} px/s";
	}

	private void OnDragStarted(object? sender, SKDragEventArgs e)
	{
		StopFlingAnimation(); // Stop any ongoing fling
		LogEvent($"Drag started at ({e.StartLocation.X:F0}, {e.StartLocation.Y:F0})");
		
		if (_selectedSticker != null)
		{
			statusLabel.Text = $"Dragging Sticker {_selectedSticker.Label}";
		}
	}

	private void OnDragUpdated(object? sender, SKDragEventArgs e)
	{
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
		LogEvent($"Drag ended at ({e.CurrentLocation.X:F0}, {e.CurrentLocation.Y:F0})");
		statusLabel.Text = "Drag completed";
	}

	private Sticker? HitTest(SKPoint location)
	{
		// Transform location by inverse of canvas transform
		var transformed = location;
		
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
