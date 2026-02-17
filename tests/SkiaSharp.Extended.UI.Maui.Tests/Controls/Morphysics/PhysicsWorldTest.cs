using System.Numerics;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests.Morphysics;

public class PhysicsWorldTest
{
	[Fact]
	public void PhysicsWorld_DeterministicSeed_ProducesSameResults()
	{
		var world1 = new PhysicsWorld();
		world1.SetSeed(12345);
		var p1 = new Particle { Position = Vector2.Zero, Velocity = new Vector2(10, 0), Mass = 1f };
		world1.AddParticle(p1);

		var world2 = new PhysicsWorld();
		world2.SetSeed(12345);
		var p2 = new Particle { Position = Vector2.Zero, Velocity = new Vector2(10, 0), Mass = 1f };
		world2.AddParticle(p2);

		// Run simulation for 100 steps
		for (int i = 0; i < 100; i++)
		{
			world1.Step(1 / 60f);
			world2.Step(1 / 60f);
		}

		// Positions should be identical (or very close due to floating point)
		Assert.Equal(p1.Position.X, p2.Position.X, 4);
		Assert.Equal(p1.Position.Y, p2.Position.Y, 4);
	}

	[Fact]
	public void PhysicsWorld_Gravity_PullsParticlesDown()
	{
		var world = new PhysicsWorld();
		world.Gravity = new Vector2(0, 100f);

		var particle = new Particle { Position = new Vector2(0, 0), Velocity = Vector2.Zero, Mass = 1f };
		world.AddParticle(particle);

		var initialY = particle.Position.Y;
		world.Step(1 / 60f);

		// Particle should have moved down
		Assert.True(particle.Position.Y > initialY, "Particle should move down due to gravity");
		Assert.True(particle.Velocity.Y > 0, "Particle should have positive Y velocity");
	}

	[Fact]
	public void PhysicsWorld_Attractor_PullsParticle()
	{
		var world = new PhysicsWorld();
		world.Gravity = Vector2.Zero; // Disable gravity
		world.AddAttractor("test", new Vector2(100, 0), 500f);

		var particle = new Particle { Position = Vector2.Zero, Velocity = Vector2.Zero, Mass = 1f };
		world.AddParticle(particle);

		world.Step(1 / 60f);

		// Particle should move toward the attractor
		Assert.True(particle.Velocity.X > 0, "Particle should have positive X velocity toward attractor");
	}

	[Fact]
	public void PhysicsWorld_Attractor_InverseSquareLaw_ForceDimishesWithDistance()
	{
		// Test that attractor force follows inverse square law: F ∝ 1/r²
		var world1 = new PhysicsWorld();
		world1.Gravity = Vector2.Zero;
		world1.AddAttractor("test", new Vector2(100, 0), 10000f);

		var particle1 = new Particle { Position = new Vector2(0, 0), Velocity = Vector2.Zero, Mass = 1f };
		world1.AddParticle(particle1);

		world1.Step(1 / 60f);
		var velocity1 = particle1.Velocity.X;

		// Now test with particle at twice the distance
		var world2 = new PhysicsWorld();
		world2.Gravity = Vector2.Zero;
		world2.AddAttractor("test", new Vector2(200, 0), 10000f);

		var particle2 = new Particle { Position = new Vector2(0, 0), Velocity = Vector2.Zero, Mass = 1f };
		world2.AddParticle(particle2);

		world2.Step(1 / 60f);
		var velocity2 = particle2.Velocity.X;

		// Force at 2x distance should be ~1/4 the force at 1x distance (inverse square law)
		// velocity is proportional to force, so velocity2 should be ~1/4 of velocity1
		var ratio = velocity2 / velocity1;
		Assert.True(ratio > 0.2f && ratio < 0.3f, 
			$"Velocity ratio should be ~0.25 (inverse square law) but was {ratio:F3}. " +
			$"velocity1={velocity1:F3}, velocity2={velocity2:F3}");
	}

	[Fact]
	public void PhysicsWorld_Attractor_CloserParticleExperiencesStrongerForce()
	{
		var world = new PhysicsWorld();
		world.Gravity = Vector2.Zero;
		world.AddAttractor("test", new Vector2(100, 0), 10000f);

		var particleClose = new Particle { Position = new Vector2(50, 0), Velocity = Vector2.Zero, Mass = 1f };
		var particleFar = new Particle { Position = new Vector2(0, 0), Velocity = Vector2.Zero, Mass = 1f };
		
		world.AddParticle(particleClose);
		world.AddParticle(particleFar);

		world.Step(1 / 60f);

		// Closer particle should have higher velocity (experienced stronger force)
		Assert.True(particleClose.Velocity.X > particleFar.Velocity.X,
			$"Closer particle should have stronger acceleration. Close: {particleClose.Velocity.X:F3}, Far: {particleFar.Velocity.X:F3}");
	}

	[Fact]
	public void PhysicsWorld_Attractor_MinimumDistancePreventsDivisionByZero()
	{
		var world = new PhysicsWorld();
		world.Gravity = Vector2.Zero;
		var attractorPos = new Vector2(100, 100);
		world.AddAttractor("test", attractorPos, 10000f);

		// Place particle very close to attractor (but not exactly on it)
		var particle = new Particle { Position = attractorPos + new Vector2(0.01f, 0.01f), Velocity = Vector2.Zero, Mass = 1f };
		world.AddParticle(particle);

		// Should not throw exception due to division by zero or extreme force
		Exception? caughtException = null;
		try
		{
			world.Step(1 / 60f);
		}
		catch (Exception ex)
		{
			caughtException = ex;
		}

		Assert.Null(caughtException);
		// Velocity should be finite (not NaN or Infinity)
		Assert.True(float.IsFinite(particle.Velocity.X), "Velocity X should be finite");
		Assert.True(float.IsFinite(particle.Velocity.Y), "Velocity Y should be finite");
	}

	[Fact]
	public void PhysicsWorld_ParticleLifetime_RemovesDeadParticles()
	{
		var world = new PhysicsWorld();
		var particle = new Particle
		{
			Position = Vector2.Zero,
			Velocity = Vector2.Zero,
			Lifetime = 0.01f // 10ms lifetime
		};
		world.AddParticle(particle);

		Assert.Single(world.Particles);

		// Step past lifetime multiple times to ensure removal
		world.Step(0.02f);
		world.Step(0.02f);

		Assert.Empty(world.Particles);
	}

	[Fact]
	public void PhysicsWorld_Collision_ParticlesBounce()
	{
		var world = new PhysicsWorld();
		world.Gravity = Vector2.Zero;
		world.EnableCollisions = true;
		world.Restitution = 0.8f;

		var p1 = new Particle
		{
			Position = new Vector2(0, 0),
			Velocity = new Vector2(50, 0),
			Mass = 1f,
			Radius = 5f
		};

		var p2 = new Particle
		{
			Position = new Vector2(20, 0), // Not overlapping, approaching
			Velocity = new Vector2(-50, 0),
			Mass = 1f,
			Radius = 5f
		};

		world.AddParticle(p1);
		world.AddParticle(p2);

		var p1InitialVelocity = p1.Velocity.X;

		// Run enough steps for particles to meet and collide
		for (int i = 0; i < 20; i++)
		{
			world.Step(1 / 60f);
		}

		// After collision, p1 should be moving in opposite direction or have stopped
		var velocityReversed = p1.Velocity.X < 0 || Math.Abs(p1.Velocity.X) < Math.Abs(p1InitialVelocity);
		Assert.True(velocityReversed, $"Velocity should have reversed or decreased from {p1InitialVelocity} but is {p1.Velocity.X}");
	}
}
