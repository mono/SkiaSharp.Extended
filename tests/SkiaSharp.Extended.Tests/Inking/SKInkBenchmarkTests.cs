using SkiaSharp;
using SkiaSharp.Extended.Inking;
using System;
using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

/// <summary>
/// Benchmark tests for inking performance.
/// </summary>
public class SKInkBenchmarkTests
{
    [Fact]
    public void PathGeneration_1000Points_CompletesInReasonableTime()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush { MinSize = new SKSize(2f, 2f), MaxSize = new SKSize(8f, 8f), CapStyle = SKStrokeCapStyle.Round, SmoothingFactor = 4 });
        
        // Add 1000 points
        for (int i = 0; i < 1000; i++)
        {
            float x = i * 0.5f;
            float y = 50f + (float)Math.Sin(i * 0.02) * 30f;
            float pressure = 0.4f + 0.3f * (float)Math.Sin(i * 0.05);
            stroke.AddPoint(new SKPoint(x, y), pressure);
        }
        
        var sw = Stopwatch.StartNew();
        var path = stroke.Path;
        sw.Stop();
        
        Assert.NotNull(path);
        
        // Should complete in under 200ms (generous for CI environment)
        Assert.True(sw.ElapsedMilliseconds < 200, $"Path generation took too long: {sw.ElapsedMilliseconds}ms");
    }
    
    [Fact]
    public void PathGeneration_CacheHit_IsFast()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        for (int i = 0; i < 100; i++)
        {
            stroke.AddPoint(new SKPoint(i * 5f, 50f), 0.5f);
        }
        
        // First call generates path
        var path1 = stroke.Path;
        
        var sw = Stopwatch.StartNew();
        // Second call should hit cache
        for (int i = 0; i < 1000; i++)
        {
            var path = stroke.Path;
        }
        sw.Stop();
        
        // Cache hits should be nearly instant
        Assert.True(sw.ElapsedMilliseconds < 50, $"Cache hits took too long: {sw.ElapsedMilliseconds}ms");
    }
    
    [Fact]
    public void AddPoint_Performance_CompletesInReasonableTime()
    {
        using var stroke = new SKInkStroke(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            stroke.AddPoint(new SKPoint(i * 0.1f, 50f + (float)Math.Sin(i * 0.01) * 20f), 0.5f);
        }
        sw.Stop();
        
        // Should complete in under 100ms
        Assert.True(sw.ElapsedMilliseconds < 100, $"AddPoint took too long: {sw.ElapsedMilliseconds}ms");
    }
    
    [Fact]
    public void SmoothingFactor_AffectsPathGeneration()
    {
        // Test that different smoothing factors produce different results
        using var strokeLow = new SKInkStroke(new SKInkStrokeBrush { MinSize = new SKSize(2f, 2f), MaxSize = new SKSize(8f, 8f), CapStyle = SKStrokeCapStyle.Round, SmoothingFactor = 1 });
        using var strokeHigh = new SKInkStroke(new SKInkStrokeBrush { MinSize = new SKSize(2f, 2f), MaxSize = new SKSize(8f, 8f), CapStyle = SKStrokeCapStyle.Round, SmoothingFactor = 10 });
        
        // Add same points to both
        for (int i = 0; i < 10; i++)
        {
            strokeLow.AddPoint(new SKPoint(i * 20f, 50f + (float)Math.Sin(i * 0.5) * 20f), 0.5f);
            strokeHigh.AddPoint(new SKPoint(i * 20f, 50f + (float)Math.Sin(i * 0.5) * 20f), 0.5f);
        }
        
        var pathLow = strokeLow.Path;
        var pathHigh = strokeHigh.Path;
        
        Assert.NotNull(pathLow);
        Assert.NotNull(pathHigh);
        
        // Higher smoothing should generate more points in the path
        Assert.True(pathHigh!.PointCount >= pathLow!.PointCount);
    }
    
    [Fact]
    public void CanvasRender_ManyStrokes_CompletesInReasonableTime()
    {
        using var canvas = new SKInkCanvas(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        // Add 50 strokes
        for (int s = 0; s < 50; s++)
        {
            canvas.StartStroke(new SKPoint(s * 10f, 50f), 0.5f);
            for (int p = 0; p < 20; p++)
            {
                canvas.ContinueStroke(new SKPoint(s * 10f + p * 5f, 50f + (float)Math.Sin(p * 0.3) * 20f), 0.5f);
            }
            canvas.EndStroke(new SKPoint(s * 10f + 100f, 50f), 0.5f);
        }
        
        var info = new SKImageInfo(800, 400);
        using var surface = SKSurface.Create(info);
        using var paint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill };
        
        var sw = Stopwatch.StartNew();
        canvas.Draw(surface.Canvas, paint);
        sw.Stop();
        
        // Should complete in under 500ms (generous for CI)
        Assert.True(sw.ElapsedMilliseconds < 500, $"Rendering took too long: {sw.ElapsedMilliseconds}ms");
    }
    
    [Fact]
    public void ToImage_Performance_CompletesInReasonableTime()
    {
        using var canvas = new SKInkCanvas(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        // Add a few strokes
        for (int s = 0; s < 10; s++)
        {
            canvas.StartStroke(new SKPoint(s * 50f, 100f), 0.5f);
            for (int p = 0; p < 30; p++)
            {
                canvas.ContinueStroke(new SKPoint(s * 50f + p * 10f, 100f + (float)Math.Sin(p * 0.2) * 50f), 0.5f);
            }
            canvas.EndStroke(new SKPoint(s * 50f + 300f, 100f), 0.5f);
        }
        
        var sw = Stopwatch.StartNew();
        using var image = canvas.ToImage(800, 400, SKColors.Black, SKColors.White);
        sw.Stop();
        
        Assert.NotNull(image);
        
        // Should complete in under 500ms
        Assert.True(sw.ElapsedMilliseconds < 500, $"ToImage took too long: {sw.ElapsedMilliseconds}ms");
    }
}
