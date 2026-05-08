using SkiaSharp;
using System.Drawing.Imaging;
using System.Drawing.Internal;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Drawing;

/// <summary>
///  Encapsulates a GDI+ bitmap, which consists of the pixel data for a graphics image and its attributes.
///  A <see cref="Bitmap"/> is an object used to work with images defined by pixel data.
/// </summary>
[ComponentModel.Editor("System.Drawing.Design.BitmapEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
public sealed partial class Bitmap : Image
{
	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified existing image.
	/// </summary>
	public Bitmap(Image original)
	{
		if (original == null) throw new ArgumentNullException(nameof(original));
		if (original.SKBitmapBacking == null) throw new ArgumentException("The source image has been disposed.", nameof(original));
		SKBitmapBacking = original.SKBitmapBacking.Copy();
		_rawFormat = original._rawFormat;
		_horizontalResolution = original._horizontalResolution;
		_verticalResolution = original._verticalResolution;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified existing image, scaled to the specified size.
	/// </summary>
	public Bitmap(Image original, Size newSize)
		: this(original, newSize.Width, newSize.Height)
	{
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified existing image, scaled to the specified size.
	/// </summary>
	public Bitmap(Image original, int width, int height)
	{
		if (original == null) throw new ArgumentNullException(nameof(original));
		if (original.SKBitmapBacking == null) throw new ArgumentException("The source image has been disposed.", nameof(original));
		if (width <= 0) throw new ArgumentException(null, nameof(width));
		if (height <= 0) throw new ArgumentException(null, nameof(height));

		var info = new SKImageInfo(width, height, original.SKBitmapBacking.ColorType, original.SKBitmapBacking.AlphaType);
		SKBitmapBacking = original.SKBitmapBacking.Resize(info, SKSamplingOptions.Default);
		if (SKBitmapBacking == null)
			throw new OutOfMemoryException("Failed to resize the image.");
		_rawFormat = original._rawFormat;
		_horizontalResolution = original._horizontalResolution;
		_verticalResolution = original._verticalResolution;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class with the specified size.
	/// </summary>
	public Bitmap(int width, int height)
	{
		if (width <= 0) throw new ArgumentException(null, nameof(width));
		if (height <= 0) throw new ArgumentException(null, nameof(height));
		SKBitmapBacking = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
		SKBitmapBacking.Erase(SKColors.Transparent);
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class with the specified size and with the resolution of the specified <see cref="Graphics"/> object.
	/// </summary>
	public Bitmap(int width, int height, Graphics g)
	{
		if (g is null) throw new ArgumentNullException(nameof(g));
		if (width <= 0) throw new ArgumentException(null, nameof(width));
		if (height <= 0) throw new ArgumentException(null, nameof(height));
		// Graphics DPI is not available; use default 96 DPI.
		SKBitmapBacking = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
		SKBitmapBacking.Erase(SKColors.Transparent);
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class with the specified size and format.
	/// </summary>
	public Bitmap(int width, int height, PixelFormat format)
	{
		if (width <= 0) throw new ArgumentException(null, nameof(width));
		if (height <= 0) throw new ArgumentException(null, nameof(height));
		var colorType = SkiaConversions.ToSKColorType(format);
		var alphaType = SkiaConversions.ToSKAlphaType(format);
		SKBitmapBacking = new SKBitmap(width, height, colorType, alphaType);
		SKBitmapBacking.Erase(SKColors.Transparent);
		_requestedPixelFormat = format;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class with the specified size, pixel format, and pixel data.
	/// </summary>
	public Bitmap(int width, int height, int stride, PixelFormat format, nint scan0)
	{
		if (width <= 0) throw new ArgumentException(null, nameof(width));
		if (height <= 0) throw new ArgumentException(null, nameof(height));
		var colorType = SkiaConversions.ToSKColorType(format);
		var alphaType = SkiaConversions.ToSKAlphaType(format);
		var info = new SKImageInfo(width, height, colorType, alphaType);
		SKBitmapBacking = new SKBitmap(info);
		if (scan0 != IntPtr.Zero)
		{
			SKBitmapBacking.InstallPixels(info, scan0, stride);
		}
		_requestedPixelFormat = format;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified data stream.
	/// </summary>
	public Bitmap(Stream stream)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		SKBitmapBacking = SKBitmap.Decode(stream);
		if (SKBitmapBacking == null)
			throw new OutOfMemoryException("The stream does not contain a valid image.");
		_rawFormat = ImageFormat.Png;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified data stream.
	/// </summary>
	public Bitmap(Stream stream, bool useIcm)
		: this(stream)
	{
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified file.
	/// </summary>
	public Bitmap(string filename)
	{
		if (filename == null) throw new ArgumentNullException(nameof(filename));
		SKBitmapBacking = SKBitmap.Decode(filename);
		if (SKBitmapBacking == null)
			throw new OutOfMemoryException("Cannot create image from the specified file: " + filename);
		_rawFormat = SkiaConversions.ImageFormatFromExtension(Path.GetExtension(filename));
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from the specified file.
	/// </summary>
	public Bitmap(string filename, bool useIcm)
		: this(filename)
	{
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Bitmap"/> class from a specified resource.
	/// </summary>
	public Bitmap(Type type, string resource)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		if (resource == null) throw new ArgumentNullException(nameof(resource));
		var stream = type.Assembly.GetManifestResourceStream(type, resource);
		if (stream == null)
			throw new ArgumentException("Resource '" + resource + "' was not found in assembly '" + type.Assembly.FullName + "'.");
		SKBitmapBacking = SKBitmap.Decode(stream);
		stream.Dispose();
		if (SKBitmapBacking == null)
			throw new OutOfMemoryException("The resource does not contain a valid image.");
		_rawFormat = ImageFormat.Png;
	}

	// Internal constructor used by Image.FromFile / Image.FromStream / Image.Clone
	internal Bitmap(SKBitmap skBitmap)
	{
		SKBitmapBacking = skBitmap ?? throw new ArgumentNullException(nameof(skBitmap));
	}

	/// <summary>
	///  Creates a GDI bitmap object from a GDI+ <see cref="Icon"/>.
	/// </summary>
	public static Bitmap FromHicon(nint hicon)
	{
		throw new PlatformNotSupportedException("FromHicon is not supported on this platform because it requires a Windows GDI handle.");
	}

	/// <summary>
	///  Creates a <see cref="Bitmap"/> from the specified Windows resource.
	/// </summary>
	public static Bitmap FromResource(nint hinstance, string bitmapName)
	{
		throw new PlatformNotSupportedException("FromResource is not supported on this platform because it requires a Windows module handle.");
	}

	/// <summary>
	///  Creates a copy of the section of this <see cref="Bitmap"/> defined by <see cref="Rectangle"/> structure and with a specified <see cref="PixelFormat"/> enumeration.
	/// </summary>
	public Bitmap Clone(Rectangle rect, PixelFormat format)
	{
		return Clone(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), format);
	}

	/// <summary>
	///  Creates a copy of the section of this <see cref="Bitmap"/> defined with a specified <see cref="PixelFormat"/> enumeration.
	/// </summary>
	public Bitmap Clone(RectangleF rect, PixelFormat format)
	{
		ThrowIfDisposed();

		int x = (int)rect.X;
		int y = (int)rect.Y;
		int w = (int)rect.Width;
		int h = (int)rect.Height;

		if (x < 0 || y < 0 || w <= 0 || h <= 0 ||
		    x + w > SKBitmapBacking!.Width || y + h > SKBitmapBacking.Height)
			throw new ArgumentOutOfRangeException(nameof(rect), "The specified rectangle is outside the bounds of the bitmap.");

		var colorType = SkiaConversions.ToSKColorType(format);
		var alphaType = SkiaConversions.ToSKAlphaType(format);

		var subset = new SKBitmap(w, h, colorType, alphaType);
		using (var canvas = new SKCanvas(subset))
		{
			var srcRect = new SKRect(x, y, x + w, y + h);
			var destRect = new SKRect(0, 0, w, h);
			canvas.DrawBitmap(SKBitmapBacking, srcRect, destRect);
		}

		var result = new Bitmap(subset);
		result._rawFormat = _rawFormat;
		result._horizontalResolution = _horizontalResolution;
		result._verticalResolution = _verticalResolution;
		return result;
	}

	/// <summary>
	///  Creates a GDI bitmap object from this <see cref="Bitmap"/>.
	/// </summary>
	[ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public nint GetHbitmap()
	{
		throw new PlatformNotSupportedException("GetHbitmap is not supported on this platform because it requires Windows GDI.");
	}

	/// <summary>
	///  Creates a GDI bitmap object from this <see cref="Bitmap"/>.
	/// </summary>
	[ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public nint GetHbitmap(Color background)
	{
		throw new PlatformNotSupportedException("GetHbitmap is not supported on this platform because it requires Windows GDI.");
	}

	/// <summary>
	///  Returns the handle to an icon.
	/// </summary>
	[ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public nint GetHicon()
	{
		throw new PlatformNotSupportedException("GetHicon is not supported on this platform because it requires Windows GDI.");
	}

	/// <summary>
	///  Gets the color of the specified pixel in this <see cref="Bitmap"/>.
	/// </summary>
	public Color GetPixel(int x, int y)
	{
		ThrowIfDisposed();
		if (x < 0 || x >= SKBitmapBacking!.Width)
			throw new ArgumentException(null, nameof(x));
		if (y < 0 || y >= SKBitmapBacking.Height)
			throw new ArgumentException(null, nameof(y));
		return SkiaConversions.ToDrawingColor(SKBitmapBacking.GetPixel(x, y));
	}

	/// <summary>
	///  Locks a <see cref="Bitmap"/> into system memory.
	/// </summary>
	public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format)
	{
		return LockBits(rect, flags, format, new BitmapData());
	}

	/// <summary>
	///  Locks a <see cref="Bitmap"/> into system memory.
	/// </summary>
	public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format, BitmapData bitmapData)
	{
		ThrowIfDisposed();
		if (bitmapData == null) throw new ArgumentNullException(nameof(bitmapData));

		var pixels = SKBitmapBacking!.GetPixels();
		if (pixels == IntPtr.Zero)
			throw new InvalidOperationException("Cannot lock bitmap pixels.");

		int bpp = SkiaConversions.GetBitsPerPixel(format);
		const int BitsPerByte = 8;
		int stride = SKBitmapBacking.RowBytes;

		bitmapData.Width = rect.Width;
		bitmapData.Height = rect.Height;
		bitmapData.Stride = stride;
		bitmapData.PixelFormat = format;
		bitmapData.Scan0 = pixels + (rect.Y * stride) + (rect.X * (bpp / BitsPerByte));

		return bitmapData;
	}

	/// <summary>
	///  Makes the default transparent color transparent for this <see cref="Bitmap"/>.
	/// </summary>
	public void MakeTransparent()
	{
		ThrowIfDisposed();
		// Default transparent color is the color of the bottom-left pixel.
		var bottomLeft = SKBitmapBacking!.GetPixel(0, SKBitmapBacking.Height - 1);
		MakeTransparent(SkiaConversions.ToDrawingColor(bottomLeft));
	}

	/// <summary>
	///  Makes the specified color transparent for this <see cref="Bitmap"/>.
	/// </summary>
	public void MakeTransparent(Color transparentColor)
	{
		ThrowIfDisposed();
		var skTransparent = SkiaConversions.ToSKColor(transparentColor);

		// Ensure we're working with a mutable bitmap in Bgra8888/Premul
		if (SKBitmapBacking!.ColorType != SKColorType.Bgra8888 || !SKBitmapBacking.IsImmutable == false)
		{
			var mutable = SKBitmapBacking.Copy(SKColorType.Bgra8888);
			if (mutable == null) throw new OutOfMemoryException("Failed to convert bitmap for MakeTransparent.");
			SKBitmapBacking.Dispose();
			SKBitmapBacking = mutable;
		}

		for (int y = 0; y < SKBitmapBacking.Height; y++)
		{
			for (int x = 0; x < SKBitmapBacking.Width; x++)
			{
				var pixel = SKBitmapBacking.GetPixel(x, y);
				if (pixel.Red == skTransparent.Red &&
				    pixel.Green == skTransparent.Green &&
				    pixel.Blue == skTransparent.Blue)
				{
					SKBitmapBacking.SetPixel(x, y, SKColors.Transparent);
				}
			}
		}
	}

	/// <summary>
	///  Sets the color of the specified pixel in this <see cref="Bitmap"/>.
	/// </summary>
	public void SetPixel(int x, int y, Color color)
	{
		ThrowIfDisposed();
		if (x < 0 || x >= SKBitmapBacking!.Width)
			throw new ArgumentException(null, nameof(x));
		if (y < 0 || y >= SKBitmapBacking.Height)
			throw new ArgumentException(null, nameof(y));
		SKBitmapBacking.SetPixel(x, y, SkiaConversions.ToSKColor(color));
	}

	/// <summary>
	///  Sets the resolution for this <see cref="Bitmap"/>.
	/// </summary>
	public void SetResolution(float xDpi, float yDpi)
	{
		ThrowIfDisposed();
		_horizontalResolution = xDpi;
		_verticalResolution = yDpi;
	}

	/// <summary>
	///  Unlocks this <see cref="Bitmap"/> from system memory.
	/// </summary>
	public void UnlockBits(BitmapData bitmapdata)
	{
		ThrowIfDisposed();
		if (bitmapdata == null) throw new ArgumentNullException(nameof(bitmapdata));
		SKBitmapBacking!.NotifyPixelsChanged();
	}

	private void ThrowIfDisposed()
	{
		if (SKBitmapBacking == null)
			throw new ObjectDisposedException(nameof(Bitmap));
	}
}
