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
			: this(totalPixels, errorPixelCount, absoluteError, sumSquaredError, 3)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="SKPixelComparisonResult"/>.
		/// </summary>
		/// <param name="totalPixels">The total number of pixels compared.</param>
		/// <param name="errorPixelCount">The number of pixels that differ between the two images.</param>
		/// <param name="absoluteError">The sum of per-channel differences across all pixels.</param>
		/// <param name="sumSquaredError">The sum of per-channel squared differences across all pixels.</param>
		/// <param name="channelCount">The number of channels compared (3 for RGB, 4 for RGBA).</param>
		public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError, long sumSquaredError, int channelCount)
		{
			TotalPixels = totalPixels;
			ErrorPixelCount = errorPixelCount;
			AbsoluteError = absoluteError;
			SumSquaredError = sumSquaredError;
			ChannelCount = channelCount;
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
		/// Returns 0 if <see cref="TotalPixels"/> is 0.
		/// </summary>
		public double ErrorPixelPercentage =>
			TotalPixels == 0 ? 0.0 : (double)ErrorPixelCount / TotalPixels;

		/// <summary>
		/// Gets the sum of per-channel absolute differences across all pixels.
		/// </summary>
		public int AbsoluteError { get; }

		/// <summary>
		/// Gets the sum of per-channel squared differences across all pixels.
		/// </summary>
		public long SumSquaredError { get; }

		/// <summary>
		/// Gets the number of channels compared (3 for RGB, 4 for RGBA).
		/// </summary>
		public int ChannelCount { get; }

		/// <summary>
		/// Gets the mean absolute error per channel, computed as <see cref="AbsoluteError"/> / (<see cref="TotalPixels"/> × <see cref="ChannelCount"/>).
		/// Range: 0 (identical) to 255 (maximum difference). Returns 0 if <see cref="TotalPixels"/> is 0.
		/// </summary>
		public double MeanAbsoluteError =>
			TotalPixels == 0 ? 0.0 : (double)AbsoluteError / (TotalPixels * (double)ChannelCount);

		/// <summary>
		/// Gets the mean squared error per channel, computed as <see cref="SumSquaredError"/> / (<see cref="TotalPixels"/> × <see cref="ChannelCount"/>).
		/// Range: 0 (identical) to 65025 (maximum difference). Returns 0 if <see cref="TotalPixels"/> is 0.
		/// </summary>
		public double MeanSquaredError =>
			TotalPixels == 0 ? 0.0 : (double)SumSquaredError / (TotalPixels * (double)ChannelCount);

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
