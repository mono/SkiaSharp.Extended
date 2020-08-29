using System.ComponentModel;

namespace SkiaSharp.Extended.Controls
{
	[TypeConverter(typeof(Converters.SKConfettiPhysicsTypeConverter))]
	public struct SKConfettiPhysics
	{
		public SKConfettiPhysics(double size, double mass)
		{
			Size = size;
			Mass = mass;
		}

		public double Size { get; set; }

		public double Mass { get; set; }
	}
}
