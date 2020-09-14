namespace SkiaSharp.Extended.UI.Media
{
	public class SKHighContrastFilter : SKColorMatrixFilter
	{
		private static readonly float[] matrix = new float[SKColorFilter.ColorMatrixSize]
		{
			4.0f, 0.0f, 0.0f, 0.0f, -4.0f / (4.0f - 1f),
			0.0f, 4.0f, 0.0f, 0.0f, -4.0f / (4.0f - 1f),
			0.0f, 0.0f, 4.0f, 0.0f, -4.0f / (4.0f - 1f),
			0.0f, 0.0f, 0.0f, 1.0f, 0.0f
		};

		public SKHighContrastFilter()
			: base(matrix)
		{
		}
	}
}
