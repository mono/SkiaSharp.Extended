using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class PlaygroundPage : ContentPage
	{
		public PlaygroundPage()
		{
			InitializeComponent();

			Image = new SKImageImageSource
			{
				Image = SKImage.FromEncodedData(App.GetImageResourceStream("img1.jpg"))
			};

			BindingContext = this;
		}

		public ImageSource Image { get; }
	}
}
