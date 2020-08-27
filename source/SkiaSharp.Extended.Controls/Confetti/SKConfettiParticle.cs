using System;

namespace SkiaSharp.Extended.Controls
{
	internal class SKConfettiParticle
	{
		private static readonly SKObjectPool<SKPaint> paintPool =
			new SKObjectPool<SKPaint>(() => new SKPaint { IsAntialias = true });

		private SKPoint acceleration = SKPoint.Empty;
		private float rotationWidth = 0f;
		private float scaleX = 1f;

		private SKPoint location;
		private float size;

		public SKPoint Location
		{
			get => location;
			set
			{
				location = value;
				Bounds = new SKRect(location.X - size, location.Y - size, size * 2, size * 2);
			}
		}

		public float Size
		{
			get => size;
			set
			{
				size = value;
				Bounds = new SKRect(location.X - size, location.Y - size, size * 2, size * 2);
			}
		}

		public float Mass { get; set; }

		public float Rotation { get; set; }

		public SKColorF Color { get; set; }

		public SKConfettiShape? Shape { get; set; }

		public SKPoint Velocity { get; set; }

		public float RotationVelocity { get; set; }

		public SKPoint MaxAcceleration { get; set; }

		public bool Accelerate { get; set; }

		public bool Rotate { get; set; }

		public bool FadeOut { get; set; }

		public double Lifetime { get; set; }

		public SKRect Bounds { get; private set; }

		public bool IsComplete { get; private set; }

		public void Draw(SKCanvas canvas, TimeSpan deltaTime)
		{
			if (IsComplete || Shape == null)
				return;

			canvas.Save();
			canvas.Translate(Location);

			if (Rotate)
			{
				canvas.RotateDegrees(Rotation);
				canvas.Scale(scaleX, 1f);
			}

			var paint = paintPool.Get();
			paint.ColorF = Color;

			Shape.Draw(canvas, paint, Size);

			paintPool.Return(paint);

			canvas.Restore();
		}

		public void ApplyForce(SKPoint force, TimeSpan deltaTime)
		{
			if (IsComplete || !Accelerate)
				return;

			var secs = (float)deltaTime.TotalSeconds;
			force.X = (force.X / Mass) * secs;
			force.Y = (force.Y / Mass) * secs;

			acceleration += force;

			if (MaxAcceleration != SKPoint.Empty)
			{
				acceleration = new SKPoint(
					Math.Min(acceleration.X, MaxAcceleration.X),
					Math.Min(acceleration.Y, MaxAcceleration.Y));
			}

			Velocity += acceleration;

			Location = new SKPoint(
				Location.X + Velocity.X * secs,
				Location.Y + Velocity.Y * secs);

			Lifetime -= deltaTime.TotalSeconds;
			if (Lifetime <= 0)
			{
				if (FadeOut)
				{
					var c = Color;
					var alpha = c.Alpha - secs;
					Color = c.WithAlpha(alpha);
					IsComplete = alpha <= 0;
				}
				else
				{
					IsComplete = true;
				}
			}

			if (Rotate)
			{
				var rv = RotationVelocity * secs;

				Rotation += rv;
				if (Rotation >= 360)
					Rotation = 0f;

				rotationWidth -= rv;
				if (rotationWidth < 0)
					rotationWidth = Size;

				scaleX = Math.Abs(rotationWidth / Size - 0.5f) * 2;
			}
		}
	}
}
