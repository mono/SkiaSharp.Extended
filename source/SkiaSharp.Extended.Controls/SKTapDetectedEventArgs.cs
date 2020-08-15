using System;

namespace SkiaSharp.Extended.Controls
{
	public class SKTapDetectedEventArgs : EventArgs
	{
		public SKTapDetectedEventArgs(SKPoint location)
			: this(location, 1)
		{
		}

		public SKTapDetectedEventArgs(SKPoint location, int tapCount)
		{
			Location = location;
			TapCount = tapCount;
		}

		public SKPoint Location { get; }

		public int TapCount { get; }

		public bool Handled { get; set; }
	}
}
