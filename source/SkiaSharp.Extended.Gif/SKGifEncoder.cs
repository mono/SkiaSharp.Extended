using System;
using System.IO;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Encodes SKBitmap frames to GIF files (GIF87a and GIF89a).
	/// </summary>
	public class SKGifEncoder : IDisposable
	{
		private readonly Stream stream;
		private bool disposed;
		private int loopCount = -1;

		/// <summary>
		/// Creates a new GIF encoder that writes to the specified stream.
		/// </summary>
		/// <param name="stream">The stream to write the GIF data to.</param>
		/// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
		public SKGifEncoder(Stream stream)
		{
			this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
		}

		/// <summary>
		/// Sets the loop count for animated GIFs.
		/// </summary>
		/// <param name="count">The number of times to loop (0 = infinite, -1 = no loop).</param>
		public void SetLoopCount(int count)
		{
			loopCount = count;
		}

		/// <summary>
		/// Adds a frame to the GIF with detailed frame information.
		/// </summary>
		/// <param name="bitmap">The bitmap to encode as a frame.</param>
		/// <param name="frameInfo">Optional frame information (duration, disposal, etc.).</param>
		/// <exception cref="ArgumentNullException">Thrown when bitmap is null.</exception>
		public void AddFrame(SKBitmap bitmap, SKGifFrameInfo? frameInfo = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException(nameof(bitmap));

			// TODO: Implement frame encoding
			throw new NotImplementedException("GIF encoding is not yet implemented. This is a placeholder for the project structure.");
		}

		/// <summary>
		/// Adds a frame to the GIF with a simple duration.
		/// </summary>
		/// <param name="bitmap">The bitmap to encode as a frame.</param>
		/// <param name="duration">The duration in milliseconds to show this frame (default: 100ms). Aligned with SKCodecFrameInfo.Duration.</param>
		/// <exception cref="ArgumentNullException">Thrown when bitmap is null.</exception>
		public void AddFrame(SKBitmap bitmap, int duration = 100)
		{
			var frameInfo = new SKGifFrameInfo { Duration = duration };
			AddFrame(bitmap, frameInfo);
		}

		/// <summary>
		/// Encodes and finalizes the GIF file.
		/// </summary>
		public void Encode()
		{
			// TODO: Implement GIF file writing
			throw new NotImplementedException("GIF encoding is not yet implemented. This is a placeholder for the project structure.");
		}

		/// <summary>
		/// Disposes the encoder and releases resources.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				// TODO: Clean up resources
				disposed = true;
			}

			GC.SuppressFinalize(this);
		}
	}
}
