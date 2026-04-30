namespace System.Drawing.Drawing2D
{
	public sealed partial class GraphicsPathIterator : System.MarshalByRefObject, System.IDisposable
	{
		public GraphicsPathIterator(System.Drawing.Drawing2D.GraphicsPath? path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int Count { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public int SubpathCount { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public int CopyData(ref System.Drawing.PointF[] points, ref byte[] types, int startIndex, int endIndex) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int Enumerate(ref System.Drawing.PointF[] points, ref byte[] types) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool HasCurve() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int NextMarker(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int NextMarker(out int startIndex, out int endIndex) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int NextPathType(out byte pathType, out int startIndex, out int endIndex) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int NextSubpath(System.Drawing.Drawing2D.GraphicsPath path, out bool isClosed) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Rewind() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~GraphicsPathIterator() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
