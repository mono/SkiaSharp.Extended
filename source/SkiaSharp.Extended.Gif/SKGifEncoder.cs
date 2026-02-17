using System;
using System.IO;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Encodes SKBitmap frames to GIF files (GIF87a and GIF89a).
	/// API aligned with SKRuntimeEffect pattern for consistent error handling.
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
		/// <exception cref="ArgumentException">Thrown when stream is not writable.</exception>
		public SKGifEncoder(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (!stream.CanWrite)
				throw new ArgumentException("Stream must be writable.", nameof(stream));

			this.stream = stream;
		}

		/// <summary>
		/// Creates a GIF encoder for the specified stream, returning detailed error information if creation fails.
		/// Pattern aligned with SKRuntimeEffect.Create* methods.
		/// </summary>
		/// <param name="stream">The stream to write the GIF data to.</param>
		/// <param name="errors">Detailed error message if creation fails, or null if successful.</param>
		/// <returns>A new SKGifEncoder instance, or null if creation failed.</returns>
		public static SKGifEncoder? CreateEncoder(Stream stream, out string? errors)
		{
			if (stream == null)
			{
				errors = "Stream cannot be null.";
				return null;
			}

			if (!stream.CanWrite)
			{
				errors = "Stream must be writable.";
				return null;
			}

			try
			{
				errors = null;
				return new SKGifEncoder(stream);
			}
			catch (Exception ex)
			{
				errors = $"Unexpected error creating GIF encoder: {ex.Message}";
				return null;
			}
		}

		/// <summary>
		/// Creates a GIF encoder for the specified stream, throwing an exception if creation fails.
		/// Pattern aligned with SKRuntimeEffect.Build* methods.
		/// </summary>
		/// <param name="stream">The stream to write the GIF data to.</param>
		/// <returns>A new SKGifEncoder instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
		/// <exception cref="ArgumentException">Thrown when stream is not writable.</exception>
		public static SKGifEncoder BuildEncoder(Stream stream)
		{
			var encoder = CreateEncoder(stream, out var errors);
			if (encoder == null)
			{
				if (string.IsNullOrEmpty(errors))
					throw new ArgumentException("Failed to create GIF encoder. There was an unknown error.");
				else
					throw new ArgumentException($"Failed to create GIF encoder: {errors}");
			}
			return encoder;
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
