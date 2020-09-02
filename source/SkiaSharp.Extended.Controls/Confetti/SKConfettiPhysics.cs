using System.ComponentModel;

namespace SkiaSharp.Extended.Controls
{
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
}
