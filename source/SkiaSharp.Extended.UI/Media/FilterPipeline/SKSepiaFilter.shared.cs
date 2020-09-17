namespace SkiaSharp.Extended.UI.Media
{
	public class SKSepiaFilter : SKColorMatrixFilter
	{
		private static readonly float[] matrix = new float[SKColorFilter.ColorMatrixSize]
		{
			0.393f, 0.769f, 0.189f, 0.0f, 0.0f,
			0.349f, 0.686f, 0.168f, 0.0f, 0.0f,
			0.272f, 0.534f, 0.131f, 0.0f, 0.0f,
			0.0f,   0.0f,   0.0f,   1.0f, 0.0f
		};

		public SKSepiaFilter()
			: base(matrix)
		{
		}
	}
}
