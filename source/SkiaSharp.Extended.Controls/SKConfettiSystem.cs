using System;
using System.Collections.Generic;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiSystem : BindableObject
	{
		private readonly Random random = new Random();
		private readonly List<SKConfettiParticle> particles = new List<SKConfettiParticle>();

		private SKConfettiEmitter? emitter;
		private Size lastViewSize;
		private SKConfettiSystemBounds actualEmitterBounds;

		public SKConfettiSystem()
		{
			Emitter = SKConfettiEmitter.Infinite(200);
		}

		public bool IsRunning { get; set; } = true;

		public SKConfettiSystemBounds EmitterBounds { get; set; } = SKConfettiSystemBounds.Top;

		public SKConfettiEmitter? Emitter
		{
			get => emitter;
			set
			{
				if (emitter == value)
					return;

				if (emitter != null)
					emitter.ParticlesCreated -= OnCreateParticle;

				emitter = value;

				if (emitter != null)
					emitter.ParticlesCreated += OnCreateParticle;
			}
		}

		public SKConfettiColorCollection Colors { get; set; } = new SKConfettiColorCollection {
			Color.FromUint(0xfffce18a),
			Color.FromUint(0xffff726d),
			Color.FromUint(0xffb48def),
			Color.FromUint(0xfff4306d),
			Color.FromUint(0xff3aaab8),
			Color.FromUint(0xff38ba9e),
			Color.FromUint(0xffbb3d72),
			Color.FromUint(0xff006ded),
		};

		public SKConfettiPhysicsCollection Physics { get; set; } = new SKConfettiPhysicsCollection {
			new SKConfettiPhysics(12, 5),
			new SKConfettiPhysics(16, 6),
		};

		public SKConfettiShapeCollection Shapes { get; set; } = new SKConfettiShapeCollection {
			SKConfettiShape.Square,
			SKConfettiShape.Circle,
			SKConfettiShape.CreateRect(0.5f),
		};

		public double StartAngle { get; set; } = 0;

		public double EndAngle { get; set; } = 360;

		public double MinimumInitialVelocity { get; set; } = 10;

		public double MaximumInitialVelocity { get; set; } = 250;

		public double MinimumRotationVelocity { get; set; } = 10;

		public double MaximumRotationVelocity { get; set; } = 50;

		public double MaximumAcceleration { get; set; } = 0;

		public double Lifetime { get; set; } = 3;

		public bool Rotate { get; set; } = true;

		public bool Accelerate { get; set; } = true;

		public bool FadeOut { get; set; } = true;

		public bool IsComplete =>
			particles.Count == 0 &&
			(Emitter?.IsComplete != false || !IsRunning);

		public Point Gravity { get; set; } = new Point(0, 0.98f);

		public void Draw(SKCanvas canvas, TimeSpan deltaTime)
		{
			if (IsRunning)
				Emitter?.Update(deltaTime);

			var g = Gravity.ToSKPoint();

			for (var i = particles.Count - 1; i >= 0; i--)
			{
				var particle = particles[i];

				particle.ApplyForce(g);

				particle.Draw(canvas, deltaTime);

				if (particle.IsComplete)
					particles.RemoveAt(i);
			}
		}

		public void UpdateEmitterBounds(double width, double height)
		{
			lastViewSize = new Size(width, height);

			var rect = EmitterBounds.Side switch
			{
				SKConfettiSystemSide.Top => new Rect(-50, -50, width + 100, 0),
				SKConfettiSystemSide.Left => new Rect(-50, -50, 0, height + 100),
				SKConfettiSystemSide.Right => new Rect(width + 50, -50, 0, height + 100),
				SKConfettiSystemSide.Bottom => new Rect(-50, height + 50, width + 100, 0),
				SKConfettiSystemSide.Center => new Rect(width / 2, height / 2, 0, 0),
				_ => EmitterBounds.Rect,
			};

			actualEmitterBounds = new SKConfettiSystemBounds(rect, EmitterBounds.Side);
		}

		private void OnCreateParticle(int count)
		{
			for (var i = 0; i < count; i++)
			{
				var particle = new SKConfettiParticle
				{
					Location = GetNewLocation().ToSKPoint(),
					Velocity = GetNewVelocity().ToSKPoint(),
					RotationVelocity = (float)GetNewRotation(),

					Color = Colors[random.Next(Colors.Count)].ToSKColor(),
					Physics = Physics[random.Next(Physics.Count)],
					Shape = Shapes[random.Next(Shapes.Count)],

					MaxAcceleration = new Point(MaximumAcceleration, MaximumAcceleration).ToSKPoint(),
					Accelerate = Accelerate,
					Rotate = Rotate,
					FadeOut = FadeOut,
					Lifetime = Lifetime,
				};

				particles.Add(particle);
			}
		}

		private Point GetNewLocation()
		{
			var rect = actualEmitterBounds.Rect;
			return new Point(
				rect.Left + random.NextDouble() * rect.Width,
				rect.Top + random.NextDouble() * rect.Height);
		}

		private Point GetNewVelocity()
		{
			var velocity = MinimumInitialVelocity + random.NextDouble() * (MaximumInitialVelocity - MinimumInitialVelocity);
			var deg = StartAngle + random.NextDouble() * (EndAngle - StartAngle);
			var rad = Math.PI / 180.0 * deg;

			var vx = velocity * Math.Cos(rad);
			var vy = velocity * Math.Sin(rad);

			return new Point(vx, vy);
		}

		private double GetNewRotation() =>
			MinimumRotationVelocity + random.NextDouble() * (MaximumRotationVelocity - MinimumRotationVelocity);
	}
}
