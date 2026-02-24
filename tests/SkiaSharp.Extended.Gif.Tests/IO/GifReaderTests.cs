using System.IO;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif.Tests.IO;

/// <summary>
/// Tests for GIF block I/O structures and reading.
/// </summary>
public class GifReaderTests
{
	[Fact]
	public void ReadHeader_ValidGif87a_ReadsCorrectly()
	{
		// Arrange: GIF87a header
		var bytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }; // "GIF87a"
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var header = reader.ReadHeader();

		// Assert
		Assert.Equal("GIF", header.Signature);
		Assert.Equal("87a", header.Version);
		Assert.True(header.IsValid);
		Assert.False(header.IsGif89a);
	}

	[Fact]
	public void ReadHeader_ValidGif89a_ReadsCorrectly()
	{
		// Arrange: GIF89a header
		var bytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // "GIF89a"
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var header = reader.ReadHeader();

		// Assert
		Assert.Equal("GIF", header.Signature);
		Assert.Equal("89a", header.Version);
		Assert.True(header.IsValid);
		Assert.True(header.IsGif89a);
	}

	[Fact]
	public void ReadHeader_InvalidSignature_ThrowsInvalidDataException()
	{
		// Arrange: Invalid header
		var bytes = new byte[] { 0x50, 0x4E, 0x47, 0x38, 0x39, 0x61 }; // "PNG89a"
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act & Assert
		Assert.Throws<InvalidDataException>(() => reader.ReadHeader());
	}

	[Fact]
	public void ReadHeader_TruncatedStream_ThrowsInvalidDataException()
	{
		// Arrange: Only 3 bytes
		var bytes = new byte[] { 0x47, 0x49, 0x46 }; // "GIF"
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act & Assert
		Assert.Throws<InvalidDataException>(() => reader.ReadHeader());
	}

	[Fact]
	public void ReadLogicalScreenDescriptor_ReadsCorrectly()
	{
		// Arrange: Logical Screen Descriptor
		var bytes = new byte[]
		{
			0x0A, 0x00, // Width = 10
			0x0F, 0x00, // Height = 15
			0xF7,       // Packed: GlobalColorTable=1, ColorRes=7, Sort=0, Size=7
			0x00,       // BackgroundColorIndex = 0
			0x00        // PixelAspectRatio = 0
		};
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var lsd = reader.ReadLogicalScreenDescriptor();

		// Assert
		Assert.Equal(10, lsd.Width);
		Assert.Equal(15, lsd.Height);
		Assert.True(lsd.HasGlobalColorTable);
		Assert.Equal(7, lsd.ColorResolution);
		Assert.False(lsd.SortFlag);
		Assert.Equal(7, lsd.GlobalColorTableSize);
		Assert.Equal(256, lsd.GlobalColorTableLength); // 2^(7+1) = 256
		Assert.Equal(0, lsd.BackgroundColorIndex);
		Assert.Equal(0, lsd.PixelAspectRatio);
	}

	[Fact]
	public void ReadColorTable_ReadsCorrectly()
	{
		// Arrange: 3 RGB triplets
		var bytes = new byte[]
		{
			0xFF, 0x00, 0x00, // Red
			0x00, 0xFF, 0x00, // Green
			0x00, 0x00, 0xFF  // Blue
		};
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var colors = reader.ReadColorTable(3);

		// Assert
		Assert.Equal(3, colors.Length);
		Assert.Equal(new SKColor(255, 0, 0), colors[0]);
		Assert.Equal(new SKColor(0, 255, 0), colors[1]);
		Assert.Equal(new SKColor(0, 0, 255), colors[2]);
	}

	[Fact]
	public void ReadImageDescriptor_ReadsCorrectly()
	{
		// Arrange: Image Descriptor
		var bytes = new byte[]
		{
			0x05, 0x00, // Left = 5
			0x0A, 0x00, // Top = 10
			0x14, 0x00, // Width = 20
			0x1E, 0x00, // Height = 30
			0xC3        // Packed: LocalColorTable=1, Interlace=1, Sort=0, Size=3
		};
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var desc = reader.ReadImageDescriptor();

		// Assert
		Assert.Equal(5, desc.Left);
		Assert.Equal(10, desc.Top);
		Assert.Equal(20, desc.Width);
		Assert.Equal(30, desc.Height);
		Assert.True(desc.HasLocalColorTable);
		Assert.True(desc.InterlaceFlag);
		Assert.False(desc.SortFlag);
		Assert.Equal(3, desc.LocalColorTableSize);
		Assert.Equal(16, desc.LocalColorTableLength); // 2^(3+1) = 16
	}

	[Fact]
	public void ReadGraphicsControlExtension_ReadsCorrectly()
	{
		// Arrange: Graphics Control Extension
		var bytes = new byte[]
		{
			0x04,       // Block size = 4
			0x09,       // Packed: Disposal=2, UserInput=0, Transparency=1
			0x0A, 0x00, // DelayTime = 10 (centiseconds) = 100ms
			0x05,       // TransparentColorIndex = 5
			0x00        // Block terminator
		};
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var gce = reader.ReadGraphicsControlExtension();

		// Assert
		Assert.Equal(2, gce.DisposalMethod);
		Assert.False(gce.UserInputFlag);
		Assert.True(gce.TransparencyFlag);
		Assert.Equal(10, gce.DelayTime);
		Assert.Equal(100, gce.DelayMs);
		Assert.Equal(5, gce.TransparentColorIndex);
	}

	[Fact]
	public void ReadDataSubBlocks_ReadsCorrectly()
	{
		// Arrange: Data sub-blocks
		var bytes = new byte[]
		{
			0x03, 0x41, 0x42, 0x43, // Block 1: size=3, data="ABC"
			0x02, 0x44, 0x45,       // Block 2: size=2, data="DE"
			0x00                     // Terminator
		};
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var data = reader.ReadDataSubBlocks();

		// Assert
		Assert.Equal(5, data.Length);
		Assert.Equal(new byte[] { 0x41, 0x42, 0x43, 0x44, 0x45 }, data);
	}

	[Fact]
	public void ReadDataSubBlocks_EmptyBlocks_ReturnsEmpty()
	{
		// Arrange: Just terminator
		var bytes = new byte[] { 0x00 };
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var data = reader.ReadDataSubBlocks();

		// Assert
		Assert.Empty(data);
	}

	[Fact]
	public void ReadApplicationExtension_NetscapeExtension_ParsesLoopCount()
	{
		// Arrange: NETSCAPE2.0 extension with loop count = 5
		var bytes = new byte[]
		{
			0x0B,                                  // Block size = 11
			0x4E, 0x45, 0x54, 0x53, 0x43, 0x41,   // "NETSCA"
			0x50, 0x45,                            // "PE"
			0x32, 0x2E, 0x30,                      // "2.0" (auth code)
			0x03, 0x01, 0x05, 0x00,                // Data sub-block: size=3, id=1, loop=5 (little endian)
			0x00                                    // Terminator
		};
		using var stream = new MemoryStream(bytes);
		using var reader = new GifReader(stream);

		// Act
		var appExt = reader.ReadApplicationExtension();

		// Assert
		Assert.Equal("NETSCAPE", appExt.ApplicationIdentifier);
		Assert.True(appExt.IsNetscapeExtension);
		Assert.Equal(5, appExt.LoopCount);
	}
}
