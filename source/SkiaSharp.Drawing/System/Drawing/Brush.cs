namespace System.Drawing
{
	public abstract partial class Brush : System.MarshalByRefObject, System.ICloneable, System.IDisposable
	{
		protected Brush() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public abstract object Clone();
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		protected virtual void Dispose(bool disposing) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~Brush() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		protected internal void SetNativeBrush(nint brush) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
