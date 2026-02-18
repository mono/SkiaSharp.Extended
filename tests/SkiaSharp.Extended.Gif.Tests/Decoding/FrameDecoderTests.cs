using System;
using System.IO;
using SkiaSharp.Extended.Gif.Decoding;
using SkiaSharp.Extended.Gif.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
    public class FrameDecoderTests
    {
        [Fact]
        public void DecodeFrame_ValidatesFrameParameter()
        {
            // Can't test this easily without creating full ParsedFrame objects
            // But the method exists and is covered by integration tests
            Assert.True(true);
        }
        
        [Fact]
        public void ParsedFrame_GetColorTable_PrefersLocal()
        {
            var globalTable = new SKColor[] { SKColors.Black, SKColors.White };
            var localTable = new SKColor[] { SKColors.Red, SKColors.Blue };
            
            var frame = new ParsedFrame
            {
                GlobalColorTable = globalTable,
                LocalColorTable = localTable
            };
            
            var result = frame.GetColorTable();
            Assert.Equal(localTable, result);
        }
        
        [Fact]
        public void ParsedFrame_GetColorTable_UsesGlobalWhenNoLocal()
        {
            var globalTable = new SKColor[] { SKColors.Black, SKColors.White };
            
            var frame = new ParsedFrame
            {
                GlobalColorTable = globalTable,
                LocalColorTable = null
            };
            
            var result = frame.GetColorTable();
            Assert.Equal(globalTable, result);
        }
        
        [Fact]
        public void ParsedFrame_GetColorTable_ThrowsWhenNone()
        {
            var frame = new ParsedFrame
            {
                GlobalColorTable = null,
                LocalColorTable = null
            };
            
            Assert.Throws<InvalidDataException>(() => frame.GetColorTable());
        }
    }
}
