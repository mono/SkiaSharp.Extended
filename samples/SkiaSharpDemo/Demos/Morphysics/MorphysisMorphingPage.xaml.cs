using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos.Morphysics;

public partial class MorphysisMorphingPage : ContentPage
{
	private readonly AnimatedCanvasView canvas;
	private readonly VectorNode vectorNode;
	private readonly MorphTarget morphTarget;

	// SVG Path definitions
	private const string SquarePath = "M 50,50 L 350,50 L 350,350 L 50,350 Z";
	private const string CirclePath = "M 200,50 A 150,150 0 1,1 200,350 A 150,150 0 1,1 200,50 Z";
	private const string TrianglePath = "M 200,50 L 350,350 L 50,350 Z";
	private const string StarPath = "M 200,50 L 230,150 L 330,150 L 250,220 L 280,320 L 200,250 L 120,320 L 150,220 L 70,150 L 170,150 Z";
	private const string HeartPath = "M 200,100 C 200,80 180,60 160,60 C 140,60 120,80 120,100 C 120,130 140,160 200,220 C 260,160 280,130 280,100 C 280,80 260,60 240,60 C 220,60 200,80 200,100 Z";

	private string currentSourcePath = SquarePath;
	private string currentTargetPath = CirclePath;

	public MorphysisMorphingPage()
	{
		InitializeComponent();

		canvas = canvasView;
		canvas.SetDeterministicSeed(42);

		// Create vector node with morphing
		vectorNode = new VectorNode
		{
			Id = "morphing-shape",
			PathData = currentSourcePath,
			FillColor = Colors.DeepPink,
			StrokeColor = Colors.White,
			StrokeWidth = 2f,
			X = 0,
			Y = 0
		};

		morphTarget = new MorphTarget(currentSourcePath, currentTargetPath);
		vectorNode.SetMorphTarget(morphTarget);

		canvas.Root = vectorNode;

		BindingContext = this;
	}

	private float morphProgress = 0f;
	public float MorphProgress
	{
		get => morphProgress;
		set
		{
			morphProgress = value;
			vectorNode.MorphProgress = value;
			OnPropertyChanged();
		}
	}

	private string selectedEasing = "Linear";
	public string SelectedEasing
	{
		get => selectedEasing;
		set
		{
			selectedEasing = value;
			OnPropertyChanged();
			UpdateMorphTarget();
		}
	}

	private string selectedSource = "Square";
	public string SelectedSource
	{
		get => selectedSource;
		set
		{
			selectedSource = value;
			OnPropertyChanged();
			UpdatePaths();
		}
	}

	private string selectedTarget = "Circle";
	public string SelectedTarget
	{
		get => selectedTarget;
		set
		{
			selectedTarget = value;
			OnPropertyChanged();
			UpdatePaths();
		}
	}

	private void UpdatePaths()
	{
		currentSourcePath = GetPathForShape(SelectedSource);
		currentTargetPath = GetPathForShape(SelectedTarget);

		vectorNode.PathData = currentSourcePath;
		UpdateMorphTarget();
	}

	private void UpdateMorphTarget()
	{
		var newMorphTarget = new MorphTarget(currentSourcePath, currentTargetPath);
		vectorNode.SetMorphTarget(newMorphTarget);
		vectorNode.MorphProgress = MorphProgress;
	}

	private static string GetPathForShape(string shape) => shape switch
	{
		"Square" => SquarePath,
		"Circle" => CirclePath,
		"Triangle" => TrianglePath,
		"Star" => StarPath,
		"Heart" => HeartPath,
		_ => SquarePath
	};

	private async void OnAnimateClicked(object sender, EventArgs e)
	{
		try
		{
			// Animate morph progress from 0 to 1
			var duration = 2000; // 2 seconds
			var steps = 60;
			var stepDelay = duration / steps;

			for (int i = 0; i <= steps; i++)
			{
				MorphProgress = (float)i / steps;
				await Task.Delay(stepDelay);
			}

			// Animate back
			for (int i = steps; i >= 0; i--)
			{
				MorphProgress = (float)i / steps;
				await Task.Delay(stepDelay);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
		}
	}
}
