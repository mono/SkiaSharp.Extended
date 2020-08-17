using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiPhysics : BindableObject
	{
		public SKConfettiPhysics()
		{
		}

		public SKConfettiPhysics(float size, float mass)
		{
			Size = size;
			Mass = mass;
		}

		public float Size { get; set; }

		public float Mass { get; set; }
	}
}
