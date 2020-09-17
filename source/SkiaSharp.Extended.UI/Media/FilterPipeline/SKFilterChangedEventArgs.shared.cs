using System;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKFilterChangedEventArgs : EventArgs
	{
		public SKFilterChangedEventArgs(SKFilter filter)
		{
			Filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		public SKFilter Filter { get; }
	}
}
