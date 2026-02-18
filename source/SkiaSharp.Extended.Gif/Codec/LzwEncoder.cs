using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace SkiaSharp.Extended.Gif.Codec
{
    /// <summary>
    /// LZW compressor for GIF image data.
    /// Implements the LZW algorithm as specified in the GIF specification.
    /// </summary>
    internal class LzwEncoder : IDisposable
    {
        private readonly int minimumCodeSize;
        private readonly int clearCode;
        private readonly int endCode;
        private int codeSize;
        private int maxCode;
        private int nextCode;
        
        private readonly Dictionary<int, int> codeTable;
        private int currentPrefix;
        private bool disposed;
        
        // Bit packing state
        private int bitBuffer;
        private int bitsInBuffer;
        
        public int MinimumCodeSize => minimumCodeSize;
        
        public LzwEncoder(int minimumCodeSize)
        {
            if (minimumCodeSize < 2 || minimumCodeSize > 8)
                throw new ArgumentOutOfRangeException(nameof(minimumCodeSize), "Minimum code size must be 2-8.");
            
            this.minimumCodeSize = minimumCodeSize;
            this.clearCode = 1 << minimumCodeSize;
            this.endCode = clearCode + 1;
            this.codeTable = new Dictionary<int, int>();
            
            InitializeTables();
        }
        
        private void InitializeTables()
        {
            codeSize = minimumCodeSize + 1;
            maxCode = (1 << codeSize) - 1;
            nextCode = clearCode + 2;
            currentPrefix = -1;
            
            codeTable.Clear();
        }
        
        /// <summary>
        /// Compresses data using LZW algorithm.
        /// </summary>
        /// <param name="input">Input byte array to compress.</param>
        /// <param name="output">Output stream for compressed data.</param>
        public void Compress(byte[] input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            
            // Reset state
            InitializeTables();
            bitBuffer = 0;
            bitsInBuffer = 0;
            
            // Start with clear code
            WriteCode(clearCode, output);
            
            currentPrefix = -1;
            
            for (int i = 0; i < input.Length; i++)
            {
                int k = input[i];
                
                if (currentPrefix == -1)
                {
                    // First byte
                    currentPrefix = k;
                }
                else
                {
                    // Combine prefix and k
                    int combined = (currentPrefix << 8) | k;
                    
                    if (codeTable.ContainsKey(combined))
                    {
                        // String is in table, extend prefix
                        currentPrefix = codeTable[combined];
                    }
                    else
                    {
                        // String not in table
                        // Output current prefix
                        WriteCode(currentPrefix, output);
                        
                        // Add new string to table if space available
                        if (nextCode < 4096)
                        {
                            codeTable[combined] = nextCode;
                            nextCode++;
                            
                            // Increase code size if needed
                            if (nextCode > maxCode && codeSize < 12)
                            {
                                codeSize++;
                                maxCode = (1 << codeSize) - 1;
                            }
                        }
                        else
                        {
                            // Table full, send clear code and reset
                            WriteCode(clearCode, output);
                            InitializeTables();
                        }
                        
                        currentPrefix = k;
                    }
                }
            }
            
            // Output final prefix
            if (currentPrefix != -1)
            {
                WriteCode(currentPrefix, output);
            }
            
            // End with end code
            WriteCode(endCode, output);
            
            // Flush remaining bits
            FlushBits(output);
        }
        
        private void WriteCode(int code, Stream output)
        {
            // Pack code into bit buffer
            bitBuffer |= code << bitsInBuffer;
            bitsInBuffer += codeSize;
            
            // Write complete bytes
            while (bitsInBuffer >= 8)
            {
                output.WriteByte((byte)(bitBuffer & 0xFF));
                bitBuffer >>= 8;
                bitsInBuffer -= 8;
            }
        }
        
        private void FlushBits(Stream output)
        {
            // Write any remaining bits
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
