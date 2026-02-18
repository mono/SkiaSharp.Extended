using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Represents a single ink stroke with pressure-sensitive variable-width rendering.
/// Uses quadratic Bézier curves for smooth path interpolation and renders
/// variable-width strokes as filled polygons based on pressure data.
/// </summary>
public class SKInkStroke : IDisposable
{
    // Implementation adapted from @colinta on StackOverflow: https://stackoverflow.com/a/35229104
    // Enhanced with pressure sensitivity for fluid ink rendering

    private const float MinimumPointDistance = 2.0f;
    private const float DefaultPressure = 0.5f;

    private readonly List<SKInkPoint> points = new List<SKInkPoint>();
    private readonly float minStrokeWidth;
    private readonly float maxStrokeWidth;

    private SKPath? cachedPath;
    private bool isDirty = true;
    private bool isDisposed;

    /// <summary>
    /// Creates a new ink stroke with the specified stroke width range.
    /// </summary>
    /// <param name="minStrokeWidth">Minimum stroke width (at zero pressure).</param>
    /// <param name="maxStrokeWidth">Maximum stroke width (at full pressure).</param>
    /// <param name="color">The stroke color, or null to use canvas default.</param>
    /// <param name="capStyle">The cap style for stroke ends.</param>
    /// <param name="smoothingFactor">The smoothing factor (1-10, higher = smoother).</param>
    public SKInkStroke(
        float minStrokeWidth = 1f, 
        float maxStrokeWidth = 8f,
        SKColor? color = null,
        SKStrokeCapStyle capStyle = SKStrokeCapStyle.Round,
        int smoothingFactor = 4)
    {
        if (minStrokeWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(minStrokeWidth), "Minimum stroke width must be non-negative.");
        if (maxStrokeWidth < minStrokeWidth)
            throw new ArgumentOutOfRangeException(nameof(maxStrokeWidth), "Maximum stroke width must be greater than or equal to minimum stroke width.");
        if (smoothingFactor < 1 || smoothingFactor > 10)
            throw new ArgumentOutOfRangeException(nameof(smoothingFactor), "Smoothing factor must be between 1 and 10.");

        this.minStrokeWidth = minStrokeWidth;
        this.maxStrokeWidth = maxStrokeWidth;
        Color = color;
        CapStyle = capStyle;
        this.smoothingFactor = smoothingFactor;
    }

    /// <summary>
    /// Gets the minimum stroke width (at zero pressure).
    /// </summary>
    public float MinStrokeWidth => minStrokeWidth;

    /// <summary>
    /// Gets the maximum stroke width (at full pressure).
    /// </summary>
    public float MaxStrokeWidth => maxStrokeWidth;

    /// <summary>
    /// Gets or sets the stroke color. If null, uses the canvas default color.
    /// </summary>
    public SKColor? Color { get; set; }

    /// <summary>
    /// Gets or sets the cap style for stroke ends.
    /// </summary>
    public SKStrokeCapStyle CapStyle { get; set; }

    private int smoothingFactor = 4;

    /// <summary>
    /// Gets or sets the smoothing factor (1-10). Higher values produce smoother curves
    /// but require more processing. Default is 4.
    /// </summary>
    public int SmoothingFactor
    {
        get => smoothingFactor;
        set
        {
            if (value < 1 || value > 10)
                throw new ArgumentOutOfRangeException(nameof(value), "Smoothing factor must be between 1 and 10.");
            smoothingFactor = value;
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets the list of points in this stroke.
    /// </summary>
    public IReadOnlyList<SKInkPoint> Points => points;

    /// <summary>
    /// Gets the number of points in this stroke.
    /// </summary>
    public int PointCount => points.Count;

    /// <summary>
    /// Gets whether the stroke has no points.
    /// </summary>
    public bool IsEmpty => points.Count == 0;

    /// <summary>
    /// Gets the filled path representing this variable-width stroke.
    /// The path is cached and only regenerated when points change.
    /// </summary>
    public SKPath? Path
    {
        get
        {
            ThrowIfDisposed();
            if (isDirty)
            {
                cachedPath?.Dispose();
                cachedPath = GenerateVariableWidthPath();
                isDirty = false;
            }
            return cachedPath;
        }
    }

    /// <summary>
    /// Gets the bounding rectangle of the stroke.
    /// </summary>
    public SKRect Bounds => Path?.Bounds ?? SKRect.Empty;

    /// <summary>
    /// Adds a point to the stroke with pressure information.
    /// </summary>
    /// <param name="point">The ink point to add.</param>
    /// <param name="isLastPoint">Whether this is the final point in the stroke.</param>
    public void AddPoint(SKInkPoint point, bool isLastPoint = false)
    {
        ThrowIfDisposed();

        var pressure = point.Pressure;

        // Use default pressure if no pressure data is available (finger touch returns 0)
        if (pressure == 0f && !isLastPoint)
            pressure = DefaultPressure;

        // Do not add a point if the current point is too close to the previous point
        if (!isLastPoint && points.Count > 0 && !HasMovedFarEnough(points[points.Count - 1].Location, point.Location))
            return;

        // Create a new point with potentially adjusted pressure
        var adjustedPoint = new SKInkPoint(point.Location, pressure, point.Timestamp);
        points.Add(adjustedPoint);
        isDirty = true;
    }

    /// <summary>
    /// Adds a point to the stroke with pressure information.
    /// </summary>
    /// <param name="location">The point location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="isLastPoint">Whether this is the final point in the stroke.</param>
    public void AddPoint(SKPoint location, float pressure, bool isLastPoint = false)
    {
        AddPoint(new SKInkPoint(location, pressure), isLastPoint);
    }

    /// <summary>
    /// Clears all points from the stroke.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        points.Clear();
        cachedPath?.Dispose();
        cachedPath = null;
        isDirty = true;
    }

    /// <summary>
    /// Disposes of the cached path resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the cached path resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            cachedPath?.Dispose();
            cachedPath = null;
        }

        isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(SKInkStroke));
    }

    private static bool HasMovedFarEnough(SKPoint prevPoint, SKPoint currPoint)
    {
        var deltaX = currPoint.X - prevPoint.X;
        var deltaY = currPoint.Y - prevPoint.Y;
        var distance = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        return distance >= MinimumPointDistance;
    }

    /// <summary>
    /// Generates a variable-width path by creating a filled polygon
    /// that represents the stroke with width varying based on pressure.
    /// </summary>
    private SKPath? GenerateVariableWidthPath()
    {
        if (points.Count < 2)
        {
            // For a single point, draw a circle
            if (points.Count == 1)
            {
                var path = new SKPath();
                var radius = GetStrokeWidth(points[0].Pressure) / 2f;
                path.AddCircle(points[0].X, points[0].Y, radius);
                return path;
            }
            return null;
        }

        // Generate smoothed centerline points using quadratic Bezier interpolation
        var smoothedPoints = GenerateSmoothedPoints();
        if (smoothedPoints.Count < 2)
            return null;

        // Generate left and right offset points based on pressure
        var leftPoints = new List<SKPoint>();
        var rightPoints = new List<SKPoint>();

        for (int i = 0; i < smoothedPoints.Count; i++)
        {
            var point = smoothedPoints[i].Point;
            var pressure = smoothedPoints[i].Pressure;
            var halfWidth = GetStrokeWidth(pressure) / 2f;

            // Calculate the normal (perpendicular) at this point
            SKPoint tangent;
            if (i == 0)
            {
                // First point: use direction to next point
                tangent = Normalize(Subtract(smoothedPoints[i + 1].Point, point));
            }
            else if (i == smoothedPoints.Count - 1)
            {
                // Last point: use direction from previous point
                tangent = Normalize(Subtract(point, smoothedPoints[i - 1].Point));
            }
            else
            {
                // Middle point: average direction
                var prev = smoothedPoints[i - 1].Point;
                var next = smoothedPoints[i + 1].Point;
                tangent = Normalize(Subtract(next, prev));
            }

            // Normal is perpendicular to tangent
            var normal = new SKPoint(-tangent.Y, tangent.X);

            // Offset points on both sides
            leftPoints.Add(new SKPoint(point.X + normal.X * halfWidth, point.Y + normal.Y * halfWidth));
            rightPoints.Add(new SKPoint(point.X - normal.X * halfWidth, point.Y - normal.Y * halfWidth));
        }

        // Build the filled polygon path
        var resultPath = new SKPath();

        // Start with the left side (forward direction)
        resultPath.MoveTo(leftPoints[0]);
        for (int i = 1; i < leftPoints.Count; i++)
        {
            resultPath.LineTo(leftPoints[i]);
        }

        // Add end cap
        var endRadius = GetStrokeWidth(smoothedPoints[smoothedPoints.Count - 1].Pressure) / 2f;
        var endCenter = smoothedPoints[smoothedPoints.Count - 1].Point;
        var endTangent = Normalize(Subtract(smoothedPoints[smoothedPoints.Count - 1].Point, smoothedPoints[smoothedPoints.Count - 2].Point));
        AddCap(resultPath, endCenter, endTangent, endRadius, isStart: false);

        // Continue with the right side (reverse direction)
        for (int i = rightPoints.Count - 1; i >= 0; i--)
        {
            resultPath.LineTo(rightPoints[i]);
        }

        // Add start cap (direction is reversed since we're drawing back to start)
        var startRadius = GetStrokeWidth(smoothedPoints[0].Pressure) / 2f;
        var startCenter = smoothedPoints[0].Point;
        var startTangent = Normalize(Subtract(smoothedPoints[0].Point, smoothedPoints[1].Point));
        AddCap(resultPath, startCenter, startTangent, startRadius, isStart: true);

        resultPath.Close();

        return resultPath;
    }

    /// <summary>
    /// Generates smoothed points using quadratic Bezier interpolation.
    /// </summary>
    private List<(SKPoint Point, float Pressure)> GenerateSmoothedPoints()
    {
        var result = new List<(SKPoint Point, float Pressure)>();

        if (points.Count == 0)
            return result;

        if (points.Count == 1)
        {
            result.Add((points[0].Location, points[0].Pressure));
            return result;
        }

        // For only 2 points, just interpolate linearly with smoothing
        if (points.Count == 2)
        {
            var p0 = points[0].Location;
            var p1 = points[1].Location;
            var pressure0 = points[0].Pressure;
            var pressure1 = points[1].Pressure;

            result.Add((p0, pressure0));
            
            // Add intermediate points based on smoothing factor
            for (int i = 1; i < SmoothingFactor; i++)
            {
                float t = i / (float)SmoothingFactor;
                var x = p0.X + t * (p1.X - p0.X);
                var y = p0.Y + t * (p1.Y - p0.Y);
                var pressure = pressure0 + t * (pressure1 - pressure0);
                result.Add((new SKPoint(x, y), pressure));
            }
            
            result.Add((p1, pressure1));
            return result;
        }

        // Add the first point
        result.Add((points[0].Location, points[0].Pressure));

        // Generate intermediate points using quadratic Bezier interpolation
        for (int i = 0; i < points.Count - 1; i++)
        {
            var p0 = points[i].Location;
            var p1 = points[i + 1].Location;
            var pressure0 = points[i].Pressure;
            var pressure1 = points[i + 1].Pressure;

            // Calculate midpoint
            var midPoint = new SKPoint((p0.X + p1.X) / 2f, (p0.Y + p1.Y) / 2f);
            var midPressure = (pressure0 + pressure1) / 2f;

            if (i == 0)
            {
                // First segment: add midpoint
                result.Add((midPoint, midPressure));
            }
            else if (i < points.Count - 2)
            {
                // Middle segments: add quadratic curve samples through the control point
                var prevMid = result[result.Count - 1];
                AddQuadraticSamples(result, prevMid.Point, prevMid.Pressure, p0, pressure0, midPoint, midPressure);
            }
            else
            {
                // Last segment (i == points.Count - 2): interpolate from previous midpoint through last control point to end
                var prevMid = result[result.Count - 1];
                AddQuadraticSamples(result, prevMid.Point, prevMid.Pressure, p0, pressure0, p1, pressure1);
            }
        }

        // Add the last point if not already added by the last segment interpolation
        var lastResultPoint = result[result.Count - 1].Point;
        var lastOriginalPoint = points[points.Count - 1].Location;
        if (Math.Abs(lastResultPoint.X - lastOriginalPoint.X) > 0.01f || 
            Math.Abs(lastResultPoint.Y - lastOriginalPoint.Y) > 0.01f)
        {
            result.Add((lastOriginalPoint, points[points.Count - 1].Pressure));
        }

        return result;
    }

    /// <summary>
    /// Adds samples along a quadratic Bezier curve for smooth interpolation.
    /// </summary>
    private void AddQuadraticSamples(
        List<(SKPoint Point, float Pressure)> result,
        SKPoint p0, float pressure0,
        SKPoint control, float controlPressure,
        SKPoint p1, float pressure1)
    {
        // Sample the quadratic curve using the smoothing factor
        int samples = SmoothingFactor;
        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            float u = 1 - t;

            // Quadratic Bezier formula: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            var x = u * u * p0.X + 2 * u * t * control.X + t * t * p1.X;
            var y = u * u * p0.Y + 2 * u * t * control.Y + t * t * p1.Y;
            var pressure = u * u * pressure0 + 2 * u * t * controlPressure + t * t * pressure1;

            result.Add((new SKPoint(x, y), pressure));
        }
    }

    /// <summary>
    /// Adds a cap at the end of the stroke based on the cap style.
    /// </summary>
    private void AddCap(SKPath path, SKPoint center, SKPoint direction, float radius, bool isStart)
    {
        switch (CapStyle)
        {
            case SKStrokeCapStyle.Round:
                AddRoundedCap(path, center, direction, radius);
                break;
            case SKStrokeCapStyle.Flat:
                // Flat cap - just continue the path, no additional geometry needed
                break;
            case SKStrokeCapStyle.Tapered:
                AddTaperedCap(path, center, direction, radius, isStart);
                break;
        }
    }

    /// <summary>
    /// Adds a rounded cap at the end of the stroke.
    /// </summary>
    private static void AddRoundedCap(SKPath path, SKPoint center, SKPoint direction, float radius)
    {
        // Add a semicircle cap
        var perpendicular = new SKPoint(-direction.Y, direction.X);
        var startAngle = (float)(Math.Atan2(perpendicular.Y, perpendicular.X) * 180.0 / Math.PI);

        var rect = new SKRect(
            center.X - radius,
            center.Y - radius,
            center.X + radius,
            center.Y + radius);

        path.ArcTo(rect, startAngle, 180, false);
    }

    /// <summary>
    /// Calculates the stroke width based on pressure.
    /// </summary>
    private float GetStrokeWidth(float pressure)
    {
        return minStrokeWidth + (maxStrokeWidth - minStrokeWidth) * pressure;
    }

    /// <summary>
    /// Adds a tapered cap that narrows to a point.
    /// The taper extends from the current path position to a sharp tip.
    /// The path then continues to the next point in the polygon, creating a triangular taper.
    /// </summary>
    private static void AddTaperedCap(SKPath path, SKPoint center, SKPoint direction, float radius, bool isStart)
    {
        _ = isStart; // Parameter kept for API consistency with other cap methods
        
        // Calculate the tip point (extends beyond center in the stroke direction)
        var tipDistance = radius * 1.5f; // Extend the taper beyond the stroke width
        var tipPoint = new SKPoint(
            center.X + direction.X * tipDistance,
            center.Y + direction.Y * tipDistance);

        // Add a line to the tip point. The polygon path structure ensures that:
        // - For end caps: path continues from left edge → tip → right edge
        // - For start caps: path continues from right edge → tip → left edge (via Close)
        path.LineTo(tipPoint);
    }

    /// <summary>
    /// Subtracts two points.
    /// </summary>
    private static SKPoint Subtract(SKPoint a, SKPoint b)
    {
        return new SKPoint(a.X - b.X, a.Y - b.Y);
    }

    /// <summary>
    /// Normalizes a vector to unit length.
    /// </summary>
    private static SKPoint Normalize(SKPoint vector)
    {
        var length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        if (length < 0.0001f)
            return new SKPoint(1, 0); // Default direction for zero-length vectors

        return new SKPoint(vector.X / length, vector.Y / length);
    }
}
