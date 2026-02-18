using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp.Extended.Gif.Codec;
using SkiaSharp.Extended.Gif.Encoding;

namespace SkiaSharp.Extended.Gif
{
    /// <summary>
    /// Encodes SKBitmap frames to GIF files (GIF87a and GIF89a).
    /// </summary>
    public class SKGifEncoder : IDisposable
    {
        private readonly Stream stream;
        private readonly GifWriter writer;
        private readonly List<FrameData> frames;
        private bool disposed;
        private bool headerWritten;
        
        private int width;
        private int height;
        private int loopCount = -1; // -1 means no loop extension
        private SKColor[]? globalPalette;
        
        public SKGifEncoder(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite)
                throw new ArgumentException("Stream must be writable", nameof(stream));
            
            this.stream = stream;
            this.writer = new GifWriter(stream);
            this.frames = new List<FrameData>();
        }
        
        /// <summary>
        /// Sets the loop count for animated GIFs.
        /// 0 = loop forever, >0 = loop N times, -1 = no loop extension (default).
        /// </summary>
        public void SetLoopCount(int count)
        {
            if (headerWritten)
                throw new InvalidOperationException("Cannot set loop count after encoding has started");
            
            loopCount = count;
        }
        
        /// <summary>
        /// Adds a frame to the GIF.
        /// </summary>
        /// <param name="bitmap">The frame bitmap.</param>
        /// <param name="duration">Frame duration in milliseconds (default 100ms).</param>
        public void AddFrame(SKBitmap bitmap, int duration = 100)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            AddFrame(bitmap, new SKGifFrameInfo
            {
                Duration = duration,
                DisposalMethod = SKGifDisposalMethod.None,
                RequiredFrame = -1,
                FrameRect = new SKRectI(0, 0, bitmap.Width, bitmap.Height),
                HasTransparency = false,
                TransparentColor = null
            });
        }
        
        /// <summary>
        /// Adds a frame with specific frame info.
        /// </summary>
        public void AddFrame(SKBitmap bitmap, SKGifFrameInfo frameInfo)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            
            if (frames.Count == 0)
            {
                width = bitmap.Width;
                height = bitmap.Height;
            }
            else if (bitmap.Width != width || bitmap.Height != height)
            {
                throw new ArgumentException($"All frames must have the same dimensions ({width}x{height})", nameof(bitmap));
            }
            
            frames.Add(new FrameData
            {
                Bitmap = bitmap,
                FrameInfo = frameInfo
            });
        }
        
        /// <summary>
        /// Encodes all added frames to the GIF file.
        /// </summary>
        public void Encode()
        {
            System.Diagnostics.Debug.WriteLine("Encode: Starting");
            if (frames.Count == 0)
                throw new InvalidOperationException("No frames added");
            
            System.Diagnostics.Debug.WriteLine($"Encode: Generating palette from {frames.Count} frames");
            // Generate global color palette from all frames
            globalPalette = GenerateGlobalPalette();
            System.Diagnostics.Debug.WriteLine($"Encode: Palette generated with {globalPalette.Length} colors");
            
            // Write header
            System.Diagnostics.Debug.WriteLine("Encode: Writing header");
            writer.WriteHeader(useGif89a: true);
            
            // Write logical screen descriptor
            System.Diagnostics.Debug.WriteLine("Encode: Writing logical screen descriptor");
            writer.WriteLogicalScreenDescriptor(
                width,
                height,
                hasGlobalColorTable: true,
                colorTableSize: globalPalette.Length,
                backgroundColorIndex: 0);
            
            // Write global color table
            System.Diagnostics.Debug.WriteLine("Encode: Writing color table");
            writer.WriteColorTable(globalPalette);
            
            // Write NETSCAPE loop extension if needed
            if (loopCount >= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Encode: Writing loop extension (count={loopCount})");
                writer.WriteNetscapeLoopExtension(loopCount);
            }
            
            headerWritten = true;
            
            // Write each frame
            System.Diagnostics.Debug.WriteLine($"Encode: Writing {frames.Count} frames");
            for (int i = 0; i < frames.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"Encode: Writing frame {i}");
                WriteFrame(frames[i]);
                System.Diagnostics.Debug.WriteLine($"Encode: Frame {i} written");
            }
            
            // Write trailer
            System.Diagnostics.Debug.WriteLine("Encode: Writing trailer");
            writer.WriteTrailer();
            System.Diagnostics.Debug.WriteLine("Encode: Flushing");
            writer.Flush();
            System.Diagnostics.Debug.WriteLine("Encode: Complete");
        }
        
        private SKColor[] GenerateGlobalPalette()
        {
            System.Diagnostics.Debug.WriteLine("GenerateGlobalPalette: Starting");
            // For simplicity, quantize the first frame
            // A better implementation would collect colors from all frames
            var firstFrame = frames[0].Bitmap;
            System.Diagnostics.Debug.WriteLine($"GenerateGlobalPalette: Quantizing {firstFrame.Width}x{firstFrame.Height} bitmap");
            var palette = ColorQuantizer.QuantizeColors(firstFrame, 256);
            System.Diagnostics.Debug.WriteLine($"GenerateGlobalPalette: Returned {palette.Length} colors");
            return palette;
        }
        
        private void WriteFrame(FrameData frameData)
        {
            System.Diagnostics.Debug.WriteLine("WriteFrame: Starting");
            var bitmap = frameData.Bitmap;
            var frameInfo = frameData.FrameInfo;
            
            // Write Graphics Control Extension if needed
            if (frameInfo.Duration > 0 || frameInfo.HasTransparency)
            {
                System.Diagnostics.Debug.WriteLine("WriteFrame: Writing GCE");
                byte transparentIndex = 0;
                if (frameInfo.HasTransparency && frameInfo.TransparentColor.HasValue)
                {
                    transparentIndex = ColorQuantizer.FindNearestColor(
                        frameInfo.TransparentColor.Value,
                        globalPalette!);
                }
                
                writer.WriteGraphicsControlExtension(
                    frameInfo.DisposalMethod,
                    frameInfo.Duration,
                    frameInfo.HasTransparency,
                    transparentIndex);
            }
            
            // Write image descriptor (no local color table)
            System.Diagnostics.Debug.WriteLine("WriteFrame: Writing image descriptor");
            writer.WriteImageDescriptor(
                left: 0,
                top: 0,
                width: bitmap.Width,
                height: bitmap.Height,
                hasLocalColorTable: false,
                localColorTableSize: 0,
                isInterlaced: false);
            
            // Map bitmap pixels to palette indices
            System.Diagnostics.Debug.WriteLine($"WriteFrame: Mapping {bitmap.Width}x{bitmap.Height} pixels to palette");
            var indexedData = ColorQuantizer.MapBitmapToPalette(bitmap, globalPalette!);
            System.Diagnostics.Debug.WriteLine($"WriteFrame: Mapped to {indexedData.Length} bytes");
            
            // Compress with LZW
            System.Diagnostics.Debug.WriteLine("WriteFrame: Compressing data");
            var compressedData = CompressData(indexedData);
            System.Diagnostics.Debug.WriteLine($"WriteFrame: Compressed to {compressedData.Length} bytes");
            
            // Write compressed image data
            byte minCodeSize = CalculateMinCodeSize(globalPalette!.Length);
            System.Diagnostics.Debug.WriteLine($"WriteFrame: Writing image data (minCodeSize={minCodeSize})");
            writer.WriteImageData(compressedData, minCodeSize);
            System.Diagnostics.Debug.WriteLine("WriteFrame: Complete");
        }
        
        private byte[] CompressData(byte[] data)
        {
            byte minCodeSize = CalculateMinCodeSize(globalPalette!.Length);
            
            using var ms = new MemoryStream();
            using var encoder = new LzwEncoder(minCodeSize);
            encoder.Compress(data, ms);
            return ms.ToArray();
        }
        
        private byte CalculateMinCodeSize(int colorCount)
        {
            byte size = 2;
            int colors = 4;
            while (colors < colorCount && size < 8)
            {
                size++;
                colors *= 2;
            }
            return size;
        }
        
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
            
            GC.SuppressFinalize(this);
        }
        
        private class FrameData
        {
            public SKBitmap Bitmap { get; set; } = null!;
            public SKGifFrameInfo FrameInfo { get; set; }
        }
    }
}
