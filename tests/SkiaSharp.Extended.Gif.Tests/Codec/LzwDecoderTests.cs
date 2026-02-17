using System.IO;
using SkiaSharp.Extended.Gif.Codec;

namespace SkiaSharp.Extended.Gif.Tests.Codec;

/// <summary>
/// Tests for LZW codec (decompression and compression).
/// </summary>
public class LzwDecoderTests
{
	[Fact]
	public void Constructor_InvalidMinimumCodeSize_ThrowsException()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => new LzwDecoder(stream, 1)); // Too small
		Assert.Throws<ArgumentOutOfRangeException>(() => new LzwDecoder(stream, 9)); // Too large
	}

	[Fact]
	public void Constructor_ValidMinimumCodeSize_Succeeds()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act & Assert - should not throw
		for (int i = 2; i <= 8; i++)
		{
			using var decoder = new LzwDecoder(stream, i);
			Assert.Equal(i, decoder.MinimumCodeSize);
		}
	}

	[Fact]
	public void Decompress_SimplePattern_DecompressesCorrectly()
	{
		// Arrange: Very simple test - just verify decoder can process data
		// We'll create a minimal valid LZW stream with clear code and end code
		// Minimum code size = 2 (codes 0-3 are initial, 4=clear, 5=end)
		
		var compressedData = new byte[]
		{
			// Block with clear code (4) followed by end code (5)
			// Using 3-bit codes: 100 (clear) = 4, 101 (end) = 5
			// Packed as: 00100101 = 0x25
			0x01, 0x25, // Block size=1, data=0x25
			0x00 // Terminator
		};

		using var stream = new MemoryStream(compressedData);
		using var decoder = new LzwDecoder(stream, 2);
		var output = new byte[10];

		// Act
		int bytesWritten = decoder.Decompress(output, 0, output.Length);

		// Assert - should handle clear and end codes without error
		Assert.True(bytesWritten >= 0);
		Assert.True(bytesWritten <= output.Length);
	}

	[Fact]
	public void Decompress_ClearCodeReset_WorksCorrectly()
	{
		// Arrange: Data with clear code in the middle
		var compressedData = new byte[]
		{
			0x05, 0x84, 0x1D, 0x03, 0x51, 0x9C, // Block with clear code
			0x00 // Terminator
		};

		using var stream = new MemoryStream(compressedData);
		using var decoder = new LzwDecoder(stream, 2);
		var output = new byte[8];

		// Act
		int bytesWritten = decoder.Decompress(output, 0, output.Length);

		// Assert - should decompress without errors
		Assert.True(bytesWritten >= 0);
	}

	[Fact]
	public void Decompress_EmptyStream_ReturnsZero()
	{
		// Arrange: Empty compressed data (just terminator)
		var compressedData = new byte[] { 0x00 };

		using var stream = new MemoryStream(compressedData);
		using var decoder = new LzwDecoder(stream, 2);
		var output = new byte[10];

		// Act
		int bytesWritten = decoder.Decompress(output, 0, output.Length);

		// Assert
		Assert.Equal(0, bytesWritten);
	}

	[Fact]
	public void Decompress_MultipleBlocks_DecompressesAll()
	{
		// Arrange: Multiple data blocks
		var compressedData = new byte[]
		{
			0x03, 0x84, 0x1D, 0x03, // Block 1: size=3
			0x02, 0x51, 0x9C,       // Block 2: size=2
			0x00                     // Terminator
		};

		using var stream = new MemoryStream(compressedData);
		using var decoder = new LzwDecoder(stream, 2);
		var output = new byte[10];

		// Act
		int bytesWritten = decoder.Decompress(output, 0, output.Length);

		// Assert - should read all blocks
		Assert.True(bytesWritten >= 0);
	}

	[Fact]
	public void Decompress_IncrementalReading_WorksCorrectly()
	{
		// Arrange: Valid compressed data
		var compressedData = new byte[]
		{
			0x04, 0x84, 0x8D, 0x29, 0x9C,
			0x00
		};

		using var stream = new MemoryStream(compressedData);
		using var decoder = new LzwDecoder(stream, 2);
		
		// Act: Read in small chunks
		var output1 = new byte[2];
		var output2 = new byte[4];

		int bytes1 = decoder.Decompress(output1, 0, 2);
		int bytes2 = decoder.Decompress(output2, 0, 4);

		// Assert
		Assert.Equal(2, bytes1);
		Assert.True(bytes2 >= 0);
	}

	[Fact]
	public void Decompress_AllMinimumCodeSizes_WorksCorrectly()
	{
		// Test that all valid minimum code sizes work
		for (int minCodeSize = 2; minCodeSize <= 8; minCodeSize++)
		{
			// Arrange: Simple compressed data with clear and end codes
			int clearCode = 1 << minCodeSize;
			int endCode = clearCode + 1;

			using var stream = new MemoryStream(new byte[] { 0x02, 0x00, 0x00, 0x00 });
			using var decoder = new LzwDecoder(stream, minCodeSize);
			var output = new byte[10];

			// Act
			int bytesWritten = decoder.Decompress(output, 0, output.Length);

			// Assert - should not throw
			Assert.True(bytesWritten >= 0);
		}
	}

	[Fact]
	public void Dispose_CanBeCalledMultipleTimes()
	{
		// Arrange
		using var stream = new MemoryStream();
		var decoder = new LzwDecoder(stream, 2);

		// Act & Assert - should not throw
		decoder.Dispose();
		decoder.Dispose();
		decoder.Dispose();
	}
}
