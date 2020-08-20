using System.Collections.Generic;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiPhysicsCollection : List<SKConfettiPhysics>
	{
		public SKConfettiPhysicsCollection()
		{
		}

		public SKConfettiPhysicsCollection(IEnumerable<SKConfettiPhysics> collection)
			: base(collection)
		{
		}

		public SKConfettiPhysicsCollection(int capacity)
			: base(capacity)
		{
		}
	}
}
