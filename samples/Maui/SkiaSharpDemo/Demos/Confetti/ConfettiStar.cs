using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public class ConfettiStar : SKConfettiShape
{
	public ConfettiStar(int points)
	{
		Points = points;
	}

	public int Points { get; }

	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		using var star = SKGeometry.CreateRegularStarPath(size, size / 2, Points);

		canvas.DrawPath(star, paint);
	}
}
