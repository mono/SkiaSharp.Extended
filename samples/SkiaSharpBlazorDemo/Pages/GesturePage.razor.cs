using SkiaSharp.Views.Blazor;

namespace SkiaSharpBlazorDemo.Pages;

public partial class GesturePage : IDisposable
{
    // Sticker data for demonstration
    private readonly List<Sticker> _stickers = new();
    private Sticker? _selectedSticker;
    private readonly Queue<string> _eventLog = new();
    private const int MaxLogEntries = 5;

    private SKGestureSurfaceView? _gestureView;

    // Feature toggles
    private bool _enablePan = true;
    private bool _enablePinch = true;
    private bool _enableRotate = true;
    private bool _enableFling = true;
    private bool _enableTap = true;
    private bool _enableDoubleTap = true;
    private bool _enableLongPress = true;
    private bool _enableScrollZoom = true;

    // Transform state
    private float _canvasScale = 1f;
    private float _canvasRotation;
    private SKPoint _canvasOffset = SKPoint.Empty;
    private int _canvasWidth;
    private int _canvasHeight;

    private string _statusText = "Ready";

    protected override void OnInitialized()
    {
        _stickers.Add(new Sticker { Position = new SKPoint(100, 100), Size = 80, Color = SKColors.Red, Label = "1" });
        _stickers.Add(new Sticker { Position = new SKPoint(200, 200), Size = 60, Color = SKColors.Green, Label = "2" });
        _stickers.Add(new Sticker { Position = new SKPoint(300, 150), Size = 70, Color = SKColors.Blue, Label = "3" });
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var width = e.Info.Width;
        var height = e.Info.Height;

        _canvasWidth = width;
        _canvasHeight = height;

        canvas.Clear(SKColors.White);

        canvas.Save();
        canvas.Translate(width / 2f, height / 2f);
        canvas.Scale(_canvasScale);
        canvas.RotateDegrees(_canvasRotation);
        canvas.Translate(_canvasOffset.X, _canvasOffset.Y);
        canvas.Translate(-width / 2f, -height / 2f);

        DrawGrid(canvas, width, height);

        foreach (var sticker in _stickers)
            DrawSticker(canvas, sticker, sticker == _selectedSticker);

        canvas.Restore();

        // Draw crosshair at center
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

        var extra = (int)(Math.Max(width, height) / _canvasScale) + gridSize * 4;
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

        using var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(50),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };
        canvas.DrawCircle(sticker.Position.X + 3, sticker.Position.Y + 3, radius, shadowPaint);

        using var fillPaint = new SKPaint
        {
            Color = sticker.Color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(sticker.Position, radius, fillPaint);

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

        using var borderPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawCircle(sticker.Position, radius, borderPaint);

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

    private void OnTap(SKTapEventArgs e)
    {
        if (!_enableTap) return;
        LogEvent($"Tap at ({e.Location.X:F0}, {e.Location.Y:F0})");

        var hit = HitTest(e.Location);
        _selectedSticker = hit;
        _statusText = hit != null ? $"Selected: Sticker {hit.Label}" : "No selection";
        _gestureView?.Invalidate();
        StateHasChanged();
    }

    private void OnDoubleTap(SKTapEventArgs e)
    {
        if (!_enableDoubleTap) return;
        LogEvent($"Double tap at ({e.Location.X:F0}, {e.Location.Y:F0})");

        const float zoomIn = 2f;
        const float maxScale = 3f;
        if (_canvasScale >= maxScale - 0.01f)
        {
            _canvasScale = 1f;
            _canvasRotation = 0f;
            _canvasOffset = SKPoint.Empty;
            _statusText = "View reset";
        }
        else
        {
            var newScale = Math.Min(_canvasScale * zoomIn, maxScale);
            AdjustOffsetForPivot(e.Location, _canvasScale, newScale, _canvasRotation, _canvasRotation);
            _canvasScale = newScale;
            _statusText = $"Zoom: {_canvasScale:F2}x";
        }
        _gestureView?.Invalidate();
        StateHasChanged();
    }

    private void OnLongPress(SKTapEventArgs e)
    {
        if (!_enableLongPress) return;
        LogEvent($"Long press at ({e.Location.X:F0}, {e.Location.Y:F0})");
        _selectedSticker = HitTest(e.Location);
        _gestureView?.Invalidate();
        StateHasChanged();
    }

    private SKPoint ScreenToContentDelta(float dx, float dy)
    {
        var inv = SKMatrix.CreateRotationDegrees(-_canvasRotation);
        var mapped = inv.MapVector(dx, dy);
        return new SKPoint(mapped.X / _canvasScale, mapped.Y / _canvasScale);
    }

    private void OnPan(SKPanEventArgs e)
    {
        if (!_enablePan) return;
        if (_selectedSticker == null)
        {
            var d = ScreenToContentDelta(e.Delta.X, e.Delta.Y);
            _canvasOffset = new SKPoint(_canvasOffset.X + d.X, _canvasOffset.Y + d.Y);
            _statusText = $"Pan: Δ({e.Delta.X:F1}, {e.Delta.Y:F1})";
        }
        _gestureView?.Invalidate();
    }

    private void OnPinch(SKPinchEventArgs e)
    {
        if (!_enablePinch) return;
        LogEvent($"Pinch scale: {e.Scale:F2}");

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
        _statusText = $"Scale: {_canvasScale:F2}";
        _gestureView?.Invalidate();
    }

    private void OnRotate(SKRotateEventArgs e)
    {
        if (!_enableRotate) return;
        LogEvent($"Rotate: {e.RotationDelta:F1}°");

        var newRotation = _canvasRotation + e.RotationDelta;
        AdjustOffsetForPivot(e.Center, _canvasScale, _canvasScale, _canvasRotation, newRotation);
        _canvasRotation = newRotation;
        _statusText = $"Rotation: {_canvasRotation:F1}°";
        _gestureView?.Invalidate();
    }

    private void OnFling(SKFlingEventArgs e)
    {
        if (!_enableFling) return;
        LogEvent($"Fling: ({e.VelocityX:F0}, {e.VelocityY:F0}) px/s");
        _statusText = $"Flinging at {e.Speed:F0} px/s";
        StateHasChanged();
    }

    private void OnFlinging(SKFlingEventArgs e)
    {
        if (!_enableFling) return;
        var d = ScreenToContentDelta(e.DeltaX, e.DeltaY);
        _canvasOffset = new SKPoint(_canvasOffset.X + d.X, _canvasOffset.Y + d.Y);
        _gestureView?.Invalidate();
        _statusText = $"Flinging... ({e.Speed:F0} px/s)";
    }

    private void OnFlingCompleted()
    {
        _statusText = "Fling ended";
        StateHasChanged();
    }

    private void OnScroll(SKScrollEventArgs e)
    {
        if (!_enableScrollZoom) return;
        const float zoomFactor = 0.1f;
        var scaleDelta = 1f + e.DeltaY * zoomFactor;
        var newScale = Math.Clamp(_canvasScale * scaleDelta, 0.1f, 10f);
        AdjustOffsetForPivot(e.Location, _canvasScale, newScale, _canvasRotation, _canvasRotation);
        _canvasScale = newScale;
        _statusText = $"Scroll zoom: {_canvasScale:F2}x";
        _gestureView?.Invalidate();
    }

    private void OnHover(SKHoverEventArgs e)
    {
        _statusText = $"Hover: ({e.Location.X:F0}, {e.Location.Y:F0})";
    }

    private void OnDragStarted(SKDragEventArgs e)
    {
        LogEvent($"Drag started at ({e.StartLocation.X:F0}, {e.StartLocation.Y:F0})");
        if (_selectedSticker != null)
            _statusText = $"Dragging Sticker {_selectedSticker.Label}";
        StateHasChanged();
    }

    private void OnDragUpdated(SKDragEventArgs e)
    {
        if (_selectedSticker != null)
        {
            _selectedSticker.Position = new SKPoint(
                _selectedSticker.Position.X + e.Delta.X,
                _selectedSticker.Position.Y + e.Delta.Y);
            _gestureView?.Invalidate();
        }
    }

    private void OnDragEnded(SKDragEventArgs e)
    {
        LogEvent($"Drag ended at ({e.CurrentLocation.X:F0}, {e.CurrentLocation.Y:F0})");
        _statusText = "Drag completed";
        StateHasChanged();
    }

    private void AdjustOffsetForPivot(SKPoint screenPivot, float oldScale, float newScale, float oldRotDeg, float newRotDeg)
    {
        var w2 = (float)_canvasWidth / 2f;
        var h2 = (float)_canvasHeight / 2f;
        var d = new SKPoint(screenPivot.X - w2, screenPivot.Y - h2);

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
        var matrix = SKMatrix.CreateTranslation(w / 2f, h / 2f);
        matrix = matrix.PreConcat(SKMatrix.CreateScale(_canvasScale, _canvasScale));
        matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees(_canvasRotation));
        matrix = matrix.PreConcat(SKMatrix.CreateTranslation(_canvasOffset.X, _canvasOffset.Y));
        matrix = matrix.PreConcat(SKMatrix.CreateTranslation(-w / 2f, -h / 2f));
        return matrix;
    }

    private Sticker? HitTest(SKPoint location)
    {
        if (_canvasWidth <= 0 || _canvasHeight <= 0) return null;
        var matrix = BuildCanvasTransform();
        if (!matrix.TryInvert(out var inverse)) return null;
        var transformed = inverse.MapPoint(location);
        for (int i = _stickers.Count - 1; i >= 0; i--)
        {
            var sticker = _stickers[i];
            if (SKPoint.Distance(transformed, sticker.Position) <= sticker.Size / 2)
                return sticker;
        }
        return null;
    }

    private void ResetView()
    {
        _canvasScale = 1f;
        _canvasRotation = 0f;
        _canvasOffset = SKPoint.Empty;
        _selectedSticker = null;
        _statusText = "View reset";
        _gestureView?.Invalidate();
    }

    private void LogEvent(string message)
    {
        _eventLog.Enqueue($"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
        while (_eventLog.Count > MaxLogEntries)
            _eventLog.Dequeue();
        StateHasChanged();
    }

    public void Dispose()
    {
        // No managed resources to clean up
    }

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
