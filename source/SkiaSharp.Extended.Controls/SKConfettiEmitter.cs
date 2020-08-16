using System;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiEmitter : BindableObject
	{
		public SKConfettiEmitter()
		{
		}

		public SKConfettiEmitter(int particleRate, int maxParticles, int duration)
		{
			ParticleRate = particleRate;
			MaxParticles = maxParticles;
			Duration = duration;
		}

		public int ParticleRate { get; set; } = 100;

		public int MaxParticles { get; set; } = -1;

		public int Duration { get; set; } = 0;

		public bool IsComplete { get; private set; }

		internal event Action<int>? ParticlesCreated;

		public void Update(TimeSpan deltaTime)
		{
			if (IsComplete)
				return;

			ParticlesCreated?.Invoke(ParticleRate);
			IsComplete = true;
		}

		public static SKConfettiEmitter Burst(int particles) =>
			new SKConfettiEmitter(particles, -1, 0);

		public static SKConfettiEmitter Stream(int particleRate, int duration) =>
			new SKConfettiEmitter(particleRate, -1, duration);

		public static SKConfettiEmitter Infinite(int particleRate, int maxParticles) =>
			new SKConfettiEmitter(particleRate, maxParticles, -1);
	}
}
