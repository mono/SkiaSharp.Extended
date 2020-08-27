using System;
using System.Collections.Generic;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiSystem : BindableObject
	{
		public static readonly BindableProperty EmitterBoundsProperty = BindableProperty.Create(
			nameof(EmitterBounds),
			typeof(SKConfettiSystemBounds),
			typeof(SKConfettiSystem),
			SKConfettiSystemBounds.Top);

		public static readonly BindableProperty EmitterProperty = BindableProperty.Create(
			nameof(Emitter),
			typeof(SKConfettiEmitter),
			typeof(SKConfettiSystem),
			null,
			propertyChanged: OnEmitterChanged,
			defaultValueCreator: _ => new SKConfettiEmitter());

		public static readonly BindableProperty ColorsProperty = BindableProperty.Create(
			nameof(Colors),
			typeof(SKConfettiColorCollection),
			typeof(SKConfettiSystem),
			null,
			defaultValueCreator: _ => CreateDefaultColors());

		public static readonly BindableProperty PhysicsProperty = BindableProperty.Create(
			nameof(Physics),
			typeof(SKConfettiPhysicsCollection),
			typeof(SKConfettiSystem),
			null,
			defaultValueCreator: _ => CreateDefaultPhysics());

		public static readonly BindableProperty ShapesProperty = BindableProperty.Create(
			nameof(Shapes),
			typeof(SKConfettiShapeCollection),
			typeof(SKConfettiSystem),
			null,
			defaultValueCreator: _ => CreateDefaultShapes());

		private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
			nameof(IsComplete),
			typeof(bool),
			typeof(SKConfettiSystem),
			false);

		public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
			nameof(IsRunning),
			typeof(bool),
			typeof(SKConfettiSystem),
			true,
			propertyChanged: OnIsRunningPropertyChanged);

		private readonly Random random = new Random();
		private readonly List<SKConfettiParticle> particles = new List<SKConfettiParticle>();

		private SKRect lastViewBounds;
		private SKConfettiSystemBounds actualEmitterBounds;

		public SKConfettiSystem()
		{
			DebugUtils.LogPropertyChanged(this);

			OnEmitterChanged(this, null, Emitter);
		}

		public bool IsRunning
		{
			get => (bool)GetValue(IsRunningProperty);
			set => SetValue(IsRunningProperty, value);
		}

		public SKConfettiSystemBounds EmitterBounds
		{
			get => (SKConfettiSystemBounds)GetValue(EmitterBoundsProperty);
			set => SetValue(EmitterBoundsProperty, value);
		}

		public SKConfettiEmitter? Emitter
		{
			get => (SKConfettiEmitter?)GetValue(EmitterProperty);
			set => SetValue(EmitterProperty, value);
		}

		public SKConfettiColorCollection? Colors
		{
			get => (SKConfettiColorCollection?)GetValue(ColorsProperty);
			set => SetValue(ColorsProperty, value);
		}

		public SKConfettiPhysicsCollection? Physics
		{
			get => (SKConfettiPhysicsCollection?)GetValue(PhysicsProperty);
			set => SetValue(PhysicsProperty, value);
		}

		public SKConfettiShapeCollection? Shapes
		{
			get => (SKConfettiShapeCollection?)GetValue(ShapesProperty);
			set => SetValue(ShapesProperty, value);
		}

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

		public Point Gravity { get; set; } = new Point(0, 9.81f);

		public bool IsComplete
		{
			get => (bool)GetValue(IsCompleteProperty);
			private set => SetValue(IsCompletePropertyKey, value);
		}

		internal int ParticleCount => particles.Count;

		public void Draw(SKCanvas canvas, TimeSpan deltaTime)
		{
			if (IsRunning)
				Emitter?.Update(deltaTime);

			var g = Gravity.ToSKPoint();

			var removed = false;
			for (var i = particles.Count - 1; i >= 0; i--)
			{
				var particle = particles[i];

				particle.ApplyForce(g, deltaTime);

				if (!particle.IsComplete && lastViewBounds.IntersectsWith(particle.Bounds))
				{
					particle.Draw(canvas, deltaTime);
				}
				else
				{
					particles.RemoveAt(i);
					removed = true;
				}
			}

			if (removed)
				UpdateIsComplete();
		}

		public void UpdateEmitterBounds(double width, double height)
		{
			lastViewBounds = new SKRect(0, 0, (float)width, (float)height);

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
			if (Colors == null || Colors.Count == 0 ||
				Physics == null || Physics.Count == 0 ||
				Shapes == null || Shapes.Count == 0)
				return;

			for (var i = 0; i < count; i++)
			{
				var c = Colors[random.Next(Colors.Count)];
				var p = Physics[random.Next(Physics.Count)];
				var s = Shapes[random.Next(Shapes.Count)];

				var particle = new SKConfettiParticle
				{
					Location = GetNewLocation().ToSKPoint(),
					Velocity = GetNewVelocity().ToSKPoint(),
					RotationVelocity = (float)GetNewRotationVelocity(),

					Color = c.ToSKColor(),
					Size = p.Size,
					Mass = p.Mass,
					Shape = s,
					Rotation = (float)GetNewRotation(),

					MaxAcceleration = new Point(MaximumAcceleration, MaximumAcceleration).ToSKPoint(),
					Accelerate = Accelerate,
					Rotate = Rotate,
					FadeOut = FadeOut,
					Lifetime = Lifetime,
				};

				particles.Add(particle);
			}

			UpdateIsComplete();

			Point GetNewLocation()
			{
				var rect = actualEmitterBounds.Rect;
				return new Point(
					rect.Left + random.NextDouble() * rect.Width,
					rect.Top + random.NextDouble() * rect.Height);
			}

			Point GetNewVelocity()
			{
				var velocity = MinimumInitialVelocity + random.NextDouble() * (MaximumInitialVelocity - MinimumInitialVelocity);
				var deg = StartAngle + random.NextDouble() * (EndAngle - StartAngle);
				var rad = Math.PI / 180.0 * deg;

				var vx = velocity * Math.Cos(rad);
				var vy = velocity * Math.Sin(rad);

				return new Point(vx, vy);
			}

			double GetNewRotationVelocity() =>
				MinimumRotationVelocity + random.NextDouble() * (MaximumRotationVelocity - MinimumRotationVelocity);

			double GetNewRotation() =>
				random.NextDouble() * 360.0;

		}

		private static void OnEmitterChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			if (bindable is SKConfettiSystem system)
			{
				if (oldValue is SKConfettiEmitter oldE)
					oldE.ParticlesCreated -= system.OnCreateParticle;

				if (newValue is SKConfettiEmitter newE)
					newE.ParticlesCreated += system.OnCreateParticle;

				system.UpdateIsComplete();
			}
		}

		private static void OnIsRunningPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKConfettiSystem system)
			{
				system.UpdateIsComplete();
			}
		}

		private bool UpdateIsComplete() =>
			IsComplete =
				particles.Count == 0 &&
				(Emitter?.IsComplete != false || !IsRunning);

		private static SKConfettiColorCollection CreateDefaultColors() =>
			new SKConfettiColorCollection
			{
				Color.FromUint(0xfffce18a),
				Color.FromUint(0xffff726d),
				Color.FromUint(0xffb48def),
				Color.FromUint(0xfff4306d),
				Color.FromUint(0xff3aaab8),
				Color.FromUint(0xff38ba9e),
				Color.FromUint(0xffbb3d72),
				Color.FromUint(0xff006ded),
			};

		private static SKConfettiPhysicsCollection CreateDefaultPhysics() =>
			new SKConfettiPhysicsCollection
			{
				new SKConfettiPhysics(12, 5),
				new SKConfettiPhysics(16, 6),
			};

		private static SKConfettiShapeCollection CreateDefaultShapes() =>
			new SKConfettiShapeCollection
			{
				new SKConfettiSquare(),
				new SKConfettiCircle(),
				new SKConfettiRect(0.5),
				new SKConfettiOval(0.5),
				new SKConfettiRect(0.1),
			};
	}
}
