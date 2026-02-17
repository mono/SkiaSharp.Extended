using System.Numerics;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Deterministic physics simulation world.
/// </summary>
public class PhysicsWorld
{
	private readonly List<Particle> particles = new List<Particle>();
	private readonly List<Attractor> attractors = new List<Attractor>();
	private readonly List<StickyZone> stickyZones = new List<StickyZone>();
	private Random random = new Random();
	private float fixedTimeStep = 1f / 60f;
	private float accumulator = 0f;

	/// <summary>
	/// Gets or sets the gravity vector applied to all particles.
	/// </summary>
	public Vector2 Gravity { get; set; } = new Vector2(0, 9.8f);

	/// <summary>
	/// Gets or sets whether to enable particle-to-particle collisions.
	/// </summary>
	public bool EnableCollisions { get; set; } = true;

	/// <summary>
	/// Gets or sets the coefficient of restitution for collisions (bounciness).
	/// </summary>
	public float Restitution { get; set; } = 0.7f;

	/// <summary>
	/// Gets the list of active particles.
	/// </summary>
	public IReadOnlyList<Particle> Particles => particles;

	/// <summary>
	/// Sets the random seed for deterministic simulation.
	/// </summary>
	public void SetSeed(int seed)
	{
		random = new Random(seed);
	}

	/// <summary>
	/// Adds a particle to the simulation.
	/// </summary>
	public void AddParticle(Particle particle)
	{
		if (particle == null)
			throw new ArgumentNullException(nameof(particle));

		particles.Add(particle);
	}

	/// <summary>
	/// Removes a particle from the simulation.
	/// </summary>
	public bool RemoveParticle(Particle particle)
	{
		return particles.Remove(particle);
	}

	/// <summary>
	/// Clears all particles from the simulation.
	/// </summary>
	public void ClearParticles()
	{
		particles.Clear();
	}

	/// <summary>
	/// Adds an attractor to the simulation.
	/// </summary>
	public void AddAttractor(string id, Vector2 position, float strength)
	{
		attractors.Add(new Attractor
		{
			Id = id,
			Position = position,
			Strength = strength
		});
	}

	/// <summary>
	/// Removes an attractor by ID.
	/// </summary>
	public bool RemoveAttractor(string id)
	{
		var attractor = attractors.FirstOrDefault(a => a.Id == id);
		return attractor != null && attractors.Remove(attractor);
	}

	/// <summary>
	/// Adds a sticky zone to the simulation.
	/// </summary>
	public void AddStickyZone(string id, Vector2 position, float radius, float stickProbability = 0.5f)
	{
		stickyZones.Add(new StickyZone
		{
			Id = id,
			Position = position,
			Radius = radius,
			StickProbability = stickProbability
		});
	}

	/// <summary>
	/// Updates the physics simulation by the given time step.
	/// </summary>
	public void Step(float deltaTime)
	{
		accumulator += deltaTime;

		// Fixed timestep integration to ensure determinism
		while (accumulator >= fixedTimeStep)
		{
			IntegrateStep(fixedTimeStep);
			accumulator -= fixedTimeStep;
		}

		// Remove dead particles
		particles.RemoveAll(p => p.IsDead);
	}

	private void IntegrateStep(float dt)
	{
		// Apply forces and integrate
		foreach (var particle in particles)
		{
			// Apply gravity
			var force = Gravity * particle.Mass;

			// Apply attractors (inverse square law)
			foreach (var attractor in attractors)
			{
				var direction = attractor.Position - particle.Position;
				var distanceSq = direction.LengthSquared();
				if (distanceSq > 0.1f)
				{
					var distance = (float)Math.Sqrt(distanceSq);
					// Inverse square law: F = k * (1/r²), with minimum distance to prevent extreme forces
					var attractorForce = (direction / distance) * (attractor.Strength / Math.Max(distanceSq, 100f));
					force += attractorForce;
				}
			}

			// Apply force to velocity (F = ma, so a = F/m)
			var acceleration = force / particle.Mass;
			particle.Velocity += acceleration * dt;

			// Update position (Velocity Verlet integration)
			particle.Position += particle.Velocity * dt;

			// Update lifetime
			particle.UpdateLifetime(dt);
		}

		// Handle collisions
		if (EnableCollisions)
		{
			for (int i = 0; i < particles.Count; i++)
			{
				for (int j = i + 1; j < particles.Count; j++)
				{
					ResolveCollision(particles[i], particles[j]);
				}
			}
		}

		// Handle sticky zones
		foreach (var zone in stickyZones)
		{
			foreach (var particle in particles)
			{
				var distance = Vector2.Distance(particle.Position, zone.Position);
				if (distance <= zone.Radius && random.NextDouble() < zone.StickProbability * dt)
				{
					// Stick the particle
					particle.Velocity = Vector2.Zero;
				}
			}
		}
	}

	private void ResolveCollision(Particle a, Particle b)
	{
		var delta = b.Position - a.Position;
		var distanceSq = delta.LengthSquared();
		var minDistance = a.Radius + b.Radius;
		var minDistanceSq = minDistance * minDistance;

		if (distanceSq < minDistanceSq && distanceSq > 0)
		{
			var distance = (float)Math.Sqrt(distanceSq);
			var normal = delta / distance;

			// Separate particles
			var overlap = minDistance - distance;
			var separation = normal * (overlap * 0.5f);
			a.Position -= separation;
			b.Position += separation;

			// Apply collision response
			var relativeVelocity = b.Velocity - a.Velocity;
			var velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

			if (velocityAlongNormal < 0)
				return; // Particles are separating

			var impulse = -(1 + Restitution) * velocityAlongNormal / (1f / a.Mass + 1f / b.Mass);
			var impulseVector = normal * impulse;

			a.Velocity -= impulseVector / a.Mass;
			b.Velocity += impulseVector / b.Mass;
		}
	}
}

/// <summary>
/// Represents an attractor that pulls particles.
/// </summary>
public class Attractor
{
	public string Id { get; set; } = string.Empty;
	public Vector2 Position { get; set; }
	public float Strength { get; set; }
}

/// <summary>
/// Represents a zone that can capture particles.
/// </summary>
public class StickyZone
{
	public string Id { get; set; } = string.Empty;
	public Vector2 Position { get; set; }
	public float Radius { get; set; }
	public float StickProbability { get; set; }
}
