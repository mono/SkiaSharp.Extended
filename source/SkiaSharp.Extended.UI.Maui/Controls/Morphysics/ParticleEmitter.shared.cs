using System.Numerics;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Emits particles over time or in bursts.
/// </summary>
public class ParticleEmitter : BindableObject
{
	public static readonly BindableProperty EmissionRateProperty = BindableProperty.Create(
		nameof(EmissionRate),
		typeof(float),
		typeof(ParticleEmitter),
		60f);

	public static readonly BindableProperty MaxParticlesProperty = BindableProperty.Create(
		nameof(MaxParticles),
		typeof(int),
		typeof(ParticleEmitter),
		1000);

	public static readonly BindableProperty ParticleLifetimeProperty = BindableProperty.Create(
		nameof(ParticleLifetime),
		typeof(float),
		typeof(ParticleEmitter),
		2f);

	public static readonly BindableProperty InitialVelocityProperty = BindableProperty.Create(
		nameof(InitialVelocity),
		typeof(Vector2),
		typeof(ParticleEmitter),
		Vector2.Zero);

	public static readonly BindableProperty VelocityVarianceProperty = BindableProperty.Create(
		nameof(VelocityVariance),
		typeof(float),
		typeof(ParticleEmitter),
		10f);

	public static readonly BindableProperty ParticleColorProperty = BindableProperty.Create(
		nameof(ParticleColor),
		typeof(Color),
		typeof(ParticleEmitter),
		Colors.White);

	public static readonly BindableProperty ParticleRadiusProperty = BindableProperty.Create(
		nameof(ParticleRadius),
		typeof(float),
		typeof(ParticleEmitter),
		5f);

	private readonly Random random = new Random();
	private float emissionAccumulator = 0f;

	/// <summary>
	/// Gets or sets the number of particles emitted per second.
	/// </summary>
	public float EmissionRate
	{
		get => (float)GetValue(EmissionRateProperty);
		set => SetValue(EmissionRateProperty, value);
	}

	/// <summary>
	/// Gets or sets the maximum number of particles that can exist.
	/// </summary>
	public int MaxParticles
	{
		get => (int)GetValue(MaxParticlesProperty);
		set => SetValue(MaxParticlesProperty, value);
	}

	/// <summary>
	/// Gets or sets the lifetime of emitted particles in seconds.
	/// </summary>
	public float ParticleLifetime
	{
		get => (float)GetValue(ParticleLifetimeProperty);
		set => SetValue(ParticleLifetimeProperty, value);
	}

	/// <summary>
	/// Gets or sets the initial velocity of emitted particles.
	/// </summary>
	public Vector2 InitialVelocity
	{
		get => (Vector2)GetValue(InitialVelocityProperty);
		set => SetValue(InitialVelocityProperty, value);
	}

	/// <summary>
	/// Gets or sets the variance in initial velocity.
	/// </summary>
	public float VelocityVariance
	{
		get => (float)GetValue(VelocityVarianceProperty);
		set => SetValue(VelocityVarianceProperty, value);
	}

	/// <summary>
	/// Gets or sets the color of emitted particles.
	/// </summary>
	public Color ParticleColor
	{
		get => (Color)GetValue(ParticleColorProperty);
		set => SetValue(ParticleColorProperty, value);
	}

	/// <summary>
	/// Gets or sets the radius of emitted particles.
	/// </summary>
	public float ParticleRadius
	{
		get => (float)GetValue(ParticleRadiusProperty);
		set => SetValue(ParticleRadiusProperty, value);
	}

	/// <summary>
	/// Gets or sets the spawn position for particles.
	/// </summary>
	public Vector2 SpawnPosition { get; set; }

	/// <summary>
	/// Emits a burst of particles.
	/// </summary>
	public List<Particle> EmitBurst(int count)
	{
		var particles = new List<Particle>();
		for (int i = 0; i < count; i++)
		{
			particles.Add(CreateParticle());
		}
		return particles;
	}

	/// <summary>
	/// Updates the emitter and returns newly created particles.
	/// </summary>
	public List<Particle> Update(float deltaTime, int currentParticleCount)
	{
		var newParticles = new List<Particle>();

		if (EmissionRate <= 0)
			return newParticles;

		emissionAccumulator += deltaTime * EmissionRate;

		while (emissionAccumulator >= 1f && currentParticleCount < MaxParticles)
		{
			newParticles.Add(CreateParticle());
			emissionAccumulator -= 1f;
			currentParticleCount++;
		}

		return newParticles;
	}

	private Particle CreateParticle()
	{
		var velocityOffset = new Vector2(
			((float)random.NextDouble() - 0.5f) * VelocityVariance * 2f,
			((float)random.NextDouble() - 0.5f) * VelocityVariance * 2f);

		return new Particle
		{
			Position = SpawnPosition,
			Velocity = InitialVelocity + velocityOffset,
			Radius = ParticleRadius,
			Color = ParticleColor.ToSKColor(),
			Lifetime = ParticleLifetime
		};
	}
}
