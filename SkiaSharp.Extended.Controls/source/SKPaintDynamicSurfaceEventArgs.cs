using System;
using SkiaSharp.Views.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKPaintDynamicSurfaceEventArgs : EventArgs
	{
		private readonly SKPaintSurfaceEventArgs canvasEvent;
		private readonly SKPaintGLSurfaceEventArgs glEvent;

		public SKPaintDynamicSurfaceEventArgs(SKPaintSurfaceEventArgs e)
		{
			canvasEvent = e;
			IsHardwareAccelerated = false;
		}

		public SKPaintDynamicSurfaceEventArgs(SKPaintGLSurfaceEventArgs e)
		{
			glEvent = e;
			IsHardwareAccelerated = true;
		}

		public bool IsHardwareAccelerated { get; }

		public SKSurface Surface => canvasEvent?.Surface ?? glEvent?.Surface;

		public GRBackendRenderTarget BackendRenderTarget => glEvent?.BackendRenderTarget;

		public SKImageInfo Info => canvasEvent?.Info ?? new SKImageInfo(glEvent.BackendRenderTarget.Width, glEvent.BackendRenderTarget.Height, glEvent.ColorType);

		public SKColorType ColorType => canvasEvent?.Info.ColorType ?? glEvent?.ColorType ?? SKColorType.Unknown;

		public GRSurfaceOrigin Origin => glEvent?.Origin ?? GRSurfaceOrigin.TopLeft;
	}
}
