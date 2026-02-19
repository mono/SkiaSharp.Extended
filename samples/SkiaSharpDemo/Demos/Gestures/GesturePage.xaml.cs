using SkiaSharp;
using SkiaSharp.Extended.Gestures;
using SkiaSharp.Extended.UI.Controls;

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

	private void OnModeChanged(object? sender, CheckedChangedEventArgs e)
	{
		if (!e.Value) return;

		if (modeImmediate.IsChecked)
			gestureView.Engine.SelectionMode = SKGestureSelectionMode.Immediate;
		else if (modeTapToSelect.IsChecked)
			gestureView.Engine.SelectionMode = SKGestureSelectionMode.TapToSelect;
		else if (modeLongPress.IsChecked)
			gestureView.Engine.SelectionMode = SKGestureSelectionMode.LongPressToSelect;

		LogEvent($"Mode: {gestureView.Engine.SelectionMode}");
	}

	private void OnTap(object? sender, SKTapEventArgs e)
	{
		LogEvent($"Tap at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		// Try to select a sticker
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			gestureView.SelectedItemId = _stickers.IndexOf(hitSticker);
			statusLabel.Text = $"Selected: Sticker {hitSticker.Label}";
		}
		else
		{
			_selectedSticker = null;
			gestureView.SelectedItemId = null;
			statusLabel.Text = "No selection";
		}
		
		gestureView.Invalidate();
	}

	private void OnDoubleTap(object? sender, SKTapEventArgs e)
	{
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
		LogEvent($"Long press at ({e.Location.X:F0}, {e.Location.Y:F0})");
		
		var hitSticker = HitTest(e.Location);
		if (hitSticker != null)
		{
			_selectedSticker = hitSticker;
			gestureView.SelectedItemId = _stickers.IndexOf(hitSticker);
			statusLabel.Text = $"Long press selected: Sticker {hitSticker.Label}";
		}
		
		gestureView.Invalidate();
	}

	private void OnPan(object? sender, SKPanEventArgs e)
	{
		// Pan the canvas
		_canvasOffset = new SKPoint(_canvasOffset.X + e.Delta.X, _canvasOffset.Y + e.Delta.Y);
		statusLabel.Text = $"Pan: Δ({e.Delta.X:F1}, {e.Delta.Y:F1})";
		
		gestureView.Invalidate();
	}

	private void OnPinch(object? sender, SKPinchEventArgs e)
	{
		LogEvent($"Pinch scale: {e.Scale:F2}");
		
		// Scale the canvas
		_canvasScale *= e.Scale;
		_canvasScale = Math.Clamp(_canvasScale, 0.5f, 3f);
		statusLabel.Text = $"Scale: {_canvasScale:F2}";
		
		gestureView.Invalidate();
	}

	private void OnRotate(object? sender, SKRotateEventArgs e)
	{
		LogEvent($"Rotate: {e.RotationDelta:F1}°");
		
		// Rotate the canvas
		_canvasRotation += e.RotationDelta;
		statusLabel.Text = $"Rotation: {_canvasRotation:F1}°";
		
		gestureView.Invalidate();
	}

	private void OnFling(object? sender, SKFlingEventArgs e)
	{
		LogEvent($"Fling: ({e.VelocityX:F0}, {e.VelocityY:F0}) px/s");
		statusLabel.Text = $"Fling detected!";
	}

	private void OnDragStarted(object? sender, SKDragEventArgs e)
	{
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
