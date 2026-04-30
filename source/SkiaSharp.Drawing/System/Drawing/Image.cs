namespace System.Drawing
{
	[System.ComponentModel.EditorAttribute("System.Drawing.Design.ImageEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[System.ComponentModel.ImmutableObjectAttribute(true)]
	[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.ImageConverter))]
	public abstract partial class Image : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
	{
		internal Image() {}
		public delegate bool GetThumbnailImageAbort();
		[System.ComponentModel.BrowsableAttribute(false)]
		public int Flags { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Guid[] FrameDimensionsList { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int Height { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public float HorizontalResolution { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.Imaging.ColorPalette Palette { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public System.Drawing.SizeF PhysicalDimension { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public System.Drawing.Imaging.PixelFormat PixelFormat { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public int[] PropertyIdList { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.Imaging.PropertyItem[] PropertyItems { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public System.Drawing.Imaging.ImageFormat RawFormat { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public System.Drawing.Size Size { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.DefaultValueAttribute(null)]
		[System.ComponentModel.LocalizableAttribute(false)]
		public object? Tag { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public float VerticalResolution { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		[System.ComponentModel.BrowsableAttribute(false)]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int Width { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
		public static System.Drawing.Image FromFile(string filename) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Image FromFile(string filename, bool useEmbeddedColorManagement) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Bitmap FromHbitmap(nint hbitmap) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Bitmap FromHbitmap(nint hbitmap, nint hpalette) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Image FromStream(System.IO.Stream stream) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static System.Drawing.Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement, bool validateImageData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static int GetPixelFormatSize(System.Drawing.Imaging.PixelFormat pixfmt) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static bool IsAlphaPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static bool IsCanonicalPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public static bool IsExtendedPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public object Clone() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.RectangleF GetBounds(ref System.Drawing.GraphicsUnit pageUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Imaging.EncoderParameters? GetEncoderParameterList(System.Guid encoder) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int GetFrameCount(System.Drawing.Imaging.FrameDimension dimension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Imaging.PropertyItem? GetPropertyItem(int propid) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public System.Drawing.Image GetThumbnailImage(int thumbWidth, int thumbHeight, System.Drawing.Image.GetThumbnailImageAbort? callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void RemovePropertyItem(int propid) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void RotateFlip(System.Drawing.RotateFlipType rotateFlipType) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Save(System.IO.Stream stream, System.Drawing.Imaging.ImageCodecInfo encoder, System.Drawing.Imaging.EncoderParameters? encoderParams) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Save(System.IO.Stream stream, System.Drawing.Imaging.ImageFormat format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Save(string filename) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Save(string filename, System.Drawing.Imaging.ImageCodecInfo encoder, System.Drawing.Imaging.EncoderParameters? encoderParams) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void Save(string filename, System.Drawing.Imaging.ImageFormat format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void SaveAdd(System.Drawing.Image image, System.Drawing.Imaging.EncoderParameters? encoderParams) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void SaveAdd(System.Drawing.Imaging.EncoderParameters? encoderParams) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public int SelectActiveFrame(System.Drawing.Imaging.FrameDimension dimension, int frameIndex) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		public void SetPropertyItem(System.Drawing.Imaging.PropertyItem propitem) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		internal virtual void Dispose(bool disposing) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		~Image() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
	}
}
