namespace System.Drawing
{
	[System.ComponentModel.EditorAttribute("System.Drawing.Design.IconEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.IconConverter))]
	public sealed partial class Icon : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
	{
		public Icon(System.Drawing.Icon original, System.Drawing.Size size) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(System.Drawing.Icon original, int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(System.IO.Stream stream) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(System.IO.Stream stream, System.Drawing.Size size) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(System.IO.Stream stream, int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(string fileName) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(string fileName, System.Drawing.Size size) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(string fileName, int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public Icon(System.Type type, string resource) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		[System.ComponentModel.BrowsableAttribute(false)]
		public nint Handle { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public int Height { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public System.Drawing.Size Size { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public int Width { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public static System.Drawing.Icon? ExtractAssociatedIcon(string filePath) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Icon FromHandle(nint handle) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public object Clone() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Save(System.IO.Stream outputStream) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Bitmap ToBitmap() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public override string ToString() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~Icon() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
