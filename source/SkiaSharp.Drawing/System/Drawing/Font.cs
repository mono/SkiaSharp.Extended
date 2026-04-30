namespace System.Drawing
{
	[System.ComponentModel.EditorAttribute("System.Drawing.Design.FontEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.FontConverter))]
	public sealed partial class Font : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
	{
		public Font(System.Drawing.Font prototype, System.Drawing.FontStyle newStyle) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(System.Drawing.FontFamily family, float emSize) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.GraphicsUnit unit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(string familyName, float emSize) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(string familyName, float emSize, System.Drawing.FontStyle style) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Font(string familyName, float emSize, System.Drawing.GraphicsUnit unit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Bold { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.FontFamily FontFamily { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public byte GdiCharSet { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool GdiVerticalFont { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public int Height { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public bool IsSystemFont { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Italic { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		[System.ComponentModel.EditorAttribute("System.Drawing.Design.FontNameEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.FontConverter.FontNameConverter))]
		public string Name { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public string? OriginalFontName { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public float Size { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public float SizeInPoints { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Strikeout { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.FontStyle Style { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public string SystemFontName { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Underline { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.FontConverter.FontUnitConverter))]
		public System.Drawing.GraphicsUnit Unit { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public static System.Drawing.Font FromHdc(nint hdc) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Font FromHfont(nint hfont) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Font FromLogFont(object lf) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Font FromLogFont(object lf, nint hdc) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public object Clone() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public override bool Equals(object? obj) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public override int GetHashCode() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public float GetHeight() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public float GetHeight(System.Drawing.Graphics graphics) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public float GetHeight(float dpi) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public nint ToHfont() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void ToLogFont(object logFont) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void ToLogFont(object logFont, System.Drawing.Graphics graphics) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public override string ToString() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~Font() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
