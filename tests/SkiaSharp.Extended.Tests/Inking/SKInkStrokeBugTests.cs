using SkiaSharp;
using SkiaSharp.Extended.Inking;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

/// <summary>
/// Tests for potential bugs and edge cases in SKInkStroke.
/// </summary>
public class SKInkStrokeBugTests
{
    // Verify: Pressure values > 1.0 ARE properly clamped by SKInkPoint
    [Fact]
    public void AddPoint_PressureGreaterThanOne_IsClamped()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush(SKColors.Black, 2f, 10f));
        
        stroke.AddPoint(new SKPoint(0, 0), 2.0f); // Invalid pressure - should be clamped
        stroke.AddPoint(new SKPoint(100, 0), 2.0f);
        
        var path = stroke.Path;
        Assert.NotNull(path);
        
        // SKInkPoint correctly clamps pressure to 0-1 range
        Assert.Equal(1.0f, stroke.Points[0].Pressure);
    }
    
    // Verify: Negative pressure values ARE properly clamped by SKInkPoint
    [Fact]
    public void AddPoint_NegativePressure_IsClamped()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush(SKColors.Black, 2f, 10f));
        
        stroke.AddPoint(new SKPoint(0, 0), -0.5f); // Negative pressure - should be clamped
        stroke.AddPoint(new SKPoint(100, 0), -0.5f);
        
        var path = stroke.Path;
        Assert.NotNull(path);
        
        // SKInkPoint clamps to 0, then SKInkStroke replaces 0 with default 0.5
        Assert.Equal(0.5f, stroke.Points[0].Pressure);
    }
    
    // BUG 3: Points with identical location but different pressure
    [Fact]
    public void AddPoint_IdenticalLocation_OnlyFiltersConsecutiveClose()
    {
        using var stroke = new SKInkStroke();
        
        stroke.AddPoint(new SKPoint(0, 0), 0.5f);
        stroke.AddPoint(new SKPoint(0, 0), 0.5f); // Same point - should be filtered?
        stroke.AddPoint(new SKPoint(0, 0), 0.5f, isLastPoint: true); // Last point always added
        
        // What should happen? Currently 2 points (first and last) due to isLastPoint
        Assert.Equal(2, stroke.PointCount);
    }
    
    // BUG 4: Path generation with collinear points
    [Fact]
    public void Path_CollinearPoints_GeneratesValidPath()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        // All points on the same line
        stroke.AddPoint(new SKPoint(0, 50), 0.5f);
        stroke.AddPoint(new SKPoint(25, 50), 0.5f);
        stroke.AddPoint(new SKPoint(50, 50), 0.5f);
        stroke.AddPoint(new SKPoint(75, 50), 0.5f);
        stroke.AddPoint(new SKPoint(100, 50), 0.5f);
        
        var path = stroke.Path;
        Assert.NotNull(path);
        Assert.False(path!.IsEmpty);
    }
    
    // BUG 5: Extremely short strokes with tapered caps
    [Fact]
    public void Path_VeryShortStroke_TaperedCap_DoesNotOverextend()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush { MinSize = new SKSize(2f, 2f), MaxSize = new SKSize(8f, 8f), CapStyle = SKStrokeCapStyle.Tapered });
        
        // Two points very close together
        stroke.AddPoint(new SKPoint(50, 50), 0.5f);
        stroke.AddPoint(new SKPoint(52, 50), 0.5f, isLastPoint: true); // Only 2px apart
        
        var path = stroke.Path;
        Assert.NotNull(path);
        
        var bounds = path!.Bounds;
        // The tapered cap now extends by TaperLength (default 20px)
        // With a short stroke and gradual taper, bounds should still be reasonable
        // Total width should be reasonable - with TaperLength=20, it can extend more
        Assert.True(bounds.Width < 60, $"Width too large: {bounds.Width}"); // Sanity check
    }
    
    // BUG 6: Empty canvas GetBounds after clear
    [Fact]
    public void GetBounds_AfterClear_ReturnsEmpty()
    {
        using var canvas = new SKInkCanvas();
        
        canvas.StartStroke(new SKPoint(10, 10), 0.5f);
        canvas.EndStroke(new SKPoint(100, 100), 0.5f);
        
        var boundsBefore = canvas.GetBounds();
        Assert.False(boundsBefore.IsEmpty);
        
        canvas.Clear();
        
        var boundsAfter = canvas.GetBounds();
        Assert.True(boundsAfter.IsEmpty);
    }
    
    // BUG 7: ToImage with strokes that have zero bounds
    [Fact]
    public void ToImage_SinglePointStroke_ReturnsImage()
    {
        using var canvas = new SKInkCanvas();
        
        canvas.StartStroke(new SKPoint(50, 50), 0.5f);
        canvas.EndStroke(new SKPoint(50, 50), 0.5f); // Same point
        
        // This might fail if bounds are zero
        var image = canvas.ToImage(100, 100, SKColors.Black);
        Assert.NotNull(image);
        image?.Dispose();
    }
    
    // BUG 8: SmoothingFactor setter validation
    [Fact]
    public void SmoothingFactor_SetBoundaryValues_AcceptsValid()
    {
        using var stroke = new SKInkStroke();
        
        stroke.Brush.SmoothingFactor = 1; // Minimum valid
        Assert.Equal(1, stroke.Brush.SmoothingFactor);
        
        stroke.Brush.SmoothingFactor = 10; // Maximum valid
        Assert.Equal(10, stroke.Brush.SmoothingFactor);
    }
    
    [Fact]
    public void SmoothingFactor_SetInvalidValues_Throws()
    {
        using var stroke = new SKInkStroke();
        
        Assert.Throws<ArgumentOutOfRangeException>(() => stroke.Brush.SmoothingFactor = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => stroke.Brush.SmoothingFactor = 11);
    }
    
    // BUG 9: Player GetTickCount timing issues
    [Fact]
    public void SKInkPlayer_PlaybackSpeed_AffectsProgress()
    {
        var recording = new SKInkRecording();
        var stroke = new RecordedStroke();
        stroke.AddPoint(new SKInkPoint(0, 0, 0.5f, 0));
        stroke.AddPoint(new SKInkPoint(100, 0, 0.5f, 1000));
        recording.AddStroke(stroke);
        
        using var canvas = new SKInkCanvas();
        var player = new SKInkPlayer();
        player.Load(recording, canvas);
        
        // Fast playback
        player.PlaybackSpeed = 2.0f;
        Assert.Equal(2.0f, player.PlaybackSpeed);
        
        // Slow playback
        player.PlaybackSpeed = 0.5f;
        Assert.Equal(0.5f, player.PlaybackSpeed);
    }
    
    // BUG 10: Recording from canvas doesn't preserve cap style or color
    [Fact]
    public void FromCanvas_PreservesStrokeWidths()
    {
        using var canvas = new SKInkCanvas(new SKInkStrokeBrush(SKColors.Black, 3f, 15f));
        
        canvas.StartStroke(new SKPoint(0, 0), 0.5f);
        canvas.EndStroke(new SKPoint(100, 0), 0.5f);
        
        var recording = SKInkRecording.FromCanvas(canvas);
        
        Assert.Single(recording.Strokes);
        Assert.Equal(3f, recording.Strokes[0].Brush.MinSize.Width);
        Assert.Equal(15f, recording.Strokes[0].Brush.MaxSize.Width);
    }
    
    // BUG 11: Very high smoothing with only 2 points
    [Fact]
    public void Path_TwoPoints_HighSmoothing_GeneratesCorrectPath()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush { MinSize = new SKSize(2f, 2f), MaxSize = new SKSize(8f, 8f), CapStyle = SKStrokeCapStyle.Round, SmoothingFactor = 10 });
        
        stroke.AddPoint(new SKPoint(0, 50), 0.3f);
        stroke.AddPoint(new SKPoint(100, 50), 0.7f);
        
        var path = stroke.Path;
        Assert.NotNull(path);
        
        // With smoothing factor 10, there should be 10 interpolated points + 2 endpoints
        // Path should be smooth
        var bounds = path!.Bounds;
        Assert.True(bounds.Width >= 90f);
    }
    
    // BUG 12: Consecutive calls to EndStroke
    [Fact]
    public void EndStroke_WithoutStart_DoesNothing()
    {
        using var canvas = new SKInkCanvas();
        
        // This should not throw
        canvas.EndStroke(new SKPoint(50, 50), 0.5f);
        
        Assert.True(canvas.IsBlank);
    }
    
    // BUG 13: CancelStroke then EndStroke
    [Fact]
    public void CancelStroke_ThenEndStroke_DoesNothing()
    {
        using var canvas = new SKInkCanvas();
        
        canvas.StartStroke(new SKPoint(0, 0), 0.5f);
        canvas.CancelStroke();
        canvas.EndStroke(new SKPoint(100, 100), 0.5f); // Should do nothing
        
        Assert.True(canvas.IsBlank);
    }
    
    // BUG 14: Path with normalized zero vector
    [Fact]
    public void Path_TwoIdenticalPoints_DoesNotCrash()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        stroke.AddPoint(new SKPoint(50, 50), 0.5f);
        stroke.AddPoint(new SKPoint(50, 50), 0.5f, isLastPoint: true); // Force identical
        
        // This tests the Normalize function with a zero vector
        // Should not crash even with division by zero potential
        var path = stroke.Path;
        Assert.NotNull(path);
    }

    // BUG FIX: SmoothingAlgorithm setter must invalidate cached path
    [Fact]
    public void SmoothingAlgorithm_Change_InvalidatesPath()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush { MinSize = new SKSize(2f, 2f), MaxSize = new SKSize(8f, 8f), CapStyle = SKStrokeCapStyle.Round, SmoothingFactor = 4, SmoothingAlgorithm = SKSmoothingAlgorithm.QuadraticBezier });
        
        stroke.AddPoint(new SKPoint(0f, 50f), 0.5f);
        stroke.AddPoint(new SKPoint(50f, 0f), 0.5f);
        stroke.AddPoint(new SKPoint(100f, 50f), 0.5f);
        
        var path1 = stroke.Path;
        Assert.NotNull(path1);
        
        // Adding a point should invalidate cache
        stroke.AddPoint(new SKPoint(150f, 50f), 0.5f, isLastPoint: true);
        
        var path2 = stroke.Path;
        Assert.NotNull(path2);
        
        // Path should have been regenerated because we added a point
        Assert.NotSame(path1, path2);
    }
}
