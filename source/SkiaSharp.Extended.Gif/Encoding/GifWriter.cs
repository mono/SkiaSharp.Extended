using System;
using System.IO;

namespace SkiaSharp.Extended.Gif.Encoding
{
    /// <summary>
    /// Writes GIF file structures and blocks.
    /// </summary>
    internal class GifWriter
    {
        private readonly Stream stream;
        private readonly BinaryWriter writer;
        
        public GifWriter(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.writer = new BinaryWriter(stream);
        }
        
        public void WriteHeader(bool useGif89a = true)
        {
            // Write signature and version
            if (useGif89a)
            {
                writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }); // "GIF89a"
            }
            else
            {
                writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }); // "GIF87a"
            }
        }
        
        public void WriteLogicalScreenDescriptor(
            int width,
            int height,
            bool hasGlobalColorTable,
            int colorTableSize,
            byte backgroundColorIndex = 0,
            byte pixelAspectRatio = 0)
        {
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            
            // Packed byte
            byte packed = 0;
            if (hasGlobalColorTable)
            {
                packed |= 0x80; // Global color table flag
                packed |= 0x70; // Color resolution = 7 (8 bits)
                
                // Calculate size bits (2^(N+1) colors)
                int sizeBits = 0;
                int colors = colorTableSize;
                while (colors > 2)
                {
                    colors >>= 1;
                    sizeBits++;
                }
                packed |= (byte)(sizeBits & 0x07);
            }
            
            writer.Write(packed);
            writer.Write(backgroundColorIndex);
            writer.Write(pixelAspectRatio);
        }
        
        public void WriteColorTable(SKColor[] colors)
        {
            if (colors == null)
                throw new ArgumentNullException(nameof(colors));
            
            foreach (var color in colors)
            {
                writer.Write(color.Red);
                writer.Write(color.Green);
                writer.Write(color.Blue);
            }
        }
        
        public void WriteGraphicsControlExtension(
            SKGifDisposalMethod disposalMethod,
            int delayMs,
            bool hasTransparency,
            byte transparentColorIndex)
        {
            writer.Write((byte)0x21); // Extension introducer
            writer.Write((byte)0xF9); // Graphics Control Label
            writer.Write((byte)0x04); // Block size
            
            byte packed = 0;
            packed |= (byte)(((int)disposalMethod & 0x07) << 2);
            if (hasTransparency)
            {
                packed |= 0x01;
            }
            
            writer.Write(packed);
            writer.Write((ushort)(delayMs / 10)); // Convert ms to centiseconds
            writer.Write(transparentColorIndex);
            writer.Write((byte)0x00); // Block terminator
        }
        
        public void WriteNetscapeLoopExtension(int loopCount)
        {
            writer.Write((byte)0x21); // Extension introducer
            writer.Write((byte)0xFF); // Application Extension Label
            writer.Write((byte)0x0B); // Block size
            
            // NETSCAPE2.0
            writer.Write(new byte[] { 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30 });
            
            writer.Write((byte)0x03); // Sub-block size
            writer.Write((byte)0x01); // Sub-block ID
            writer.Write((ushort)loopCount); // Loop count
            writer.Write((byte)0x00); // Block terminator
        }
        
        public void WriteImageDescriptor(
            int left,
            int top,
            int width,
            int height,
            bool hasLocalColorTable,
            int localColorTableSize,
            bool isInterlaced = false)
        {
            writer.Write((byte)0x2C); // Image separator
            writer.Write((ushort)left);
            writer.Write((ushort)top);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            
            byte packed = 0;
            if (hasLocalColorTable)
            {
                packed |= 0x80;
                
                int sizeBits = 0;
                int colors = localColorTableSize;
                while (colors > 2)
                {
                    colors >>= 1;
                    sizeBits++;
                }
                packed |= (byte)(sizeBits & 0x07);
            }
            if (isInterlaced)
            {
                packed |= 0x40;
            }
            
            writer.Write(packed);
        }
        
        public void WriteImageData(byte[] compressedData, byte lzwMinimumCodeSize)
        {
            writer.Write(lzwMinimumCodeSize);
            
            // Write data in sub-blocks (max 255 bytes each)
            int offset = 0;
            while (offset < compressedData.Length)
            {
                int blockSize = Math.Min(255, compressedData.Length - offset);
                writer.Write((byte)blockSize);
                writer.Write(compressedData, offset, blockSize);
                offset += blockSize;
            }
            
            writer.Write((byte)0x00); // Block terminator
        }
        
        public void WriteTrailer()
        {
            writer.Write((byte)0x3B); // Trailer
        }
        
        public void Flush()
        {
            writer.Flush();
        }
    }
}
