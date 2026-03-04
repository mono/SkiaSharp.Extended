namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Represents a single confetti particle system with emitter, physics, and rendering configuration.
/// </summary>
public class SKConfettiSystem : BindableObject
{
	/// <summary>
	/// Identifies the <see cref="EmitterBounds"/> bindable property.
	/// </summary>
	public static readonly BindableProperty EmitterBoundsProperty = BindableProperty.Create(
		nameof(EmitterBounds),
		typeof(SKConfettiEmitterBounds),
		typeof(SKConfettiSystem),
		SKConfettiEmitterBounds.Top);

	/// <summary>
	/// Identifies the <see cref="Emitter"/> bindable property.
	/// </summary>
	public static readonly BindableProperty EmitterProperty = BindableProperty.Create(
		nameof(Emitter),
		typeof(SKConfettiEmitter),
		typeof(SKConfettiSystem),
		null,
		propertyChanged: OnEmitterChanged,
		defaultValueCreator: _ => new SKConfettiEmitter());

	/// <summary>
	/// Identifies the <see cref="Colors"/> bindable property.
	/// </summary>
	public static readonly BindableProperty ColorsProperty = BindableProperty.Create(
		nameof(Colors),
		typeof(SKConfettiColorCollection),
		typeof(SKConfettiSystem),
		null,
		defaultValueCreator: _ => CreateDefaultColors());

	/// <summary>
	/// Identifies the <see cref="Physics"/> bindable property.
	/// </summary>
	public static readonly BindableProperty PhysicsProperty = BindableProperty.Create(
		nameof(Physics),
		typeof(SKConfettiPhysicsCollection),
		typeof(SKConfettiSystem),
		null,
		defaultValueCreator: _ => CreateDefaultPhysics());

	/// <summary>
	/// Identifies the <see cref="Shapes"/> bindable property.
	/// </summary>
	public static readonly BindableProperty ShapesProperty = BindableProperty.Create(
		nameof(Shapes),
		typeof(SKConfettiShapeCollection),
		typeof(SKConfettiSystem),
		null,
		defaultValueCreator: _ => CreateDefaultShapes());

	/// <summary>
	/// Identifies the <see cref="StartAngle"/> bindable property.
	/// </summary>
	public static readonly BindableProperty StartAngleProperty = BindableProperty.Create(
		nameof(StartAngle),
		typeof(double),
		typeof(SKConfettiSystem),
		0.0);

	/// <summary>
	/// Identifies the <see cref="EndAngle"/> bindable property.
	/// </summary>
	public static readonly BindableProperty EndAngleProperty = BindableProperty.Create(
		nameof(EndAngle),
		typeof(double),
		typeof(SKConfettiSystem),
		360.0);

	/// <summary>
	/// Identifies the <see cref="MinimumInitialVelocity"/> bindable property.
	/// </summary>
	public static readonly BindableProperty MinimumInitialVelocityProperty = BindableProperty.Create(
		nameof(MinimumInitialVelocity),
		typeof(double),
		typeof(SKConfettiSystem),
		100.0);

	/// <summary>
	/// Identifies the <see cref="MaximumInitialVelocity"/> bindable property.
	/// </summary>
	public static readonly BindableProperty MaximumInitialVelocityProperty = BindableProperty.Create(
		nameof(MaximumInitialVelocity),
		typeof(double),
		typeof(SKConfettiSystem),
		200.0);

	/// <summary>
	/// Identifies the <see cref="MinimumRotationVelocity"/> bindable property.
	/// </summary>
	public static readonly BindableProperty MinimumRotationVelocityProperty = BindableProperty.Create(
		nameof(MinimumRotationVelocity),
		typeof(double),
		typeof(SKConfettiSystem),
		10.0);

	/// <summary>
	/// Identifies the <see cref="MaximumRotationVelocity"/> bindable property.
	/// </summary>
	public static readonly BindableProperty MaximumRotationVelocityProperty = BindableProperty.Create(
		nameof(MaximumRotationVelocity),
		typeof(double),
		typeof(SKConfettiSystem),
		75.0);

	/// <summary>
	/// Identifies the <see cref="MaximumVelocity"/> bindable property.
	/// </summary>
	public static readonly BindableProperty MaximumVelocityProperty = BindableProperty.Create(
		nameof(MaximumVelocity),
		typeof(double),
		typeof(SKConfettiSystem),
		0.0);

	/// <summary>
	/// Identifies the <see cref="Lifetime"/> bindable property.
	/// </summary>
	public static readonly BindableProperty LifetimeProperty = BindableProperty.Create(
		nameof(Lifetime),
		typeof(double),
		typeof(SKConfettiSystem),
		2.0);

	/// <summary>
	/// Identifies the <see cref="FadeOut"/> bindable property.
	/// </summary>
	public static readonly BindableProperty FadeOutProperty = BindableProperty.Create(
		nameof(FadeOut),
		typeof(bool),
		typeof(SKConfettiSystem),
		true);

	/// <summary>
	/// Identifies the <see cref="Gravity"/> bindable property.
	/// </summary>
	public static readonly BindableProperty GravityProperty = BindableProperty.Create(
		nameof(Gravity),
		typeof(Point),
		typeof(SKConfettiSystem),
		new Point(0, 9.81f));

	private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsComplete),
		typeof(bool),
		typeof(SKConfettiSystem),
		false,
		defaultBindingMode: BindingMode.OneWayToSource);

	/// <summary>
	/// Identifies the <see cref="IsComplete"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

	/// <summary>
	/// Identifies the <see cref="IsAnimationEnabled"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsAnimationEnabledProperty = BindableProperty.Create(
		nameof(IsAnimationEnabled),
		typeof(bool),
		typeof(SKConfettiSystem),
		true,
		propertyChanged: OnIsAnimationEnabledPropertyChanged);

	private readonly Random random = new Random();
	private readonly List<SKConfettiParticle> particles = new List<SKConfettiParticle>();

	private SKRect lastViewBounds;
	private Rect actualEmitterBounds;

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiSystem"/> class.
	/// </summary>
	public SKConfettiSystem()
	{
		DebugUtils.LogPropertyChanged(this);

		OnEmitterChanged(this, null, Emitter);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the animation is enabled.
	/// </summary>
	public bool IsAnimationEnabled
	{
		get => (bool)GetValue(IsAnimationEnabledProperty);
		set => SetValue(IsAnimationEnabledProperty, value);
	}

	/// <summary>
	/// Gets or sets the bounds from which particles are emitted.
	/// </summary>
	public SKConfettiEmitterBounds EmitterBounds
	{
		get => (SKConfettiEmitterBounds)GetValue(EmitterBoundsProperty);
		set => SetValue(EmitterBoundsProperty, value);
	}

	/// <summary>
	/// Gets or sets the particle emitter.
	/// </summary>
	public SKConfettiEmitter? Emitter
	{
		get => (SKConfettiEmitter?)GetValue(EmitterProperty);
		set => SetValue(EmitterProperty, value);
	}

	/// <summary>
	/// Gets or sets the collection of colors used for particles.
	/// </summary>
	public SKConfettiColorCollection? Colors
	{
		get => (SKConfettiColorCollection?)GetValue(ColorsProperty);
		set => SetValue(ColorsProperty, value);
	}

	/// <summary>
	/// Gets or sets the collection of physics configurations for particles.
	/// </summary>
	public SKConfettiPhysicsCollection? Physics
	{
		get => (SKConfettiPhysicsCollection?)GetValue(PhysicsProperty);
		set => SetValue(PhysicsProperty, value);
	}

	/// <summary>
	/// Gets or sets the collection of shapes used for particles.
	/// </summary>
	public SKConfettiShapeCollection? Shapes
	{
		get => (SKConfettiShapeCollection?)GetValue(ShapesProperty);
		set => SetValue(ShapesProperty, value);
	}

	/// <summary>
	/// Gets or sets the start angle in degrees for particle emission direction.
	/// </summary>
	public double StartAngle
	{
		get => (double)GetValue(StartAngleProperty);
		set => SetValue(StartAngleProperty, value);
	}

	/// <summary>
	/// Gets or sets the end angle in degrees for particle emission direction.
	/// </summary>
	public double EndAngle
	{
		get => (double)GetValue(EndAngleProperty);
		set => SetValue(EndAngleProperty, value);
	}

	/// <summary>
	/// Gets or sets the minimum initial velocity of particles.
	/// </summary>
	public double MinimumInitialVelocity
	{
		get => (double)GetValue(MinimumInitialVelocityProperty);
		set => SetValue(MinimumInitialVelocityProperty, value);
	}

	/// <summary>
	/// Gets or sets the maximum initial velocity of particles.
	/// </summary>
	public double MaximumInitialVelocity
	{
		get => (double)GetValue(MaximumInitialVelocityProperty);
		set => SetValue(MaximumInitialVelocityProperty, value);
	}

	/// <summary>
	/// Gets or sets the minimum rotation velocity of particles in degrees per second.
	/// </summary>
	public double MinimumRotationVelocity
	{
		get => (double)GetValue(MinimumRotationVelocityProperty);
		set => SetValue(MinimumRotationVelocityProperty, value);
	}

	/// <summary>
	/// Gets or sets the maximum rotation velocity of particles in degrees per second.
	/// </summary>
	public double MaximumRotationVelocity
	{
		get => (double)GetValue(MaximumRotationVelocityProperty);
		set => SetValue(MaximumRotationVelocityProperty, value);
	}

	/// <summary>
	/// Gets or sets the maximum velocity cap for particles. A value of 0 means no cap.
	/// </summary>
	public double MaximumVelocity
	{
		get => (double)GetValue(MaximumVelocityProperty);
		set => SetValue(MaximumVelocityProperty, value);
	}

	/// <summary>
	/// Gets or sets the lifetime of each particle in seconds.
	/// </summary>
	public double Lifetime
	{
		get => (double)GetValue(LifetimeProperty);
		set => SetValue(LifetimeProperty, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether particles fade out at the end of their lifetime.
	/// </summary>
	public bool FadeOut
	{
		get => (bool)GetValue(FadeOutProperty);
		set => SetValue(FadeOutProperty, value);
	}

	/// <summary>
	/// Gets or sets the gravity vector applied to particles.
	/// </summary>
	public Point Gravity
	{
		get => (Point)GetValue(GravityProperty);
		set => SetValue(GravityProperty, value);
	}

	/// <summary>
	/// Gets a value indicating whether this system has completed emitting and all particles are gone.
	/// </summary>
	public bool IsComplete
	{
		get => (bool)GetValue(IsCompleteProperty);
		private set => SetValue(IsCompletePropertyKey, value);
	}

	internal int ParticleCount => particles.Count;

	/// <summary>
	/// Updates the particle system state for the given elapsed time.
	/// </summary>
	/// <param name="deltaTime">The time elapsed since the last update.</param>
	public void Update(TimeSpan deltaTime)
	{
		if (IsAnimationEnabled)
			Emitter?.Update(deltaTime);

		var g = Gravity.ToSKPoint();

		var removed = false;
		for (var i = particles.Count - 1; i >= 0; i--)
		{
			var particle = particles[i];

			particle.ApplyForce(g, deltaTime);

			if (particle.IsComplete || !lastViewBounds.IntersectsWith(particle.Bounds))
			{
				particles.RemoveAt(i);
				removed = true;
			}
		}

		if (removed)
			UpdateIsComplete();
	}

	/// <summary>
	/// Draws all particles onto the specified canvas.
	/// </summary>
	/// <param name="canvas">The canvas to draw on.</param>
	public void Draw(SKCanvas canvas)
	{
		foreach (var particle in particles)
		{
			particle.Draw(canvas);
		}
	}

	/// <summary>
	/// Updates the emitter bounds based on the view dimensions.
	/// </summary>
	/// <param name="width">The width of the view.</param>
	/// <param name="height">The height of the view.</param>
	public void UpdateEmitterBounds(double width, double height)
	{
		lastViewBounds = new SKRect(0, 0, (float)width, (float)height);

		actualEmitterBounds = EmitterBounds.Side switch
		{
			SKConfettiEmitterSide.Top => new Rect(0, -10, width, 0),
			SKConfettiEmitterSide.Left => new Rect(-10, 0, 0, height),
			SKConfettiEmitterSide.Right => new Rect(width + 10, 0, 0, height),
			SKConfettiEmitterSide.Bottom => new Rect(0, height + 10, width, 0),
			SKConfettiEmitterSide.Center => new Rect(width / 2, height / 2, 0, 0),
			_ => EmitterBounds.Rect,
		};
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
				Size = (float)p.Size,
				Mass = (float)p.Mass,
				Shape = s,
				Rotation = (float)GetNewRotation(),

				MaximumVelocity = new Point(MaximumVelocity, MaximumVelocity).ToSKPoint(),
				FadeOut = FadeOut,
				Lifetime = Lifetime,
			};

			particles.Add(particle);
		}

		UpdateIsComplete();

		Point GetNewLocation()
		{
			var rect = actualEmitterBounds;
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

		double GetNewRotationVelocity()
		{
			if (MaximumRotationVelocity < MinimumRotationVelocity)
				return 0;

			return MinimumRotationVelocity + random.NextDouble() * (MaximumRotationVelocity - MinimumRotationVelocity);
		}

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

	private static void OnIsAnimationEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKConfettiSystem system)
		{
			system.UpdateIsComplete();
		}
	}

	private bool UpdateIsComplete() =>
		IsComplete =
			particles.Count == 0 &&
			Emitter?.IsComplete != false &&
			IsAnimationEnabled;

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
			new SKConfettiPhysics(12, 2),
			new SKConfettiPhysics(16, 3),
		};

	private static SKConfettiShapeCollection CreateDefaultShapes() =>
		new SKConfettiShapeCollection
		{
			new SKConfettiSquareShape(),
			new SKConfettiCircleShape(),
			new SKConfettiRectShape(0.5),
			new SKConfettiOvalShape(0.5),
			new SKConfettiRectShape(0.1),
		};
}
