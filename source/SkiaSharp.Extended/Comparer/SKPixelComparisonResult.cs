namespace SkiaSharp.Extended
{
	public class SKPixelComparisonResult
	{
		public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError)
		{
			TotalPixels = totalPixels;
			ErrorPixelCount = errorPixelCount;
			AbsoluteError = absoluteError;
		}

		public int TotalPixels { get; }

		public int ErrorPixelCount { get; }

		public double ErrorPixelPercentage =>
			(double)ErrorPixelCount / TotalPixels;

		public int AbsoluteError { get; }
	}
}
