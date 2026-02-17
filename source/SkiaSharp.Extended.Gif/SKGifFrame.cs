using System;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Represents a single frame from a GIF file.
	/// </summary>
	public class SKGifFrame : IDisposable
	{
		/// <summary>
		/// Gets the decoded bitmap for this frame.
		/// </summary>
		public SKBitmap Bitmap { get; internal set; } = null!;

		/// <summary>
		/// Gets the delay in milliseconds before displaying the next frame.
		/// </summary>
		public int DelayMs { get; internal set; }

		/// <summary>
		/// Gets the disposal method for this frame.
		/// </summary>
		public SKGifDisposalMethod DisposalMethod { get; internal set; }

		/// <summary>
		/// Gets the X offset of this frame within the logical screen.
		/// </summary>
		public int Left { get; internal set; }

		/// <summary>
		/// Gets the Y offset of this frame within the logical screen.
		/// </summary>
		public int Top { get; internal set; }

		/// <summary>
		/// Gets the width of this frame.
		/// </summary>
		public int Width { get; internal set; }

		/// <summary>
		/// Gets the height of this frame.
		/// </summary>
		public int Height { get; internal set; }

		/// <summary>
		/// Disposes the frame and releases the bitmap.
		/// </summary>
		public void Dispose()
		{
			Bitmap?.Dispose();
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Specifies how a frame should be disposed before rendering the next frame.
	/// </summary>
	public enum SKGifDisposalMethod
	{
		/// <summary>
		/// No disposal specified. The decoder is not required to take any action.
		/// </summary>
		None = 0,

		/// <summary>
		/// Do not dispose. The graphic is to be left in place.
		/// </summary>
		DoNotDispose = 1,

		/// <summary>
		/// Restore to background color. The area used by the graphic must be restored to the background color.
		/// </summary>
		RestoreToBackground = 2,

		/// <summary>
		/// Restore to previous. The decoder is required to restore the area overwritten by the graphic with what was there prior to rendering the graphic.
		/// </summary>
		RestoreToPrevious = 3
	}
}
