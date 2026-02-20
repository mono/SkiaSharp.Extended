using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Internal struct representing a smoothed point with pressure and velocity.
/// </summary>
internal readonly struct SmoothedPoint
{
    public SmoothedPoint(SKPoint point, float pressure, float velocity)
    {
        Point = point;
        Pressure = pressure;
        Velocity = velocity;
    }

    public SKPoint Point { get; }
    public float Pressure { get; }
    public float Velocity { get; }
}

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

    private SKPath? cachedPath;
    private bool isDirty = true;
    private bool isDisposed;

    /// <summary>
    /// Creates a new ink stroke with a brush defining its appearance.
    /// </summary>
    /// <param name="brush">The brush defining stroke appearance. If null, uses a default brush.</param>
    public SKInkStroke(SKInkStrokeBrush? brush = null)
    {
        Brush = brush ?? new SKInkStrokeBrush();
    }

    /// <summary>
    /// Gets the brush defining this stroke's appearance.
    /// </summary>
    public SKInkStrokeBrush Brush { get; }

    /// <summary>
    /// Gets or sets whether this stroke is selected.
    /// </summary>
    public bool IsSelected { get; set; }

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

        // Calculate velocity from the previous point if timestamps are available
        float velocity = 0f;
        if (points.Count > 0 && point.TimestampMicroseconds > 0)
        {
            velocity = SKInkPoint.CalculateVelocity(points[points.Count - 1], point);
        }

        // Create a new point with adjusted pressure and calculated velocity, preserving tilt
        var adjustedPoint = new SKInkPoint(point.Location, pressure, point.TiltX, point.TiltY, point.TimestampMicroseconds, velocity);
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
                var radius = GetStrokeWidth(points[0].Pressure, points[0].Velocity) / 2f;
                path.AddCircle(points[0].X, points[0].Y, radius);
                return path;
            }
            return null;
        }

        // Generate smoothed centerline points using quadratic Bezier interpolation
        var smoothedPoints = GenerateSmoothedPoints();
        if (smoothedPoints.Count < 2)
            return null;

        // Generate left and right offset points based on pressure and velocity
        var leftPoints = new List<SKPoint>();
        var rightPoints = new List<SKPoint>();

        for (int i = 0; i < smoothedPoints.Count; i++)
        {
            var point = smoothedPoints[i].Point;
            var pressure = smoothedPoints[i].Pressure;
            var velocity = smoothedPoints[i].Velocity;
            var halfWidth = GetStrokeWidth(pressure, velocity) / 2f;

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
        var lastSmoothedPoint = smoothedPoints[smoothedPoints.Count - 1];
        var endRadius = GetStrokeWidth(lastSmoothedPoint.Pressure, lastSmoothedPoint.Velocity) / 2f;
        var endCenter = lastSmoothedPoint.Point;
        var endTangent = Normalize(Subtract(lastSmoothedPoint.Point, smoothedPoints[smoothedPoints.Count - 2].Point));
        AddCap(resultPath, endCenter, endTangent, endRadius, isStart: false);

        // Continue with the right side (reverse direction)
        for (int i = rightPoints.Count - 1; i >= 0; i--)
        {
            resultPath.LineTo(rightPoints[i]);
        }

        // Add start cap (direction is reversed since we're drawing back to start)
        var firstSmoothedPoint = smoothedPoints[0];
        var startRadius = GetStrokeWidth(firstSmoothedPoint.Pressure, firstSmoothedPoint.Velocity) / 2f;
        var startCenter = firstSmoothedPoint.Point;
        var startTangent = Normalize(Subtract(firstSmoothedPoint.Point, smoothedPoints[1].Point));
        AddCap(resultPath, startCenter, startTangent, startRadius, isStart: true);

        resultPath.Close();

        return resultPath;
    }

    /// <summary>
    /// Generates smoothed points using the selected smoothing algorithm.
    /// </summary>
    private List<SmoothedPoint> GenerateSmoothedPoints()
    {
        var result = new List<SmoothedPoint>();

        if (points.Count == 0)
            return result;

        if (points.Count == 1)
        {
            result.Add(new SmoothedPoint(points[0].Location, points[0].Pressure, points[0].Velocity));
            return result;
        }

        // For only 2 points, just interpolate linearly with smoothing
        if (points.Count == 2)
        {
            var p0 = points[0].Location;
            var p1 = points[1].Location;
            var pressure0 = points[0].Pressure;
            var pressure1 = points[1].Pressure;
            var velocity0 = points[0].Velocity;
            var velocity1 = points[1].Velocity;

            result.Add(new SmoothedPoint(p0, pressure0, velocity0));
            
            // Add intermediate points based on smoothing factor
            for (int i = 1; i < Brush.SmoothingFactor; i++)
            {
                float t = i / (float)Brush.SmoothingFactor;
                var x = p0.X + t * (p1.X - p0.X);
                var y = p0.Y + t * (p1.Y - p0.Y);
                var pressure = pressure0 + t * (pressure1 - pressure0);
                var velocity = velocity0 + t * (velocity1 - velocity0);
                result.Add(new SmoothedPoint(new SKPoint(x, y), pressure, velocity));
            }
            
            result.Add(new SmoothedPoint(p1, pressure1, velocity1));
            return result;
        }

        // Use selected smoothing algorithm for 3+ points
        if (Brush.SmoothingAlgorithm == SKSmoothingAlgorithm.CatmullRom)
        {
            return GenerateCatmullRomPoints();
        }
        else
        {
            return GenerateQuadraticBezierPoints();
        }
    }

    /// <summary>
    /// Generates smoothed points using quadratic Bezier interpolation.
    /// </summary>
    private List<SmoothedPoint> GenerateQuadraticBezierPoints()
    {
        var result = new List<SmoothedPoint>();

        // Add the first point
        result.Add(new SmoothedPoint(points[0].Location, points[0].Pressure, points[0].Velocity));

        // Generate intermediate points using quadratic Bezier interpolation
        for (int i = 0; i < points.Count - 1; i++)
        {
            var p0 = points[i].Location;
            var p1 = points[i + 1].Location;
            var pressure0 = points[i].Pressure;
            var pressure1 = points[i + 1].Pressure;
            var velocity0 = points[i].Velocity;
            var velocity1 = points[i + 1].Velocity;

            // Calculate midpoint
            var midPoint = new SKPoint((p0.X + p1.X) / 2f, (p0.Y + p1.Y) / 2f);
            var midPressure = (pressure0 + pressure1) / 2f;
            var midVelocity = (velocity0 + velocity1) / 2f;

            if (i == 0)
            {
                // First segment: add midpoint
                result.Add(new SmoothedPoint(midPoint, midPressure, midVelocity));
            }
            else if (i < points.Count - 2)
            {
                // Middle segments: add quadratic curve samples through the control point
                var prevMid = result[result.Count - 1];
                AddQuadraticSamples(result, prevMid.Point, prevMid.Pressure, prevMid.Velocity, p0, pressure0, velocity0, midPoint, midPressure, midVelocity);
            }
            else
            {
                // Last segment (i == points.Count - 2): interpolate from previous midpoint through last control point to end
                var prevMid = result[result.Count - 1];
                AddQuadraticSamples(result, prevMid.Point, prevMid.Pressure, prevMid.Velocity, p0, pressure0, velocity0, p1, pressure1, velocity1);
            }
        }

        // Add the last point if not already added by the last segment interpolation
        var lastResultPoint = result[result.Count - 1].Point;
        var lastOriginalPoint = points[points.Count - 1].Location;
        if (Math.Abs(lastResultPoint.X - lastOriginalPoint.X) > 0.01f || 
            Math.Abs(lastResultPoint.Y - lastOriginalPoint.Y) > 0.01f)
        {
            result.Add(new SmoothedPoint(lastOriginalPoint, points[points.Count - 1].Pressure, points[points.Count - 1].Velocity));
        }

        return result;
    }

    /// <summary>
    /// Generates smoothed points using Catmull-Rom spline interpolation.
    /// This algorithm passes through all control points, making it ideal for handwriting.
    /// </summary>
    private List<SmoothedPoint> GenerateCatmullRomPoints()
    {
        var result = new List<SmoothedPoint>();

        // Add the first point
        result.Add(new SmoothedPoint(points[0].Location, points[0].Pressure, points[0].Velocity));

        // Generate intermediate points using Catmull-Rom spline
        for (int i = 0; i < points.Count - 1; i++)
        {
            // Get 4 control points for Catmull-Rom (P0, P1, P2, P3)
            // For endpoints, use reflection/clamping
            var p0 = (i > 0) ? points[i - 1].Location : points[0].Location;
            var p1 = points[i].Location;
            var p2 = points[i + 1].Location;
            var p3 = (i < points.Count - 2) ? points[i + 2].Location : points[points.Count - 1].Location;

            var pr0 = (i > 0) ? points[i - 1].Pressure : points[0].Pressure;
            var pr1 = points[i].Pressure;
            var pr2 = points[i + 1].Pressure;
            var pr3 = (i < points.Count - 2) ? points[i + 2].Pressure : points[points.Count - 1].Pressure;

            var v0 = (i > 0) ? points[i - 1].Velocity : points[0].Velocity;
            var v1 = points[i].Velocity;
            var v2 = points[i + 1].Velocity;
            var v3 = (i < points.Count - 2) ? points[i + 2].Velocity : points[points.Count - 1].Velocity;

            // Add samples along this segment
            for (int j = 1; j <= Brush.SmoothingFactor; j++)
            {
                float t = j / (float)Brush.SmoothingFactor;
                
                // Catmull-Rom spline formula
                var point = CatmullRom(p0, p1, p2, p3, t);
                var pressure = CatmullRomScalar(pr0, pr1, pr2, pr3, t);
                var velocity = CatmullRomScalar(v0, v1, v2, v3, t);
                // Velocity must be non-negative
                velocity = Math.Max(0f, velocity);
                
                result.Add(new SmoothedPoint(point, pressure, velocity));
            }
        }

        return result;
    }

    /// <summary>
    /// Catmull-Rom spline interpolation for a point.
    /// </summary>
    private static SKPoint CatmullRom(SKPoint p0, SKPoint p1, SKPoint p2, SKPoint p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        // Catmull-Rom matrix coefficients (with tension=0.5)
        float x = 0.5f * (
            (2f * p1.X) +
            (-p0.X + p2.X) * t +
            (2f * p0.X - 5f * p1.X + 4f * p2.X - p3.X) * t2 +
            (-p0.X + 3f * p1.X - 3f * p2.X + p3.X) * t3);

        float y = 0.5f * (
            (2f * p1.Y) +
            (-p0.Y + p2.Y) * t +
            (2f * p0.Y - 5f * p1.Y + 4f * p2.Y - p3.Y) * t2 +
            (-p0.Y + 3f * p1.Y - 3f * p2.Y + p3.Y) * t3);

        return new SKPoint(x, y);
    }

    /// <summary>
    /// Catmull-Rom spline interpolation for a scalar value (pressure).
    /// </summary>
    private static float CatmullRomScalar(float p0, float p1, float p2, float p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float result = 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);

        // Clamp pressure to valid range
        return Math.Max(0f, Math.Min(1f, result));
    }

    /// <summary>
    /// Adds samples along a quadratic Bezier curve for smooth interpolation.
    /// </summary>
    private void AddQuadraticSamples(
        List<SmoothedPoint> result,
        SKPoint p0, float pressure0, float velocity0,
        SKPoint control, float controlPressure, float controlVelocity,
        SKPoint p1, float pressure1, float velocity1)
    {
        // Sample the quadratic curve using the smoothing factor
        int samples = Brush.SmoothingFactor;
        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            float u = 1 - t;

            // Quadratic Bezier formula: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            var x = u * u * p0.X + 2 * u * t * control.X + t * t * p1.X;
            var y = u * u * p0.Y + 2 * u * t * control.Y + t * t * p1.Y;
            var pressure = u * u * pressure0 + 2 * u * t * controlPressure + t * t * pressure1;
            var velocity = u * u * velocity0 + 2 * u * t * controlVelocity + t * t * velocity1;
            // Velocity must be non-negative
            velocity = Math.Max(0f, velocity);

            result.Add(new SmoothedPoint(new SKPoint(x, y), pressure, velocity));
        }
    }

    /// <summary>
    /// Adds a cap at the end of the stroke based on the cap style.
    /// </summary>
    private void AddCap(SKPath path, SKPoint center, SKPoint direction, float radius, bool isStart)
    {
        switch (Brush.CapStyle)
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
    /// Calculates the stroke width based on pressure only.
    /// </summary>
    private float GetStrokeWidth(float pressure)
    {
        return Brush.GetWidthForPressure(pressure);
    }

    /// <summary>
    /// Calculates the stroke width based on pressure and velocity.
    /// </summary>
    private float GetStrokeWidth(float pressure, float velocity)
    {
        return Brush.GetWidthForPressureAndVelocity(pressure, velocity);
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
