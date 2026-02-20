using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpDemo.Demos;

public partial class PlaygroundPage : ContentPage
{
	private int _eventCount;
	private readonly Queue<string> _logLines = new();
	private const int MaxLogLines = 200;

	public PlaygroundPage()
	{
		InitializeComponent();
	}

	private void OnCanvasPaint(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.White);

		using var paint = new SKPaint
		{
			Color = SKColors.Gray,
			TextSize = 20,
			TextAlign = SKTextAlign.Center,
			IsAntialias = true
		};
		canvas.DrawText("Move mouse / scroll wheel / touch here", e.Info.Width / 2f, e.Info.Height / 2f, paint);
	}

	private void OnCanvasTouch(object? sender, SKTouchEventArgs e)
	{
		_eventCount++;
		var line = $"#{_eventCount:D4} | {e.ActionType,-14} | Id={e.Id} | Loc=({e.Location.X:F0},{e.Location.Y:F0}) | InContact={e.InContact} | Device={e.DeviceType} | Wheel={e.WheelDelta:F2} | Pressure={e.Pressure:F2}";

		_logLines.Enqueue(line);
		while (_logLines.Count > MaxLogLines)
			_logLines.Dequeue();

		logEditor.Text = string.Join('\n', _logLines.Reverse());

		// Always mark handled so we keep receiving events
		e.Handled = true;
	}
}
