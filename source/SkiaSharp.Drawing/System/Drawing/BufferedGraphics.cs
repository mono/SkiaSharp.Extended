namespace System.Drawing
{
	public sealed partial class BufferedGraphics : System.IDisposable
	{
		internal BufferedGraphics() {}
		public System.Drawing.Graphics Graphics { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Render() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Render(System.Drawing.Graphics? target) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Render(nint targetDC) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
