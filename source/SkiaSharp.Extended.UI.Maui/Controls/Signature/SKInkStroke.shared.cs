namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Represents a single ink stroke with pressure-sensitive variable-width rendering.
/// Uses quadratic Bezier curves for smooth path interpolation and renders
/// variable-width strokes as filled polygons based on pressure data.
/// </summary>
public class SKInkStroke : IDisposable
{
	// Implementation adapted from @colinta on StackOverflow: https://stackoverflow.com/a/35229104
	// Enhanced with pressure sensitivity for fluid ink rendering

	private const float MinimumPointDistance = 2.0f;
	private const float DefaultPressure = 0.5f;

	private readonly List<SKPoint> points = new();
	private readonly List<float> pressures = new();
	private readonly float minStrokeWidth;
	private readonly float maxStrokeWidth;

	private SKPath? cachedPath;
	private bool isDirty = true;

	/// <summary>
	/// Creates a new ink stroke with the specified stroke width range.
	/// </summary>
	/// <param name="minStrokeWidth">Minimum stroke width (at zero pressure).</param>
	/// <param name="maxStrokeWidth">Maximum stroke width (at full pressure).</param>
	public SKInkStroke(float minStrokeWidth = 1f, float maxStrokeWidth = 8f)
	{
		this.minStrokeWidth = minStrokeWidth;
		this.maxStrokeWidth = maxStrokeWidth;
	}

	/// <summary>
	/// Gets the list of points in this stroke.
	/// </summary>
	public IReadOnlyList<SKPoint> Points => points;

	/// <summary>
	/// Gets the list of pressure values for each point.
	/// </summary>
	public IReadOnlyList<float> Pressures => pressures;

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
	/// Adds a point to the stroke with pressure information.
	/// </summary>
	/// <param name="point">The point location.</param>
	/// <param name="pressure">The pressure value (0.0 to 1.0).</param>
	/// <param name="isLastPoint">Whether this is the final point in the stroke.</param>
	public void AddPoint(SKPoint point, float pressure, bool isLastPoint = false)
	{
		// Clamp pressure to valid range
		pressure = Math.Clamp(pressure, 0f, 1f);

		// Use default pressure if no pressure data is available (finger touch)
		if (pressure == 0f && !isLastPoint)
			pressure = DefaultPressure;

		// Do not add a point if the current point is too close to the previous point
		if (!isLastPoint && points.Count > 0 && !HasMovedFarEnough(points[^1], point))
			return;

		points.Add(point);
		pressures.Add(pressure);
		isDirty = true;
	}

	/// <summary>
	/// Clears all points from the stroke.
	/// </summary>
	public void Clear()
	{
		points.Clear();
		pressures.Clear();
		cachedPath?.Dispose();
		cachedPath = null;
		isDirty = true;
	}

	/// <summary>
	/// Disposes of the cached path resources.
	/// </summary>
	public void Dispose()
	{
		cachedPath?.Dispose();
		cachedPath = null;
	}

	private bool HasMovedFarEnough(SKPoint prevPoint, SKPoint currPoint)
	{
		var deltaX = currPoint.X - prevPoint.X;
		var deltaY = currPoint.Y - prevPoint.Y;
		var distance = MathF.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
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
				var radius = GetStrokeWidth(pressures[0]) / 2f;
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
				tangent = Normalize(smoothedPoints[i + 1].Point - point);
			}
			else if (i == smoothedPoints.Count - 1)
			{
				// Last point: use direction from previous point
				tangent = Normalize(point - smoothedPoints[i - 1].Point);
			}
			else
			{
				// Middle point: average direction
				var prev = smoothedPoints[i - 1].Point;
				var next = smoothedPoints[i + 1].Point;
				tangent = Normalize(next - prev);
			}

			// Normal is perpendicular to tangent
			var normal = new SKPoint(-tangent.Y, tangent.X);

			// Offset points on both sides
			leftPoints.Add(new SKPoint(point.X + normal.X * halfWidth, point.Y + normal.Y * halfWidth));
			rightPoints.Add(new SKPoint(point.X - normal.X * halfWidth, point.Y - normal.Y * halfWidth));
		}

		// Build the filled polygon path
		var path = new SKPath();

		// Start with the left side (forward direction)
		path.MoveTo(leftPoints[0]);
		for (int i = 1; i < leftPoints.Count; i++)
		{
			path.LineTo(leftPoints[i]);
		}

		// Add rounded end cap at the end
		var endRadius = GetStrokeWidth(smoothedPoints[^1].Pressure) / 2f;
		var endCenter = smoothedPoints[^1].Point;
		var endTangent = Normalize(smoothedPoints[^1].Point - smoothedPoints[^2].Point);
		AddRoundedCap(path, endCenter, endTangent, endRadius);

		// Continue with the right side (reverse direction)
		for (int i = rightPoints.Count - 1; i >= 0; i--)
		{
			path.LineTo(rightPoints[i]);
		}

		// Add rounded start cap (direction is reversed since we're drawing back to start)
		var startRadius = GetStrokeWidth(smoothedPoints[0].Pressure) / 2f;
		var startCenter = smoothedPoints[0].Point;
		var startTangent = Normalize(smoothedPoints[0].Point - smoothedPoints[1].Point);
		AddRoundedCap(path, startCenter, startTangent, startRadius);

		path.Close();

		return path;
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
			result.Add((points[0], pressures[0]));
			return result;
		}

		// Add the first point
		result.Add((points[0], pressures[0]));

		// Generate intermediate points using quadratic Bezier interpolation
		for (int i = 0; i < points.Count - 1; i++)
		{
			var p0 = points[i];
			var p1 = points[i + 1];
			var pressure0 = pressures[i];
			var pressure1 = pressures[i + 1];

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
				// Middle segments: add quadratic curve samples
				var prevMid = result[^1];
				AddQuadraticSamples(result, prevMid.Point, prevMid.Pressure, p0, pressure0, midPoint, midPressure);
			}
		}

		// Add the last point
		result.Add((points[^1], pressures[^1]));

		return result;
	}

	/// <summary>
	/// Adds samples along a quadratic Bezier curve for smooth interpolation.
	/// </summary>
	private static void AddQuadraticSamples(
		List<(SKPoint Point, float Pressure)> result,
		SKPoint p0, float pressure0,
		SKPoint control, float controlPressure,
		SKPoint p1, float pressure1)
	{
		// Sample the quadratic curve at a few points for smooth rendering
		const int samples = 4;
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
	/// Adds a rounded cap at the end of the stroke.
	/// </summary>
	private static void AddRoundedCap(SKPath path, SKPoint center, SKPoint direction, float radius)
	{
		// Add a semicircle cap
		var perpendicular = new SKPoint(-direction.Y, direction.X);
		var startAngle = MathF.Atan2(perpendicular.Y, perpendicular.X) * 180f / MathF.PI;

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
	/// Normalizes a vector to unit length.
	/// </summary>
	private static SKPoint Normalize(SKPoint vector)
	{
		var length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
		if (length < 0.0001f)
			return new SKPoint(1, 0); // Default direction for zero-length vectors

		return new SKPoint(vector.X / length, vector.Y / length);
	}
}
