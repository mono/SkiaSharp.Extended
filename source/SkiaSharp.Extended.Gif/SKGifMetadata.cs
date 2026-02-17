using System;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Provides metadata about a GIF file.
	/// </summary>
	public class SKGifMetadata
	{
		/// <summary>
		/// Gets the width of the logical screen in pixels.
		/// </summary>
		public int Width { get; internal set; }

		/// <summary>
		/// Gets the height of the logical screen in pixels.
		/// </summary>
		public int Height { get; internal set; }

		/// <summary>
		/// Gets the number of frames in the GIF.
		/// </summary>
		public int FrameCount { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether the GIF has animation (more than one frame).
		/// </summary>
		public bool IsAnimated => FrameCount > 1;

		/// <summary>
		/// Gets the background color index from the logical screen descriptor.
		/// </summary>
		public byte BackgroundColorIndex { get; internal set; }

		/// <summary>
		/// Gets the loop count for animations (0 = infinite, -1 = no loop).
		/// </summary>
		public int LoopCount { get; internal set; } = -1;
	}
}
