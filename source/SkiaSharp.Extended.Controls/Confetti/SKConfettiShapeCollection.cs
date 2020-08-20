using System.Collections.Generic;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiShapeCollection : List<SKConfettiShape>
	{
		public SKConfettiShapeCollection()
		{
		}

		public SKConfettiShapeCollection(IEnumerable<SKConfettiShape> collection)
			: base(collection)
		{
		}

		public SKConfettiShapeCollection(int capacity)
			: base(capacity)
		{
		}
	}
}
