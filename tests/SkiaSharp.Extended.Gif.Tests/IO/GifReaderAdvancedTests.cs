using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.Extended.Gif.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.IO
{
    public class GifReaderAdvancedTests
    {
        [Fact]
        public void ReadGraphicsControlExtension_ParsesDisposalMethod()
        {
            var data = new byte[]
            {
                0x04, // Block size
                0x0C, // Packed byte: disposal=3 (RestoreToPrevious), transparency=false
                0x64, 0x00, // Delay = 100 centiseconds
                0x00, // Transparent color index
                0x00  // Block terminator
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var gce = reader.ReadGraphicsControlExtension();
            
            Assert.Equal(3, gce.DisposalMethod);
            Assert.False(gce.HasTransparency);
            Assert.Equal(100, gce.DelayTime);
        }
        
        [Fact]
        public void ReadGraphicsControlExtension_ParsesTransparency()
        {
            var data = new byte[]
            {
                0x04, // Block size
                0x01, // Packed byte: disposal=0, transparency=true
                0x32, 0x00, // Delay = 50
                0x05, // Transparent color index = 5
                0x00  // Block terminator
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var gce = reader.ReadGraphicsControlExtension();
            
            Assert.True(gce.HasTransparency);
            Assert.Equal(5, gce.TransparentColorIndex);
        }
        
        [Fact]
        public void ReadApplicationExtension_ParsesNetscape()
        {
            var data = new byte[]
            {
                0x0B, // Block size
                (byte)'N', (byte)'E', (byte)'T', (byte)'S', (byte)'C', (byte)'A', (byte)'P', (byte)'E',
                (byte)'2', (byte)'.', (byte)'0',
                0x03, // Sub-block size
                0x01, // Sub-block ID
                0x05, 0x00, // Loop count = 5
                0x00  // Block terminator
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var appExt = reader.ReadApplicationExtension();
            
            Assert.Equal("NETSCAPE", appExt.ApplicationIdentifier);
            Assert.True(appExt.IsNetscapeExtension);
            Assert.Equal(5, appExt.LoopCount);
        }
        
        [Fact]
        public void ReadApplicationExtension_NonNetscape_ReadsData()
        {
            var data = new byte[]
            {
                0x0B, // Block size
                (byte)'M', (byte)'Y', (byte)'A', (byte)'P', (byte)'P', (byte)'D', (byte)'A', (byte)'T',
                (byte)'A', (byte)'1', (byte)'0',
                0x05, // Sub-block size
                0x01, 0x02, 0x03, 0x04, 0x05,
                0x00  // Block terminator
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var appExt = reader.ReadApplicationExtension();
            
            Assert.Equal("MYAPPDAT", appExt.ApplicationIdentifier);
            Assert.NotNull(appExt.Data);
            Assert.True(appExt.Data.Length > 0);
        }
        
        [Fact]
        public void ReadCommentExtension_ReadsText()
        {
            var comment = "Test Comment";
            var commentBytes = System.Text.Encoding.ASCII.GetBytes(comment);
            
            var data = new byte[commentBytes.Length + 2];
            data[0] = (byte)commentBytes.Length;
            Array.Copy(commentBytes, 0, data, 1, commentBytes.Length);
            data[data.Length - 1] = 0x00; // Block terminator
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var commentText = reader.ReadCommentExtension();
            
            Assert.Equal(comment, commentText);
        }
        
        [Fact]
        public void ReadBlockType_ReturnsCorrectType()
        {
            var data = new byte[] { 0x2C }; // Image separator
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var blockType = reader.ReadBlockType();
            
            Assert.Equal(BlockType.ImageDescriptor, blockType);
        }
        
        [Fact]
        public void ReadExtensionType_ReturnsCorrectType()
        {
            var data = new byte[] { 0xF9 }; // Graphics Control Extension
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var extType = reader.ReadExtensionType();
            
            Assert.Equal(ExtensionType.GraphicsControl, extType);
        }
        
        [Fact]
        public void PeekByte_DoesNotAdvance()
        {
            var data = new byte[] { 0xAA, 0xBB };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var peeked = reader.PeekByte();
            var read = reader.ReadByte();
            
            Assert.Equal(peeked, read);
        }
        
        [Fact]
        public void ReadColorTable_ReadsCorrectColors()
        {
            var data = new byte[]
            {
                0xFF, 0x00, 0x00, // Red
                0x00, 0xFF, 0x00, // Green
                0x00, 0x00, 0xFF, // Blue
                0xFF, 0xFF, 0x00, // Yellow
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var colors = reader.ReadColorTable(4);
            
            Assert.Equal(4, colors.Length);
            Assert.Equal(SKColors.Red, colors[0]);
            Assert.Equal(SKColors.Lime, colors[1]);
            Assert.Equal(SKColors.Blue, colors[2]);
            Assert.Equal(SKColors.Yellow, colors[3]);
        }
        
        [Fact]
        public void ReadDataSubBlocks_ConcatenatesBlocks()
        {
            var data = new byte[]
            {
                0x03, // Block size 3
                0xAA, 0xBB, 0xCC,
                0x02, // Block size 2
                0xDD, 0xEE,
                0x00  // Block terminator
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            var result = reader.ReadDataSubBlocks();
            
            Assert.Equal(5, result.Length);
            Assert.Equal(0xAA, result[0]);
            Assert.Equal(0xBB, result[1]);
            Assert.Equal(0xCC, result[2]);
            Assert.Equal(0xDD, result[3]);
            Assert.Equal(0xEE, result[4]);
        }
        
        [Fact]
        public void SkipExtension_SkipsData()
        {
            var data = new byte[]
            {
                0x05, // Block size
                0x01, 0x02, 0x03, 0x04, 0x05,
                0x00, // Block terminator
                0xAA  // Byte after
            };
            
            using var ms = new MemoryStream(data);
            using var reader = new GifReader(ms);
            
            reader.SkipExtension();
            
            // Should be at 0xAA now
            Assert.Equal(0xAA, reader.ReadByte());
        }
    }
}
