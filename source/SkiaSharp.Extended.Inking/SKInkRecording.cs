using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// A recording of ink strokes that can be replayed.
/// This allows capturing ink input and playing it back to simulate a person writing.
/// </summary>
public class SKInkRecording
{
    private readonly List<RecordedStroke> recordedStrokes = new List<RecordedStroke>();

    /// <summary>
    /// Gets the recorded strokes.
    /// </summary>
    public IReadOnlyList<RecordedStroke> Strokes => recordedStrokes;

    /// <summary>
    /// Gets the total duration of the recording.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            if (recordedStrokes.Count == 0)
                return TimeSpan.Zero;

            long maxEndTime = 0;
            foreach (var stroke in recordedStrokes)
            {
                if (stroke.Points.Count > 0)
                {
                    var lastPoint = stroke.Points[stroke.Points.Count - 1];
                    if (lastPoint.Timestamp > maxEndTime)
                        maxEndTime = lastPoint.Timestamp;
                }
            }
            return TimeSpan.FromMilliseconds(maxEndTime);
        }
    }

    /// <summary>
    /// Gets whether the recording is empty.
    /// </summary>
    public bool IsEmpty => recordedStrokes.Count == 0;

    /// <summary>
    /// Adds a stroke to the recording.
    /// </summary>
    /// <param name="stroke">The stroke to add.</param>
    public void AddStroke(RecordedStroke stroke)
    {
        if (stroke == null)
            throw new ArgumentNullException(nameof(stroke));
        recordedStrokes.Add(stroke);
    }

    /// <summary>
    /// Clears all recorded strokes.
    /// </summary>
    public void Clear()
    {
        recordedStrokes.Clear();
    }

    /// <summary>
    /// Creates a recording from an ink canvas.
    /// </summary>
    /// <param name="canvas">The ink canvas to record from.</param>
    /// <returns>A recording of all strokes in the canvas.</returns>
    public static SKInkRecording FromCanvas(SKInkCanvas canvas)
    {
        if (canvas == null)
            throw new ArgumentNullException(nameof(canvas));

        var recording = new SKInkRecording();
        foreach (var stroke in canvas.Strokes)
        {
            var recordedStroke = new RecordedStroke(stroke.MinStrokeWidth, stroke.MaxStrokeWidth);
            foreach (var point in stroke.Points)
            {
                recordedStroke.AddPoint(point);
            }
            recording.AddStroke(recordedStroke);
        }
        return recording;
    }

    /// <summary>
    /// Creates a sample signature recording that simulates a person signing.
    /// </summary>
    /// <param name="width">The width of the signing area.</param>
    /// <param name="height">The height of the signing area.</param>
    /// <returns>A recording simulating a signature.</returns>
    public static SKInkRecording CreateSampleSignature(float width, float height)
    {
        var recording = new SKInkRecording();
        var random = new Random(42); // Fixed seed for reproducibility

        // First name initial "J"
        var stroke1 = new RecordedStroke(2f, 8f);
        AddCurvedStroke(stroke1, 
            new SKPoint(width * 0.1f, height * 0.3f),
            new SKPoint(width * 0.15f, height * 0.7f),
            new SKPoint(width * 0.08f, height * 0.8f),
            startTime: 0, duration: 400, random);
        recording.AddStroke(stroke1);

        // Connecting line
        var stroke2 = new RecordedStroke(2f, 8f);
        AddCurvedStroke(stroke2,
            new SKPoint(width * 0.12f, height * 0.5f),
            new SKPoint(width * 0.25f, height * 0.45f),
            new SKPoint(width * 0.35f, height * 0.5f),
            startTime: 500, duration: 300, random);
        recording.AddStroke(stroke2);

        // "o" loop
        var stroke3 = new RecordedStroke(2f, 8f);
        AddLoopStroke(stroke3,
            new SKPoint(width * 0.4f, height * 0.55f),
            width * 0.06f, height * 0.1f,
            startTime: 900, duration: 350, random);
        recording.AddStroke(stroke3);

        // "hn" wavy line
        var stroke4 = new RecordedStroke(2f, 8f);
        AddWavyStroke(stroke4,
            new SKPoint(width * 0.48f, height * 0.5f),
            new SKPoint(width * 0.75f, height * 0.55f),
            3, height * 0.08f,
            startTime: 1350, duration: 500, random);
        recording.AddStroke(stroke4);

        // Last name - underline flourish
        var stroke5 = new RecordedStroke(2f, 8f);
        AddFlourishStroke(stroke5,
            new SKPoint(width * 0.08f, height * 0.85f),
            new SKPoint(width * 0.85f, height * 0.82f),
            startTime: 2000, duration: 400, random);
        recording.AddStroke(stroke5);

        return recording;
    }

    private static void AddCurvedStroke(RecordedStroke stroke, SKPoint start, SKPoint mid, SKPoint end, 
        long startTime, int duration, Random random)
    {
        int pointCount = 20;
        for (int i = 0; i <= pointCount; i++)
        {
            float t = i / (float)pointCount;
            float u = 1 - t;
            
            // Quadratic bezier
            float x = u * u * start.X + 2 * u * t * mid.X + t * t * end.X;
            float y = u * u * start.Y + 2 * u * t * mid.Y + t * t * end.Y;
            
            // Pressure varies naturally
            float pressure = 0.4f + 0.4f * (float)Math.Sin(t * Math.PI) + 0.1f * (float)(random.NextDouble() - 0.5);
            pressure = Math.Max(0.2f, Math.Min(1f, pressure));
            
            long timestamp = startTime + (long)(t * duration);
            stroke.AddPoint(new SKInkPoint(x, y, pressure, timestamp));
        }
    }

    private static void AddLoopStroke(RecordedStroke stroke, SKPoint center, float radiusX, float radiusY,
        long startTime, int duration, Random random)
    {
        int pointCount = 24;
        for (int i = 0; i <= pointCount; i++)
        {
            float t = i / (float)pointCount;
            float angle = t * 2 * (float)Math.PI;
            
            float x = center.X + radiusX * (float)Math.Cos(angle);
            float y = center.Y + radiusY * (float)Math.Sin(angle);
            
            // Pressure varies around the loop
            float pressure = 0.5f + 0.3f * (float)Math.Sin(angle * 2) + 0.1f * (float)(random.NextDouble() - 0.5);
            pressure = Math.Max(0.2f, Math.Min(1f, pressure));
            
            long timestamp = startTime + (long)(t * duration);
            stroke.AddPoint(new SKInkPoint(x, y, pressure, timestamp));
        }
    }

    private static void AddWavyStroke(RecordedStroke stroke, SKPoint start, SKPoint end, int waves, float amplitude,
        long startTime, int duration, Random random)
    {
        int pointCount = 30;
        for (int i = 0; i <= pointCount; i++)
        {
            float t = i / (float)pointCount;
            
            float x = start.X + t * (end.X - start.X);
            float baseY = start.Y + t * (end.Y - start.Y);
            float y = baseY + amplitude * (float)Math.Sin(t * waves * 2 * Math.PI);
            
            // Pressure varies with waves
            float pressure = 0.4f + 0.3f * (float)Math.Abs(Math.Sin(t * waves * Math.PI)) + 0.1f * (float)(random.NextDouble() - 0.5);
            pressure = Math.Max(0.2f, Math.Min(1f, pressure));
            
            long timestamp = startTime + (long)(t * duration);
            stroke.AddPoint(new SKInkPoint(x, y, pressure, timestamp));
        }
    }

    private static void AddFlourishStroke(RecordedStroke stroke, SKPoint start, SKPoint end,
        long startTime, int duration, Random random)
    {
        int pointCount = 25;
        for (int i = 0; i <= pointCount; i++)
        {
            float t = i / (float)pointCount;
            
            float x = start.X + t * (end.X - start.X);
            float y = start.Y + t * (end.Y - start.Y);
            
            // Add a slight upward curve at the end
            float curveAmount = 0.03f * (float)Math.Sin(t * Math.PI);
            y -= curveAmount * (end.X - start.X);
            
            // Pressure starts strong and tapers off
            float pressure = 0.8f * (1 - t * 0.5f) + 0.1f * (float)(random.NextDouble() - 0.5);
            pressure = Math.Max(0.2f, Math.Min(1f, pressure));
            
            long timestamp = startTime + (long)(t * duration);
            stroke.AddPoint(new SKInkPoint(x, y, pressure, timestamp));
        }
    }
}

/// <summary>
/// A recorded stroke containing points with timestamps.
/// </summary>
public class RecordedStroke
{
    private readonly List<SKInkPoint> points = new List<SKInkPoint>();

    /// <summary>
    /// Creates a new recorded stroke with default stroke widths.
    /// </summary>
    public RecordedStroke() : this(1f, 8f)
    {
    }

    /// <summary>
    /// Creates a new recorded stroke with the specified stroke width range.
    /// </summary>
    /// <param name="minStrokeWidth">Minimum stroke width.</param>
    /// <param name="maxStrokeWidth">Maximum stroke width.</param>
    public RecordedStroke(float minStrokeWidth, float maxStrokeWidth)
    {
        MinStrokeWidth = minStrokeWidth;
        MaxStrokeWidth = maxStrokeWidth;
    }

    /// <summary>
    /// Gets the minimum stroke width.
    /// </summary>
    public float MinStrokeWidth { get; }

    /// <summary>
    /// Gets the maximum stroke width.
    /// </summary>
    public float MaxStrokeWidth { get; }

    /// <summary>
    /// Gets the recorded points.
    /// </summary>
    public IReadOnlyList<SKInkPoint> Points => points;

    /// <summary>
    /// Adds a point to the recording.
    /// </summary>
    /// <param name="point">The point to add.</param>
    public void AddPoint(SKInkPoint point)
    {
        points.Add(point);
    }
}

/// <summary>
/// Player for replaying ink recordings to an ink canvas.
/// </summary>
public class SKInkPlayer
{
    private SKInkRecording? recording;
    private SKInkCanvas? canvas;
    private int currentStrokeIndex;
    private int currentPointIndex;
    private long playbackStartTime;
    private bool isPlaying;

    /// <summary>
    /// Gets or sets the playback speed multiplier. Default is 1.0.
    /// </summary>
    public float PlaybackSpeed { get; set; } = 1f;

    /// <summary>
    /// Gets whether the player is currently playing.
    /// </summary>
    public bool IsPlaying => isPlaying;

    /// <summary>
    /// Gets the current playback progress (0.0 to 1.0).
    /// </summary>
    public float Progress
    {
        get
        {
            if (recording == null || recording.Duration.TotalMilliseconds == 0)
                return 0;
            return (float)(CurrentTime / recording.Duration.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Gets the current playback time in milliseconds.
    /// </summary>
    public double CurrentTime { get; private set; }

    /// <summary>
    /// Occurs when playback is complete.
    /// </summary>
    public event EventHandler? PlaybackCompleted;

    /// <summary>
    /// Loads a recording for playback.
    /// </summary>
    /// <param name="recording">The recording to play.</param>
    /// <param name="canvas">The canvas to replay to.</param>
    public void Load(SKInkRecording recording, SKInkCanvas canvas)
    {
        this.recording = recording ?? throw new ArgumentNullException(nameof(recording));
        this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        Reset();
    }

    /// <summary>
    /// Starts or resumes playback.
    /// </summary>
    public void Play()
    {
        if (recording == null || canvas == null)
            return;

        isPlaying = true;
        playbackStartTime = GetTickCount64() - (long)(CurrentTime / PlaybackSpeed);
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Resets playback to the beginning.
    /// </summary>
    public void Reset()
    {
        currentStrokeIndex = 0;
        currentPointIndex = 0;
        CurrentTime = 0;
        isPlaying = false;
        canvas?.Clear();
    }

    /// <summary>
    /// Updates the playback state. Call this from your render loop.
    /// </summary>
    /// <returns>True if there are more frames to play, false if playback is complete.</returns>
    public bool Update()
    {
        if (!isPlaying || recording == null || canvas == null)
            return false;

        CurrentTime = (GetTickCount64() - playbackStartTime) * PlaybackSpeed;

        // Process all points up to the current time
        while (currentStrokeIndex < recording.Strokes.Count)
        {
            var stroke = recording.Strokes[currentStrokeIndex];
            
            while (currentPointIndex < stroke.Points.Count)
            {
                var point = stroke.Points[currentPointIndex];
                
                if (point.Timestamp > CurrentTime)
                    return true; // Wait for this point's time

                if (currentPointIndex == 0)
                {
                    // Start new stroke
                    canvas.StartStroke(point);
                }
                else if (currentPointIndex == stroke.Points.Count - 1)
                {
                    // End stroke
                    canvas.EndStroke(point);
                }
                else
                {
                    // Continue stroke
                    canvas.ContinueStroke(point);
                }

                currentPointIndex++;
            }

            // Move to next stroke
            currentStrokeIndex++;
            currentPointIndex = 0;
        }

        // Playback complete
        isPlaying = false;
        PlaybackCompleted?.Invoke(this, EventArgs.Empty);
        return false;
    }

    /// <summary>
    /// Plays the entire recording instantly without timing.
    /// </summary>
    public void PlayInstant()
    {
        if (recording == null || canvas == null)
            return;

        Reset();
        
        foreach (var stroke in recording.Strokes)
        {
            if (stroke.Points.Count == 0)
                continue;

            canvas.StartStroke(stroke.Points[0]);
            
            for (int i = 1; i < stroke.Points.Count - 1; i++)
            {
                canvas.ContinueStroke(stroke.Points[i]);
            }

            if (stroke.Points.Count > 1)
            {
                canvas.EndStroke(stroke.Points[stroke.Points.Count - 1]);
            }
            else
            {
                canvas.EndStroke(stroke.Points[0]);
            }
        }

        currentStrokeIndex = recording.Strokes.Count;
        CurrentTime = recording.Duration.TotalMilliseconds;
    }

    /// <summary>
    /// Gets a monotonic tick count compatible with .NET Standard 2.0.
    /// </summary>
    private static long GetTickCount64()
    {
#if NETSTANDARD2_0
        return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
#else
        return Environment.TickCount64;
#endif
    }
}
