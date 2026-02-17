using System.Numerics;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Represents a particle in the physics simulation.
/// </summary>
public class Particle
{
	/// <summary>
	/// Gets or sets the position of the particle.
	/// </summary>
	public Vector2 Position { get; set; }

	/// <summary>
	/// Gets or sets the velocity of the particle.
	/// </summary>
	public Vector2 Velocity { get; set; }

	/// <summary>
	/// Gets or sets the mass of the particle.
	/// </summary>
	public float Mass { get; set; } = 1f;

	/// <summary>
	/// Gets or sets the radius of the particle for collision detection.
	/// </summary>
	public float Radius { get; set; } = 5f;

	/// <summary>
	/// Gets or sets the remaining lifetime of the particle in seconds.
	/// </summary>
	public float Lifetime { get; set; } = -1f; // -1 means infinite

	/// <summary>
	/// Gets or sets the color of the particle.
	/// </summary>
	public SKColor Color { get; set; } = SKColors.White;

	/// <summary>
	/// Gets or sets user data attached to this particle.
	/// </summary>
	public object? UserData { get; set; }

	/// <summary>
	/// Gets whether this particle is dead (lifetime expired).
	/// </summary>
	public bool IsDead => Lifetime >= -0.1f && Lifetime <= 0.0001f && Lifetime != -1f; // Allow some overshoot but not infinite (-1)

	/// <summary>
	/// Updates the particle's lifetime.
	/// </summary>
	internal void UpdateLifetime(float deltaTime)
	{
		if (Lifetime > 0)
			Lifetime -= deltaTime;
	}
}
