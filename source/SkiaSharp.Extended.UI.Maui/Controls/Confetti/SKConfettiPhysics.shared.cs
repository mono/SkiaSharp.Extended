namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKConfettiPhysicsTypeConverter))]
public readonly struct SKConfettiPhysics
{
	public SKConfettiPhysics(double size, double mass)
	{
		Size = size;
		Mass = mass;
	}

	public double Size { get; }

	public double Mass { get; }
}
