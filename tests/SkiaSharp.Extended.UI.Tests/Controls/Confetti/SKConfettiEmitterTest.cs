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
			Assert.True(emitter.IsComplete);

			emitter.Update(dt);

			Assert.Equal(100, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.True(emitter.IsComplete);
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
			Assert.True(emitter.IsComplete);

			emitter.Update(dt);

			Assert.Equal(50, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.True(emitter.IsComplete);
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
			Assert.True(emitter.IsComplete);

			emitter.Update(dt);

			Assert.Equal(100, totalCreated);
			Assert.Equal(1, totalInvoke);
			Assert.True(emitter.IsComplete);
		}

		[Theory]
		[InlineData(0, 0, 0, false, false)]
		[InlineData(10, 1, 2, false, false)]
		[InlineData(500, 50, 100, false, true)]
		[InlineData(1000, 100, 100, true, true)]
		[InlineData(10000, 100, 100, true, true)]
		public void ParticleRateNoMaxOneMin(int milliseconds, int expectedCreated, int expectedCreated2, bool complete, bool complete2)
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
			Assert.Equal(complete, emitter.IsComplete);

			emitter.Update(dt);

			Assert.Equal(expectedCreated2, totalCreated);
			if (complete)
				Assert.Equal(1, totalInvoke);
			else
				Assert.Equal(2, totalInvoke);
			Assert.Equal(complete2, emitter.IsComplete);
		}
	}
}
