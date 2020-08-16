using SkiaSharp.Extended.Controls.Themes;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiView : TemplatedView
	{
		public SKConfettiView()
		{
			Generic.EnsureRegistered();
		}

		protected override void OnApplyTemplate()
		{
			var templateChild = GetTemplateChild("PART_DrawingSurface");
			if (templateChild is SKCanvasView canvasView)
			{
				canvasView.PaintSurface += OnPaintSurface;
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// TODO: write the control
		}
	}
}
