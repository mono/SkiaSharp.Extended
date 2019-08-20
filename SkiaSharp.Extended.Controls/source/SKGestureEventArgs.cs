using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Controls
{
	public class SKGestureEventArgs : EventArgs
	{
		public SKGestureEventArgs(IEnumerable<SKPoint> locations)
		{
			Locations = locations;
		}

		public IEnumerable<SKPoint> Locations { get; }

		public bool Handled { get; set; }
	}
}
