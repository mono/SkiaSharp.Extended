using System;

namespace SkiaSharp.Extended.Controls
{
	public class SKFlingDetectedEventArgs : EventArgs
	{
		public SKFlingDetectedEventArgs(float velocityX, float velocityY)
		{
			VelocityX = velocityX;
			VelocityY = velocityY;
		}

		public float VelocityX { get; }

		public float VelocityY { get; }

		public bool Handled { get; set; }
	}
}
