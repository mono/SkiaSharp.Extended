namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Represents a morph target for vector path interpolation.
/// </summary>
public class MorphTarget
{
	private readonly SKPath targetPath;
	private readonly SKPoint[] sourcePoints;
	private readonly SKPoint[] targetPoints;

	/// <summary>
	/// Creates a new morph target from an SVG path string.
	/// </summary>
	public MorphTarget(string sourceSvg, string targetSvg)
	{
		if (string.IsNullOrEmpty(sourceSvg))
			throw new ArgumentException("Source path cannot be empty", nameof(sourceSvg));
		if (string.IsNullOrEmpty(targetSvg))
			throw new ArgumentException("Target path cannot be empty", nameof(targetSvg));

		using var source = SKPath.ParseSvgPathData(sourceSvg);
		targetPath = SKPath.ParseSvgPathData(targetSvg);

		// Extract points from paths
		sourcePoints = ExtractPoints(source);
		targetPoints = ExtractPoints(targetPath);

		// Align point counts for smooth morphing
		AlignPointCounts(ref sourcePoints, ref targetPoints);
	}

	/// <summary>
	/// Interpolates between source and target paths based on progress.
	/// </summary>
	/// <param name="sourcePath">The source path (unused, kept for API compatibility)</param>
	/// <param name="progress">Progress value from 0.0 to 1.0</param>
	/// <returns>Interpolated path</returns>
	public SKPath Interpolate(SKPath sourcePath, float progress)
	{
		progress = Math.Clamp(progress, 0f, 1f);

		var interpolated = new SKPath();
		if (sourcePoints.Length == 0)
			return interpolated;

		// Interpolate points
		var easedProgress = ApplyEasing(progress, EasingFunction.EaseInOut);

		for (int i = 0; i < sourcePoints.Length; i++)
		{
			var sx = sourcePoints[i].X;
			var sy = sourcePoints[i].Y;
			var tx = targetPoints[i].X;
			var ty = targetPoints[i].Y;

			var x = sx + (tx - sx) * easedProgress;
			var y = sy + (ty - sy) * easedProgress;

			if (i == 0)
				interpolated.MoveTo(x, y);
			else
				interpolated.LineTo(x, y);
		}

		interpolated.Close();
		return interpolated;
	}

	private static SKPoint[] ExtractPoints(SKPath path)
	{
		var points = new List<SKPoint>();
		using var iter = path.CreateIterator(false);
		var points2 = new SKPoint[4];

		while (iter.Next(points2) != SKPathVerb.Done)
		{
			// Add the first point of each verb
			points.Add(points2[0]);
		}

		return points.ToArray();
	}

	private static void AlignPointCounts(ref SKPoint[] source, ref SKPoint[] target)
	{
		if (source.Length == target.Length)
			return;

		var maxCount = Math.Max(source.Length, target.Length);

		if (source.Length < maxCount)
			source = InterpolatePoints(source, maxCount);

		if (target.Length < maxCount)
			target = InterpolatePoints(target, maxCount);
	}

	private static SKPoint[] InterpolatePoints(SKPoint[] points, int targetCount)
	{
		if (points.Length == 0)
			return new SKPoint[targetCount];

		var result = new List<SKPoint>();
		var ratio = (float)(points.Length - 1) / (targetCount - 1);

		for (int i = 0; i < targetCount; i++)
		{
			var index = i * ratio;
			var lowerIndex = (int)Math.Floor(index);
			var upperIndex = Math.Min(lowerIndex + 1, points.Length - 1);
			var t = index - lowerIndex;

			var p1 = points[lowerIndex];
			var p2 = points[upperIndex];

			result.Add(new SKPoint(
				p1.X + (p2.X - p1.X) * t,
				p1.Y + (p2.Y - p1.Y) * t));
		}

		return result.ToArray();
	}

	private static float ApplyEasing(float t, EasingFunction easing)
	{
		return easing switch
		{
			EasingFunction.Linear => t,
			EasingFunction.EaseIn => t * t,
			EasingFunction.EaseOut => t * (2 - t),
			EasingFunction.EaseInOut => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t,
			_ => t
		};
	}
}

/// <summary>
/// Easing functions for morph interpolation.
/// </summary>
public enum EasingFunction
{
	Linear,
	EaseIn,
	EaseOut,
	EaseInOut
}
