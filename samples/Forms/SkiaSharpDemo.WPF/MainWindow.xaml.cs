using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace SkiaSharpDemo.WPF
{
	public partial class MainWindow : FormsApplicationPage
	{
		public MainWindow()
		{
			InitializeComponent();

			Forms.Init();
			LoadApplication(new SkiaSharpDemo.App());
		}
	}
}
