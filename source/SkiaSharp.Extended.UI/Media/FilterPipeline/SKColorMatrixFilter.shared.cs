using System;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKColorMatrixFilter : SKFilter
	{
		private SKColorFilter colorFilter;

		public SKColorMatrixFilter(float[] colorMatrix)
		{
			if (colorMatrix == null)
				throw new ArgumentNullException(nameof(colorMatrix));

			colorFilter = SKColorFilter.CreateColorMatrix(colorMatrix);
			Paint.ColorFilter = colorFilter;
		}
	}
}
