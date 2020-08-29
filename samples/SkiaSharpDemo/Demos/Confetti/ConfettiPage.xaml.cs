using System;
using System.Collections.Generic;
using SkiaSharp.Extended.Controls;
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

		public Dictionary<string, ConfettiConfig> Configs { get; } = new Dictionary<string, ConfettiConfig>
		{
			["Top"] = new ConfettiConfig(),
			["Center"] = new ConfettiConfig
			{
				MinSpeed = 30,
				MaxSpeed = 150,
				Lifetime = 4,
				OnCreateSystem = (i, system) =>
				{
					system.Emitter = SKConfettiEmitter.Burst(100);
					system.EmitterBounds = SKConfettiEmitterBounds.Center;
				}
			},
			["Sides"] = new ConfettiConfig
			{
			},
		};

		public ConfettiConfig CurrentConfig => Configs[ConfigName];

		public string Message => messages[ConfigName];

		private void OnTapped(object sender, EventArgs e)
		{
			foreach (var system in CurrentConfig.CreateSystems())
				confettiView.Systems!.Add(system);
		}
	}
}
