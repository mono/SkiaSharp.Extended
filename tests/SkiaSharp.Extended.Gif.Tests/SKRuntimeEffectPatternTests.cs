using System.IO;

namespace SkiaSharp.Extended.Gif.Tests;

/// <summary>
/// Tests for SKRuntimeEffect-style Create*/Build* pattern on SKGifDecoder.
/// </summary>
public class SKGifDecoderPatternTests
{
	[Fact]
	public void CreateDecoder_NullStream_ReturnsNullWithErrors()
	{
		// Act
		var decoder = SKGifDecoder.CreateDecoder(null!, out var errors);

		// Assert
		Assert.Null(decoder);
		Assert.NotNull(errors);
		Assert.Contains("null", errors, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void BuildDecoder_NullStream_ThrowsException()
	{
		// Act & Assert
		var exception = Assert.Throws<InvalidDataException>(() => SKGifDecoder.BuildDecoder(null!));
		Assert.NotNull(exception.Message);
		Assert.Contains("null", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void CreateDecoder_InvalidGifData_ReturnsNullWithErrors()
	{
		// Arrange: Not a GIF file
		var bytes = new byte[] { 0x50, 0x4E, 0x47 }; // "PNG" not "GIF"
		using var stream = new MemoryStream(bytes);

		// Act
		var decoder = SKGifDecoder.CreateDecoder(stream, out var errors);

		// Assert
		Assert.Null(decoder);
		Assert.NotNull(errors);
		// Error message should be informative
		Assert.False(string.IsNullOrWhiteSpace(errors));
	}

	[Fact]
	public void BuildDecoder_InvalidGifData_ThrowsExceptionWithDetails()
	{
		// Arrange: Not a GIF file
		var bytes = new byte[] { 0x50, 0x4E, 0x47 }; // "PNG" not "GIF"
		using var stream = new MemoryStream(bytes);

		// Act & Assert
		var exception = Assert.Throws<InvalidDataException>(() => SKGifDecoder.BuildDecoder(stream));
		Assert.NotNull(exception.Message);
		Assert.NotEqual("Failed to decode GIF file. There was an unknown error.", exception.Message);
	}

	[Fact]
	public void Create_CallsBuildDecoder()
	{
		// The Create method should delegate to BuildDecoder for backwards compatibility
		// Arrange: Invalid data to test error handling
		var bytes = new byte[] { 0x00 };
		using var stream = new MemoryStream(bytes);

		// Act & Assert - both should throw the same exception type
		Assert.Throws<InvalidDataException>(() => SKGifDecoder.Create(stream));
	}

	[Fact]
	public void CreateDecoder_EmptyStream_ReturnsNullWithErrors()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act
		var decoder = SKGifDecoder.CreateDecoder(stream, out var errors);

		// Assert
		Assert.Null(decoder);
		Assert.NotNull(errors);
	}

	[Fact]
	public void BuildDecoder_EmptyStream_ThrowsException()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act & Assert
		Assert.Throws<InvalidDataException>(() => SKGifDecoder.BuildDecoder(stream));
	}
}

/// <summary>
/// Tests for SKRuntimeEffect-style Create*/Build* pattern on SKGifEncoder.
/// </summary>
public class SKGifEncoderPatternTests
{
	[Fact]
	public void CreateEncoder_NullStream_ReturnsNullWithErrors()
	{
		// Act
		var encoder = SKGifEncoder.CreateEncoder(null!, out var errors);

		// Assert
		Assert.Null(encoder);
		Assert.NotNull(errors);
		Assert.Contains("null", errors, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void BuildEncoder_NullStream_ThrowsException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => SKGifEncoder.BuildEncoder(null!));
		Assert.NotNull(exception.Message);
		Assert.Contains("null", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void CreateEncoder_NonWritableStream_ReturnsNullWithErrors()
	{
		// Arrange: Read-only stream
		var bytes = new byte[100];
		using var stream = new MemoryStream(bytes, writable: false);

		// Act
		var encoder = SKGifEncoder.CreateEncoder(stream, out var errors);

		// Assert
		Assert.Null(encoder);
		Assert.NotNull(errors);
		Assert.Contains("writable", errors, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void BuildEncoder_NonWritableStream_ThrowsException()
	{
		// Arrange: Read-only stream
		var bytes = new byte[100];
		using var stream = new MemoryStream(bytes, writable: false);

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => SKGifEncoder.BuildEncoder(stream));
		Assert.NotNull(exception.Message);
		Assert.Contains("writable", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void CreateEncoder_ValidStream_ReturnsEncoderWithoutErrors()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act
		var encoder = SKGifEncoder.CreateEncoder(stream, out var errors);

		// Assert
		Assert.NotNull(encoder);
		Assert.Null(errors);

		// Cleanup
		encoder?.Dispose();
	}

	[Fact]
	public void BuildEncoder_ValidStream_ReturnsEncoder()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act
		using var encoder = SKGifEncoder.BuildEncoder(stream);

		// Assert
		Assert.NotNull(encoder);
	}

	[Fact]
	public void Constructor_ValidStream_Works()
	{
		// Arrange
		using var stream = new MemoryStream();

		// Act
		using var encoder = new SKGifEncoder(stream);

		// Assert
		Assert.NotNull(encoder);
	}

	[Fact]
	public void Constructor_NullStream_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new SKGifEncoder(null!));
	}

	[Fact]
	public void Constructor_NonWritableStream_ThrowsArgumentException()
	{
		// Arrange
		var bytes = new byte[100];
		using var stream = new MemoryStream(bytes, writable: false);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => new SKGifEncoder(stream));
	}
}
