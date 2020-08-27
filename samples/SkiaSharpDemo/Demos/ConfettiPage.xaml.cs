using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class ConfettiPage : ContentPage
	{
		private static readonly Random random = new Random();

		private static readonly SKPath heartGeometry = SKPath.ParseSvgPathData("M 311.92745,171.20458 170.5061,312.62594 29.08474,171.20458 A 100,100 0 0 1 170.5061,29.783225 100,100 0 0 1 311.92745,171.20458 Z");

		public ConfettiPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public ObservableCollection<string> AddTypes { get; } = new ObservableCollection<string>
		{
			"Top",
		};

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

		private void OnTapped(object sender, EventArgs e)
		{
			confettiView.Systems!.Add(new SKConfettiSystem
			{
				Emitter = SKConfettiEmitter.Burst(200),
				Colors = new SKConfettiColorCollection(Colors),
				Shapes = new SKConfettiShapeCollection(GetShapes(Shapes).SelectMany(s => s)),
			});
		}

		private static IEnumerable<IEnumerable<SKConfettiShape>> GetShapes(IEnumerable<string> shapes)
		{
			foreach (var shape in shapes)
			{
				yield return shape.ToLowerInvariant() switch
				{
					"square" => new SKConfettiShape[] { new SKConfettiSquare(), new SKConfettiRect(0.5) },
					"circle" => new SKConfettiShape[] { new SKConfettiCircle(), new SKConfettiOval(0.5) },
					"line" => new[] { new SKConfettiRect(0.1) },
					"heart" => new[] { new SKConfettiPath(heartGeometry) },
					"star" => new[] { new SKConfettiStar(5) },
				};
			}
		}
	}

	public class SKConfettiStar : SKConfettiShape
	{
		public SKConfettiStar(int points)
		{
			Points = points;
		}

		public int Points { get; }

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			using var star = SKGeometry.CreateRegularStarPath(size, size / 2, Points);

			canvas.DrawPath(star, paint);
		}
	}
}
