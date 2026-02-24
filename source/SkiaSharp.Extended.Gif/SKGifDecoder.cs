using System;
using System.IO;
using System.Linq;
using SkiaSharp.Extended.Gif.Decoding;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif
{
    /// <summary>
    /// Decodes GIF files (GIF87a and GIF89a) to SKBitmap frames.
    /// API aligned with SKCodec patterns for consistency.
    /// </summary>
    public class SKGifDecoder : IDisposable
    {
        private readonly Stream stream;
        private readonly ParsedGif parsedGif;
        private bool disposed;

        private SKGifDecoder(Stream stream, ParsedGif parsedGif)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.parsedGif = parsedGif ?? throw new ArgumentNullException(nameof(parsedGif));
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
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            
            // Parse the GIF file
            var parser = new GifParser(stream);
            var parsedGif = parser.Parse();
            
            // Create decoder with parsed data
            var decoder = new SKGifDecoder(stream, parsedGif);
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
            
            var parsedFrame = parsedGif.Frames[index];
            var frameInfo = FrameInfo[index];
            
            // Decode the frame
            var bitmap = FrameDecoder.DecodeFrame(
                parsedFrame,
                parsedGif.ScreenDescriptor.Width,
                parsedGif.ScreenDescriptor.Height);
            
            return new SKGifFrame
            {
                Bitmap = bitmap,
                FrameInfo = frameInfo
            };
        }

        /// <summary>
        /// Disposes the decoder and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        private void Initialize()
        {
            var screenDescriptor = parsedGif.ScreenDescriptor;
            
            // Set up image info
            Info = new SKImageInfo(
                screenDescriptor.Width,
                screenDescriptor.Height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul);
            
            // Create GifInfo
            GifInfo = new SKGifInfo
            {
                ImageInfo = Info,
                FrameCount = parsedGif.Frames.Length,
                LoopCount = parsedGif.LoopCount
            };
            
            // Build frame info array
            FrameInfo = parsedGif.Frames.Select((frame, index) => CreateFrameInfo(frame, index)).ToArray();
        }
        
        private SKGifFrameInfo CreateFrameInfo(ParsedFrame frame, int index)
        {
            var descriptor = frame.ImageDescriptor;
            var gce = frame.GraphicsControlExtension;
            
            // Determine required frame based on disposal method
            int requiredFrame = -1;
            if (index > 0)
            {
                var prevGce = parsedGif.Frames[index - 1].GraphicsControlExtension;
                var prevDisposal = prevGce?.DisposalMethod ?? 0;
                
                // If previous frame doesn't dispose, this frame needs it
                if (prevDisposal == 1) // DoNotDispose
                {
                    requiredFrame = index - 1;
                }
            }
            
            return new SKGifFrameInfo
            {
                Duration = gce?.DelayTime ?? 0,
                DisposalMethod = MapDisposalMethod(gce?.DisposalMethod ?? 0),
                RequiredFrame = requiredFrame,
                FrameRect = new SKRectI(descriptor.Left, descriptor.Top, descriptor.Left + descriptor.Width, descriptor.Top + descriptor.Height),
                HasTransparency = gce?.HasTransparency ?? false,
                TransparentColor = GetTransparentColor(frame)
            };
        }
        
        private SKGifDisposalMethod MapDisposalMethod(byte disposalMethod)
        {
            return disposalMethod switch
            {
                0 => SKGifDisposalMethod.None,
                1 => SKGifDisposalMethod.DoNotDispose,
                2 => SKGifDisposalMethod.RestoreToBackground,
                3 => SKGifDisposalMethod.RestoreToPrevious,
                _ => SKGifDisposalMethod.None
            };
        }
        
        private SKColor? GetTransparentColor(ParsedFrame frame)
        {
            var gce = frame.GraphicsControlExtension;
            if (!gce.HasValue || !gce.Value.HasTransparency)
                return null;
            
            var colorTable = frame.GetColorTable();
            var transparentIndex = gce.Value.TransparentColorIndex;
            
            if (transparentIndex < colorTable.Length)
                return colorTable[transparentIndex];
            
            return null;
        }
    }
}
