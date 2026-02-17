namespace SkiaSharp.Extended.Gif.Tests;

/// <summary>
/// Basic tests to verify the GIF project structure and API surface.
/// </summary>
public class BasicApiTests
{
	[Fact]
	public void SKGifDecoder_Create_RequiresNonNullStream()
	{
		// Arrange & Act & Assert
		Assert.Throws<ArgumentNullException>(() => SKGifDecoder.Create(null!));
	}

	[Fact]
	public void SKGifEncoder_Constructor_RequiresNonNullStream()
	{
		// Arrange & Act & Assert
		Assert.Throws<ArgumentNullException>(() => new SKGifEncoder(null!));
	}

	[Fact]
	public void SKGifEncoder_AddFrame_RequiresNonNullBitmap()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var encoder = new SKGifEncoder(stream);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!));
	}

	[Fact]
	public void SKGifFrame_CanBeCreated()
	{
		// Arrange & Act
		var frame = new SKGifFrame();

		// Assert
		Assert.NotNull(frame);
	}

	[Fact]
	public void SKGifMetadata_CanBeCreated()
	{
		// Arrange & Act
		var metadata = new SKGifMetadata();

		// Assert
		Assert.NotNull(metadata);
	}
}
