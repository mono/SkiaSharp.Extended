using System;

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
			: this(totalPixels, errorPixelCount, absoluteError, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="SKPixelComparisonResult"/>.
		/// </summary>
		/// <param name="totalPixels">The total number of pixels compared.</param>
		/// <param name="errorPixelCount">The number of pixels that differ between the two images.</param>
		/// <param name="absoluteError">The sum of per-channel (RGB) differences across all pixels.</param>
		/// <param name="sumSquaredError">The sum of per-channel (RGB) squared differences across all pixels.</param>
		public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError, long sumSquaredError)
		{
			TotalPixels = totalPixels;
			ErrorPixelCount = errorPixelCount;
			AbsoluteError = absoluteError;
			SumSquaredError = sumSquaredError;
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

		/// <summary>
		/// Gets the sum of per-channel (RGB) squared differences across all pixels: Σ(ΔR² + ΔG² + ΔB²).
		/// </summary>
		public long SumSquaredError { get; }

		/// <summary>
		/// Gets the mean absolute error per channel, computed as <see cref="AbsoluteError"/> / (<see cref="TotalPixels"/> × 3).
		/// Range: 0 (identical) to 255 (maximum difference).
		/// </summary>
		public double MeanAbsoluteError =>
			(double)AbsoluteError / (TotalPixels * 3.0);

		/// <summary>
		/// Gets the mean squared error per channel, computed as <see cref="SumSquaredError"/> / (<see cref="TotalPixels"/> × 3).
		/// Range: 0 (identical) to 65025 (maximum difference).
		/// </summary>
		public double MeanSquaredError =>
			(double)SumSquaredError / (TotalPixels * 3.0);

		/// <summary>
		/// Gets the root mean squared error, computed as the square root of <see cref="MeanSquaredError"/>.
		/// Range: 0 (identical) to 255 (maximum difference).
		/// </summary>
		public double RootMeanSquaredError =>
			Math.Sqrt(MeanSquaredError);

		/// <summary>
		/// Gets the normalized root mean squared error, computed as <see cref="RootMeanSquaredError"/> / 255.
		/// Range: 0 (identical) to 1 (maximum difference). Useful for threshold-based pass/fail testing.
		/// </summary>
		public double NormalizedRootMeanSquaredError =>
			RootMeanSquaredError / 255.0;

		/// <summary>
		/// Gets the peak signal-to-noise ratio in decibels, computed as 10 × log₁₀(255² / <see cref="MeanSquaredError"/>).
		/// Returns <see cref="double.PositiveInfinity"/> for identical images.
		/// </summary>
		public double PeakSignalToNoiseRatio =>
			MeanSquaredError == 0 ? double.PositiveInfinity : 10.0 * Math.Log10(255.0 * 255.0 / MeanSquaredError);
	}
}
