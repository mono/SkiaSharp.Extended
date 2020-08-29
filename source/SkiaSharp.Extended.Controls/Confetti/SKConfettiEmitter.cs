using System;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiEmitter : BindableObject
	{
		public static readonly BindableProperty ParticleRateProperty = BindableProperty.Create(
			nameof(ParticleRate),
			typeof(int),
			typeof(SKConfettiEmitter),
			100);

		public static readonly BindableProperty MaxParticlesProperty = BindableProperty.Create(
			nameof(MaxParticles),
			typeof(int),
			typeof(SKConfettiEmitter),
			-1);

		public static readonly BindableProperty DurationProperty = BindableProperty.Create(
			nameof(Duration),
			typeof(double),
			typeof(SKConfettiEmitter),
			5.0);

		private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
			nameof(IsComplete),
			typeof(bool),
			typeof(SKConfettiEmitter),
			false);

		public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

		private int totalParticles = 0;
		private double totalDuration = 0;

		public SKConfettiEmitter()
		{
			DebugUtils.LogPropertyChanged(this);
		}

		public SKConfettiEmitter(int particleRate, int maxParticles, double duration)
			: this()
		{
			ParticleRate = particleRate;
			MaxParticles = maxParticles;
			Duration = duration;
		}

		public int ParticleRate
		{
			get => (int)GetValue(ParticleRateProperty);
			set => SetValue(ParticleRateProperty, value);
		}

		public int MaxParticles
		{
			get => (int)GetValue(MaxParticlesProperty);
			set => SetValue(MaxParticlesProperty, value);
		}

		public double Duration
		{
			get => (double)GetValue(DurationProperty);
			set => SetValue(DurationProperty, value);
		}

		public bool IsComplete
		{
			get => (bool)GetValue(IsCompleteProperty);
			private set => SetValue(IsCompletePropertyKey, value);
		}

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
				Duration == 0 || // burst mode
				(MaxParticles > 0 && totalParticles >= MaxParticles) || // reached the max particles
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
