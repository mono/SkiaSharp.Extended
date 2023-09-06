namespace SkiaSharp.Extended.UI.Controls;

internal class SKInkPath
{
	// Implementation adapted from @colinta on StackOverflow: https://stackoverflow.com/a/35229104

	const float MinimumPointDistance = 2.0f;

	LinkedList<SKPoint> currentInkPathPoints = new();

	SKPath currentInkPath = new();

	bool isFirst = true;

	SKPoint? lastTouchPoint;

	public SKPath Path => currentInkPath;

	public void AddPoint(SKPoint point, bool isLastPoint)
	{
		// do not add a point if the current point is too close to the previous point
		if (!isLastPoint &&
			lastTouchPoint is SKPoint prev &&
			!HasMovedFarEnough(prev, point))
			return;

		currentInkPathPoints.AddLast(point);

		if (lastTouchPoint is not SKPoint prevPoint)
		{
			// 1. move to the first touch location

			currentInkPath.MoveTo(point);
		}
		else
		{
			// 2. for every other touch location

			// calculate the midpoint
			var midPoint = new SKPoint(
				(point.X + prevPoint.X) / 2f,
				(point.Y + prevPoint.Y) / 2f);

			if (isFirst)
			{
				// 2a) if this is the first part of the curve, start with a line to the midpoint
				currentInkPath.LineTo(midPoint);
				isFirst = false;
			}
			else
			{
				// 2b) if this is an additional point,
				//     - add a quad curve that terminates at the midPoint
				//     - use the prevPoint as the control point
				currentInkPath.QuadTo(prevPoint, midPoint);
			}

			if (isLastPoint)
			{
				// 3. finish up the path to the final touch location
				currentInkPath.LineTo(prevPoint);
			}
		}

		lastTouchPoint = point;
	}

	bool HasMovedFarEnough(SKPoint prevPoint, SKPoint currPoint)
	{
		var deltaX = currPoint.X - prevPoint.X;
		var deltaY = currPoint.Y - prevPoint.Y;
		var distance = MathF.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
		return distance >= MinimumPointDistance;
	}

	static SKPath? GenerateSmoothInkPath(LinkedList<SKPoint> points)
	{
		if (points is null || points.Count <= 1)
			return null;

		// 1. move to the first touch location
		var prevPoint = points.First!.Value;
		var smoothPath = new SKPath();
		smoothPath.MoveTo(prevPoint);

		// 2. for every touch location
		var isFirst = true;
		foreach (var currPoint in points.Skip(1))
		{
			// calculate the midpoint
			var midPoint = new SKPoint(
				(currPoint.X + prevPoint.X) / 2f,
				(currPoint.Y + prevPoint.Y) / 2f);

			if (isFirst)
			{
				// 2a) if this is the part of the curve, start with a line to the midpoint
				smoothPath.LineTo(midPoint);
			}
			else
			{
				// 2b) if this is an additional point,
				//     - add a quad curve that terminates at the midPoint
				//     - use the prevPoint as the control point
				smoothPath.QuadTo(prevPoint, midPoint);
			}
			isFirst = false;

			prevPoint = currPoint;
		}

		// 3. finish up the path to the final touch location
		smoothPath.LineTo(prevPoint);

		return smoothPath;
	}
}
