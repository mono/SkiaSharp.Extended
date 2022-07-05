using System;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests
{
	public class SKConfettiEmitterTest
	{
		[Theory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(1000)]
		[InlineData(10000)]
		public void ParticleRateNoMaxBurstRunsOnce(int milliseconds)
		{
			var dt = TimeSpan.FromMilliseconds(milliseconds);

			var totalCreated = 0;
			var totalInvoke = 0;

			var emitter = new SKConfettiEmitter(100, -1, 0);
			emitter.ParticlesCreated += count =>
			{
				totalCreated += count;
				totalInvoke++;
			};

			emitter.Update(dt);

			Assert.Equal(100, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.False(emitter.IsRunning);

			emitter.Update(dt);

			Assert.Equal(100, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.False(emitter.IsRunning);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(1000)]
		[InlineData(10000)]
		public void ParticleRateWithMaxBurstRunsOnce(int milliseconds)
		{
			var dt = TimeSpan.FromMilliseconds(milliseconds);

			var totalCreated = 0;
			var totalInvoke = 0;

			var emitter = new SKConfettiEmitter(100, 50, 0);
			emitter.ParticlesCreated += count =>
			{
				totalCreated += count;
				totalInvoke++;
			};

			emitter.Update(dt);

			Assert.Equal(50, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.False(emitter.IsRunning);

			emitter.Update(dt);

			Assert.Equal(50, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.False(emitter.IsRunning);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(10)]
		[InlineData(1000)]
		[InlineData(10000)]
		public void ParticleRateLargerMaxBurstRunsOnce(int milliseconds)
		{
			var dt = TimeSpan.FromMilliseconds(milliseconds);

			var totalCreated = 0;
			var totalInvoke = 0;

			var emitter = new SKConfettiEmitter(100, 200, 0);
			emitter.ParticlesCreated += count =>
			{
				totalCreated += count;
				totalInvoke++;
			};

			emitter.Update(dt);

			Assert.Equal(100, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.False(emitter.IsRunning);

			emitter.Update(dt);

			Assert.Equal(100, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.False(emitter.IsRunning);
		}

		[Theory]
		[InlineData(0, 0, 0, true, true)]
		[InlineData(10, 1, 2, true, true)]
		[InlineData(500, 50, 100, true, false)]
		[InlineData(1000, 100, 100, false, false)]
		[InlineData(10000, 100, 100, false, false)]
		public void ParticleRateNoMaxOneMin(int milliseconds, int expectedCreated, int expectedCreated2, bool isRunning, bool isRunning2)
		{
			var dt = TimeSpan.FromMilliseconds(milliseconds);

			var totalCreated = 0;
			var totalInvoke = 0;

			var emitter = new SKConfettiEmitter(100, -1, 1.0);
			emitter.ParticlesCreated += count =>
			{
				totalCreated += count;
				totalInvoke++;
			};

			emitter.Update(dt);

			Assert.Equal(expectedCreated, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.Equal(isRunning, emitter.IsRunning);

			emitter.Update(dt);

			Assert.Equal(expectedCreated2, totalCreated);
			if (!isRunning)
				Assert.Equal(1, totalInvoke);
			else
				Assert.Equal(2, totalInvoke);
			Assert.Equal(isRunning2, emitter.IsRunning);
		}
	}
}
