using SkiaSharp;
using System.Drawing.Imaging;
using System.Drawing.Internal;
using System.IO;

namespace System.Drawing
{
	/// <summary>
	///  An abstract base class that provides functionality for the <see cref="Bitmap"/> and Metafile descended classes.
	/// </summary>
	[System.ComponentModel.EditorAttribute("System.Drawing.Design.ImageEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[System.ComponentModel.ImmutableObjectAttribute(true)]
	[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.ImageConverter))]
	public abstract partial class Image : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
	{
		internal SKBitmap? SKBitmapBacking;
		internal ImageFormat _rawFormat = ImageFormat.MemoryBmp;
		internal float _horizontalResolution = 96f;
		internal float _verticalResolution = 96f;
		internal Imaging.PixelFormat _requestedPixelFormat;
		private object? _tag;
		private byte[]? _codecData;

		/// <summary>
		///  Initializes a new instance of the <see cref="Image"/> class.
		/// </summary>
		internal Image() {}

		/// <summary>
		///  Represents a callback method for the <see cref="GetThumbnailImage"/> method.
		/// </summary>
		public delegate bool GetThumbnailImageAbort();

		/// <summary>
		///  Gets attribute flags for the pixel data of this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public int Flags
		{
			get
			{
				ThrowIfDisposed();
				var flags = (int)(ImageFlags.HasRealPixelSize | ImageFlags.ColorSpaceRgb | ImageFlags.ReadOnly);
				if (SKBitmapBacking!.AlphaType != SKAlphaType.Opaque)
					flags |= (int)ImageFlags.HasAlpha;
				return flags;
			}
		}

		/// <summary>
		///  Gets an array of GUIDs that represent the dimensions of frames within this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Guid[] FrameDimensionsList
		{
			get
			{
				ThrowIfDisposed();
				return new Guid[] { FrameDimension.Page.Guid };
			}
		}

		/// <summary>
		///  Gets the height, in pixels, of this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int Height
		{
			get
			{
				ThrowIfDisposed();
				return SKBitmapBacking!.Height;
			}
		}

		/// <summary>
		///  Gets the horizontal resolution, in pixels per inch, of this <see cref="Image"/>.
		/// </summary>
		public float HorizontalResolution
		{
			get
			{
				ThrowIfDisposed();
				return _horizontalResolution;
			}
		}

		/// <summary>
		///  Gets or sets the color palette used for this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.Imaging.ColorPalette Palette
		{
			get
			{
				ThrowIfDisposed();
				return new ColorPalette();
			}
			set
			{
				ThrowIfDisposed();
				// No-op: SkiaSharp bitmaps do not use indexed palettes.
			}
		}

		/// <summary>
		///  Gets the width and height of this image.
		/// </summary>
		public System.Drawing.SizeF PhysicalDimension
		{
			get
			{
				ThrowIfDisposed();
				return new SizeF(SKBitmapBacking!.Width, SKBitmapBacking.Height);
			}
		}

		/// <summary>
		///  Gets the pixel format for this <see cref="Image"/>.
		/// </summary>
		public System.Drawing.Imaging.PixelFormat PixelFormat
		{
			get
			{
				ThrowIfDisposed();
				if (_requestedPixelFormat != 0)
					return _requestedPixelFormat;
				return SkiaConversions.ToPixelFormat(SKBitmapBacking!.ColorType);
			}
		}

		/// <summary>
		///  Gets IDs of the property items stored in this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public int[] PropertyIdList
		{
			get
			{
				ThrowIfDisposed();
				return Array.Empty<int>();
			}
		}

		/// <summary>
		///  Gets all the property items (pieces of metadata) stored in this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.Imaging.PropertyItem[] PropertyItems
		{
			get
			{
				ThrowIfDisposed();
				return Array.Empty<PropertyItem>();
			}
		}

		/// <summary>
		///  Gets the file format of this <see cref="Image"/>.
		/// </summary>
		public System.Drawing.Imaging.ImageFormat RawFormat
		{
			get
			{
				ThrowIfDisposed();
				return _rawFormat;
			}
		}

		/// <summary>
		///  Gets the width and height, in pixels, of this image.
		/// </summary>
		public System.Drawing.Size Size
		{
			get
			{
				ThrowIfDisposed();
				return new Size(SKBitmapBacking!.Width, SKBitmapBacking.Height);
			}
		}

		/// <summary>
		///  Gets or sets an object that provides additional data about the image.
		/// </summary>
		[System.ComponentModel.DefaultValueAttribute(null)]
		[System.ComponentModel.LocalizableAttribute(false)]
		public object? Tag { get { return _tag; } set { _tag = value; } }

		/// <summary>
		///  Gets the vertical resolution, in pixels per inch, of this <see cref="Image"/>.
		/// </summary>
		public float VerticalResolution
		{
			get
			{
				ThrowIfDisposed();
				return _verticalResolution;
			}
		}

		/// <summary>
		///  Gets the width, in pixels, of this <see cref="Image"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int Width
		{
			get
			{
				ThrowIfDisposed();
				return SKBitmapBacking!.Width;
			}
		}

		/// <summary>
		///  Creates an <see cref="Image"/> from the specified file.
		/// </summary>
		public static System.Drawing.Image FromFile(string filename)
		{
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			var bitmap = SKBitmap.Decode(filename);
			if (bitmap == null) throw new OutOfMemoryException("Cannot create image from the specified file: " + filename);
			var ext = Path.GetExtension(filename);
			var result = new Bitmap(bitmap);
			result._rawFormat = SkiaConversions.ImageFormatFromExtension(ext);
			// Store raw data for multi-frame support (e.g. animated GIF)
			try { result._codecData = File.ReadAllBytes(filename); } catch { }
			return result;
		}

		/// <summary>
		///  Creates an <see cref="Image"/> from the specified file using embedded color management information in that file.
		/// </summary>
		public static System.Drawing.Image FromFile(string filename, bool useEmbeddedColorManagement)
		{
			// useEmbeddedColorManagement is ignored; SkiaSharp handles color spaces internally.
			return FromFile(filename);
		}

		/// <summary>
		///  Creates a <see cref="Bitmap"/> from a Windows handle to a bitmap.
		/// </summary>
		public static System.Drawing.Bitmap FromHbitmap(nint hbitmap) { throw new System.PlatformNotSupportedException("FromHbitmap is not supported on this platform because it requires a Windows GDI handle."); }

		/// <summary>
		///  Creates a <see cref="Bitmap"/> from a Windows handle to a bitmap and a Windows handle to a palette.
		/// </summary>
		public static System.Drawing.Bitmap FromHbitmap(nint hbitmap, nint hpalette) { throw new System.PlatformNotSupportedException("FromHbitmap is not supported on this platform because it requires a Windows GDI handle."); }

		/// <summary>
		///  Creates an <see cref="Image"/> from the specified data stream.
		/// </summary>
		public static System.Drawing.Image FromStream(System.IO.Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			// Copy stream to byte array for codec support
			byte[] data;
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				data = ms.ToArray();
			}
			var bitmap = SKBitmap.Decode(data);
			if (bitmap == null) throw new ArgumentException("The stream does not contain a valid image.", nameof(stream));
			var result = new Bitmap(bitmap);
			result._rawFormat = ImageFormat.Png; // default when format cannot be determined from a stream
			result._codecData = data;
			return result;
		}

		/// <summary>
		///  Creates an <see cref="Image"/> from the specified data stream, optionally using embedded color management information in that stream.
		/// </summary>
		public static System.Drawing.Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement)
		{
			return FromStream(stream);
		}

		/// <summary>
		///  Creates an <see cref="Image"/> from the specified data stream, optionally using embedded color management information and validating the image data.
		/// </summary>
		public static System.Drawing.Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement, bool validateImageData)
		{
			return FromStream(stream);
		}

		/// <summary>
		///  Returns the number of bits per pixel for the specified pixel format.
		/// </summary>
		public static int GetPixelFormatSize(System.Drawing.Imaging.PixelFormat pixfmt)
		{
			return SkiaConversions.GetBitsPerPixel(pixfmt);
		}

		/// <summary>
		///  Returns a value that indicates whether the pixel format for this <see cref="Image"/> contains alpha information.
		/// </summary>
		public static bool IsAlphaPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt)
		{
			return (pixfmt & Imaging.PixelFormat.Alpha) != 0 || (pixfmt & Imaging.PixelFormat.PAlpha) != 0;
		}

		/// <summary>
		///  Returns a value that indicates whether the pixel format is canonical.
		/// </summary>
		public static bool IsCanonicalPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt)
		{
			return (pixfmt & Imaging.PixelFormat.Canonical) != 0;
		}

		/// <summary>
		///  Returns a value that indicates whether the pixel format is extended (64 bits per pixel).
		/// </summary>
		public static bool IsExtendedPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt)
		{
			return (pixfmt & Imaging.PixelFormat.Extended) != 0;
		}

		/// <summary>
		///  Creates an exact copy of this <see cref="Image"/>.
		/// </summary>
		public object Clone()
		{
			ThrowIfDisposed();
			var copy = SKBitmapBacking!.Copy();
			if (copy == null) throw new OutOfMemoryException("Failed to clone the image.");
			var result = new Bitmap(copy);
			result._rawFormat = _rawFormat;
			result._horizontalResolution = _horizontalResolution;
			result._verticalResolution = _verticalResolution;
			result._tag = _tag;
			return result;
		}

		/// <summary>
		///  Releases all resources used by this <see cref="Image"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///  Gets the bounds of the image in the specified unit.
		/// </summary>
		public System.Drawing.RectangleF GetBounds(ref System.Drawing.GraphicsUnit pageUnit)
		{
			ThrowIfDisposed();
			pageUnit = GraphicsUnit.Pixel;
			return new RectangleF(0, 0, SKBitmapBacking!.Width, SKBitmapBacking.Height);
		}

		/// <summary>
		///  Returns information about the encoder parameters supported by the specified image encoder.
		/// </summary>
		public System.Drawing.Imaging.EncoderParameters? GetEncoderParameterList(System.Guid encoder)
		{
			throw new System.PlatformNotSupportedException("GetEncoderParameterList is not supported in the SkiaSharp-backed System.Drawing implementation because SkiaSharp does not use GDI+ codec parameters.");
		}

		/// <summary>
		///  Returns the number of frames of the specified dimension.
		/// </summary>
		public int GetFrameCount(System.Drawing.Imaging.FrameDimension dimension)
		{
			ThrowIfDisposed();
			if (_codecData != null)
			{
				using var codec = SKCodec.Create(new MemoryStream(_codecData));
				if (codec != null)
					return Math.Max(1, codec.FrameCount);
			}
			return 1;
		}

		/// <summary>
		///  Gets the specified property item from this <see cref="Image"/>.
		/// </summary>
		public System.Drawing.Imaging.PropertyItem? GetPropertyItem(int propid)
		{
			ThrowIfDisposed();
			throw new ArgumentException("Property ID " + propid + " was not found in the image.");
		}

		/// <summary>
		///  Returns a thumbnail for this <see cref="Image"/>.
		/// </summary>
		public System.Drawing.Image GetThumbnailImage(int thumbWidth, int thumbHeight, System.Drawing.Image.GetThumbnailImageAbort? callback, nint callbackData)
		{
			ThrowIfDisposed();
			if (thumbWidth <= 0) throw new ArgumentOutOfRangeException(nameof(thumbWidth));
			if (thumbHeight <= 0) throw new ArgumentOutOfRangeException(nameof(thumbHeight));

			var info = new SKImageInfo(thumbWidth, thumbHeight, SKBitmapBacking!.ColorType, SKBitmapBacking.AlphaType);
			var resized = SKBitmapBacking.Resize(info, SKSamplingOptions.Default);
			if (resized == null) throw new OutOfMemoryException("Failed to create thumbnail.");

			var result = new Bitmap(resized);
			result._rawFormat = _rawFormat;
			return result;
		}

		/// <summary>
		///  Removes the specified property item from this <see cref="Image"/>.
		/// </summary>
		public void RemovePropertyItem(int propid)
		{
			ThrowIfDisposed();
			throw new ArgumentException("Property ID " + propid + " was not found in the image.");
		}

		/// <summary>
		///  Rotates, flips, or rotates and flips the <see cref="Image"/>.
		/// </summary>
		public void RotateFlip(System.Drawing.RotateFlipType rotateFlipType)
		{
			ThrowIfDisposed();

			var source = SKBitmapBacking!;
			bool rotate90 = rotateFlipType == RotateFlipType.Rotate90FlipNone ||
			                rotateFlipType == RotateFlipType.Rotate90FlipX ||
			                rotateFlipType == RotateFlipType.Rotate90FlipY ||
			                rotateFlipType == RotateFlipType.Rotate90FlipXY;
			bool rotate180 = rotateFlipType == RotateFlipType.Rotate180FlipNone ||
			                 rotateFlipType == RotateFlipType.Rotate180FlipX ||
			                 rotateFlipType == RotateFlipType.Rotate180FlipY ||
			                 rotateFlipType == RotateFlipType.Rotate180FlipXY;
			bool rotate270 = rotateFlipType == RotateFlipType.Rotate270FlipNone ||
			                 rotateFlipType == RotateFlipType.Rotate270FlipX ||
			                 rotateFlipType == RotateFlipType.Rotate270FlipY ||
			                 rotateFlipType == RotateFlipType.Rotate270FlipXY;
			bool flipX = rotateFlipType == RotateFlipType.RotateNoneFlipX ||
			             rotateFlipType == RotateFlipType.Rotate90FlipX ||
			             rotateFlipType == RotateFlipType.Rotate180FlipX ||
			             rotateFlipType == RotateFlipType.Rotate270FlipX;
			bool flipY = rotateFlipType == RotateFlipType.RotateNoneFlipY ||
			             rotateFlipType == RotateFlipType.Rotate90FlipY ||
			             rotateFlipType == RotateFlipType.Rotate180FlipY ||
			             rotateFlipType == RotateFlipType.Rotate270FlipY;

			int destWidth = (rotate90 || rotate270) ? source.Height : source.Width;
			int destHeight = (rotate90 || rotate270) ? source.Width : source.Height;
			var dest = new SKBitmap(destWidth, destHeight, source.ColorType, source.AlphaType);

			using (var canvas = new SKCanvas(dest))
			{
				canvas.Clear(SKColors.Transparent);

				float cx = destWidth / 2f;
				float cy = destHeight / 2f;

				canvas.Translate(cx, cy);

				if (rotate90) canvas.RotateDegrees(90);
				else if (rotate180) canvas.RotateDegrees(180);
				else if (rotate270) canvas.RotateDegrees(270);

				float scaleX = flipX ? -1 : 1;
				float scaleY = flipY ? -1 : 1;
				canvas.Scale(scaleX, scaleY);

				canvas.Translate(-source.Width / 2f, -source.Height / 2f);
				canvas.DrawBitmap(source, 0, 0);
			}

			source.Dispose();
			SKBitmapBacking = dest;
		}

		/// <summary>
		///  Saves this <see cref="Image"/> to the specified stream in the specified format with the specified encoder parameters.
		/// </summary>
		public void Save(System.IO.Stream stream, System.Drawing.Imaging.ImageCodecInfo encoder, System.Drawing.Imaging.EncoderParameters? encoderParams)
		{
			ThrowIfDisposed();
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			// Use the codec's FormatID to determine the format
			var skFormat = SKEncodedImageFormat.Png;
			if (encoder != null)
			{
				var format = new ImageFormat(encoder.FormatID);
				skFormat = SkiaConversions.ToSKFormat(format);
			}
			SaveToStream(stream, skFormat);
		}

		/// <summary>
		///  Saves this image to the specified stream in the specified format.
		/// </summary>
		public void Save(System.IO.Stream stream, System.Drawing.Imaging.ImageFormat format)
		{
			ThrowIfDisposed();
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (format == null) throw new ArgumentNullException(nameof(format));
			SaveToStream(stream, SkiaConversions.ToSKFormat(format));
		}

		/// <summary>
		///  Saves this <see cref="Image"/> to the specified file or stream.
		/// </summary>
		public void Save(string filename)
		{
			ThrowIfDisposed();
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			var ext = Path.GetExtension(filename);
			var skFormat = SkiaConversions.FormatFromExtension(ext);
			using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				SaveToStream(fs, skFormat);
			}
		}

		/// <summary>
		///  Saves this <see cref="Image"/> to the specified file with the specified encoder and image-encoder parameters.
		/// </summary>
		public void Save(string filename, System.Drawing.Imaging.ImageCodecInfo encoder, System.Drawing.Imaging.EncoderParameters? encoderParams)
		{
			ThrowIfDisposed();
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			var skFormat = SKEncodedImageFormat.Png;
			if (encoder != null)
			{
				var format = new ImageFormat(encoder.FormatID);
				skFormat = SkiaConversions.ToSKFormat(format);
			}
			using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				SaveToStream(fs, skFormat);
			}
		}

		/// <summary>
		///  Saves this <see cref="Image"/> to the specified file in the specified format.
		/// </summary>
		public void Save(string filename, System.Drawing.Imaging.ImageFormat format)
		{
			ThrowIfDisposed();
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			if (format == null) throw new ArgumentNullException(nameof(format));
			using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				SaveToStream(fs, SkiaConversions.ToSKFormat(format));
			}
		}

		/// <summary>
		///  Adds a frame to the file or stream specified in a previous call to the <see cref="Save(string)"/> method.
		/// </summary>
		public void SaveAdd(System.Drawing.Image image, System.Drawing.Imaging.EncoderParameters? encoderParams)
		{
			throw new System.PlatformNotSupportedException("Multi-frame image saving (SaveAdd) is not supported in the SkiaSharp-backed System.Drawing implementation.");
		}

		/// <summary>
		///  Adds a frame to the file or stream specified in a previous call to the <see cref="Save(string)"/> method.
		/// </summary>
		public void SaveAdd(System.Drawing.Imaging.EncoderParameters? encoderParams)
		{
			throw new System.PlatformNotSupportedException("Multi-frame image saving (SaveAdd) is not supported in the SkiaSharp-backed System.Drawing implementation.");
		}

		/// <summary>
		///  Selects the frame specified by the dimension and index.
		/// </summary>
		public int SelectActiveFrame(System.Drawing.Imaging.FrameDimension dimension, int frameIndex)
		{
			ThrowIfDisposed();
			if (_codecData != null)
			{
				using var codec = SKCodec.Create(new MemoryStream(_codecData));
				if (codec != null && codec.FrameCount > 1 && frameIndex >= 0 && frameIndex < codec.FrameCount)
				{
					var info = codec.Info;
					var decoded = new SKBitmap(info);
					var opts = new SKCodecOptions(frameIndex);
					codec.GetPixels(info, decoded.GetPixels(), opts);
					SKBitmapBacking?.Dispose();
					SKBitmapBacking = decoded;
					return frameIndex;
				}
			}
			if (frameIndex != 0)
				throw new ArgumentException("Frame index is out of range for this image.", nameof(frameIndex));
			return 0;
		}

		/// <summary>
		///  Stores a property item (piece of metadata) in this <see cref="Image"/>.
		/// </summary>
		public void SetPropertyItem(System.Drawing.Imaging.PropertyItem propitem)
		{
			ThrowIfDisposed();
			// No-op: SkiaSharp does not support setting property items on decoded bitmaps.
		}

		/// <summary>
		///  Releases the unmanaged resources used by the <see cref="Image"/> and optionally releases the managed resources.
		/// </summary>
		internal virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				SKBitmapBacking?.Dispose();
				SKBitmapBacking = null;
			}
		}

		/// <summary>
		///  Allows an <see cref="Image"/> to attempt to free resources and perform other cleanup operations before the <see cref="Image"/> is reclaimed by garbage collection.
		/// </summary>
		~Image()
		{
			Dispose(false);
		}

		/// <summary>
		///  Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			throw new System.PlatformNotSupportedException("Binary serialization of Image objects is not supported in the SkiaSharp-backed System.Drawing implementation.");
		}

		private void SaveToStream(Stream stream, SKEncodedImageFormat format)
		{
			using (var image = SKImage.FromBitmap(SKBitmapBacking!))
			{
				using var data = image.Encode(format, 100);
				if (data == null)
					throw new ArgumentException("Failed to encode the image to the specified format.");
				data.SaveTo(stream);
			}
		}

		private void ThrowIfDisposed()
		{
			if (SKBitmapBacking == null)
				throw new ObjectDisposedException(nameof(Image));
		}
	}
}
