namespace System.Drawing.Text
{
    public abstract partial class FontCollection : System.IDisposable
    {
        public System.Drawing.FontFamily[] Families { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected virtual void Dispose(bool disposing) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected ~FontCollection() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
    }
}
