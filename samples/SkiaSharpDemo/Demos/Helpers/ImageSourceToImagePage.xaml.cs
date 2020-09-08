using System;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class ImageSourceToImagePage : ContentPage
	{
		public ImageSourceToImagePage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public ImageSource FileImage =>
			new FileImageSource { File = "logo.png" };

		public ImageSource StreamImage =>
			new StreamImageSource { Stream = token => Task.Run(() => App.GetImageResourceStream("logo.png")) };

		public ImageSource UriImage =>
			new UriImageSource { Uri = new Uri("https://raw.githubusercontent.com/mono/SkiaSharp.Extended/main/images/logo.png") };

		public ImageSource FontImage =>
			new FontImageSource { Glyph = "S", FontFamily = "Arial", Color = Color.Black };

		public ImageSource SkiaSharpImage =>
			new SKBitmapImageSource { Bitmap = SKBitmap.Decode(App.GetImageResourceStream("logo.png")) };
	}
}
