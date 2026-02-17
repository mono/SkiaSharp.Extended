using System.Numerics;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// GPU-accelerated canvas for rendering interactive scenes with physics and morphing.
/// </summary>
public class AnimatedCanvasView : SKAnimatedSurfaceView
{
	public static readonly BindableProperty RootProperty = BindableProperty.Create(
		nameof(Root),
		typeof(SceneNode),
		typeof(AnimatedCanvasView),
		null);

	private readonly PhysicsWorld physics = new PhysicsWorld();
	private readonly List<ParticleEmitter> emitters = new List<ParticleEmitter>();
	private readonly SKPaint particlePaint = new SKPaint
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill
	};

	private bool hasRendered = false;

	/// <summary>
	/// Event fired when the canvas is ready for rendering.
	/// </summary>
	public event EventHandler? CanvasReady;

	public AnimatedCanvasView()
	{
		IsAnimationEnabled = true;
	}

	/// <summary>
	/// Gets or sets the root node of the scene graph.
	/// </summary>
	public SceneNode? Root
	{
		get => (SceneNode?)GetValue(RootProperty);
		set => SetValue(RootProperty, value);
	}

	/// <summary>
	/// Gets the physics simulation world.
	/// </summary>
	public PhysicsWorld Physics => physics;

	/// <summary>
	/// Sets the random seed for deterministic physics replay.
	/// </summary>
	public void SetDeterministicSeed(int seed)
	{
		physics.SetSeed(seed);
	}

	/// <summary>
	/// Adds a particle emitter to the canvas.
	/// </summary>
	public void AddEmitter(ParticleEmitter emitter)
	{
		if (emitter == null)
			throw new ArgumentNullException(nameof(emitter));

		emitters.Add(emitter);
	}

	/// <summary>
	/// Finds a scene node by its string ID.
	/// </summary>
	public T? FindNodeById<T>(string id) where T : SceneNode
	{
		if (Root == null)
			return null;

		return FindNodeByIdRecursive<T>(Root, id);
	}

	protected override void Update(TimeSpan deltaTime)
	{
		var dt = (float)deltaTime.TotalSeconds;

		// Update physics
		physics.Step(dt);

		// Update particle emitters
		foreach (var emitter in emitters)
		{
			var newParticles = emitter.Update(dt, physics.Particles.Count);
			foreach (var particle in newParticles)
			{
				physics.AddParticle(particle);
			}
		}
	}

	protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
		canvas.Clear(SKColors.Transparent);

		// Fire canvas ready event after first render
		if (!hasRendered)
		{
			hasRendered = true;
			CanvasReady?.Invoke(this, EventArgs.Empty);
		}

		// Render scene graph
		if (Root != null)
		{
			Root.Render(canvas, size);
		}

		// Render particles
		foreach (var particle in physics.Particles)
		{
			particlePaint.Color = particle.Color;
			canvas.DrawCircle(particle.Position.X, particle.Position.Y, particle.Radius, particlePaint);
		}
	}

	private static T? FindNodeByIdRecursive<T>(SceneNode node, string id) where T : SceneNode
	{
		if (node.Id == id && node is T typedNode)
			return typedNode;

		foreach (var child in node.Children)
		{
			var found = FindNodeByIdRecursive<T>(child, id);
			if (found != null)
				return found;
		}

		return null;
	}
}
