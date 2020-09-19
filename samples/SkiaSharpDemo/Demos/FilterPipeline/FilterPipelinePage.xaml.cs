using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Extended.UI.Media;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class FilterPipelinePage : ContentPage
	{
		private double blurSigma = 5;
		private bool enableFilters = true;
		private SKFilter selectedFilter;

		public FilterPipelinePage()
		{
			InitializeComponent();

			Image = new SKImageImageSource
			{
				Image = SKImage.FromEncodedData(App.GetImageResourceStream("img1.jpg")).ToRasterImage(true)
			};

			ToggleFilterCommand = new Command(OnToggleFilter);

			BindingContext = this;
		}

		public ImageSource Image { get; }

		public bool EnableFilters
		{
			get => enableFilters;
			set
			{
				enableFilters = value;
				OnPropertyChanged();
			}
		}

		public SKFilter SelectedFilter
		{
			get => selectedFilter;
			set
			{
				selectedFilter = value;
				OnPropertyChanged();
			}
		}

		public ICommand ToggleFilterCommand { get; }

		public double BlurSigma
		{
			get => blurSigma;
			set
			{
				blurSigma = value;
				OnPropertyChanged();
			}
		}

		private void OnToggleFilter()
		{
			EnableFilters = !EnableFilters;
		}
	}
}
