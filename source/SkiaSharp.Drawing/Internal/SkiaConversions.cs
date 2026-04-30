using SkiaSharp;
using System.Drawing.Imaging;

namespace System.Drawing.Internal
{
	/// <summary>
	///  Provides conversion utilities between System.Drawing types and SkiaSharp types.
	/// </summary>
	internal static class SkiaConversions
	{
		/// <summary>
		///  Converts a <see cref="System.Drawing.Color"/> to an <see cref="SKColor"/>.
		/// </summary>
		public static SKColor ToSKColor(Color color)
			=> new SKColor(color.R, color.G, color.B, color.A);

		/// <summary>
		///  Converts an <see cref="SKColor"/> to a <see cref="System.Drawing.Color"/>.
		/// </summary>
		public static Color ToDrawingColor(SKColor color)
			=> Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

		/// <summary>
		///  Converts an <see cref="ImageFormat"/> to an <see cref="SKEncodedImageFormat"/>.
		/// </summary>
		public static SKEncodedImageFormat ToSKFormat(ImageFormat format)
		{
			if (format.Guid == ImageFormat.Png.Guid) return SKEncodedImageFormat.Png;
			if (format.Guid == ImageFormat.Jpeg.Guid) return SKEncodedImageFormat.Jpeg;
			if (format.Guid == ImageFormat.Gif.Guid) return SKEncodedImageFormat.Gif;
			if (format.Guid == ImageFormat.Bmp.Guid) return SKEncodedImageFormat.Bmp;
			if (format.Guid == ImageFormat.Icon.Guid) return SKEncodedImageFormat.Ico;
			if (format.Guid == ImageFormat.Tiff.Guid) return SKEncodedImageFormat.Png; // SkiaSharp doesn't support TIFF encoding; fall back to PNG
			return SKEncodedImageFormat.Png;
		}

		/// <summary>
		///  Converts a <see cref="PixelFormat"/> to an <see cref="SKColorType"/>.
		/// </summary>
		public static SKColorType ToSKColorType(PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
					return SKColorType.Bgra8888;
				case PixelFormat.Format32bppRgb:
					return SKColorType.Bgra8888;
				case PixelFormat.Format24bppRgb:
					return SKColorType.Rgb888x;
				case PixelFormat.Format16bppRgb555:
				case PixelFormat.Format16bppRgb565:
					return SKColorType.Rgb565;
				case PixelFormat.Format16bppArgb1555:
					return SKColorType.Bgra8888;
				case PixelFormat.Format16bppGrayScale:
					return SKColorType.Gray8;
				case PixelFormat.Format8bppIndexed:
					return SKColorType.Gray8;
				case PixelFormat.Format48bppRgb:
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format64bppPArgb:
					return SKColorType.RgbaF16;
				default:
					return SKColorType.Bgra8888;
			}
		}

		/// <summary>
		///  Converts an <see cref="SKColorType"/> to a <see cref="PixelFormat"/>.
		/// </summary>
		public static PixelFormat ToPixelFormat(SKColorType colorType)
		{
			switch (colorType)
			{
				case SKColorType.Bgra8888:
					return PixelFormat.Format32bppArgb;
				case SKColorType.Rgba8888:
					return PixelFormat.Format32bppArgb;
				case SKColorType.Rgb888x:
					return PixelFormat.Format32bppRgb;
				case SKColorType.Rgb565:
					return PixelFormat.Format16bppRgb565;
				case SKColorType.Gray8:
					return PixelFormat.Format8bppIndexed;
				case SKColorType.RgbaF16:
					return PixelFormat.Format64bppArgb;
				default:
					return PixelFormat.Format32bppArgb;
			}
		}

		/// <summary>
		///  Gets the <see cref="SKAlphaType"/> for a given <see cref="PixelFormat"/>.
		/// </summary>
		public static SKAlphaType ToSKAlphaType(PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format64bppPArgb:
					return SKAlphaType.Premul;
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format16bppArgb1555:
					return SKAlphaType.Unpremul;
				case PixelFormat.Format32bppRgb:
				case PixelFormat.Format24bppRgb:
				case PixelFormat.Format16bppRgb555:
				case PixelFormat.Format16bppRgb565:
				case PixelFormat.Format48bppRgb:
					return SKAlphaType.Opaque;
				default:
					return SKAlphaType.Premul;
			}
		}

		/// <summary>
		///  Determines the <see cref="SKEncodedImageFormat"/> from a file extension.
		/// </summary>
		public static SKEncodedImageFormat FormatFromExtension(string extension)
		{
			switch (extension.ToLowerInvariant())
			{
				case ".png": return SKEncodedImageFormat.Png;
				case ".jpg":
				case ".jpeg": return SKEncodedImageFormat.Jpeg;
				case ".gif": return SKEncodedImageFormat.Gif;
				case ".bmp": return SKEncodedImageFormat.Bmp;
				case ".ico": return SKEncodedImageFormat.Ico;
				case ".webp": return SKEncodedImageFormat.Webp;
				default: return SKEncodedImageFormat.Png;
			}
		}

		/// <summary>
		///  Determines the <see cref="ImageFormat"/> from a file extension.
		/// </summary>
		public static ImageFormat ImageFormatFromExtension(string extension)
		{
			switch (extension.ToLowerInvariant())
			{
				case ".png": return ImageFormat.Png;
				case ".jpg":
				case ".jpeg": return ImageFormat.Jpeg;
				case ".gif": return ImageFormat.Gif;
				case ".bmp": return ImageFormat.Bmp;
				case ".ico": return ImageFormat.Icon;
				case ".tiff":
				case ".tif": return ImageFormat.Tiff;
				default: return ImageFormat.Png;
			}
		}

		/// <summary>
		///  Gets the bits per pixel for a given <see cref="PixelFormat"/>.
		/// </summary>
		public static int GetBitsPerPixel(PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.Format1bppIndexed: return 1;
				case PixelFormat.Format4bppIndexed: return 4;
				case PixelFormat.Format8bppIndexed: return 8;
				case PixelFormat.Format16bppRgb555:
				case PixelFormat.Format16bppRgb565:
				case PixelFormat.Format16bppArgb1555:
				case PixelFormat.Format16bppGrayScale: return 16;
				case PixelFormat.Format24bppRgb: return 24;
				case PixelFormat.Format32bppRgb:
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb: return 32;
				case PixelFormat.Format48bppRgb: return 48;
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format64bppPArgb: return 64;
				default: return 0;
			}
		}
	}
}
