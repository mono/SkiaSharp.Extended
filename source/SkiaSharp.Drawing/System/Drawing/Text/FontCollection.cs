namespace System.Drawing.Text
{
	public abstract partial class FontCollection : System.IDisposable
	{
		internal FontCollection() {}
		public System.Drawing.FontFamily[] Families { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		internal virtual void Dispose(bool disposing) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~FontCollection() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
