using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class BlurHashPage : ContentPage
	{
		private StreamImageSource? source;
		private int componentsX = 4;
		private int componentsY = 3;
		private string hash = @"LEHV6nWB2yk8pyo0adR*.7kCMdnj";
		private int punch = 1;
		private ImageSource? blur;

		public BlurHashPage()
		{
			InitializeComponent();

			PickImageCommand = new Command(OnPickImage);

			Sources = new ObservableCollection<StreamImageSource?>
			{
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img1.jpg")) },
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img2.jpg")) },
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img3.jpg")) },
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img4.jpg")) },
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img5.jpg")) },
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img6.png")) },
				new StreamImageSource { Stream = token => Task.FromResult(App.GetImageResourceStream("img7.png")) },
				null,
			};

			BindingContext = this;
		}

		public ICommand PickImageCommand { get; }

		public ObservableCollection<StreamImageSource?> Sources { get; }

		public StreamImageSource? Source
		{
			get => source;
			set
			{
				source = value;
				OnPropertyChanged();
				_ = UpdateHash();
			}
		}

		public int ComponentsX
		{
			get => componentsX;
			set
			{
				componentsX = value;
				OnPropertyChanged();
				_ = UpdateHash();
			}
		}

		public int ComponentsY
		{
			get => componentsY;
			set
			{
				componentsY = value;
				OnPropertyChanged();
				_ = UpdateHash();
			}
		}

		public string BlurHash
		{
			get => hash;
			set
			{
				hash = value;
				OnPropertyChanged();
				_ = UpdateBlur();
			}
		}

		public int Punch
		{
			get => punch;
			set
			{
				punch = value;
				OnPropertyChanged();
				_ = UpdateBlur();
			}
		}

		public ImageSource? BlurImage
		{
			get => blur;
			set
			{
				blur = value;
				OnPropertyChanged();
			}
		}

		private async Task UpdateHash()
		{
			if (Source == null)
				return;

			var stream = await Source.Stream(default);
			if (stream == null)
				return;

			BlurHash = await Task.Run(() =>
			{
				using var bitmap = SKBitmap.Decode(stream);

				var sw = new Stopwatch();
				sw.Start();

				var encoded = SKBlurHash.Serialize(bitmap, ComponentsX, ComponentsY);

				sw.Stop();
				Console.WriteLine($"Encode: {sw.ElapsedMilliseconds}ms");

				return encoded;
			});
		}

		private async Task UpdateBlur()
		{
			try
			{
				var bmp = await Task.Run(() =>
				{
					var sw = new Stopwatch();
					sw.Start();

					var decoded = SKBlurHash.DeserializeBitmap(BlurHash, 20, 12, Punch);

					sw.Stop();
					Console.WriteLine($"Decode: {sw.ElapsedMilliseconds}ms");

					return decoded;
				});

				BlurImage = new SKBitmapImageSource { Bitmap = bmp };
			}
			catch
			{
				BlurImage = null;
			}
		}

		private async void OnPickImage()
		{
			//var options = new MediaPickerOptions
			//{
			//	Title = "Custom Image",
			//};

			//var picker = MediaPicker.IsCaptureSupported
			//	? MediaPicker.CapturePhotoAsync(options)
			//	: MediaPicker.PickPhotoAsync(options);

			//var image = await picker;

			//if (image != null)
			//{
			//	var newSource = new StreamImageSource { Stream = _ => image.OpenReadAsync() };

			//	Sources[Sources.Count - 1] = newSource;
			//	Source = newSource;
			//}
		}
	}
}
