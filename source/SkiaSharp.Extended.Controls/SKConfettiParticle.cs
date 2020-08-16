using System;

namespace SkiaSharp.Extended.Controls
{
	internal class SKConfettiParticle
	{
		private readonly SKPaint paint = new SKPaint { IsAntialias = true };

		private SKPoint acceleration = SKPoint.Empty;
		private float rotation = 0f;
		private float rotationWidth = 0f;

		public SKPoint Location { get; set; }

		public SKColor Color
		{
			get => paint.Color;
			set => paint.Color = value;
		}

		public SKConfettiPhysics? Physics { get; set; }

		public SKConfettiShape? Shape { get; set; }

		public SKPoint Velocity { get; set; }

		public float RotationVelocity { get; set; }

		public SKPoint MaxAcceleration { get; set; }

		public bool Accelerate { get; set; }

		public bool Rotate { get; set; }

		public bool FadeOut { get; set; }

		public int Lifetime { get; set; }


		public bool IsComplete { get; private set; } = false;


		public void Draw(SKCanvas canvas, TimeSpan deltaTime)
		{
			if (IsComplete || Physics == null || Shape == null)
				return;

			var ms = (float)deltaTime.TotalMilliseconds;
			Location = new SKPoint(
				Location.X + Velocity.X * ms,
				Location.Y + Velocity.Y * ms);

			Lifetime -= (int)deltaTime.TotalMilliseconds;
			if (Lifetime <= 0)
			{
				if (FadeOut)
				{
					var c = Color;
					float alpha = c.Alpha - (0.255f * ms);

					IsComplete = alpha <= 0;
					if (!IsComplete)
						Color = c.WithAlpha((byte)alpha);
				}
				else
				{
					IsComplete = true;
				}
			}

			if (IsComplete)
				return;

			canvas.Save();
			canvas.Translate(Location);

			if (Rotate)
			{
				var rv = RotationVelocity * ms;

				rotation += rv;
				if (rotation >= 360)
					rotation = 0f;

				rotationWidth -= rv;
				if (rotationWidth < 0)
					rotationWidth = Physics.Size;

				var scaleX = Math.Abs(rotationWidth / Physics.Size - 0.5f) * 2;

				canvas.RotateDegrees(rotation);
				canvas.Scale(scaleX, 1f);
			}

			Shape.Draw(canvas, paint, Physics.Size);

			canvas.Restore();
		}

		public void ApplyForce(SKPoint force)
		{
			if (IsComplete || Physics == null || !Accelerate)
				return;

			force.X /= Physics.Mass;
			force.Y /= Physics.Mass;

			acceleration += force;

			if (MaxAcceleration != SKPoint.Empty)
			{
				acceleration = new SKPoint(
					Math.Min(acceleration.X, MaxAcceleration.X),
					Math.Min(acceleration.Y, MaxAcceleration.Y));
			}

			Velocity += acceleration;
		}
	}
}
