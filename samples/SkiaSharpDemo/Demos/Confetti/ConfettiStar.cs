using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Extended.Controls;

namespace SkiaSharpDemo.Demos
{
	public class ConfettiStar : SKConfettiShape
	{
		public ConfettiStar(int points)
		{
			Points = points;
		}

		public int Points { get; }

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			using var star = SKGeometry.CreateRegularStarPath(size, size / 2, Points);

			canvas.DrawPath(star, paint);
		}
	}
}
