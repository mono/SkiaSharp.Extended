using System.Numerics;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos.Morphysics;

public partial class MorphysicsPlaygroundPage : ContentPage
{
	private readonly AnimatedCanvasView canvas;
	private readonly ParticleEmitter emitter;

	public MorphysicsPlaygroundPage()
	{
		InitializeComponent();

		canvas = canvasView;
		canvas.SetDeterministicSeed(42);

		// Create particle emitter
		emitter = new ParticleEmitter
		{
			EmissionRate = 30f,
			MaxParticles = 300,
			ParticleLifetime = 5f,
			InitialVelocity = new Vector2(0, -80),
			VelocityVariance = 40f,
			ParticleColor = Colors.Orange,
			ParticleRadius = 4f
		};

		canvas.AddEmitter(emitter);

		// Configure physics
		canvas.Physics.Gravity = new Vector2(0, 150f);
		canvas.Physics.EnableCollisions = true;
		canvas.Physics.Restitution = 0.6f;

		// Add initial attractor
		canvas.Physics.AddAttractor("attractor1", new Vector2(200, 300), 300f);

		BindingContext = this;
	}

	public int ParticleCount => canvas.Physics.Particles.Count;

	public float AttractorStrength
	{
		get => 300f;
		set
		{
			// Update attractor strength
			canvas.Physics.RemoveAttractor("attractor1");
			if (value > 0)
			{
				var centerX = (float)(Width / 2);
				var centerY = (float)(Height / 2);
				canvas.Physics.AddAttractor("attractor1", new Vector2(centerX, centerY), value);
			}
			OnPropertyChanged();
		}
	}

	private bool stickyZoneEnabled = false;
	public bool StickyZoneEnabled
	{
		get => stickyZoneEnabled;
		set
		{
			stickyZoneEnabled = value;
			if (value)
			{
				var centerX = (float)(Width / 2);
				var centerY = (float)(Height / 2);
				canvas.Physics.AddStickyZone("sticky1", new Vector2(centerX, centerY), 80f, 0.3f);
			}
			else
			{
				// Clear sticky zones (no remove method, so we'd need to add one)
			}
			OnPropertyChanged();
		}
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

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		// Update positions when page appears
		var timer = Dispatcher.CreateTimer();
		timer.Interval = TimeSpan.FromMilliseconds(100);
		timer.Tick += (s, e) =>
		{
			if (!IsLoaded)
			{
				timer.Stop();
				return;
			}

			emitter.SpawnPosition = new Vector2((float)(Width / 2), 50);
			OnPropertyChanged(nameof(ParticleCount));
		};
		timer.Start();
	}

	private void OnSpawnBurstClicked(object sender, EventArgs e)
	{
		var particles = emitter.EmitBurst(50);
		foreach (var particle in particles)
		{
			// Randomize spawn position
			var random = new Random();
			particle.Position = new Vector2(
				(float)(random.NextDouble() * Width),
				(float)(random.NextDouble() * Height / 2));
			canvas.Physics.AddParticle(particle);
		}
	}

	private void OnResetClicked(object sender, EventArgs e)
	{
		canvas.Physics.ClearParticles();
		canvas.SetDeterministicSeed((int)DateTime.Now.Ticks);
		OnPropertyChanged(nameof(ParticleCount));
	}
}
