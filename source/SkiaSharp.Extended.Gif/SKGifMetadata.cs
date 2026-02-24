using System;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Provides GIF-specific metadata and information.
	/// Complements SKImageInfo with GIF-specific properties like loop count and extensions.
	/// </summary>
	public class SKGifInfo
	{
		/// <summary>
		/// Gets the image information (width, height, color type, alpha type).
		/// Aligned with SKCodec.Info pattern.
		/// </summary>
		public SKImageInfo ImageInfo { get; internal set; }

		/// <summary>
		/// Gets the width of the logical screen in pixels.
		/// Convenience property equivalent to ImageInfo.Width.
		/// </summary>
		public int Width => ImageInfo.Width;

		/// <summary>
		/// Gets the height of the logical screen in pixels.
		/// Convenience property equivalent to ImageInfo.Height.
		/// </summary>
		public int Height => ImageInfo.Height;

		/// <summary>
		/// Gets the number of frames in the GIF.
		/// Aligned with SKCodec.FrameCount.
		/// </summary>
		public int FrameCount { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether the GIF has animation (more than one frame).
		/// </summary>
		public bool IsAnimated => FrameCount > 1;

		/// <summary>
		/// Gets the background color from the logical screen descriptor.
		/// </summary>
		public SKColor BackgroundColor { get; internal set; }

		/// <summary>
		/// Gets the loop count for animations (0 = infinite, -1 = no loop specified).
		/// From NETSCAPE2.0 application extension.
		/// </summary>
		public int LoopCount { get; internal set; } = -1;

		/// <summary>
		/// Gets the comment text from the GIF comment extension, if present.
		/// </summary>
		public string? Comment { get; internal set; }

		/// <summary>
		/// Gets application-specific data from application extensions, if present.
		/// </summary>
		public byte[]? ApplicationData { get; internal set; }

		/// <summary>
		/// Gets the application identifier from application extension, if present.
		/// </summary>
		public string? ApplicationIdentifier { get; internal set; }
	}
}
