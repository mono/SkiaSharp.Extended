using System;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKInvertFilter : SKFilter
	{
		private static readonly float[] colorMatrix = new float[SKColorFilter.ColorMatrixSize]
		{
			-1f,  0f,  0f, 0f, 1f,
			 0f, -1f,  0f, 0f, 1f,
			 0f,  0f, -1f, 0f, 1f,
			 0f,  0f,  0f, 1f, 0f
		};

		private SKColorFilter colorFilter;

		public SKInvertFilter()
		{
			if (colorMatrix == null)
				throw new ArgumentNullException(nameof(colorMatrix));

			colorFilter = SKColorFilter.CreateColorMatrix(colorMatrix);
			Paint.ColorFilter = colorFilter;
		}
	}
}
