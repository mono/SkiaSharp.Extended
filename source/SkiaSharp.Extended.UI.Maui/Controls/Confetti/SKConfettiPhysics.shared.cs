namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Defines the physical properties of a confetti particle, including size and mass.
/// </summary>
[TypeConverter(typeof(Converters.SKConfettiPhysicsTypeConverter))]
public readonly struct SKConfettiPhysics
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiPhysics"/> struct.
	/// </summary>
	/// <param name="size">The size of the particle.</param>
	/// <param name="mass">The mass of the particle.</param>
	public SKConfettiPhysics(double size, double mass)
	{
		Size = size;
		Mass = mass;
	}

	/// <summary>
	/// Gets the size of the particle.
	/// </summary>
	public double Size { get; }

	/// <summary>
	/// Gets the mass of the particle.
	/// </summary>
	public double Mass { get; }
}
