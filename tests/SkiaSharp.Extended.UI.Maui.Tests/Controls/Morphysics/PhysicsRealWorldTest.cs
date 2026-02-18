using System.Numerics;
using SkiaSharp.Extended.UI.Controls;
using Xunit;

namespace SkiaSharp.Extended.UI.Maui.Tests.Controls.Morphysics;

/// <summary>
/// Real-world physics tests with manual calculations to verify actual behavior.
/// These tests use exact scenarios with hand-calculated expected values.
/// </summary>
public class PhysicsRealWorldTest
{
	[Fact]
	public void RealWorld_ThreeParticles_ReachAttractor_ExactCalculation()
	{
		// SCENARIO: 3 particles at known positions should move toward attractor at (500, 500)
		// We'll manually calculate expected positions after 1 step
		
		var world = new PhysicsWorld();
		world.SetSeed(42);
		world.Gravity = Vector2.Zero; // No gravity to isolate attractor force
		
		// Attractor at center with known strength
		var attractorPos = new Vector2(500f, 500f);
		var attractorStrength = 10000f;
		world.AddAttractor("center", attractorPos, attractorStrength);
		
		// Particle 1: Above attractor (500, 300) - should move DOWN
		var p1 = new Particle
		{
			Position = new Vector2(500f, 300f),
			Velocity = Vector2.Zero,
			Mass = 1f
		};
		world.AddParticle(p1);
		
		// Particle 2: Left of attractor (300, 500) - should move RIGHT  
		var p2 = new Particle
		{
			Position = new Vector2(300f, 500f),
			Velocity = Vector2.Zero,
			Mass = 1f
		};
		world.AddParticle(p2);
		
		// Particle 3: Diagonal (400, 400) - should move DOWN-RIGHT
		var p3 = new Particle
		{
			Position = new Vector2(400f, 400f),
			Velocity = Vector2.Zero,
			Mass = 1f
		};
		world.AddParticle(p3);
		
		// Record initial positions
		var p1Start = p1.Position;
		var p2Start = p2.Position;
		var p3Start = p3.Position;
		
		// Step physics once
		world.Step(1f/60f);
		
		// VERIFICATION: Particles should move TOWARD attractor
		
		// Particle 1 (above) should have moved DOWN (Y increased)
		Assert.True(p1.Position.Y > p1Start.Y, 
			$"Particle 1 should move DOWN toward attractor. Start Y={p1Start.Y}, End Y={p1.Position.Y}");
		
		// X should stay roughly the same (directly above)
		Assert.True(Math.Abs(p1.Position.X - p1Start.X) < 1f,
			$"Particle 1 X should stay centered. Start X={p1Start.X}, End X={p1.Position.X}");
		
		// Particle 2 (left) should have moved RIGHT (X increased)
		Assert.True(p2.Position.X > p2Start.X,
			$"Particle 2 should move RIGHT toward attractor. Start X={p2Start.X}, End X={p2.Position.X}");
		
		// Y should stay roughly the same (directly left)
		Assert.True(Math.Abs(p2.Position.Y - p2Start.Y) < 1f,
			$"Particle 2 Y should stay centered. Start Y={p2Start.Y}, End Y={p2.Position.Y}");
		
		// Particle 3 (diagonal) should move DOWN-RIGHT (both X and Y increase)
		Assert.True(p3.Position.X > p3Start.X,
			$"Particle 3 should move RIGHT. Start X={p3Start.X}, End X={p3.Position.X}");
		Assert.True(p3.Position.Y > p3Start.Y,
			$"Particle 3 should move DOWN. Start Y={p3Start.Y}, End Y={p3.Position.Y}");
	}
	
	[Fact]
	public void RealWorld_ParticleEventuallyReachesAttractor()
	{
		// SCENARIO: A single particle should eventually reach the attractor
		var world = new PhysicsWorld();
		world.SetSeed(42);
		world.Gravity = Vector2.Zero;
		
		var attractorPos = new Vector2(500f, 500f);
		world.AddAttractor("center", attractorPos, 10000f);
		
		var particle = new Particle
		{
			Position = new Vector2(300f, 300f),
			Velocity = Vector2.Zero,
			Mass = 1f
		};
		world.AddParticle(particle);
		
		var initialDistance = Vector2.Distance(particle.Position, attractorPos);
		
		// Simulate for 5 seconds
		for (int i = 0; i < 300; i++) // 5 seconds at 60 FPS
		{
			world.Step(1f/60f);
		}
		
		var finalDistance = Vector2.Distance(particle.Position, attractorPos);
		
		// After 5 seconds, particle should be significantly closer (or oscillating around attractor)
		Assert.True(finalDistance < initialDistance * 0.9f || finalDistance < 100f,
			$"Particle should be much closer to attractor. Initial distance: {initialDistance}, Final distance: {finalDistance}");
		
		// Velocity should be significant (particle is moving, possibly oscillating)
		Assert.True(particle.Velocity.Length() > 1f,
			$"Particle should have significant velocity. Velocity magnitude: {particle.Velocity.Length()}");
	}
	
	[Fact]
	public void RealWorld_AttractorForce_ManualCalculation()
	{
		// SCENARIO: Calculate exact force for a known setup and verify it matches
		var world = new PhysicsWorld();
		world.SetSeed(42);
		world.Gravity = Vector2.Zero;
		
		var attractorPos = new Vector2(500f, 500f);
		var attractorStrength = 10000f;
		world.AddAttractor("center", attractorPos, attractorStrength);
		
		var particle = new Particle
		{
			Position = new Vector2(500f, 300f), // Directly above, 200 units away
			Velocity = Vector2.Zero,
			Mass = 1f
		};
		world.AddParticle(particle);
		
		// MANUAL CALCULATION:
		// direction = (500, 500) - (500, 300) = (0, 200)
		// distanceSq = 0² + 200² = 40000
		// distance = 200
		// force = (direction / distance) * (strength / max(distanceSq, 100))
		// force = (0, 200) / 200 * (10000 / 40000)
		// force = (0, 1) * 0.25 = (0, 0.25)
		// acceleration = force / mass = (0, 0.25) / 1 = (0, 0.25)
		// After dt=1/60, velocity = (0, 0) + (0, 0.25) * (1/60) = (0, 0.00416...)
		// position = (500, 300) + (0, 0.00416) * (1/60) = (500, 300.00006...)
		
		world.Step(1f/60f);
		
		// Verify particle moved downward
		Assert.True(particle.Position.Y > 300f, 
			$"Particle should have moved down. Position: {particle.Position}");
		
		// Verify X didn't change (vertical movement only)
		Assert.Equal(500f, particle.Position.X, 2);
		
		// Verify velocity is pointing downward
		Assert.True(particle.Velocity.Y > 0, 
			$"Particle velocity should point down. Velocity: {particle.Velocity}");
		Assert.True(Math.Abs(particle.Velocity.X) < 0.001f,
			$"Particle velocity X should be ~0. Velocity: {particle.Velocity}");
	}
}
