using System.Collections.ObjectModel;
using System.IO;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class BlurHashPage : ContentPage
	{
		private ImageSource? source;
		private int componentsX = 4;
		private int componentsY = 3;
		private string hash = @"LEHV6nWB2yk8pyo0adR*.7kCMdnj";
		private int punch = 1;

		public BlurHashPage()
		{
			InitializeComponent();

			Sources = new ObservableCollection<ImageSource>
			{
				ImageSource.FromStream(() => GetImage("img1.jpg")),
				ImageSource.FromStream(() => GetImage("img2.jpg")),
				ImageSource.FromStream(() => GetImage("img3.jpg")),
				ImageSource.FromStream(() => GetImage("img4.jpg")),
				ImageSource.FromStream(() => GetImage("img5.jpg")),
				null,
			};

			BindingContext = this;

			static Stream GetImage(string name)
			{
				var assembly = typeof(App).Assembly;
				return assembly.GetManifestResourceStream("SkiaSharpDemo.images." + name);
			}
		}

		public ObservableCollection<ImageSource> Sources { get; }

		public string BlurHash
		{
			get => hash;
			set
			{
				hash = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(BlurImage));
			}
		}

		public int Punch
		{
			get => punch;
			set
			{
				punch = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(BlurImage));
			}
		}

		public ImageSource? Source
		{
			get => source;
			set
			{
				source = value;
				OnPropertyChanged();

				//var blurHash = new SKBlurHash();
				//BlurHash = blurHash.Encode();
			}
		}

		public ImageSource? BlurImage
		{
			get
			{
				try
				{
					var blurHash = new SKBlurHash();
					var bmp = blurHash.DecodeBitmap(BlurHash, 20, 12, Punch);
					return new SKBitmapImageSource { Bitmap = bmp };
				}
				catch
				{
					return null;
				}
			}
		}
	}
}
