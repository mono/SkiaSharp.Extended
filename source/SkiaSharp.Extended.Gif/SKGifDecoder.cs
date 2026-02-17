using System;
using System.IO;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Decodes GIF files (GIF87a and GIF89a) to SKBitmap frames.
	/// API aligned with SKCodec and SKRuntimeEffect patterns for consistency.
	/// </summary>
	public class SKGifDecoder : IDisposable
	{
		private readonly Stream stream;
		private bool disposed;

		private SKGifDecoder(Stream stream)
		{
			this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
		}

		/// <summary>
		/// Gets the image information (width, height, color type, alpha type).
		/// Aligned with SKCodec.Info.
		/// </summary>
		public SKImageInfo Info { get; private set; }

		/// <summary>
		/// Gets GIF-specific metadata including loop count and extensions.
		/// </summary>
		public SKGifInfo GifInfo { get; private set; } = null!;

		/// <summary>
		/// Gets the number of frames in the GIF.
		/// Aligned with SKCodec.FrameCount.
		/// </summary>
		public int FrameCount => GifInfo.FrameCount;

		/// <summary>
		/// Gets information about all frames in the GIF.
		/// Aligned with SKCodec.FrameInfo.
		/// </summary>
		public SKGifFrameInfo[] FrameInfo { get; private set; } = null!;

		/// <summary>
		/// Creates a GIF decoder from a stream, returning detailed error information if decoding fails.
		/// Pattern aligned with SKRuntimeEffect.Create* methods.
		/// </summary>
		/// <param name="stream">The stream containing the GIF data.</param>
		/// <param name="errors">Detailed error message if decoding fails, or null if successful.</param>
		/// <returns>A new SKGifDecoder instance, or null if decoding failed.</returns>
		public static SKGifDecoder? CreateDecoder(Stream stream, out string? errors)
		{
			if (stream == null)
			{
				errors = "Stream cannot be null.";
				return null;
			}

			try
			{
				var decoder = new SKGifDecoder(stream);
				decoder.Initialize();
				errors = null;
				return decoder;
			}
			catch (InvalidDataException ex)
			{
				errors = ex.Message;
				return null;
			}
			catch (Exception ex)
			{
				errors = $"Unexpected error decoding GIF: {ex.Message}";
				return null;
			}
		}

		/// <summary>
		/// Creates a GIF decoder from a stream, throwing an exception if decoding fails.
		/// Pattern aligned with SKRuntimeEffect.Build* methods.
		/// </summary>
		/// <param name="stream">The stream containing the GIF data.</param>
		/// <returns>A new SKGifDecoder instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
		/// <exception cref="InvalidDataException">Thrown when the stream does not contain valid GIF data.</exception>
		public static SKGifDecoder BuildDecoder(Stream stream)
		{
			var decoder = CreateDecoder(stream, out var errors);
			if (decoder == null)
			{
				if (string.IsNullOrEmpty(errors))
					throw new InvalidDataException("Failed to decode GIF file. There was an unknown error.");
				else
					throw new InvalidDataException($"Failed to decode GIF file: {errors}");
			}
			return decoder;
		}

		/// <summary>
		/// Creates a new GIF decoder from a stream.
		/// Factory pattern aligned with SKCodec.Create.
		/// </summary>
		/// <param name="stream">The stream containing the GIF data.</param>
		/// <returns>A new SKGifDecoder instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
		/// <exception cref="InvalidDataException">Thrown when the stream does not contain valid GIF data.</exception>
		public static SKGifDecoder Create(Stream stream) => BuildDecoder(stream);

		/// <summary>
		/// Gets information about a specific frame.
		/// </summary>
		/// <param name="index">The zero-based frame index.</param>
		/// <returns>Frame information.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
		public SKGifFrameInfo GetFrameInfo(int index)
		{
			if (index < 0 || index >= FrameCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			return FrameInfo[index];
		}

		/// <summary>
		/// Gets the specified frame from the GIF as a decoded bitmap.
		/// </summary>
		/// <param name="index">The zero-based index of the frame to retrieve.</param>
		/// <returns>The decoded frame with bitmap and metadata.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
		public SKGifFrame GetFrame(int index)
		{
			if (index < 0 || index >= FrameCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			// TODO: Implement frame decoding
			throw new NotImplementedException("GIF decoding is not yet implemented. This is a placeholder for the project structure.");
		}

		/// <summary>
		/// Decodes the specified frame directly into a pixel buffer.
		/// Lower-level API aligned with SKCodec.GetPixels pattern.
		/// </summary>
		/// <param name="index">The zero-based frame index.</param>
		/// <param name="info">The image info for the destination buffer.</param>
		/// <param name="pixels">Pointer to the destination pixel buffer.</param>
		/// <returns>Result code indicating success or failure.</returns>
		public SKCodecResult GetPixels(int index, SKImageInfo info, IntPtr pixels)
		{
			if (index < 0 || index >= FrameCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			// TODO: Implement direct pixel decoding
			throw new NotImplementedException("GIF decoding is not yet implemented. This is a placeholder for the project structure.");
		}

		/// <summary>
		/// Disposes the decoder and releases resources.
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

		private void Initialize()
		{
			// TODO: Implement GIF header parsing and metadata extraction
			Info = new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul);
			GifInfo = new SKGifInfo
			{
				ImageInfo = Info,
				FrameCount = 0
			};
			FrameInfo = Array.Empty<SKGifFrameInfo>();

			throw new NotImplementedException("GIF decoding is not yet implemented. This is a placeholder for the project structure.");
		}
	}
}
