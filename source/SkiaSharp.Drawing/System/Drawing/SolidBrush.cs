namespace System.Drawing
{
	public sealed partial class SolidBrush : System.Drawing.Brush
	{
		public SolidBrush(System.Drawing.Color color) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Color Color { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public override object Clone() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		protected override void Dispose(bool disposing) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Creates an <see cref="SkiaSharp.SKPaint"/> configured for fill operations with this brush's color.
		/// </summary>
		internal override SkiaSharp.SKPaint CreatePaint() { throw new System.NotImplementedException("SolidBrush.CreatePaint is not yet implemented. The Brush-implementing agent will provide this."); }
	}
}
