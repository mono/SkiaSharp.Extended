using NLipsum.Core;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Topten.RichTextKit;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class RichTextKitPage : ContentPage
	{
		private RichString richString;

		public RichTextKitPage()
		{
			InitializeComponent();

			richString = new RichString()
				.MarginLeft(12)
				.MarginTop(12)
				.MarginRight(12)
				.MarginBottom(12)
				.FontSize(18)
				.FontFamily("Segoe UI")
				.Add("Welcome to RichTextKit!\n", fontSize: 24, fontWeight: 700)
				.Add("\nRichTextKit is a rich text layout, rendering and measurement library for SkiaSharp.\n\nIt supports normal, ")
				.Add("bold", fontWeight: 700)
				.Add(", ")
				.Add("italic", fontItalic: true)
				.Add(", ")
				.Add("underline", underline: UnderlineStyle.Gapped)
				.Add(" (including ")
				.Add("gaps over descenders", underline: UnderlineStyle.Gapped)
				.Add("), ")
				.Add("strikethrough", strikeThrough: StrikeThroughStyle.Solid)
				.Add(", superscript (E=mc")
				.Add("2", fontVariant: FontVariant.SuperScript)
				.Add("), subscript (H")
				.Add("2", fontVariant: FontVariant.SubScript)
				.Add("O), ")
				.Add("colored ", textColor: SKColors.Red)
				.Add("text", textColor: SKColors.Blue)
				.Add(" and ")
				.Add("mixed ")
				.Add("sizes, ", fontSize: 12)
				.Add("widths", fontFamily: "Consolas")
				.Add(" and ")
				.Add("fonts", fontFamily: "Segoe Script")
				.Add(".\n\n")
				.Add("Font fallback means emojis work: 🌐 🍪 🍕 🚀 and ")
				.Add("text shaping and bi-directional text support means complex scripts and languages like Arabic: مرحبا بالعالم, Japanese: ハローワールド, Chinese: 世界您好 and Hindi: हैलो वर्ल्ड are rendered correctly!\n\n")
				.Add("RichTextKit also supports left/center/right text alignment, word wrapping, truncation with ellipsis place-holder, text measurement, hit testing, painting a selection range, caret position & shape helpers.")
				.Paragraph()
				.Add(LipsumGenerator.Generate(1));
		}

		private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Transparent);
			canvas.Scale(e.Info.Width / (float)Width);

			richString.Paint(canvas);
		}

		private void OnPageSizeChanged(object sender, System.EventArgs e)
		{
			richString.MaxWidth = (float)Width;

			skiaView.HeightRequest = richString.MeasuredHeight;
		}
	}
}
