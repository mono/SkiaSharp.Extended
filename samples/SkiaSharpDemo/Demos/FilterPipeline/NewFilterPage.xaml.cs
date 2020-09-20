using System;
using System.Collections.Generic;
using System.Windows.Input;
using SkiaSharp.Extended.UI.Media;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class NewFilterPage : ContentPage
	{
		private ICommand? pickedCommand;

		public NewFilterPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public ICommand? PickedCommand
		{
			get => pickedCommand;
			set
			{
				pickedCommand = value;
				OnPropertyChanged();
			}
		}

		public Dictionary<string, Type> PotentialFilters { get; } = new Dictionary<string, Type>
		{
			{ "Blur", typeof(SKBlurFilter) },
			{ "High Contrast", typeof(SKHighContrastFilter) },
			{ "Invert", typeof(SKInvertFilter) },
			{ "Sepia", typeof(SKSepiaFilter) },
		};
	}
}
