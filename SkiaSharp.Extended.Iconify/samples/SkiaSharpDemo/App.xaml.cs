using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

using SkiaSharp.Extended.Iconify;

namespace SkiaSharpDemo
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			// register the default fonts that we want
			SKTextRunLookup.Instance.AddFontAwesome();
			SKTextRunLookup.Instance.AddIonIcons();
			SKTextRunLookup.Instance.AddMaterialDesignIcons();
			SKTextRunLookup.Instance.AddMaterialIcons();
			SKTextRunLookup.Instance.AddMeteocons();
			SKTextRunLookup.Instance.AddSimpleLineIcons();
			SKTextRunLookup.Instance.AddTypicons();
			SKTextRunLookup.Instance.AddWeatherIcons();

			MainPage = new NavigationPage(new MainPage());
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
