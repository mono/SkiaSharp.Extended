using System;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiEmitter : BindableObject
	{
		private int totalParticles = 0;
		private double totalDuration = 0;

		public SKConfettiEmitter()
		{
		}

		public SKConfettiEmitter(int particleRate, int maxParticles, double duration)
		{
			ParticleRate = particleRate;
			MaxParticles = maxParticles;
			Duration = duration;
		}

		public int ParticleRate { get; set; } = 100;

		public int MaxParticles { get; set; } = 0;

		public double Duration { get; set; } = 0;

		public bool IsComplete { get; private set; }

		public event Action<int>? ParticlesCreated;

		public void Update(TimeSpan deltaTime)
		{
			if (IsComplete)
				return;

			var prevDuration = totalDuration;
			var currDuration = deltaTime.TotalSeconds;
			totalDuration += currDuration;

			double secs;
			if (Duration == 0)
				secs = 1.0; // burst mode, so pop them all
			else if (Duration > 0 && totalDuration > Duration)
				secs = Duration - prevDuration; // took longer, so trim
			else
				secs = currDuration; // either infinite or normal

			var particles = (int)(ParticleRate * secs);
			if (MaxParticles > 0)
				particles = Math.Min(particles, MaxParticles);
			totalParticles += particles;

			ParticlesCreated?.Invoke(particles);

			IsComplete =
				(MaxParticles > 0 && totalParticles >= MaxParticles) || // reached the max particles
				Duration == 0 || // burst mode
				(Duration > 0 && totalDuration >= Duration); // reached the max duration
		}

		public static SKConfettiEmitter Burst(int particles) =>
			new SKConfettiEmitter(particles, -1, 0);

		public static SKConfettiEmitter Stream(int particleRate, double duration) =>
			new SKConfettiEmitter(particleRate, -1, duration);

		public static SKConfettiEmitter Infinite(int particleRate) =>
			new SKConfettiEmitter(particleRate, -1, -1);

		public static SKConfettiEmitter Infinite(int particleRate, int maxParticles) =>
			new SKConfettiEmitter(particleRate, maxParticles, -1);
	}
}
