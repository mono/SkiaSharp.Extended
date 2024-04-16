namespace SkiaSharp.Extended.UI;

internal class SKFrameCounter
{
#if DEBUG
	private const int DefaultSampleCount = 100;
#else
	private const int DefaultSampleCount = 0;
#endif

	private readonly int sampleCount;
	private readonly int[] samples;
	private int index;
	private int sum;

	private bool firstRender = true;
	private int lastTick;

	public SKFrameCounter(int sampleCount = DefaultSampleCount)
	{
		this.sampleCount = Math.Max(0, sampleCount);
		samples = new int[sampleCount];

		Reset();
	}

	public TimeSpan Duration { get; private set; }

	public float Rate { get; private set; }

	public void Reset()
	{
		firstRender = true;

		Duration = TimeSpan.Zero;
		Rate = 0;
	}

	public TimeSpan NextFrame()
	{
		if (firstRender)
		{
			lastTick = Environment.TickCount;
			firstRender = false;
		}

		var ticks = Environment.TickCount;
		var delta = ticks - lastTick;
		lastTick = ticks;

		Duration = TimeSpan.FromMilliseconds(delta);
		if (sampleCount <= 0)
		{
			Rate = 0;
		}
		else
		{
			var avg = CalculateAverage(delta);
			Rate = avg <= 0 ? 0 : 1000f / avg;
		}

		return Duration;
	}

	private float CalculateAverage(int delta)
	{
		sum -= samples[index];
		sum += delta;
		samples[index] = delta;

		if (++index == sampleCount)
			index = 0;

		return (float)sum / sampleCount;
	}
}
