using System.Collections.Generic;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Forms.Controls
{
	[TypeConverter(typeof(Converters.SKConfettiColorCollectionTypeConverter))]
	public class SKConfettiColorCollection : List<Color>
	{
		public SKConfettiColorCollection()
		{
		}

		public SKConfettiColorCollection(IEnumerable<Color> collection)
			: base(collection)
		{
		}

		public SKConfettiColorCollection(int capacity)
			: base(capacity)
		{
		}
	}
}
