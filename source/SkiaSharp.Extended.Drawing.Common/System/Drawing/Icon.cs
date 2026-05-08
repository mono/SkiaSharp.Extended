using SkiaSharp;

namespace System.Drawing;

/// <summary>
///  Represents a Windows icon, which is a small bitmap image that is used to represent an object.
/// </summary>
[ComponentModel.Editor("System.Drawing.Design.IconEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[ComponentModel.TypeConverter(typeof(IconConverter))]
public sealed partial class Icon : MarshalByRefObject, ICloneable, IDisposable, Runtime.Serialization.ISerializable
{
	private SKBitmap? _bitmap;
	private bool _disposed;

	private Icon(SKBitmap bitmap)
	{
		_bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
	}

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified original icon and size.</summary>
	public Icon(Icon original, Size size) : this(original, size.Width, size.Height) { }

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified original icon, at the specified size.</summary>
	public Icon(Icon original, int width, int height)
	{
		if (original is null) throw new ArgumentNullException(nameof(original));
		if (original._bitmap is null) throw new ArgumentException("The source icon has been disposed.", nameof(original));
		if (width == original._bitmap.Width && height == original._bitmap.Height)
			_bitmap = original._bitmap.Copy();
		else
		{
			var info = new SKImageInfo(Math.Max(1, width), Math.Max(1, height), original._bitmap.ColorType, original._bitmap.AlphaType);
			_bitmap = original._bitmap.Resize(info, SKSamplingOptions.Default);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified data stream.</summary>
	public Icon(IO.Stream stream) { _bitmap = LoadFromStream(stream); }
	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified stream, at the specified size.</summary>
	public Icon(IO.Stream stream, Size size) : this(stream, size.Width, size.Height) { }
	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified stream, at the specified width and height.</summary>
	public Icon(IO.Stream stream, int width, int height)
	{
		var full = LoadFromStream(stream);
		if (full.Width == width && full.Height == height)
			_bitmap = full;
		else
		{
			var info = new SKImageInfo(Math.Max(1, width), Math.Max(1, height), full.ColorType, full.AlphaType);
			_bitmap = full.Resize(info, SKSamplingOptions.Default);
			full.Dispose();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified file name.</summary>
	public Icon(string fileName)
	{
		if (fileName is null) throw new ArgumentNullException(nameof(fileName));
		using var stream = System.IO.File.OpenRead(fileName);
		_bitmap = LoadFromStream(stream);
	}

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified file name, at the specified size.</summary>
	public Icon(string fileName, Size size) : this(fileName, size.Width, size.Height) { }

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from the specified file name, at the specified width and height.</summary>
	public Icon(string fileName, int width, int height)
	{
		if (fileName is null) throw new ArgumentNullException(nameof(fileName));
		using var stream = System.IO.File.OpenRead(fileName);
		var full = LoadFromStream(stream);
		if (full.Width == width && full.Height == height)
			_bitmap = full;
		else
		{
			var info = new SKImageInfo(Math.Max(1, width), Math.Max(1, height), full.ColorType, full.AlphaType);
			_bitmap = full.Resize(info, SKSamplingOptions.Default);
			full.Dispose();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="Icon"/> class from a resource in the specified assembly.</summary>
	public Icon(Type type, string resource)
	{
		if (type is null) throw new ArgumentNullException(nameof(type));
		if (resource is null) throw new ArgumentNullException(nameof(resource));
		var stream = type.Assembly.GetManifestResourceStream(type, resource);
		if (stream is null) throw new ArgumentException($"Resource '{resource}' not found.", nameof(resource));
		_bitmap = LoadFromStream(stream);
		stream.Dispose();
	}

	/// <summary>Gets the handle of this <see cref="Icon"/>. This is not a true Win32 HICON.</summary>
	[ComponentModel.Browsable(false)]
	public nint Handle { get { throw new PlatformNotSupportedException("Icon handles are not supported in SkiaSharp.Extended.Drawing.Common."); } }

	/// <summary>Gets the height of this <see cref="Icon"/>.</summary>
	[ComponentModel.Browsable(false)]
	public int Height => _bitmap?.Height ?? 0;

	/// <summary>Gets the size of this <see cref="Icon"/>.</summary>
	public Size Size => new Size(Width, Height);

	/// <summary>Gets the width of this <see cref="Icon"/>.</summary>
	[ComponentModel.Browsable(false)]
	public int Width => _bitmap?.Width ?? 0;

	/// <summary>Returns an icon representation of an image that is contained in the specified file.</summary>
	public static Icon? ExtractAssociatedIcon(string filePath) { throw new PlatformNotSupportedException("ExtractAssociatedIcon is not supported in SkiaSharp.Extended.Drawing.Common."); }

	/// <summary>Creates a GDI+ Icon from the specified Windows handle.</summary>
	public static Icon FromHandle(nint handle) { throw new PlatformNotSupportedException("Icon handles are not supported in SkiaSharp.Extended.Drawing.Common."); }

	/// <summary>Clones the <see cref="Icon"/>, creating a duplicate image.</summary>
	public object Clone()
	{
		if (_bitmap is null) throw new ObjectDisposedException(nameof(Icon));
		return new Icon(_bitmap.Copy());
	}

	/// <summary>Releases all resources used by this <see cref="Icon"/>.</summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_bitmap?.Dispose();
			_bitmap = null;
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>Saves this <see cref="Icon"/> to the specified output stream.</summary>
	/// <param name="outputStream">The <see cref="IO.Stream"/> to save to.</param>
	public void Save(IO.Stream outputStream)
	{
		if (outputStream is null) throw new ArgumentNullException(nameof(outputStream));
		if (_bitmap is null) throw new ObjectDisposedException(nameof(Icon));
		using var image = SKImage.FromBitmap(_bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		data.SaveTo(outputStream);
	}

	/// <summary>Converts this <see cref="Icon"/> to a GDI+ <see cref="Bitmap"/>.</summary>
	/// <returns>A <see cref="Bitmap"/> that represents the converted <see cref="Icon"/>.</returns>
	public Bitmap ToBitmap()
	{
		if (_bitmap is null) throw new ObjectDisposedException(nameof(Icon));
		var bmp = new Bitmap(Width, Height);
		bmp.SKBitmapBacking = _bitmap.Copy();
		return bmp;
	}

	/// <summary>Gets a human-readable string that describes this <see cref="Icon"/>.</summary>
	public override string ToString() => $"Icon [Size={Size}]";

	/// <summary>Allows an object to try to free resources before being reclaimed by garbage collection.</summary>
	~Icon() { Dispose(); }

	/// <inheritdoc/>
	void Runtime.Serialization.ISerializable.GetObjectData(Runtime.Serialization.SerializationInfo info, Runtime.Serialization.StreamingContext context)
	{
		throw new PlatformNotSupportedException("Icon serialization is not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	private static SKBitmap LoadFromStream(IO.Stream stream)
	{
		if (stream is null) throw new ArgumentNullException(nameof(stream));
		var bitmap = SKBitmap.Decode(stream);
		if (bitmap is null) throw new ArgumentException("The stream does not contain a valid image.", nameof(stream));
		return bitmap;
	}
}
