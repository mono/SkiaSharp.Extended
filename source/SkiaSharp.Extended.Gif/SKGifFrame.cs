using System;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Represents a single decoded frame from a GIF file.
	/// </summary>
	public class SKGifFrame : IDisposable
	{
		/// <summary>
		/// Gets the decoded bitmap for this frame.
		/// </summary>
		public SKBitmap Bitmap { get; internal set; } = null!;

		/// <summary>
		/// Gets the frame information including duration, disposal method, and bounds.
		/// </summary>
		public SKGifFrameInfo FrameInfo { get; internal set; }

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
	/// Contains information about a GIF frame (aligned with SKCodecFrameInfo pattern).
	/// </summary>
	public struct SKGifFrameInfo
	{
		/// <summary>
		/// Gets or sets the duration in milliseconds to show this frame.
		/// Aligned with SKCodecFrameInfo.Duration.
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// Gets or sets the disposal method indicating how the frame should be modified before decoding the next one.
		/// Aligned with SKCodecFrameInfo.DisposalMethod.
		/// </summary>
		public SKGifDisposalMethod DisposalMethod { get; set; }

		/// <summary>
		/// Gets or sets the frame that this frame needs to be blended with, or -1 if independent.
		/// Aligned with SKCodecFrameInfo.RequiredFrame.
		/// </summary>
		public int RequiredFrame { get; set; }

		/// <summary>
		/// Gets or sets the rectangle occupied by this frame within the logical screen.
		/// Aligned with SKCodecFrameInfo.FrameRect.
		/// </summary>
		public SKRectI FrameRect { get; set; }

		/// <summary>
		/// Gets or sets whether this frame has transparency.
		/// </summary>
		public bool HasTransparency { get; set; }

		/// <summary>
		/// Gets or sets the transparent color for this frame, if any.
		/// </summary>
		public SKColor? TransparentColor { get; set; }
	}

	/// <summary>
	/// Specifies how a frame should be disposed before rendering the next frame.
	/// Values aligned with SKCodecAnimationDisposalMethod for consistency.
	/// </summary>
	public enum SKGifDisposalMethod
	{
		/// <summary>
		/// No disposal specified. The decoder is not required to take any action.
		/// </summary>
		None = 0,

		/// <summary>
		/// Do not dispose. The next frame should be drawn on top of this one.
		/// Corresponds to SKCodecAnimationDisposalMethod.Keep.
		/// </summary>
		DoNotDispose = 1,

		/// <summary>
		/// Restore to background color. The area used by the graphic must be cleared to the background color before drawing the next frame.
		/// Corresponds to SKCodecAnimationDisposalMethod.RestoreBackgroundColor.
		/// </summary>
		RestoreToBackground = 2,

		/// <summary>
		/// Restore to previous. The decoder is required to restore the area to what was there prior to rendering the graphic.
		/// Corresponds to SKCodecAnimationDisposalMethod.RestorePrevious.
		/// </summary>
		RestoreToPrevious = 3
	}
}
