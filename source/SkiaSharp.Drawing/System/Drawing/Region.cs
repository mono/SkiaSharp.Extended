namespace System.Drawing
{
	public sealed partial class Region : System.MarshalByRefObject, System.IDisposable
	{
		public Region() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Region(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Region(System.Drawing.Drawing2D.RegionData rgnData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Region(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Region(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Region FromHrgn(nint hrgn) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Region Clone() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Complement(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Complement(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Complement(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Complement(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool Equals(System.Drawing.Region region, System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Exclude(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Exclude(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Exclude(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Exclude(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.RectangleF GetBounds(System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public nint GetHrgn(System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Drawing2D.RegionData? GetRegionData() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.RectangleF[] GetRegionScans(System.Drawing.Drawing2D.Matrix matrix) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Intersect(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Intersect(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Intersect(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Intersect(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsEmpty(System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsInfinite(System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.Point point) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.Point point, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.PointF point) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.PointF point, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.Rectangle rect, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(System.Drawing.RectangleF rect, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(int x, int y, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(int x, int y, int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(int x, int y, int width, int height, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(float x, float y) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(float x, float y, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(float x, float y, float width, float height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public bool IsVisible(float x, float y, float width, float height, System.Drawing.Graphics? g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void MakeEmpty() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void MakeInfinite() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void ReleaseHrgn(nint regionHandle) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Transform(System.Drawing.Drawing2D.Matrix matrix) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Translate(int dx, int dy) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Translate(float dx, float dy) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Union(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Union(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Union(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Union(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Xor(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Xor(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Xor(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Xor(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~Region() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
