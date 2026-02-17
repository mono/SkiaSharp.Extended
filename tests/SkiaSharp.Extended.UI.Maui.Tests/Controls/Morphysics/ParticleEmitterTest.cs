using System.Numerics;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests.Morphysics;

public class ParticleEmitterTest
{
	[Fact]
	public void ParticleEmitter_EmitBurst_CreatesExactCount()
	{
		var emitter = new ParticleEmitter
		{
			ParticleRadius = 5f,
			ParticleLifetime = 2f
		};

		var particles = emitter.EmitBurst(50);

		Assert.Equal(50, particles.Count);
	}

	[Fact]
	public void ParticleEmitter_EmissionRate_CreatesParticlesOverTime()
	{
		var emitter = new ParticleEmitter
		{
			EmissionRate = 60f, // 60 particles per second
			ParticleRadius = 5f
		};

		var particles = emitter.Update(1f, 0); // 1 second

		// Should create approximately 60 particles
		Assert.InRange(particles.Count, 58, 62);
	}

	[Fact]
	public void ParticleEmitter_MaxParticles_CapEnforced()
	{
		var emitter = new ParticleEmitter
		{
			EmissionRate = 100f,
			MaxParticles = 50
		};

		var particles = emitter.Update(1f, 0);

		// Should not exceed max particles
		Assert.True(particles.Count <= 50);
	}

	[Fact]
	public void ParticleEmitter_InitialVelocity_AppliedToParticles()
	{
		var emitter = new ParticleEmitter
		{
			InitialVelocity = new Vector2(10, 20),
			VelocityVariance = 0f // No variance for predictable test
		};

		var particles = emitter.EmitBurst(1);

		Assert.Single(particles);
		Assert.Equal(10f, particles[0].Velocity.X, 1);
		Assert.Equal(20f, particles[0].Velocity.Y, 1);
	}
}
