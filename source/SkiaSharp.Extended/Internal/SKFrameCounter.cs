using System;
using System.Diagnostics;

namespace SkiaSharp.Extended.Internal;

internal sealed class SKFrameCounter
{
#if DEBUG
	private const int DefaultSampleCount = 100;
#else
	private const int DefaultSampleCount = 0;
#endif

	private readonly Func<long> getTimestamp;
	private readonly long[] samples;
	private readonly int sampleCount;
	private int sampleIndex;
	private int recordedSampleCount;
	private long sampleSum;
	private bool firstFrame = true;
	private long lastTimestamp;

	internal SKFrameCounter()
		: this(DefaultSampleCount, Stopwatch.GetTimestamp)
	{
	}

	internal SKFrameCounter(Func<long> getTimestamp)
		: this(DefaultSampleCount, getTimestamp)
	{
	}

	internal SKFrameCounter(int sampleCount, Func<long> getTimestamp)
	{
		this.sampleCount = Math.Max(0, sampleCount);
		this.getTimestamp = getTimestamp ?? throw new ArgumentNullException(nameof(getTimestamp));
		samples = this.sampleCount == 0 ? Array.Empty<long>() : new long[this.sampleCount];
	}

	internal TimeSpan Duration { get; private set; }

	internal float Rate { get; private set; }

	internal void Reset()
	{
		firstFrame = true;
		sampleIndex = 0;
		recordedSampleCount = 0;
		sampleSum = 0;
		Array.Clear(samples, 0, samples.Length);

		Duration = TimeSpan.Zero;
		Rate = 0;
	}

	internal TimeSpan NextFrame()
	{
		var timestamp = getTimestamp();
		if (firstFrame)
		{
			firstFrame = false;
			lastTimestamp = timestamp;
			Duration = TimeSpan.Zero;
			Rate = 0;
			return Duration;
		}

		var elapsedTimestamp = Math.Max(0, timestamp - lastTimestamp);
		lastTimestamp = timestamp;
		Duration = TimeSpan.FromSeconds((double)elapsedTimestamp / Stopwatch.Frequency);

		if (sampleCount == 0)
		{
			Rate = 0;
			return Duration;
		}

		sampleSum -= samples[sampleIndex];
		sampleSum += elapsedTimestamp;
		samples[sampleIndex] = elapsedTimestamp;
		sampleIndex = (sampleIndex + 1) % sampleCount;
		recordedSampleCount = Math.Min(sampleCount, recordedSampleCount + 1);

		var averageTimestamp = (double)sampleSum / recordedSampleCount;
		Rate = averageTimestamp <= 0 ? 0 : (float)(Stopwatch.Frequency / averageTimestamp);

		return Duration;
	}
}
