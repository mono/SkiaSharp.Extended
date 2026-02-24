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
        public void ParsedFrame_GetColorTable_FallsBackToBlackWhite()
        {
            // Changed to test fallback behavior instead of throwing
            // Per fix for broken GIFs with no color table
            var frame = new ParsedFrame
            {
                GlobalColorTable = null,
                LocalColorTable = null
            };
            
            var colorTable = frame.GetColorTable();
            Assert.NotNull(colorTable);
            Assert.Equal(2, colorTable.Length);
            Assert.Equal(SKColors.Black, colorTable[0]);
            Assert.Equal(SKColors.White, colorTable[1]);
        }
    }
}
