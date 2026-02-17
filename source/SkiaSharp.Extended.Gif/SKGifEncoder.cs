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
		/// Adds a frame to the GIF.
		/// </summary>
		/// <param name="bitmap">The bitmap to encode as a frame.</param>
		/// <param name="delayMs">The delay in milliseconds before displaying the next frame (default: 100ms).</param>
		/// <exception cref="ArgumentNullException">Thrown when bitmap is null.</exception>
		public void AddFrame(SKBitmap bitmap, int delayMs = 100)
		{
			if (bitmap == null)
				throw new ArgumentNullException(nameof(bitmap));

			// TODO: Implement frame encoding
			throw new NotImplementedException("GIF encoding is not yet implemented. This is a placeholder for the project structure.");
		}

		/// <summary>
		/// Saves the GIF to the stream.
		/// </summary>
		public void Save()
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
