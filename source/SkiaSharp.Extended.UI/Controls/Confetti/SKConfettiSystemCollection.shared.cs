using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SkiaSharp.Extended.UI.Controls
{
	public class SKConfettiSystemCollection : ObservableCollection<SKConfettiSystem>
	{
		public SKConfettiSystemCollection()
		{
		}

		public SKConfettiSystemCollection(IEnumerable<SKConfettiSystem> collection)
			: base(collection)
		{
		}

		public SKConfettiSystemCollection(List<SKConfettiSystem> list)
			: base(list)
		{
		}
	}
}
