using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkiaSharp.Extended.Gif.Codec
{
    /// <summary>
    /// LZW compressor for GIF image data.
    /// </summary>
    internal class LzwEncoder : IDisposable
    {
        private readonly int minimumCodeSize;
        private readonly int clearCode;
        private readonly int endCode;
        private int codeSize;
        private int maxCode;
        private int nextCode;
        
        private readonly Dictionary<string, int> codeTable;
        private bool disposed;
        
        private int bitBuffer;
        private int bitsInBuffer;
        
        public int MinimumCodeSize => minimumCodeSize;
        
        public LzwEncoder(int minimumCodeSize)
        {
            if (minimumCodeSize < 0 || minimumCodeSize > 8)
                throw new ArgumentOutOfRangeException(nameof(minimumCodeSize));
            
            this.minimumCodeSize = minimumCodeSize;
            this.clearCode = 1 << minimumCodeSize;
            this.endCode = clearCode + 1;
            this.codeTable = new Dictionary<string, int>();
            
            InitializeTables();
        }
        
        private void InitializeTables()
        {
            codeSize = minimumCodeSize + 1;
            maxCode = (1 << codeSize) - 1;
            nextCode = clearCode + 2;
            codeTable.Clear();
        }
        
        public void Compress(byte[] input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            
            InitializeTables();
            bitBuffer = 0;
            bitsInBuffer = 0;
            
            WriteCode(clearCode, output);
            
            if (input.Length == 0)
            {
                WriteCode(endCode, output);
                FlushBits(output);
                return;
            }
            
            var sb = new StringBuilder();
            sb.Append((char)input[0]);
            
            for (int i = 1; i < input.Length; i++)
            {
                var k = (char)input[i];
                var candidate = sb.ToString() + k;
                
                if (codeTable.ContainsKey(candidate))
                {
                    sb.Append(k);
                }
                else
                {
                    // Output code for current string
                    var str = sb.ToString();
                    int code;
                    if (str.Length == 1)
                    {
                        code = (byte)str[0];
                    }
                    else
                    {
                        code = codeTable[str];
                    }
                    WriteCode(code, output);
                    
                    // Add new string if space
                    if (nextCode < 4096)
                    {
                        codeTable[candidate] = nextCode;
                        nextCode++;
                        
                        if (nextCode > maxCode && codeSize < 12)
                        {
                            codeSize++;
                            maxCode = (1 << codeSize) - 1;
                        }
                    }
                    else
                    {
                        WriteCode(clearCode, output);
                        InitializeTables();
                    }
                    
                    sb.Clear();
                    sb.Append(k);
                }
            }
            
            // Output final string
            if (sb.Length > 0)
            {
                var str = sb.ToString();
                int code;
                if (str.Length == 1)
                {
                    code = (byte)str[0];
                }
                else if (codeTable.ContainsKey(str))
                {
                    code = codeTable[str];
                }
                else
                {
                    code = (byte)str[0];
                }
                WriteCode(code, output);
            }
            
            WriteCode(endCode, output);
            FlushBits(output);
        }
        
        private void WriteCode(int code, Stream output)
        {
            bitBuffer |= code << bitsInBuffer;
            bitsInBuffer += codeSize;
            
            while (bitsInBuffer >= 8)
            {
                output.WriteByte((byte)(bitBuffer & 0xFF));
                bitBuffer >>= 8;
                bitsInBuffer -= 8;
            }
        }
        
        private void FlushBits(Stream output)
        {
            if (bitsInBuffer > 0)
            {
                output.WriteByte((byte)(bitBuffer & 0xFF));
                bitBuffer = 0;
                bitsInBuffer = 0;
            }
        }
        
        public void Dispose()
        {
            if (!disposed)
            {
                codeTable.Clear();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
