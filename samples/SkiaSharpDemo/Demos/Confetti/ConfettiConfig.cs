using System.Collections.ObjectModel;
using SkiaSharp;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public class ConfettiConfig : BindableObject
{
	private static readonly SKPath heartGeometry = SKPath.ParseSvgPathData("M 311.92745,171.20458 170.5061,312.62594 29.08474,171.20458 A 100,100 0 0 1 170.5061,29.783225 100,100 0 0 1 311.92745,171.20458 Z");

	private int minSpeed = 100;
	private int maxSpeed = 200;
	private double lifetime = 2;
	private double duration = 5;

	public ConfettiConfig(int numberOfSystems = 1)
	{
		NumberOfSystems = numberOfSystems;
	}

	public int NumberOfSystems { get; }

	public int MinSpeed
	{
		get => minSpeed;
		set
		{
			minSpeed = value;
			OnPropertyChanged();
		}
	}

	public int MaxSpeed
	{
		get => maxSpeed;
		set
		{
			maxSpeed = value;
			OnPropertyChanged();
		}
	}

	public double Lifetime
	{
		get => lifetime;
		set
		{
			lifetime = value;
			OnPropertyChanged();
		}
	}

	public double Duration
	{
		get => duration;
		set
		{
			duration = value;
			OnPropertyChanged();
		}
	}

	public ObservableCollection<string> Shapes { get; } = new ObservableCollection<string>
	{
		"Square",
		"Circle",
		"Line",
	};

	public ObservableCollection<Color> Colors { get; } = new ObservableCollection<Color>
	{
		Color.FromUint(0xfffce18a),
		Color.FromUint(0xffff726d),
		Color.FromUint(0xfff4306d),
		Color.FromUint(0xffb48def),
	};

	public Action<int, SKConfettiSystem>? OnCreateSystem { get; set; }

	public IEnumerable<SKConfettiSystem> CreateSystems()
	{
		for (var i = 0; i < NumberOfSystems; i++)
		{
			var system = new SKConfettiSystem
			{
				Lifetime = Lifetime,
				Emitter = SKConfettiEmitter.Stream(100, Duration),
				EmitterBounds = SKConfettiEmitterBounds.Top,
				Colors = new SKConfettiColorCollection(Colors),
				Shapes = new SKConfettiShapeCollection(GetShapes(Shapes).SelectMany(s => s)),
				MinimumInitialVelocity = MinSpeed,
				MaximumInitialVelocity = MaxSpeed,
			};

			OnCreateSystem?.Invoke(i, system);

			yield return system;
		}
	}

	private static IEnumerable<IEnumerable<SKConfettiShape>> GetShapes(IEnumerable<string> shapes)
	{
		foreach (var shape in shapes)
		{
			yield return shape.ToLowerInvariant() switch
			{
				"square" => new SKConfettiShape[] { new SKConfettiSquareShape(), new SKConfettiRectShape(0.5) },
				"circle" => new SKConfettiShape[] { new SKConfettiCircleShape(), new SKConfettiOvalShape(0.5) },
				"line" => new[] { new SKConfettiRectShape(0.1) },
				"heart" => new[] { new SKConfettiPathShape(heartGeometry) },
				"star" => new[] { new ConfettiStar(5) },
				_ => throw new ArgumentOutOfRangeException(nameof(shape)),
			};
		}
	}
}
