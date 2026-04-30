namespace System.Drawing
{
	[System.ComponentModel.EditorAttribute("System.Drawing.Design.BitmapEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public sealed partial class Bitmap : System.Drawing.Image
	{
		public Bitmap(System.Drawing.Image original) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(System.Drawing.Image original, System.Drawing.Size newSize) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(System.Drawing.Image original, int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(int width, int height, System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(int width, int height, System.Drawing.Imaging.PixelFormat format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(int width, int height, int stride, System.Drawing.Imaging.PixelFormat format, nint scan0) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(System.IO.Stream stream) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(System.IO.Stream stream, bool useIcm) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(string filename) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(string filename, bool useIcm) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Bitmap(System.Type type, string resource) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Bitmap FromHicon(nint hicon) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Bitmap FromResource(nint hinstance, string bitmapName) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Bitmap Clone(System.Drawing.Rectangle rect, System.Drawing.Imaging.PixelFormat format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Bitmap Clone(System.Drawing.RectangleF rect, System.Drawing.Imaging.PixelFormat format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public nint GetHbitmap() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public nint GetHbitmap(System.Drawing.Color background) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public nint GetHicon() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Color GetPixel(int x, int y) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Imaging.BitmapData LockBits(System.Drawing.Rectangle rect, System.Drawing.Imaging.ImageLockMode flags, System.Drawing.Imaging.PixelFormat format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Imaging.BitmapData LockBits(System.Drawing.Rectangle rect, System.Drawing.Imaging.ImageLockMode flags, System.Drawing.Imaging.PixelFormat format, System.Drawing.Imaging.BitmapData bitmapData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void MakeTransparent() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void MakeTransparent(System.Drawing.Color transparentColor) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void SetPixel(int x, int y, System.Drawing.Color color) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void SetResolution(float xDpi, float yDpi) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void UnlockBits(System.Drawing.Imaging.BitmapData bitmapdata) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
