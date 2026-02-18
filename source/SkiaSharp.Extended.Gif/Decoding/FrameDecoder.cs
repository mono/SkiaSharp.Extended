using System;
using System.Buffers;
using System.IO;
using SkiaSharp.Extended.Gif.Codec;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif.Decoding
{
    /// <summary>
    /// Decodes individual GIF frames from compressed data to bitmaps.
    /// </summary>
    internal class FrameDecoder
    {
        /// <summary>
        /// Decodes a frame to a bitmap.
        /// </summary>
        /// <param name="frame">The parsed frame data.</param>
        /// <param name="screenWidth">The logical screen width.</param>
        /// <param name="screenHeight">The logical screen height.</param>
        /// <returns>The decoded bitmap.</returns>
        public static SKBitmap DecodeFrame(ParsedFrame frame, int screenWidth, int screenHeight)
        {
            var colorTable = frame.GetColorTable();
            var descriptor = frame.ImageDescriptor;
            
            // Decompress LZW data
            var indexStream = DecompressLzw(frame.CompressedData, frame.LzwMinimumCodeSize);
            
            // Get transparency info
            var transparentIndex = frame.GraphicsControlExtension?.TransparentColorIndex;
            var hasTransparency = frame.GraphicsControlExtension?.HasTransparency ?? false;
            
            // Create bitmap for this frame
            var frameBitmap = new SKBitmap(descriptor.Width, descriptor.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            
            // Decode pixels
            DecodePixels(frameBitmap, indexStream, colorTable, descriptor, transparentIndex, hasTransparency);
            
            // If frame is smaller than screen or positioned, create full-size bitmap
            if (descriptor.Left != 0 || descriptor.Top != 0 || 
                descriptor.Width != screenWidth || descriptor.Height != screenHeight)
            {
                var fullBitmap = new SKBitmap(screenWidth, screenHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (var canvas = new SKCanvas(fullBitmap))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawBitmap(frameBitmap, descriptor.Left, descriptor.Top);
                }
                frameBitmap.Dispose();
                return fullBitmap;
            }
            
            return frameBitmap;
        }
        
        private static byte[] DecompressLzw(byte[] compressedData, byte minCodeSize)
        {
            using var memoryStream = new MemoryStream(compressedData);
            using var decoder = new LzwDecoder(memoryStream, minCodeSize);
            
            var outputBuffer = ArrayPool<byte>.Shared.Rent(1024 * 1024); // 1MB initial buffer
            try
            {
                int totalRead = 0;
                int bytesRead;
                
                while ((bytesRead = decoder.Decompress(outputBuffer, totalRead, outputBuffer.Length - totalRead)) > 0)
                {
                    totalRead += bytesRead;
                    
                    // Expand buffer if needed
                    if (totalRead + 4096 > outputBuffer.Length)
                    {
                        var newBuffer = ArrayPool<byte>.Shared.Rent(outputBuffer.Length * 2);
                        Array.Copy(outputBuffer, newBuffer, totalRead);
                        ArrayPool<byte>.Shared.Return(outputBuffer);
                        outputBuffer = newBuffer;
                    }
                }
                
                var result = new byte[totalRead];
                Array.Copy(outputBuffer, result, totalRead);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(outputBuffer);
            }
        }
        
        private static void DecodePixels(
            SKBitmap bitmap,
            byte[] indexStream,
            SKColor[] colorTable,
            ImageDescriptor descriptor,
            byte? transparentIndex,
            bool hasTransparency)
        {
            var width = descriptor.Width;
            var height = descriptor.Height;
            var isInterlaced = descriptor.IsInterlaced;
            
            unsafe
            {
                var pixels = (uint*)bitmap.GetPixels().ToPointer();
                
                if (isInterlaced)
                {
                    DecodeInterlaced(pixels, indexStream, colorTable, width, height, transparentIndex, hasTransparency);
                }
                else
                {
                    DecodeNonInterlaced(pixels, indexStream, colorTable, width, height, transparentIndex, hasTransparency);
                }
            }
        }
        
        private static unsafe void DecodeNonInterlaced(
            uint* pixels,
            byte[] indexStream,
            SKColor[] colorTable,
            int width,
            int height,
            byte? transparentIndex,
            bool hasTransparency)
        {
            int pixelCount = width * height;
            int streamPos = 0;
            
            for (int i = 0; i < pixelCount && streamPos < indexStream.Length; i++, streamPos++)
            {
                byte colorIndex = indexStream[streamPos];
                
                if (hasTransparency && colorIndex == transparentIndex)
                {
                    pixels[i] = 0; // Transparent
                }
                else
                {
                    if (colorIndex < colorTable.Length)
                    {
                        var color = colorTable[colorIndex];
                        pixels[i] = (uint)color;
                    }
                    else
                    {
                        pixels[i] = 0; // Invalid index, use transparent
                    }
                }
            }
        }
        
        private static unsafe void DecodeInterlaced(
            uint* pixels,
            byte[] indexStream,
            SKColor[] colorTable,
            int width,
            int height,
            byte? transparentIndex,
            bool hasTransparency)
        {
            // GIF interlacing uses 4 passes with specific row patterns
            int streamPos = 0;
            
            // Pass 1: Every 8th row, starting with row 0
            for (int y = 0; y < height; y += 8)
            {
                DecodeRow(pixels, indexStream, ref streamPos, colorTable, width, y, transparentIndex, hasTransparency);
            }
            
            // Pass 2: Every 8th row, starting with row 4
            for (int y = 4; y < height; y += 8)
            {
                DecodeRow(pixels, indexStream, ref streamPos, colorTable, width, y, transparentIndex, hasTransparency);
            }
            
            // Pass 3: Every 4th row, starting with row 2
            for (int y = 2; y < height; y += 4)
            {
                DecodeRow(pixels, indexStream, ref streamPos, colorTable, width, y, transparentIndex, hasTransparency);
            }
            
            // Pass 4: Every 2nd row, starting with row 1
            for (int y = 1; y < height; y += 2)
            {
                DecodeRow(pixels, indexStream, ref streamPos, colorTable, width, y, transparentIndex, hasTransparency);
            }
        }
        
        private static unsafe void DecodeRow(
            uint* pixels,
            byte[] indexStream,
            ref int streamPos,
            SKColor[] colorTable,
            int width,
            int y,
            byte? transparentIndex,
            bool hasTransparency)
        {
            int rowStart = y * width;
            
            for (int x = 0; x < width && streamPos < indexStream.Length; x++, streamPos++)
            {
                byte colorIndex = indexStream[streamPos];
                int pixelIndex = rowStart + x;
                
                if (hasTransparency && colorIndex == transparentIndex)
                {
                    pixels[pixelIndex] = 0; // Transparent
                }
                else
                {
                    if (colorIndex < colorTable.Length)
                    {
                        var color = colorTable[colorIndex];
                        pixels[pixelIndex] = (uint)color;
                    }
                    else
                    {
                        pixels[pixelIndex] = 0; // Invalid index
                    }
                }
            }
        }
    }
}
