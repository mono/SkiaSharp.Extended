namespace System.Drawing.Imaging
{
	/// <summary>
	///  Specifies the attributes of a bitmap image. The <see cref="BitmapData"/> class is used by the
	///  <see cref="System.Drawing.Bitmap.LockBits(System.Drawing.Rectangle, ImageLockMode, PixelFormat)"/> and
	///  <see cref="System.Drawing.Bitmap.UnlockBits(BitmapData)"/> methods of the <see cref="System.Drawing.Bitmap"/> class.
	/// </summary>
	public sealed partial class BitmapData
	{
		private int _width;
		private int _height;
		private int _stride;
		private PixelFormat _pixelFormat;
		private nint _scan0;
		private int _reserved;

		/// <summary>
		///  Initializes a new instance of the <see cref="BitmapData"/> class.
		/// </summary>
		public BitmapData() { }

		/// <summary>
		///  Gets or sets the pixel height of the <see cref="System.Drawing.Bitmap"/> object. Also sometimes referred to as the number of scan lines.
		/// </summary>
		public int Height { get { return _height; } set { _height = value; } }

		/// <summary>
		///  Gets or sets the format of the pixel information in the <see cref="System.Drawing.Bitmap"/> object that returned this <see cref="BitmapData"/> object.
		/// </summary>
		public System.Drawing.Imaging.PixelFormat PixelFormat { get { return _pixelFormat; } set { _pixelFormat = value; } }

		/// <summary>
		///  Reserved. Do not use.
		/// </summary>
		public int Reserved { get { return _reserved; } set { _reserved = value; } }

		/// <summary>
		///  Gets or sets the address of the first pixel data in the bitmap. This can also be thought of as the first scan line in the bitmap.
		/// </summary>
		public nint Scan0 { get { return _scan0; } set { _scan0 = value; } }

		/// <summary>
		///  Gets or sets the stride width (also called scan width) of the <see cref="System.Drawing.Bitmap"/> object.
		/// </summary>
		public int Stride { get { return _stride; } set { _stride = value; } }

		/// <summary>
		///  Gets or sets the pixel width of the <see cref="System.Drawing.Bitmap"/> object. This can also be thought of as the number of pixels in one scan line.
		/// </summary>
		public int Width { get { return _width; } set { _width = value; } }
	}
}
