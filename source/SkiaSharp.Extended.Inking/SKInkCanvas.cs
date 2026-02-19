using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// A canvas engine for managing digital ink strokes. This class handles stroke creation,
/// management, and rendering without any UI framework dependencies.
/// </summary>
public class SKInkCanvas : IDisposable
{
    private readonly List<SKInkStroke> strokes = new List<SKInkStroke>();
    private SKInkStroke? currentStroke;
    private bool isDisposed;

    /// <summary>
    /// Creates a new ink canvas with a default brush.
    /// </summary>
    public SKInkCanvas()
    {
        Brush = new SKInkStrokeBrush();
    }

    /// <summary>
    /// Creates a new ink canvas with a custom brush.
    /// </summary>
    /// <param name="brush">The default brush for new strokes.</param>
    public SKInkCanvas(SKInkStrokeBrush brush)
    {
        Brush = brush ?? throw new ArgumentNullException(nameof(brush));
    }

    /// <summary>
    /// Gets or sets the default brush for new strokes.
    /// When a stroke is started, the brush is cloned so each stroke has independent settings.
    /// </summary>
    public SKInkStrokeBrush Brush { get; set; }

    #region Events

    /// <summary>
    /// Occurs when the canvas content has changed and needs to be redrawn.
    /// </summary>
    public event EventHandler? Invalidated;

    /// <summary>
    /// Occurs when all strokes are cleared from the canvas.
    /// </summary>
    public event EventHandler? Cleared;

    /// <summary>
    /// Occurs when a stroke is completed.
    /// </summary>
    public event EventHandler<SKInkStrokeCompletedEventArgs>? StrokeCompleted;

    /// <summary>
    /// Occurs when a stroke is started.
    /// </summary>
    public event EventHandler? StrokeStarted;

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the completed strokes in the canvas.
    /// </summary>
    public IReadOnlyList<SKInkStroke> Strokes => strokes;

    /// <summary>
    /// Gets the current stroke being drawn (if any).
    /// </summary>
    public SKInkStroke? CurrentStroke => currentStroke;

    /// <summary>
    /// Gets the number of completed strokes in the canvas.
    /// </summary>
    public int StrokeCount => strokes.Count;

    /// <summary>
    /// Gets whether the canvas is blank (has no strokes).
    /// </summary>
    public bool IsBlank => strokes.Count == 0 && currentStroke == null;

    /// <summary>
    /// Gets whether there is an active stroke being drawn.
    /// </summary>
    public bool IsDrawing => currentStroke != null;

    /// <summary>
    /// Gets the selected strokes.
    /// </summary>
    public IReadOnlyList<SKInkStroke> SelectedStrokes => strokes.Where(s => s.IsSelected).ToList();

    /// <summary>
    /// Gets whether any strokes are selected.
    /// </summary>
    public bool HasSelection => strokes.Any(s => s.IsSelected);

    #endregion

    #region Stroke Creation

    /// <summary>
    /// Starts a new stroke at the specified point using a clone of the canvas brush.
    /// </summary>
    /// <param name="point">The starting point with pressure.</param>
    public void StartStroke(SKInkPoint point)
    {
        StartStroke(point, Brush.Clone());
    }

    /// <summary>
    /// Starts a new stroke at the specified point with a custom brush.
    /// </summary>
    /// <param name="point">The starting point with pressure.</param>
    /// <param name="brush">The brush for this stroke.</param>
    public void StartStroke(SKInkPoint point, SKInkStrokeBrush brush)
    {
        ThrowIfDisposed();

        if (brush == null)
            throw new ArgumentNullException(nameof(brush));

        // Cancel any existing stroke
        if (currentStroke != null)
        {
            currentStroke.Dispose();
        }

        currentStroke = new SKInkStroke(brush);
        currentStroke.AddPoint(point);

        StrokeStarted?.Invoke(this, EventArgs.Empty);
        Invalidated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Starts a new stroke at the specified location.
    /// </summary>
    /// <param name="location">The starting location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    public void StartStroke(SKPoint location, float pressure)
    {
        StartStroke(new SKInkPoint(location, pressure));
    }

    /// <summary>
    /// Continues the current stroke with a new point.
    /// </summary>
    /// <param name="point">The point to add.</param>
    public void ContinueStroke(SKInkPoint point)
    {
        ThrowIfDisposed();

        if (currentStroke != null)
        {
            currentStroke.AddPoint(point);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Continues the current stroke with a new point.
    /// </summary>
    /// <param name="location">The point location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    public void ContinueStroke(SKPoint location, float pressure)
    {
        ContinueStroke(new SKInkPoint(location, pressure));
    }

    /// <summary>
    /// Ends the current stroke.
    /// </summary>
    /// <param name="point">The final point of the stroke.</param>
    public void EndStroke(SKInkPoint point)
    {
        ThrowIfDisposed();

        if (currentStroke != null)
        {
            currentStroke.AddPoint(point, isLastPoint: true);

            if (!currentStroke.IsEmpty)
            {
                strokes.Add(currentStroke);
                StrokeCompleted?.Invoke(this, new SKInkStrokeCompletedEventArgs(currentStroke, strokes.Count));
            }
            else
            {
                currentStroke.Dispose();
            }

            currentStroke = null;
            Invalidated?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Ends the current stroke at the specified location.
    /// </summary>
    /// <param name="location">The final location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    public void EndStroke(SKPoint location, float pressure)
    {
        EndStroke(new SKInkPoint(location, pressure));
    }

    /// <summary>
    /// Cancels the current stroke without adding it to the canvas.
    /// </summary>
    public void CancelStroke()
    {
        ThrowIfDisposed();

        if (currentStroke != null)
        {
            currentStroke.Dispose();
            currentStroke = null;
            Invalidated?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Stroke Management

    /// <summary>
    /// Clears all strokes from the canvas.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();

        foreach (var stroke in strokes)
        {
            stroke.Dispose();
        }
        strokes.Clear();

        currentStroke?.Dispose();
        currentStroke = null;

        Cleared?.Invoke(this, EventArgs.Empty);
        Invalidated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes the last stroke from the canvas (undo).
    /// </summary>
    /// <returns>True if a stroke was removed, false if there were no strokes.</returns>
    public bool Undo()
    {
        ThrowIfDisposed();

        if (strokes.Count == 0)
            return false;

        var lastStroke = strokes[strokes.Count - 1];
        strokes.RemoveAt(strokes.Count - 1);
        lastStroke.Dispose();

        Invalidated?.Invoke(this, EventArgs.Empty);
        return true;
    }

    #endregion

    #region Selection

    /// <summary>
    /// Selects a stroke.
    /// </summary>
    /// <param name="stroke">The stroke to select.</param>
    /// <returns>True if the stroke was found and selected.</returns>
    public bool SelectStroke(SKInkStroke stroke)
    {
        ThrowIfDisposed();

        if (stroke == null)
            return false;

        if (!strokes.Contains(stroke))
            return false;

        if (!stroke.IsSelected)
        {
            stroke.IsSelected = true;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        return true;
    }

    /// <summary>
    /// Deselects a stroke.
    /// </summary>
    /// <param name="stroke">The stroke to deselect.</param>
    /// <returns>True if the stroke was found and deselected.</returns>
    public bool DeselectStroke(SKInkStroke stroke)
    {
        ThrowIfDisposed();

        if (stroke == null)
            return false;

        if (!strokes.Contains(stroke))
            return false;

        if (stroke.IsSelected)
        {
            stroke.IsSelected = false;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        return true;
    }

    /// <summary>
    /// Selects all strokes that intersect with the specified rectangle.
    /// </summary>
    /// <param name="rect">The selection rectangle.</param>
    /// <returns>The number of strokes selected.</returns>
    public int SelectStrokesInRect(SKRect rect)
    {
        ThrowIfDisposed();

        int count = 0;
        foreach (var stroke in strokes)
        {
            if (rect.IntersectsWith(stroke.Bounds))
            {
                if (!stroke.IsSelected)
                {
                    stroke.IsSelected = true;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        return count;
    }

    /// <summary>
    /// Deselects all strokes.
    /// </summary>
    /// <returns>The number of strokes deselected.</returns>
    public int DeselectAll()
    {
        ThrowIfDisposed();

        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke.IsSelected)
            {
                stroke.IsSelected = false;
                count++;
            }
        }

        if (count > 0)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        return count;
    }

    /// <summary>
    /// Deletes all selected strokes.
    /// </summary>
    /// <returns>The number of strokes deleted.</returns>
    public int DeleteSelected()
    {
        ThrowIfDisposed();

        var selected = strokes.Where(s => s.IsSelected).ToList();
        foreach (var stroke in selected)
        {
            strokes.Remove(stroke);
            stroke.Dispose();
        }

        if (selected.Count > 0)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        return selected.Count;
    }

    #endregion

    #region Export

    /// <summary>
    /// Gets a combined path of all strokes.
    /// </summary>
    /// <returns>An SKPath containing all strokes, or null if the canvas is blank.</returns>
    public SKPath? ToPath()
    {
        ThrowIfDisposed();

        if (strokes.Count == 0)
            return null;

        var combinedPath = new SKPath();
        foreach (var stroke in strokes)
        {
            if (stroke.Path is SKPath path)
            {
                combinedPath.AddPath(path);
            }
        }

        return combinedPath;
    }

    /// <summary>
    /// Gets the bounding rectangle of all strokes.
    /// </summary>
    /// <returns>The bounding rectangle, or empty if the canvas is blank.</returns>
    public SKRect GetBounds()
    {
        ThrowIfDisposed();

        if (strokes.Count == 0)
            return SKRect.Empty;

        var bounds = SKRect.Empty;
        foreach (var stroke in strokes)
        {
            var strokeBounds = stroke.Bounds;
            if (!strokeBounds.IsEmpty)
            {
                if (bounds.IsEmpty)
                {
                    bounds = strokeBounds;
                }
                else
                {
                    bounds = SKRect.Union(bounds, strokeBounds);
                }
            }
        }

        return bounds;
    }

    /// <summary>
    /// Draws all strokes to the specified canvas using per-stroke colors from their brushes.
    /// </summary>
    /// <param name="canvas">The canvas to draw to.</param>
    /// <param name="paint">The paint to use for drawing (color may be overridden per-stroke).</param>
    public void Draw(SKCanvas canvas, SKPaint paint)
    {
        ThrowIfDisposed();

        if (canvas == null)
            throw new ArgumentNullException(nameof(canvas));
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));

        var originalColor = paint.Color;

        // Draw all completed strokes
        foreach (var stroke in strokes)
        {
            if (stroke.Path is SKPath path)
            {
                paint.Color = stroke.Brush.Color;
                canvas.DrawPath(path, paint);
            }
        }

        // Draw the current stroke being drawn
        if (currentStroke?.Path is SKPath currentPath)
        {
            paint.Color = currentStroke.Brush.Color;
            canvas.DrawPath(currentPath, paint);
        }

        // Restore original color
        paint.Color = originalColor;
    }

    /// <summary>
    /// Renders the canvas to an SKImage.
    /// </summary>
    /// <param name="width">The width of the output image.</param>
    /// <param name="height">The height of the output image.</param>
    /// <param name="strokeColor">The color for the strokes.</param>
    /// <param name="backgroundColor">The background color, or null for transparent.</param>
    /// <param name="padding">The padding around the strokes (as a fraction of the image size, 0.0 to 1.0).</param>
    /// <returns>An SKImage containing the rendered strokes.</returns>
    public SKImage? ToImage(int width, int height, SKColor strokeColor, SKColor? backgroundColor = null, float padding = 0.1f)
    {
        ThrowIfDisposed();

        if (strokes.Count == 0)
            return null;

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);

        if (surface == null)
            return null;

        var canvas = surface.Canvas;

        if (backgroundColor.HasValue)
        {
            canvas.Clear(backgroundColor.Value);
        }
        else
        {
            canvas.Clear(SKColors.Transparent);
        }

        // Calculate scale to fit the strokes in the image
        var bounds = GetBounds();
        if (bounds.IsEmpty)
            return null;

        var paddingFactor = Clamp(1f - padding * 2, 0.1f, 1f);
        var scale = Math.Min(width / bounds.Width, height / bounds.Height) * paddingFactor;
        var offsetX = (width - bounds.Width * scale) / 2f - bounds.Left * scale;
        var offsetY = (height - bounds.Height * scale) / 2f - bounds.Top * scale;

        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale);

        using var paint = new SKPaint
        {
            Color = strokeColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        Draw(canvas, paint);

        return surface.Snapshot();
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Disposes all resources used by this canvas.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes all resources used by this canvas.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            foreach (var stroke in strokes)
            {
                stroke.Dispose();
            }
            strokes.Clear();

            currentStroke?.Dispose();
            currentStroke = null;
        }

        isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(SKInkCanvas));
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    #endregion
}
