namespace SkiaSharp.Extended
{
	/// <summary>
	/// Holds the results of a pixel-by-pixel image comparison.
	/// </summary>
	public class SKPixelComparisonResult
	{
		/// <summary>
		/// Initializes a new instance of <see cref="SKPixelComparisonResult"/>.
		/// </summary>
		/// <param name="totalPixels">The total number of pixels compared.</param>
		/// <param name="errorPixelCount">The number of pixels that differ between the two images.</param>
		/// <param name="absoluteError">The sum of per-channel (RGB) differences across all pixels.</param>
		public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError)
		{
			TotalPixels = totalPixels;
			ErrorPixelCount = errorPixelCount;
			AbsoluteError = absoluteError;
		}

		/// <summary>
		/// Gets the total number of pixels compared.
		/// </summary>
		public int TotalPixels { get; }

		/// <summary>
		/// Gets the number of pixels that differ between the two images.
		/// </summary>
		public int ErrorPixelCount { get; }

		/// <summary>
		/// Gets the ratio of differing pixels to total pixels (0.0 to 1.0).
		/// </summary>
		public double ErrorPixelPercentage =>
			(double)ErrorPixelCount / TotalPixels;

		/// <summary>
		/// Gets the sum of per-channel (RGB) absolute differences across all pixels.
		/// </summary>
		public int AbsoluteError { get; }
	}
}
