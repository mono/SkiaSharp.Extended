﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class InterpolationPage : ContentPage
	{
		private const string heart = "M480,437l-29-26.4c-103-93.4-171-155-171-230.6c0-61.6,48.4-110,110-110c34.8,0,68.2,16.2,90,41.8 C501.8,86.2,535.2,70,570,70c61.6,0,110,48.4,110,110c0,75.6-68,137.2-171,230.8L480,437z";
		private const string horse = "M595,82.1c1,1-1,2-1,2s-6.9,2-8.9,4.9c-2,2-4.9,8.8-4.9,8.8c3.9-1,8.9-2,13.8-4c1,0,2,1,3,2c1,0-11.8,4.9-14.8,6.9c-2,2-11.8,9.9-14.8,9.9c-2.9,0-9.9,1-9.9,1c1,2,2,3.9,3.9,6.9c0,0-6.9,4-6.9,4.9c-1,1-5.9,6.9-5.9,6.9s17.7,1.9,23.6-7.9c-5.9,9.8-19.7,19.7-48.2,19.7c-29.5,0-53.1-11.8-68.9-17.7c-16.7-6.9-38.4-14.8-56.1-14.8c-16.7,0-36.4,4.9-49.2,16.8c-22.6-8.8-54.1-4-68.9,9.8c-13.8,13.8-27.5,30.5-29.5,42.3c-2.9,12.9-9.8,43.3-19.7,57.2c-13.8,22.5-29.5,28.5-34.5,38.3c-4.9,9.9-4.9,30.5-4,30.5c2,1,8.9,0,12.8-2c7.9-2.9,29.5-25.6,37.4-36.4c7.9-10.9,34.5-58.1,38.4-74.8s7.9-33.5,19.7-42.3c12.8-8.8,28.5-4.9,28.5-3.9c0,0-14.7,11.8-15.7,44.3s18.7,28.6,8.8,49.2c-9.9,17.7-39.3,5.9-49.2,16.7c-7.9,8.9,0,40.3,0,46.2c0,6-3,33.5-4.9,40.4c-1,5.9,0,9.8-1,13.8c-1,3,6,3.9,6,3.9s-6,7.8-8.9,5.9c-2.9-1-4.9-1-6.9,0c-2,0-5.9,1.9-9.9,0L232.9,401c2,1,4.9,1.9,7.9,1c4-1,23.6-9.9,25.6-11.9c2.9-1,19.7-10.8,22.6-16.7c2-5.9,5.9-24.6,5.9-30.5c1-6,2-24.6,2-29.5s-1-13.8,0-17.7c2-2.9,4.9-6.9,8.9-8.9c4.9-1,10.8-1,11.8-1c2,0,18.7,2,21.6,2c3.9,0,19.7-2.9,23.6-5c4.9-0.9,7.8,0,8.9,2c2,1.9-2,4.9-2,5.9c-1,1-8.8,10.8-10.8,14.7c-2,4.9-8.8,13.8-6.9,17.7c2,3.9,2,4.9,7.8,7.9c5.9,1.9,28.5,13.8,41.3,25.6c13.8,12.7,26.6,28.4,28.6,36.4c2.9,8.9,7.8,9.8,10.8,9.8c3,1,8.9,2,8.9,5.9s-1,8.8-1,8.8l36.4,13.8c0,0,0-12.8-1-17.7c-1-5.9-6.9-11.8-11.8-17.7c-4.9-6.9-56-57.1-61-61c-4.9-3-8.9-6.9-9.8-14.7c-1-7.9,8.8-13.8,14.8-20.6c3.9-4.9,14.7-27.6,16.7-30.6c2-2.9,8.9-10.8,12.8-10.8c4.9,0,15.8,6.9,29.5,11.8c5.9,2,48.2,12.8,54.1,14.8c5.9,1,18.6,0,22.6,3.9c3.9,2.9,0,10.9-1,15.8c-1,5.9-11.8,27.5-11.8,27.5s2,7.8,2,13.8c0,6.9-2.9,31.5-5.9,39.3c-2,8.9-15.8,31.6-18.7,35.5c-2,2.9-4.9,4.9-4.9,9.9c0,4.9,8.8,6,11.8,9.8c4,3,1,8.8,0,14.8l39.4,16.7c0-2.9,2-7.9,0-9.9c-1-2.9-5.9-8.8-8.8-12.8c-2-2.9-8.9-13.8-10.8-15.8c-2-2.9-2-8.8,0-13.8c1-4.9,13.8-38.3,14.7-42.3c2-4.9,20.7-44.3,22.6-49.2c2-5.9,17.7-34.4,19.7-39.4c2-5.9,14.8-10.8,18.7-10.8c4.9,0,29.5,8.8,33.4,10.8c2.9,1,25.6,10.9,29.5,12.8c4.9,1.9,2,5.9-1,6.9c-2.9,1.9-39.4,26.5-42.3,27.5c-2.9,1-5.9,3.9-7.9,3.9c-2.9,0-6.9,3.1-6.9,4c0,2-1,5.9-5.9,5.9c-3.9,0-11.8-5.9-16.7-11.8c-6.9,3.9-11.8,6.9-14.8,12.8c-4.9,4.9-6.9,8.9-9.8,15.8c2,2,5.9,2.9,8.8,2.9h31.5c3,0,6.9-0.9,9.9-1.9c2.9-2,80.7-53.1,80.7-53.1s12.8-9.9,12.8-18.7c0-6.9-5.9-8.9-7.9-11.8c-3-1.9-20.7-13.8-23.6-15.7c-4-2.9-17.7-10.9-21.6-12.9c-3-1.9-13.8-5.8-13.8-5.8c3-8.9,5-15.8,5.9-17.7c1-2,1-19.7,2-22.7c0-2.9,5-15.7,6.9-17.7c2-2,6.9-17.7,7.9-20.7c1-1.9,8.8-24.6,12.8-24.6c3.9-1,7.9,2.9,11.8,2.9c4,1,18.7-1,26.6,0c6.9,1,15.8,9.9,17.7,10.8c2.9,1,9.8,3.9,11.8,3.9c1,0,10.8-6.9,10.8-8.8c0-2-6.9-5.9-7.9-5.9c-1-1-7.8-4.9-7.8-4.9c0,1,2.9-1.9,7.8-1.9c3.9,0,7.9,3.9,8.8,4.9c2,1,6.9,3.9,7.9,1.9c1-1,4.9-5.9,4.9-8.9c0-4-3.9-8.8-5.9-10.8s-24.6-23.6-26.6-24.6c-2.9-1-14.7-11.8-14.7-14.7c-1-2-6.9-6.9-7.9-7.9s-30.5-21.6-34.5-24.6c-3.9-2.9-7.9-7.8-7.9-12.7s-2-17.7-2-17.7s-6.9-1-9.8,1.9c-2.9,2-9.8,17.8-13.8,17.8c-10.9-2-24.6,1-24.6,2.9c1,2.9,10.8,1,10.8,1c0,1-3.9,5.9-6.9,5.9c-2,0-7.8,2-8.8,2.9c-2,0-5.9,3.1-5.9,3.1c2.9,0,5.9,0,9.8,0.9c0,0-5.9,4-8.9,4c-2.9,0-12.8,2.9-15.7,3.9c-2,1.9-9.9,7.9-9.9,7.9H589l1,2h4.9L595,82.1L595,82.1z";
		private const string star = "M480,50L423.8,182.6L280,194.8L389.2,289.4L356.4,430L480,355.4L480,355.4L603.6,430L570.8,289.4L680,194.8L536.2,182.6Z";
		private const string chair = "M638.9,259.3v-23.8H380.4c-0.7-103.8-37.3-200.6-37.3-200.6s-8.5,0-22.1,0C369.7,223,341.4,465,341.4,465h22.1 c0,0,11.4-89.5,15.8-191h210.2l11.9,191h22.1c0,0-5.3-96.6-0.6-205.7H638.9z";

		private readonly SKPath heartPath;
		private readonly SKPath horsePath;
		private readonly SKPath starPath;
		private readonly SKPath chairPath;

		private int interpolationValue;
		private List<SKPathInterpolation> interpolations;
		private bool timerEnabled;

		public InterpolationPage()
		{
			InitializeComponent();

			AnimateCommand = new Command(OnAnimateClicked);

			heartPath = SKPath.ParseSvgPathData(heart);
			horsePath = SKPath.ParseSvgPathData(horse);
			starPath = SKPath.ParseSvgPathData(star);
			chairPath = SKPath.ParseSvgPathData(chair);

			interpolations = new List<SKPathInterpolation>
			{
				new SKPathInterpolation(heartPath, horsePath),
				new SKPathInterpolation(horsePath, starPath),
				new SKPathInterpolation(starPath, chairPath),
				new SKPathInterpolation(chairPath, heartPath),
			};

			BindingContext = this;
		}

		public int Interpolation
		{
			get => interpolationValue;
			set
			{
				interpolationValue = value;
				OnPropertyChanged();
				interpolationCanvas.InvalidateSurface();
			}
		}

		public ICommand AnimateCommand { get; }

		private void OnAnimateClicked()
		{
			timerEnabled = !timerEnabled;
			if (timerEnabled)
			{
				Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
				{
					Interpolation = (Interpolation + 1) % 100;
					return timerEnabled;
				});
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			// clear the surface
			canvas.Clear(SKColors.Transparent);

			var fill = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.Gold,
			};

			var step = (Interpolation / 100f) * interpolations.Count;
			var interpolation = interpolations[Math.Min((int)step, interpolations.Count - 1)];
			var stepPercent = Interpolation == 100 ? 1 : step - (int)step;

			if (stepPercent <= 0)
				canvas.DrawPath(interpolation.From, fill);
			else if (stepPercent >= 1)
				canvas.DrawPath(interpolation.To, fill);
			else
				canvas.DrawPath(interpolation.Interpolate(stepPercent), fill);
		}
	}
}
