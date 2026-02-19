using System;
using System.Buffers;
using System.IO;

namespace SkiaSharp.Extended.Gif.Codec
{
	/// <summary>
	/// LZW decompressor for GIF image data.
	/// Implements the LZW algorithm as specified in the GIF specification.
	/// </summary>
	internal class LzwDecoder : IDisposable
	{
		private readonly Stream inputStream;
		private int[] codeTable;
		private byte[] suffixTable;
		private byte[] pixelStack;
		private int stackPointer;
		private int codeSize;
		private int clearCode;
		private int endCode;
		private int nextCode;
		private int maxCode;
		private int oldCode;
		private byte firstByte;
		private int bitsRead;
		private int dataBuffer;
		private int bitsInBuffer;
		private byte[] blockBuffer;
		private int blockIndex;
		private int blockSize;
		private bool disposed;

		/// <summary>
		/// Minimum code size for this image (0-8 bits).
		/// </summary>
		public int MinimumCodeSize { get; }

		public LzwDecoder(Stream input, int minimumCodeSize)
		{
			if (minimumCodeSize < 0 || minimumCodeSize > 8)
				throw new ArgumentOutOfRangeException(nameof(minimumCodeSize), "Minimum code size must be 0-8.");

			this.inputStream = input ?? throw new ArgumentNullException(nameof(input));
			this.MinimumCodeSize = minimumCodeSize;

			// Initialize LZW tables
			InitializeTables();
		}

		private void InitializeTables()
		{
			// Initial code size is minimum + 1
			codeSize = MinimumCodeSize + 1;
			clearCode = 1 << MinimumCodeSize;
			endCode = clearCode + 1;
			nextCode = clearCode + 2;
			maxCode = (1 << codeSize) - 1;

			// Allocate tables (max size is 4096)
			const int maxTableSize = 4096;
			codeTable = ArrayPool<int>.Shared.Rent(maxTableSize);
			suffixTable = ArrayPool<byte>.Shared.Rent(maxTableSize);
			pixelStack = ArrayPool<byte>.Shared.Rent(maxTableSize + 1);

			// Initialize code table
			for (int i = 0; i < clearCode; i++)
			{
				codeTable[i] = -1; // No prefix
				suffixTable[i] = (byte)i;
			}

			stackPointer = 0;
			oldCode = -1;
			firstByte = 0;
			bitsRead = 0;
			dataBuffer = 0;
			bitsInBuffer = 0;
			blockBuffer = ArrayPool<byte>.Shared.Rent(256);
			blockIndex = 0;
			blockSize = 0;
		}

		/// <summary>
		/// Decompresses LZW data into the output array.
		/// Returns the number of bytes written.
		/// </summary>
		public int Decompress(byte[] output, int offset, int count)
		{
			if (output == null)
				throw new ArgumentNullException(nameof(output));
			if (offset < 0 || offset >= output.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > output.Length)
				throw new ArgumentOutOfRangeException(nameof(count));

			int bytesWritten = 0;

			while (bytesWritten < count)
			{
				// Pop from stack if available
				if (stackPointer > 0)
				{
					output[offset + bytesWritten++] = pixelStack[--stackPointer];
					continue;
				}

				// Read next code
				int code = ReadCode();
				if (code == -1 || code == endCode)
					break; // End of data

				if (code == clearCode)
				{
					// Reset tables
					ResetTables();
					code = ReadCode();
					if (code == -1 || code == endCode)
						break;

					// Output the code directly
					firstByte = (byte)code;
					output[offset + bytesWritten++] = firstByte;
					oldCode = code;
					continue;
				}

				// Handle code
				int inCode = code;
				if (code >= nextCode)
				{
					// Code not in table yet (special case)
					pixelStack[stackPointer++] = firstByte;
					code = oldCode;
				}

				// Decode the code (traverse the string table backwards)
				int loopGuard = 0;
				while (code >= clearCode)
				{
					// Bounds check
					if (code >= 4096)
						throw new InvalidDataException($"LZW decompression error: invalid code {code}");
					if (stackPointer >= pixelStack.Length - 1)
						throw new InvalidDataException("LZW decompression error: stack overflow.");
					
					// Cycle detection
					if (++loopGuard > 4096)
						throw new InvalidDataException("LZW decompression error: circular reference in code table.");
					
					pixelStack[stackPointer++] = suffixTable[code];
					code = codeTable[code];
				}

				// Code is now < clearCode, it's a direct color value
				firstByte = (byte)code;
				pixelStack[stackPointer++] = firstByte;

				// Output the decoded string (reverse order from stack)
				while (stackPointer > 0 && bytesWritten < count)
				{
					output[offset + bytesWritten++] = pixelStack[--stackPointer];
				}

				// Add new entry to table
				if (nextCode < 4096 && oldCode != -1)
				{
					codeTable[nextCode] = oldCode;
					suffixTable[nextCode] = firstByte;
					nextCode++;

					// Increase code size if needed
					if (nextCode > maxCode && codeSize < 12)
					{
						codeSize++;
						maxCode = (1 << codeSize) - 1;
					}
				}

				oldCode = inCode;
			}

			return bytesWritten;
		}

		private void ResetTables()
		{
			codeSize = MinimumCodeSize + 1;
			nextCode = clearCode + 2;
			maxCode = (1 << codeSize) - 1;
			oldCode = -1;
		}

		private int ReadCode()
		{
			// Fill buffer if needed
			while (bitsInBuffer < codeSize)
			{
				// Read next byte from block
				if (blockIndex >= blockSize)
				{
					// Read next block size
					int size = inputStream.ReadByte();
					if (size <= 0)
						return -1; // End of stream

					blockSize = size;
					blockIndex = 0;

					// Read block data
					int bytesRead = inputStream.Read(blockBuffer, 0, blockSize);
					if (bytesRead < blockSize)
						return -1; // Truncated data
				}

				dataBuffer |= blockBuffer[blockIndex++] << bitsInBuffer;
				bitsInBuffer += 8;
			}

			// Extract code
			int code = dataBuffer & ((1 << codeSize) - 1);
			dataBuffer >>= codeSize;
			bitsInBuffer -= codeSize;
			bitsRead += codeSize;

			return code;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				if (codeTable != null)
					ArrayPool<int>.Shared.Return(codeTable);
				if (suffixTable != null)
					ArrayPool<byte>.Shared.Return(suffixTable);
				if (pixelStack != null)
					ArrayPool<byte>.Shared.Return(pixelStack);
				if (blockBuffer != null)
					ArrayPool<byte>.Shared.Return(blockBuffer);

				codeTable = null!;
				suffixTable = null!;
				pixelStack = null!;
				blockBuffer = null!;

				disposed = true;
			}
		}
	}
}
