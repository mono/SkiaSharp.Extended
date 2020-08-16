using System;

namespace SkiaSharp.Extended.Controls
{
	public abstract class SKConfettiShape
	{
		public abstract void Draw(SKCanvas canvas, SKPaint paint, float size);

		public static SKConfettiShape Square =>
			CreateRect(1f);

		public static SKConfettiShape Circle =>
			CreateOval(1f);

		public static SKConfettiShape CreateRect(float heightRatio) =>
			new SKConfettiRect(heightRatio);

		public static SKConfettiShape CreateOval(float heightRatio) =>
			new SKConfettiOval(heightRatio);
	}

	internal class SKConfettiRect : SKConfettiShape
	{
		private readonly float heightRatio;

		public SKConfettiRect(float heightRatio)
		{
			if (heightRatio < 0 || heightRatio > 1)
				throw new ArgumentOutOfRangeException(nameof(heightRatio), "The height ratio must be in the range of [0, 1].");

			this.heightRatio = heightRatio;
		}

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			var height = size * heightRatio;
			var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
			canvas.DrawRect(rect, paint);
		}
	}

	internal class SKConfettiOval : SKConfettiShape
	{
		private readonly float heightRatio;

		public SKConfettiOval(float heightRatio)
		{
			if (heightRatio < 0 || heightRatio > 1)
				throw new ArgumentOutOfRangeException(nameof(heightRatio), "The height ratio must be in the range of [0, 1].");

			this.heightRatio = heightRatio;
		}

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			var height = size * heightRatio;
			var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
			canvas.DrawOval(rect, paint);
		}
	}
}
