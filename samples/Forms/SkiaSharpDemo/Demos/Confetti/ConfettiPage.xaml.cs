using System;
using System.Collections.Generic;
using SkiaSharp.Extended.UI.Forms.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class ConfettiPage : ContentPage
	{
		private static readonly Dictionary<string, string> messages = new Dictionary<string, string>
		{
			["Top"] = "Tap to rain 🎊",
			["Center"] = "Tap to explode 🎊",
			["Sides"] = "Tap to spray 🎊",
		};

		private string configName = "Top";

		public ConfettiPage()
		{
			InitializeComponent();

			Configs = new Dictionary<string, ConfettiConfig>
			{
				["Top"] = new ConfettiConfig(),
				["Center"] = new ConfettiConfig
				{
					MinSpeed = 30,
					MaxSpeed = 150,
					Duration = 0,
					OnCreateSystem = (i, system) =>
					{
						system.Emitter = SKConfettiEmitter.Burst(100);
						system.EmitterBounds = SKConfettiEmitterBounds.Center;
					}
				},
				["Sides"] = new ConfettiConfig(2)
				{
					MinSpeed = 50,
					MaxSpeed = 400,
					Duration = 0,
					OnCreateSystem = (i, system) =>
					{
						system.Emitter = SKConfettiEmitter.Burst(100);
						if (i % 2 == 0)
						{
							system.EmitterBounds = SKConfettiEmitterBounds.Point(0, Height);
							system.StartAngle = -85;
							system.EndAngle = -35;
						}
						else
						{
							system.EmitterBounds = SKConfettiEmitterBounds.Point(Width, Height);
							system.StartAngle = 265;
							system.EndAngle = 215;
						}
					}
				},
			};

			BindingContext = this;
		}

		public int SelectedTab { get; set; } = 0;

		public string ConfigName
		{
			get => configName;
			set
			{
				configName = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CurrentConfig));
				OnPropertyChanged(nameof(Message));
			}
		}

		public Dictionary<string, ConfettiConfig> Configs { get; }

		public ConfettiConfig CurrentConfig => Configs[ConfigName];

		public string Message => messages[ConfigName];

		private void OnTapped(object sender, EventArgs e)
		{
			foreach (var system in CurrentConfig.CreateSystems())
				confettiView.Systems!.Add(system);
		}
	}
}
