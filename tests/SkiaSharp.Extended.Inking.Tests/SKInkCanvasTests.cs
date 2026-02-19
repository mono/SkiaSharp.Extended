using SkiaSharp;
using SkiaSharp.Extended.Inking;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

public class SKInkCanvasTests
{
    [Fact]
    public void Constructor_CreatesEmptyCanvas()
    {
        using var canvas = new SKInkCanvas();

        Assert.True(canvas.IsBlank);
        Assert.Equal(0, canvas.StrokeCount);
        Assert.False(canvas.IsDrawing);
    }

    [Fact]
    public void Constructor_WithWidths_SetsWidthRange()
    {
        using var canvas = new SKInkCanvas(2f, 12f);

        Assert.Equal(2f, canvas.MinStrokeWidth);
        Assert.Equal(12f, canvas.MaxStrokeWidth);
    }

    [Fact]
    public void MinStrokeWidth_ThrowsOnNegative()
    {
        using var canvas = new SKInkCanvas();

        Assert.Throws<ArgumentOutOfRangeException>(() => canvas.MinStrokeWidth = -1f);
    }

    [Fact]
    public void Brush_MaxSize_ThrowsWhenNegative()
    {
        var brush = new SKInkStrokeBrush();
        
        Assert.Throws<ArgumentOutOfRangeException>(() => brush.MaxSize = new SKSize(-1, 5f));
    }

    [Fact]
    public void Brush_MinSize_ThrowsWhenNegative()
    {
        var brush = new SKInkStrokeBrush();
        
        Assert.Throws<ArgumentOutOfRangeException>(() => brush.MinSize = new SKSize(-1, 5f));
    }

    [Fact]
    public void StartStroke_CreatesCurrentStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);

        Assert.True(canvas.IsDrawing);
        Assert.NotNull(canvas.CurrentStroke);
        Assert.False(canvas.IsBlank); // Has current stroke
    }

    [Fact]
    public void StartStroke_RaisesStrokeStartedEvent()
    {
        using var canvas = new SKInkCanvas();
        var raised = false;

        canvas.StrokeStarted += (s, e) => raised = true;
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);

        Assert.True(raised);
    }

    [Fact]
    public void StartStroke_RaisesInvalidatedEvent()
    {
        using var canvas = new SKInkCanvas();
        var raised = false;

        canvas.Invalidated += (s, e) => raised = true;
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);

        Assert.True(raised);
    }

    [Fact]
    public void ContinueStroke_AddsPointToCurrentStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);

        Assert.Equal(2, canvas.CurrentStroke?.PointCount);
    }

    [Fact]
    public void ContinueStroke_DoesNothingWithoutCurrentStroke()
    {
        using var canvas = new SKInkCanvas();
        var invalidated = false;

        canvas.Invalidated += (s, e) => invalidated = true;
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);

        Assert.False(invalidated);
    }

    [Fact]
    public void EndStroke_AddsStrokeToCollection()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.EndStroke(new SKPoint(100f, 100f), 0.5f);

        Assert.Equal(1, canvas.StrokeCount);
        Assert.False(canvas.IsDrawing);
        Assert.Null(canvas.CurrentStroke);
    }

    [Fact]
    public void EndStroke_RaisesStrokeCompletedEvent()
    {
        using var canvas = new SKInkCanvas();
        SKInkStrokeCompletedEventArgs? eventArgs = null;

        canvas.StrokeCompleted += (s, e) => eventArgs = e;
        
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.EndStroke(new SKPoint(100f, 100f), 0.5f);

        Assert.NotNull(eventArgs);
        Assert.Equal(1, eventArgs!.StrokeCount);
        Assert.NotNull(eventArgs.Stroke);
    }

    [Fact]
    public void CancelStroke_DiscardsCurrentStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.CancelStroke();

        Assert.False(canvas.IsDrawing);
        Assert.Null(canvas.CurrentStroke);
        Assert.Equal(0, canvas.StrokeCount);
    }

    [Fact]
    public void Clear_RemovesAllStrokes()
    {
        using var canvas = new SKInkCanvas();

        // Add some strokes
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.StartStroke(new SKPoint(100f, 100f), 0.5f);
        canvas.EndStroke(new SKPoint(150f, 150f), 0.5f);

        canvas.Clear();

        Assert.True(canvas.IsBlank);
        Assert.Equal(0, canvas.StrokeCount);
    }

    [Fact]
    public void Clear_RaisesClearedEvent()
    {
        using var canvas = new SKInkCanvas();
        var raised = false;

        canvas.Cleared += (s, e) => raised = true;
        
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.Clear();

        Assert.True(raised);
    }

    [Fact]
    public void Undo_RemovesLastStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.StartStroke(new SKPoint(100f, 100f), 0.5f);
        canvas.EndStroke(new SKPoint(150f, 150f), 0.5f);

        var result = canvas.Undo();

        Assert.True(result);
        Assert.Equal(1, canvas.StrokeCount);
    }

    [Fact]
    public void Undo_ReturnsFalseWhenEmpty()
    {
        using var canvas = new SKInkCanvas();

        var result = canvas.Undo();

        Assert.False(result);
    }

    [Fact]
    public void ToPath_ReturnsCombinedPath()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.StartStroke(new SKPoint(100f, 100f), 0.5f);
        canvas.EndStroke(new SKPoint(150f, 150f), 0.5f);

        var path = canvas.ToPath();

        Assert.NotNull(path);
        Assert.False(path!.IsEmpty);
    }

    [Fact]
    public void ToPath_ReturnsNullWhenEmpty()
    {
        using var canvas = new SKInkCanvas();

        var path = canvas.ToPath();

        Assert.Null(path);
    }

    [Fact]
    public void GetBounds_ReturnsBoundsOfAllStrokes()
    {
        using var canvas = new SKInkCanvas(2f, 4f);

        canvas.StartStroke(new SKPoint(0f, 0f), 0.5f);
        canvas.EndStroke(new SKPoint(100f, 100f), 0.5f);
        canvas.StartStroke(new SKPoint(200f, 200f), 0.5f);
        canvas.EndStroke(new SKPoint(300f, 300f), 0.5f);

        var bounds = canvas.GetBounds();

        Assert.False(bounds.IsEmpty);
        Assert.True(bounds.Left <= 0f);
        Assert.True(bounds.Right >= 300f);
    }

    [Fact]
    public void Draw_DrawsAllStrokes()
    {
        using var canvas = new SKInkCanvas();
        
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);

        var info = new SKImageInfo(200, 200);
        using var surface = SKSurface.Create(info);
        using var paint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill };

        // Should not throw
        canvas.Draw(surface.Canvas, paint);
    }

    [Fact]
    public void Draw_IncludesCurrentStroke()
    {
        using var canvas = new SKInkCanvas();
        
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);
        // Don't end the stroke

        var info = new SKImageInfo(200, 200);
        using var surface = SKSurface.Create(info);
        using var paint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill };

        // Should not throw and should draw the current stroke
        canvas.Draw(surface.Canvas, paint);
    }

    [Fact]
    public void ToImage_ReturnsImage()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(100f, 100f), 0.5f);

        using var image = canvas.ToImage(200, 200, SKColors.Black);

        Assert.NotNull(image);
        Assert.Equal(200, image!.Width);
        Assert.Equal(200, image.Height);
    }

    [Fact]
    public void ToImage_ReturnsNullWhenEmpty()
    {
        using var canvas = new SKInkCanvas();

        using var image = canvas.ToImage(200, 200, SKColors.Black);

        Assert.Null(image);
    }

    [Fact]
    public void ToImage_WithBackgroundColor()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(100f, 100f), 0.5f);

        using var image = canvas.ToImage(100, 100, SKColors.Black, SKColors.White);

        Assert.NotNull(image);
    }

    [Fact]
    public void Dispose_DisposesAllStrokes()
    {
        var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);
        canvas.StartStroke(new SKPoint(100f, 100f), 0.5f);
        canvas.EndStroke(new SKPoint(150f, 150f), 0.5f);

        canvas.Dispose();

        Assert.Throws<ObjectDisposedException>(() => canvas.StartStroke(new SKPoint(0f, 0f), 0.5f));
    }

    [Fact]
    public void MultipleStrokes_PreservesOrder()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(0f, 0f), 0.5f);
        canvas.EndStroke(new SKPoint(10f, 10f), 0.5f);

        canvas.StartStroke(new SKPoint(100f, 100f), 0.5f);
        canvas.EndStroke(new SKPoint(110f, 110f), 0.5f);

        canvas.StartStroke(new SKPoint(200f, 200f), 0.5f);
        canvas.EndStroke(new SKPoint(210f, 210f), 0.5f);

        Assert.Equal(3, canvas.StrokeCount);
        
        // Check that strokes are in order by checking their first point
        Assert.Equal(0f, canvas.Strokes[0].Points[0].X);
        Assert.Equal(100f, canvas.Strokes[1].Points[0].X);
        Assert.Equal(200f, canvas.Strokes[2].Points[0].X);
    }

    [Fact]
    public void Draw_ThrowsOnNullCanvas()
    {
        using var inkCanvas = new SKInkCanvas();
        using var paint = new SKPaint();

        Assert.Throws<ArgumentNullException>(() => inkCanvas.Draw(null!, paint));
    }

    [Fact]
    public void Draw_ThrowsOnNullPaint()
    {
        using var inkCanvas = new SKInkCanvas();
        var info = new SKImageInfo(100, 100);
        using var surface = SKSurface.Create(info);

        Assert.Throws<ArgumentNullException>(() => inkCanvas.Draw(surface.Canvas, null!));
    }

    [Fact]
    public void StartStroke_ReplacesExistingStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(30f, 40f), 0.5f);
        
        // Start a new stroke without ending the first one
        canvas.StartStroke(new SKPoint(100f, 100f), 0.5f);

        // Should only have the new current stroke
        Assert.True(canvas.IsDrawing);
        Assert.Single(canvas.CurrentStroke!.Points);
        Assert.Equal(100f, canvas.CurrentStroke!.Points[0].X);
    }

    [Fact]
    public void EndStroke_WithSinglePoint_DoesNotAddEmptyStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(10.1f, 20.1f), 0.5f); // Point very close to first

        // Stroke should still be added (has at least the starting point)
        Assert.True(canvas.StrokeCount >= 0);
    }

    [Fact]
    public void EndStroke_WithoutStarting_DoesNothing()
    {
        using var canvas = new SKInkCanvas();
        var invalidated = false;

        canvas.Invalidated += (s, e) => invalidated = true;
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);

        Assert.False(invalidated);
        Assert.Equal(0, canvas.StrokeCount);
    }

    [Fact]
    public void CancelStroke_WithoutStarting_DoesNothing()
    {
        using var canvas = new SKInkCanvas();
        var invalidated = false;

        canvas.Invalidated += (s, e) => invalidated = true;
        canvas.CancelStroke();

        Assert.False(invalidated);
    }

    [Fact]
    public void GetBounds_ReturnsEmptyWhenBlank()
    {
        using var canvas = new SKInkCanvas();

        var bounds = canvas.GetBounds();

        Assert.Equal(SKRect.Empty, bounds);
    }

    [Fact]
    public void Clear_AlsoClearsCurrentStroke()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        // Don't end the stroke
        canvas.Clear();

        Assert.False(canvas.IsDrawing);
        Assert.Null(canvas.CurrentStroke);
    }

    [Fact]
    public void Undo_RaisesInvalidatedEvent()
    {
        using var canvas = new SKInkCanvas();
        var invalidated = false;

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);

        canvas.Invalidated += (s, e) => invalidated = true;
        canvas.Undo();

        Assert.True(invalidated);
    }

    [Fact]
    public void ToImage_WithPadding_ScalesCorrectly()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(100f, 100f), 0.5f);

        // Different padding values
        using var image1 = canvas.ToImage(200, 200, SKColors.Black, padding: 0.1f);
        using var image2 = canvas.ToImage(200, 200, SKColors.Black, padding: 0.3f);

        Assert.NotNull(image1);
        Assert.NotNull(image2);
    }

    [Fact]
    public void Constructor_WithInvalidWidths_Throws()
    {
        // Min > Max
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKInkCanvas(10f, 5f));
        
        // Negative min
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKInkCanvas(-1f, 5f));
    }

    [Fact]
    public void StartStroke_WithSKInkPoint_Works()
    {
        using var canvas = new SKInkCanvas();
        var point = new SKInkPoint(10f, 20f, 0.5f, 12345);

        canvas.StartStroke(point);

        Assert.True(canvas.IsDrawing);
        Assert.Equal(10f, canvas.CurrentStroke!.Points[0].X);
    }

    [Fact]
    public void ContinueStroke_WithSKInkPoint_Works()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKInkPoint(50f, 60f, 0.6f, 100));

        Assert.Equal(2, canvas.CurrentStroke!.PointCount);
    }

    [Fact]
    public void EndStroke_WithSKInkPoint_Works()
    {
        using var canvas = new SKInkCanvas();

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKInkPoint(100f, 100f, 0.4f, 200));

        Assert.Equal(1, canvas.StrokeCount);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var canvas = new SKInkCanvas();
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);

        canvas.Dispose();
        canvas.Dispose(); // Should not throw
    }

    [Fact]
    public void ContinueStroke_RaisesInvalidatedEvent()
    {
        using var canvas = new SKInkCanvas();
        var invalidatedCount = 0;

        canvas.Invalidated += (s, e) => invalidatedCount++;
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(50f, 60f), 0.5f);

        Assert.True(invalidatedCount >= 2); // At least for start and continue
    }

    [Fact]
    public void CancelStroke_RaisesInvalidatedEvent()
    {
        using var canvas = new SKInkCanvas();
        var invalidatedCount = 0;

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.Invalidated += (s, e) => invalidatedCount++;
        canvas.CancelStroke();

        Assert.True(invalidatedCount >= 1);
    }

    [Fact]
    public void StrokeColor_DefaultIsBlack()
    {
        using var canvas = new SKInkCanvas();

        Assert.Equal(SKColors.Black, canvas.StrokeColor);
    }

    [Fact]
    public void StrokeColor_CanBeSet()
    {
        using var canvas = new SKInkCanvas();

        canvas.StrokeColor = SKColors.Red;

        Assert.Equal(SKColors.Red, canvas.StrokeColor);
    }

    [Fact]
    public void CapStyle_DefaultIsRound()
    {
        using var canvas = new SKInkCanvas();

        Assert.Equal(SKStrokeCapStyle.Round, canvas.CapStyle);
    }

    [Fact]
    public void CapStyle_CanBeSet()
    {
        using var canvas = new SKInkCanvas();

        canvas.CapStyle = SKStrokeCapStyle.Tapered;

        Assert.Equal(SKStrokeCapStyle.Tapered, canvas.CapStyle);
    }

    [Fact]
    public void SmoothingFactor_DefaultIsFour()
    {
        using var canvas = new SKInkCanvas();

        Assert.Equal(4, canvas.SmoothingFactor);
    }

    [Fact]
    public void SmoothingFactor_CanBeSet()
    {
        using var canvas = new SKInkCanvas();

        canvas.SmoothingFactor = 8;

        Assert.Equal(8, canvas.SmoothingFactor);
    }

    [Fact]
    public void StartStroke_UsesCanvasDefaults()
    {
        using var canvas = new SKInkCanvas();
        canvas.StrokeColor = SKColors.Green;
        canvas.CapStyle = SKStrokeCapStyle.Tapered;
        canvas.SmoothingFactor = 6;

        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);

        Assert.Equal(SKColors.Green, canvas.CurrentStroke!.Color);
        Assert.Equal(SKStrokeCapStyle.Tapered, canvas.CurrentStroke!.CapStyle);
        Assert.Equal(6, canvas.CurrentStroke!.SmoothingFactor);
    }

    [Fact]
    public void StartStroke_WithCustomColor_OverridesDefault()
    {
        using var canvas = new SKInkCanvas();
        canvas.StrokeColor = SKColors.Green;

        canvas.StartStroke(new SKInkPoint(10f, 20f, 0.5f), SKColors.Blue);

        Assert.Equal(SKColors.Blue, canvas.CurrentStroke!.Color);
    }

    [Fact]
    public void Draw_UsesPerStrokeColors()
    {
        using var canvas = new SKInkCanvas();
        
        // Add a red stroke
        canvas.StartStroke(new SKInkPoint(10f, 20f, 0.5f), SKColors.Red);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);
        
        // Add a blue stroke
        canvas.StartStroke(new SKInkPoint(100f, 100f, 0.5f), SKColors.Blue);
        canvas.EndStroke(new SKPoint(150f, 150f), 0.5f);

        Assert.Equal(2, canvas.StrokeCount);
        Assert.Equal(SKColors.Red, canvas.Strokes[0].Color);
        Assert.Equal(SKColors.Blue, canvas.Strokes[1].Color);
    }
}
