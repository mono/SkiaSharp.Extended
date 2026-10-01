using System.Diagnostics;
using SkiaSharp.Extended.Internal;

namespace SkiaSharp.Extended.Tests.Internal;

public class SKFrameCounterTest
{
	[Fact]
	public void NextFrame_TracksMonotonicDeltaAndRollingRate()
	{
		var frequency = Stopwatch.Frequency;
		var timestamps = new Queue<long>(new long[] { 0, frequency, frequency * 3, frequency * 6, frequency * 10 });
		var counter = new SKFrameCounter(3, timestamps.Dequeue);

		Assert.Equal(TimeSpan.Zero, counter.NextFrame());
		Assert.Equal(0, counter.Rate);

		Assert.Equal(TimeSpan.FromSeconds(1), counter.NextFrame());
		Assert.Equal(1, counter.Rate);

		Assert.Equal(TimeSpan.FromSeconds(2), counter.NextFrame());
		Assert.Equal(2f / 3f, counter.Rate);

		Assert.Equal(TimeSpan.FromSeconds(3), counter.NextFrame());
		Assert.Equal(0.5f, counter.Rate);

		Assert.Equal(TimeSpan.FromSeconds(4), counter.NextFrame());
		Assert.Equal(1f / 3f, counter.Rate);
	}

	[Fact]
	public void Reset_DiscardsPriorTimingSamples()
	{
		var frequency = Stopwatch.Frequency;
		var timestamps = new Queue<long>(new long[] { 0, frequency, frequency * 5, frequency * 9 });
		var counter = new SKFrameCounter(3, timestamps.Dequeue);

		counter.NextFrame();
		counter.NextFrame();
		counter.Reset();

		Assert.Equal(TimeSpan.Zero, counter.NextFrame());
		Assert.Equal(0, counter.Rate);

		Assert.Equal(TimeSpan.FromSeconds(4), counter.NextFrame());
		Assert.Equal(0.25f, counter.Rate);
	}
}
