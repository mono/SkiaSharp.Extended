using System;

namespace SkiaSharp.Extended.Controls
{
	public class SKHoverDetectedEventArgs : EventArgs
	{
		public SKHoverDetectedEventArgs(SKPoint location)
		{
			Location = location;
		}

		public SKPoint Location { get; }

		public bool Handled { get; set; }
	}
}
