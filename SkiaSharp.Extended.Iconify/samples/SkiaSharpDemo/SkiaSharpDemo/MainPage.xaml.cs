using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

using SkiaSharp.Extended.Iconify;

namespace SkiaSharpDemo
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			canvas.Clear(SKColors.White);

			var fontAwesome = "I {{fa-heart-o color=ff0000}} to {{fa-code}} on {{fa-windows color=1BA1E2}}!";
			var ionIcons = "{{ion-ios-cloud-download-outline color=0000ff}} the SkiaSharp source from {{ion-social-github}}.";
			var materialDesignIcons = "SkiaSharp works on {{mdi-apple}}, {{mdi-android color=a4c639}}, {{mdi-windows}} and {{mdi-linux}}!";
			var materialIcons = "SkiaSharp supports {{brush}} and {{photo color=006400}}!";
			var meteocons = "We love the {{mc-sun color=f9d71c}} and some {{mc-cloud-double-o}} s.";
			var simple = "We all {{icon-heart color=ff0000}} a {{icon-present}}!";
			var typicons = "SkiaSharp runs on {{typcn-device-desktop}}, {{typcn-device-laptop}}, {{typcn-device-phone}} and {{typcn-device-tablet}} devices!";
			var weather = "An {{wi-solar-eclipse}} is when the {{wi-day-sunny color=f9d71c}} is hidden (there might be {{wi-wind}}).";

			using (var textPaint = new SKPaint())
			{
				textPaint.IsAntialias = true;
				textPaint.TextSize = 48;
				textPaint.Typeface = SKTypeface.FromFamilyName("Arial");

				// the DrawIconifiedText method will re-calculate the text runs
				// it may be better to cache this using the:
				//     var runs = SKTextRun.Create(text, lookup);
				// and then drawing it using the DrawText method.
				var padding = 24;
				var yOffset = padding + textPaint.TextSize;

				canvas.DrawIconifiedText(fontAwesome, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(ionIcons, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(materialDesignIcons, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(materialIcons, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(meteocons, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(simple, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(typicons, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;

				canvas.DrawIconifiedText(weather, padding, yOffset, textPaint);
				yOffset += padding + textPaint.TextSize;
			}
		}
	}
}
