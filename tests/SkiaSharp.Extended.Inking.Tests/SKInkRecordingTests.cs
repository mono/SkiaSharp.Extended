using SkiaSharp;
using SkiaSharp.Extended.Inking;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

public class SKInkRecordingTests
{
    [Fact]
    public void Constructor_CreatesEmptyRecording()
    {
        var recording = new SKInkRecording();

        Assert.True(recording.IsEmpty);
        Assert.Equal(TimeSpan.Zero, recording.Duration);
        Assert.Empty(recording.Strokes);
    }

    [Fact]
    public void AddStroke_AddsStrokeToRecording()
    {
        var recording = new SKInkRecording();
        var stroke = new RecordedStroke(2f, 8f);
        stroke.AddPoint(new SKInkPoint(10f, 20f, 0.5f, 100));
        
        recording.AddStroke(stroke);

        Assert.Single(recording.Strokes);
        Assert.False(recording.IsEmpty);
    }

    [Fact]
    public void AddStroke_ThrowsOnNull()
    {
        var recording = new SKInkRecording();

        Assert.Throws<ArgumentNullException>(() => recording.AddStroke(null!));
    }

    [Fact]
    public void Duration_ReturnsMaxTimestamp()
    {
        var recording = new SKInkRecording();
        
        var stroke1 = new RecordedStroke();
        stroke1.AddPoint(new SKInkPoint(0f, 0f, 0.5f, 0));
        stroke1.AddPoint(new SKInkPoint(10f, 10f, 0.5f, 500_000)); // 500ms in microseconds
        
        var stroke2 = new RecordedStroke();
        stroke2.AddPoint(new SKInkPoint(20f, 20f, 0.5f, 600_000)); // 600ms in microseconds
        stroke2.AddPoint(new SKInkPoint(30f, 30f, 0.5f, 1_000_000)); // 1000ms = 1s in microseconds
        
        recording.AddStroke(stroke1);
        recording.AddStroke(stroke2);

        Assert.Equal(TimeSpan.FromMilliseconds(1000), recording.Duration);
    }

    [Fact]
    public void Duration_ReturnsZero_WhenStrokesHaveNoPoints()
    {
        var recording = new SKInkRecording();
        recording.AddStroke(new RecordedStroke());
        recording.AddStroke(new RecordedStroke());

        Assert.Equal(TimeSpan.Zero, recording.Duration);
    }

    [Fact]
    public void Clear_RemovesAllStrokes()
    {
        var recording = new SKInkRecording();
        recording.AddStroke(new RecordedStroke());
        recording.AddStroke(new RecordedStroke());

        recording.Clear();

        Assert.True(recording.IsEmpty);
    }

    [Fact]
    public void CreateSampleSignature_CreatesNonEmptyRecording()
    {
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);

        Assert.False(recording.IsEmpty);
        Assert.True(recording.Strokes.Count > 0);
        Assert.True(recording.Duration.TotalMilliseconds > 0);
    }

    [Fact]
    public void CreateSampleSignature_HasMultipleStrokes()
    {
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);

        Assert.True(recording.Strokes.Count >= 3);
    }

    [Fact]
    public void CreateSampleSignature_StrokesHaveValidPoints()
    {
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);

        foreach (var stroke in recording.Strokes)
        {
            Assert.True(stroke.Points.Count > 0);
            foreach (var point in stroke.Points)
            {
                Assert.True(point.Pressure >= 0f && point.Pressure <= 1f);
            }
        }
    }

    [Fact]
    public void FromCanvas_RecordsAllStrokes()
    {
        using var canvas = new SKInkCanvas(new SKInkStrokeBrush(SKColors.Black, 2f, 8f));
        
        // Add first stroke
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.ContinueStroke(new SKPoint(30f, 40f), 0.6f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.4f);
        
        // Add second stroke
        canvas.StartStroke(new SKPoint(100f, 100f), 0.7f);
        canvas.EndStroke(new SKPoint(150f, 150f), 0.8f);

        var recording = SKInkRecording.FromCanvas(canvas);

        Assert.Equal(2, recording.Strokes.Count);
    }

    [Fact]
    public void FromCanvas_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => SKInkRecording.FromCanvas(null!));
    }

    [Fact]
    public void FromCanvas_PreservesStrokeWidths()
    {
        using var canvas = new SKInkCanvas(new SKInkStrokeBrush(SKColors.Black, 3f, 12f));
        
        canvas.StartStroke(new SKPoint(10f, 20f), 0.5f);
        canvas.EndStroke(new SKPoint(50f, 60f), 0.5f);

        var recording = SKInkRecording.FromCanvas(canvas);

        Assert.Equal(3f, recording.Strokes[0].Brush.MinSize.Width);
        Assert.Equal(12f, recording.Strokes[0].Brush.MaxSize.Width);
    }

    [Fact]
    public void RecordedStroke_StoresPoints()
    {
        var stroke = new RecordedStroke(2f, 8f);
        
        stroke.AddPoint(new SKInkPoint(10f, 20f, 0.5f, 100));
        stroke.AddPoint(new SKInkPoint(30f, 40f, 0.6f, 200));
        stroke.AddPoint(new SKInkPoint(50f, 60f, 0.7f, 300));

        Assert.Equal(3, stroke.Points.Count);
        Assert.Equal(10f, stroke.Points[0].X);
        Assert.Equal(0.7f, stroke.Points[2].Pressure);
    }

    [Fact]
    public void RecordedStroke_PreservesWidthSettings()
    {
        var stroke = new RecordedStroke(3f, 12f);

        Assert.Equal(3f, stroke.Brush.MinSize.Width);
        Assert.Equal(12f, stroke.Brush.MaxSize.Width);
    }

    [Fact]
    public void RecordedStroke_DefaultConstructor_HasDefaultWidths()
    {
        var stroke = new RecordedStroke();

        Assert.Equal(1f, stroke.Brush.MinSize.Width);
        Assert.Equal(8f, stroke.Brush.MaxSize.Width);
    }

    [Fact]
    public void RecordedStroke_ThrowsOnNegativeMinWidth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecordedStroke(-1f, 8f));
    }

    [Fact]
    public void RecordedStroke_ThrowsWhenMaxLessThanMin()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecordedStroke(10f, 5f));
    }
}

public class SKInkPlayerTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var player = new SKInkPlayer();

        Assert.False(player.IsPlaying);
        Assert.Equal(0f, player.Progress);
        Assert.Equal(1f, player.PlaybackSpeed);
    }

    [Fact]
    public void Load_SetsUpPlayer()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);

        Assert.Equal(0f, player.Progress);
    }

    [Fact]
    public void Load_ThrowsOnNullRecording()
    {
        var player = new SKInkPlayer();
        using var canvas = new SKInkCanvas();

        Assert.Throws<ArgumentNullException>(() => player.Load(null!, canvas));
    }

    [Fact]
    public void Load_ThrowsOnNullCanvas()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);

        Assert.Throws<ArgumentNullException>(() => player.Load(recording, null!));
    }

    [Fact]
    public void PlayInstant_PlaysEntireRecording()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.PlayInstant();

        // Canvas should have strokes from the recording
        Assert.Equal(recording.Strokes.Count, canvas.StrokeCount);
    }

    [Fact]
    public void PlayInstant_SkipsEmptyStrokes()
    {
        var player = new SKInkPlayer();
        var recording = new SKInkRecording();
        recording.AddStroke(new RecordedStroke()); // Empty stroke
        
        var stroke = new RecordedStroke();
        stroke.AddPoint(new SKInkPoint(10f, 20f, 0.5f, 100));
        stroke.AddPoint(new SKInkPoint(50f, 60f, 0.5f, 200));
        recording.AddStroke(stroke);
        
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.PlayInstant();

        // Only the non-empty stroke should be added
        Assert.Equal(1, canvas.StrokeCount);
    }

    [Fact]
    public void PlayInstant_HandlesSinglePointStroke()
    {
        var player = new SKInkPlayer();
        var recording = new SKInkRecording();
        
        var stroke = new RecordedStroke();
        stroke.AddPoint(new SKInkPoint(10f, 20f, 0.5f, 100));
        recording.AddStroke(stroke);
        
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.PlayInstant();

        Assert.Equal(1, canvas.StrokeCount);
    }

    [Fact]
    public void Reset_ClearsProgress()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.PlayInstant();
        player.Reset();

        Assert.Equal(0f, player.Progress);
        Assert.True(canvas.IsBlank);
    }

    [Fact]
    public void PlaybackSpeed_CanBeModified()
    {
        var player = new SKInkPlayer();
        
        player.PlaybackSpeed = 2f;

        Assert.Equal(2f, player.PlaybackSpeed);
    }

    [Fact]
    public void Play_SetsIsPlayingTrue()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.Play();

        Assert.True(player.IsPlaying);
    }

    [Fact]
    public void Play_DoesNothing_WhenNotLoaded()
    {
        var player = new SKInkPlayer();

        player.Play(); // Should not throw

        Assert.False(player.IsPlaying);
    }

    [Fact]
    public void Pause_SetsIsPlayingFalse()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.Play();
        player.Pause();

        Assert.False(player.IsPlaying);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenNotPlaying()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        // Don't call Play()

        var result = player.Update();

        Assert.False(result);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenNotLoaded()
    {
        var player = new SKInkPlayer();

        var result = player.Update();

        Assert.False(result);
    }

    [Fact]
    public void PlayInstant_DoesNothing_WhenNotLoaded()
    {
        var player = new SKInkPlayer();

        player.PlayInstant(); // Should not throw
    }

    [Fact]
    public void Reset_DoesNothing_WhenNotLoaded()
    {
        var player = new SKInkPlayer();

        player.Reset(); // Should not throw
    }

    [Fact]
    public void Progress_ReturnsZero_WhenEmptyRecording()
    {
        var player = new SKInkPlayer();
        var recording = new SKInkRecording();
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);

        Assert.Equal(0f, player.Progress);
    }

    [Fact]
    public void CurrentTime_ReturnsCorrectValue_AfterPlayInstant()
    {
        var player = new SKInkPlayer();
        var recording = SKInkRecording.CreateSampleSignature(400f, 200f);
        using var canvas = new SKInkCanvas();

        player.Load(recording, canvas);
        player.PlayInstant();

        Assert.True(player.CurrentTime > 0);
    }
}
