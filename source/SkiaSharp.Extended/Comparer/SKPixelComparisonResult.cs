using System;

namespace SkiaSharp.Extended
{
	public class SKPixelComparisonResult
	{
		public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError, long sumSquaredError = 0)
		{
			TotalPixels = totalPixels;
			ErrorPixelCount = errorPixelCount;
			AbsoluteError = absoluteError;
			SumSquaredError = sumSquaredError;
		}

		public int TotalPixels { get; }

		public int ErrorPixelCount { get; }

		public double ErrorPixelPercentage =>
			(double)ErrorPixelCount / TotalPixels;

		public int AbsoluteError { get; }

		public long SumSquaredError { get; }

		public double MeanAbsoluteError =>
			(double)AbsoluteError / (TotalPixels * 3.0);

		public double MeanSquaredError =>
			(double)SumSquaredError / (TotalPixels * 3.0);

		public double RootMeanSquaredError =>
			Math.Sqrt(MeanSquaredError);

		public double NormalizedRootMeanSquaredError =>
			RootMeanSquaredError / 255.0;

		public double PeakSignalToNoiseRatio =>
			MeanSquaredError == 0 ? double.PositiveInfinity : 10.0 * Math.Log10(255.0 * 255.0 / MeanSquaredError);
	}
}
