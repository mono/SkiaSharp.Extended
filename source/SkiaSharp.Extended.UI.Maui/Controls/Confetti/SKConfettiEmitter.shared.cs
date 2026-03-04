using System;

namespace SkiaSharp.Extended.UI.Controls
{
	/// <summary>
	/// Controls the rate and duration of particle emission for a confetti system.
	/// </summary>
	public class SKConfettiEmitter : BindableObject
	{
		/// <summary>
		/// Identifies the <see cref="ParticleRate"/> bindable property.
		/// </summary>
		public static readonly BindableProperty ParticleRateProperty = BindableProperty.Create(
			nameof(ParticleRate),
			typeof(int),
			typeof(SKConfettiEmitter),
			100);

		/// <summary>
		/// Identifies the <see cref="MaxParticles"/> bindable property.
		/// </summary>
		public static readonly BindableProperty MaxParticlesProperty = BindableProperty.Create(
			nameof(MaxParticles),
			typeof(int),
			typeof(SKConfettiEmitter),
			-1);

		/// <summary>
		/// Identifies the <see cref="Duration"/> bindable property.
		/// </summary>
		public static readonly BindableProperty DurationProperty = BindableProperty.Create(
			nameof(Duration),
			typeof(double),
			typeof(SKConfettiEmitter),
			5.0);

		private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
			nameof(IsComplete),
			typeof(bool),
			typeof(SKConfettiEmitter),
			false,
			defaultBindingMode: BindingMode.OneWayToSource);

		/// <summary>
		/// Identifies the <see cref="IsComplete"/> bindable property.
		/// </summary>
		public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

		private int totalParticles = 0;
		private double totalDuration = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="SKConfettiEmitter"/> class.
		/// </summary>
		public SKConfettiEmitter()
		{
			DebugUtils.LogPropertyChanged(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SKConfettiEmitter"/> class with the specified settings.
		/// </summary>
		/// <param name="particleRate">The number of particles emitted per second.</param>
		/// <param name="maxParticles">The maximum number of particles, or -1 for unlimited.</param>
		/// <param name="duration">The emission duration in seconds, 0 for burst, or -1 for infinite.</param>
		public SKConfettiEmitter(int particleRate, int maxParticles, double duration)
			: this()
		{
			ParticleRate = particleRate;
			MaxParticles = maxParticles;
			Duration = duration;
		}

		/// <summary>
		/// Gets or sets the number of particles emitted per second.
		/// </summary>
		public int ParticleRate
		{
			get => (int)GetValue(ParticleRateProperty);
			set => SetValue(ParticleRateProperty, value);
		}

		/// <summary>
		/// Gets or sets the maximum total number of particles. Use -1 for unlimited.
		/// </summary>
		public int MaxParticles
		{
			get => (int)GetValue(MaxParticlesProperty);
			set => SetValue(MaxParticlesProperty, value);
		}

		/// <summary>
		/// Gets or sets the emission duration in seconds. Use 0 for burst mode or -1 for infinite.
		/// </summary>
		public double Duration
		{
			get => (double)GetValue(DurationProperty);
			set => SetValue(DurationProperty, value);
		}

		/// <summary>
		/// Gets a value indicating whether the emitter has completed emitting particles.
		/// </summary>
		public bool IsComplete
		{
			get => (bool)GetValue(IsCompleteProperty);
			private set => SetValue(IsCompletePropertyKey, value);
		}

		/// <summary>
		/// Occurs when new particles have been created.
		/// </summary>
		public event Action<int>? ParticlesCreated;

		/// <summary>
		/// Updates the emitter state for the given elapsed time.
		/// </summary>
		/// <param name="deltaTime">The time elapsed since the last update.</param>
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

		/// <summary>
		/// Creates a burst emitter that emits all particles at once.
		/// </summary>
		/// <param name="particles">The number of particles to emit.</param>
		/// <returns>A new burst-mode emitter.</returns>
		public static SKConfettiEmitter Burst(int particles) =>
			new SKConfettiEmitter(particles, -1, 0);

		/// <summary>
		/// Creates a stream emitter that emits particles over a specified duration.
		/// </summary>
		/// <param name="particleRate">The number of particles emitted per second.</param>
		/// <param name="duration">The emission duration in seconds.</param>
		/// <returns>A new stream-mode emitter.</returns>
		public static SKConfettiEmitter Stream(int particleRate, double duration) =>
			new SKConfettiEmitter(particleRate, -1, duration);

		/// <summary>
		/// Creates an infinite emitter with no particle limit.
		/// </summary>
		/// <param name="particleRate">The number of particles emitted per second.</param>
		/// <returns>A new infinite emitter.</returns>
		public static SKConfettiEmitter Infinite(int particleRate) =>
			new SKConfettiEmitter(particleRate, -1, -1);

		/// <summary>
		/// Creates an infinite emitter with a maximum particle limit.
		/// </summary>
		/// <param name="particleRate">The number of particles emitted per second.</param>
		/// <param name="maxParticles">The maximum total number of particles.</param>
		/// <returns>A new infinite emitter with a particle cap.</returns>
		public static SKConfettiEmitter Infinite(int particleRate, int maxParticles) =>
			new SKConfettiEmitter(particleRate, maxParticles, -1);
	}
}
