using System;
using System.IO;
using SkiaSharp.Extended.Gif.Decoding;

namespace SkiaSharp.Extended.Gif
{
	/// <summary>
	/// Decodes GIF files (GIF87a and GIF89a) to SKBitmap frames.
	/// API aligned with SKCodec patterns for consistency.
	/// </summary>
	public class SKGifDecoder : IDisposable
	{
		private readonly Stream stream;
		private readonly GifImageDecoder decoder;
		private readonly GifFrameCompositor compositor;
		private bool disposed;

		private SKGifDecoder(Stream stream)
		{
			this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
			this.decoder = new GifImageDecoder(stream);
			this.compositor = null!; // Initialized in Initialize()
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
		/// Creates a new GIF decoder from a stream.
		/// Factory pattern aligned with SKCodec.Create.
		/// </summary>
		/// <param name="stream">The stream containing the GIF data.</param>
		/// <returns>A new SKGifDecoder instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
		/// <exception cref="InvalidDataException">Thrown when the stream does not contain valid GIF data.</exception>
		public static SKGifDecoder Create(Stream stream)
		{
			var decoder = new SKGifDecoder(stream);
			decoder.Initialize();
			return decoder;
		}

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

			// Decompress frame data
			var frameData = decoder.GetFrameData(index);
			var indexedPixels = decoder.DecompressFrameData(index);
			var colorTable = decoder.GetColorTableForFrame(index);

			// Render frame with compositing
			var bitmap = compositor.RenderFrame(
				index,
				indexedPixels,
				frameData.ImageDescriptor,
				colorTable,
				frameData.GraphicsControl);

			return new SKGifFrame
			{
				Bitmap = bitmap,
				FrameInfo = FrameInfo[index]
			};
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

			try
			{
				using var frame = GetFrame(index);
				
				// Copy pixels to buffer
				frame.Bitmap.CopyPixelsTo(pixels, info.RowBytes * info.Height, info.RowBytes);
				
				return SKCodecResult.Success;
			}
			catch
			{
				return SKCodecResult.InvalidInput;
			}
		}

		/// <summary>
		/// Disposes the decoder and releases resources.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				compositor?.Dispose();
				disposed = true;
			}

			GC.SuppressFinalize(this);
		}

		private void Initialize()
		{
			// Parse the GIF file
			decoder.Parse();

			// Set up image info
			Info = new SKImageInfo(
				decoder.Width,
				decoder.Height,
				SKColorType.Rgba8888,
				SKAlphaType.Premul);

			// Build frame info array
			var frameInfoList = new SKGifFrameInfo[decoder.FrameCount];
			
			for (int i = 0; i < decoder.FrameCount; i++)
			{
				var frameData = decoder.GetFrameData(i);
				var imageDesc = frameData.ImageDescriptor;
				var gce = frameData.GraphicsControl;

				frameInfoList[i] = new SKGifFrameInfo
				{
					Duration = gce?.DelayTime ?? 0,
					DisposalMethod = ConvertDisposalMethod(gce?.DisposalMethod ?? IO.DisposalMethod.None),
					RequiredFrame = i > 0 ? i - 1 : -1, // Simple dependency for now
					FrameRect = new SKRectI(imageDesc.Left, imageDesc.Top, imageDesc.Left + imageDesc.Width, imageDesc.Top + imageDesc.Height),
					HasTransparency = gce?.HasTransparency ?? false,
					TransparentColor = null // Color not stored, just index
				};
			}

			FrameInfo = frameInfoList;

			// Set GIF-specific info
			GifInfo = new SKGifInfo
			{
				ImageInfo = Info,
				FrameCount = decoder.FrameCount,
				Width = decoder.Width,
				Height = decoder.Height,
				LoopCount = decoder.LoopCount
			};

			// Initialize compositor with global properties
			var compositorField = typeof(SKGifDecoder).GetField("compositor", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			compositorField?.SetValue(this, new GifFrameCompositor(decoder.Width, decoder.Height, decoder.BackgroundColorIndex));
		}

		private SKGifDisposalMethod ConvertDisposalMethod(IO.DisposalMethod method)
		{
			return method switch
			{
				IO.DisposalMethod.None => SKGifDisposalMethod.None,
				IO.DisposalMethod.DoNotDispose => SKGifDisposalMethod.DoNotDispose,
				IO.DisposalMethod.RestoreToBackground => SKGifDisposalMethod.RestoreToBackground,
				IO.DisposalMethod.RestoreToPrevious => SKGifDisposalMethod.RestoreToPrevious,
				_ => SKGifDisposalMethod.None
			};
		}
	}
}
