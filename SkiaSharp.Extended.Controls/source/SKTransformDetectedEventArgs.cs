using System;

namespace SkiaSharp.Extended.Controls
{
	public class SKTransformDetectedEventArgs : EventArgs
	{
		public SKTransformDetectedEventArgs(SKPoint center, SKPoint previousCenter)
			: this(center, previousCenter, 1f, 0f)
		{
		}

		public SKTransformDetectedEventArgs(SKPoint center, SKPoint previousCenter, float scaleDelta, float rotationDelta)
		{
			Center = center;
			PreviousCenter = previousCenter;
			ScaleDelta = scaleDelta;
			RotationDelta = rotationDelta;
		}

		public SKPoint Center { get; }

		public SKPoint PreviousCenter { get; }

		public float ScaleDelta { get; }

		public float RotationDelta { get; }
	}
}
