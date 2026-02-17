using System;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Event arguments for when a stroke is completed.
/// </summary>
public class SKInkStrokeCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new instance of the event arguments.
    /// </summary>
    /// <param name="stroke">The completed stroke.</param>
    /// <param name="strokeCount">The total number of strokes after completion.</param>
    public SKInkStrokeCompletedEventArgs(SKInkStroke stroke, int strokeCount)
    {
        Stroke = stroke ?? throw new ArgumentNullException(nameof(stroke));
        StrokeCount = strokeCount;
    }

    /// <summary>
    /// Gets the completed stroke.
    /// </summary>
    public SKInkStroke Stroke { get; }

    /// <summary>
    /// Gets the total number of strokes on the canvas.
    /// </summary>
    public int StrokeCount { get; }
}
