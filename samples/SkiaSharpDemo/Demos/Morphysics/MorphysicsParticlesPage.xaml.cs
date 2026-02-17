using System.Numerics;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos.Morphysics;

public partial class MorphysicsParticlesPage : ContentPage
{
	private readonly AnimatedCanvasView canvas;
	private readonly ParticleEmitter emitter;
	private IDispatcherTimer? updateTimer;

	public MorphysicsParticlesPage()
	{
		InitializeComponent();

		canvas = canvasView;
		canvas.SetDeterministicSeed(42);

		// Create particle emitter
		emitter = new ParticleEmitter
		{
			EmissionRate = 60f,
			MaxParticles = 500,
			ParticleLifetime = 3f,
			InitialVelocity = new Vector2(0, -100),
			VelocityVariance = 30f,
			ParticleColor = Colors.DeepSkyBlue,
			ParticleRadius = 5f
		};

		canvas.AddEmitter(emitter);

		// Configure physics
		canvas.Physics.Gravity = new Vector2(0, 200f);
		canvas.Physics.EnableCollisions = true;
		canvas.Physics.Restitution = 0.7f;

		BindingContext = this;
	}

	public int EmissionRate
	{
		get => (int)emitter.EmissionRate;
		set
		{
			emitter.EmissionRate = value;
			OnPropertyChanged();
		}
	}

	public float GravityY
	{
		get => canvas.Physics.Gravity.Y;
		set
		{
			canvas.Physics.Gravity = new Vector2(0, value);
			OnPropertyChanged();
		}
	}

	public float Restitution
	{
		get => canvas.Physics.Restitution;
		set
		{
			canvas.Physics.Restitution = value;
			OnPropertyChanged();
		}
	}

	public bool CollisionsEnabled
	{
		get => canvas.Physics.EnableCollisions;
		set
		{
			canvas.Physics.EnableCollisions = value;
			OnPropertyChanged();
		}
	}

	public int ParticleCount => canvas.Physics.Particles.Count;

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		// Update spawn position when page appears
		updateTimer = Dispatcher.CreateTimer();
		updateTimer.Interval = TimeSpan.FromMilliseconds(100);
		updateTimer.Tick += (s, e) =>
		{
			if (!IsLoaded)
			{
				updateTimer?.Stop();
				return;
			}

			emitter.SpawnPosition = new Vector2((float)(Width / 2), 50);
			OnPropertyChanged(nameof(ParticleCount));
		};
		updateTimer.Start();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		updateTimer?.Stop();
	}

	private void OnBurstClicked(object sender, EventArgs e)
	{
		var particles = emitter.EmitBurst(100);
		foreach (var particle in particles)
		{
			canvas.Physics.AddParticle(particle);
		}
	}

	private void OnClearClicked(object sender, EventArgs e)
	{
		canvas.Physics.ClearParticles();
		OnPropertyChanged(nameof(ParticleCount));
	}
}
