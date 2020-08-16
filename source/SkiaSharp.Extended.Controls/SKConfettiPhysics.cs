using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiPhysics : BindableObject
	{
		private const float DefaultSize = 12f;
		private const float DefaultMass = 5f;

		public SKConfettiPhysics()
			: this(DefaultSize, DefaultMass)
		{
		}

		public SKConfettiPhysics(float size)
			: this(size, DefaultMass)
		{
		}

		public SKConfettiPhysics(float size, float mass)
		{
			Size = size;
			Mass = mass;
		}

		public float Size { get; set; }

		public float Mass { get; set; }

		public static SKConfettiPhysics Default =>
			new SKConfettiPhysics();
	}
}
