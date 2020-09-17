namespace SkiaSharp.Extended.UI.Media
{
	public class SKInvertFilter : SKColorMatrixFilter
	{
		private static readonly float[] matrix = new float[SKColorFilter.ColorMatrixSize]
		{
			-1f,  0f,  0f, 0f, 1f,
			 0f, -1f,  0f, 0f, 1f,
			 0f,  0f, -1f, 0f, 1f,
			 0f,  0f,  0f, 1f, 0f
		};

		public SKInvertFilter()
			: base(matrix)
		{
		}
	}
}
