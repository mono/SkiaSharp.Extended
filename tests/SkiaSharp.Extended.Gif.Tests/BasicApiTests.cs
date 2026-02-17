namespace SkiaSharp.Extended.Gif.Tests;

/// <summary>
/// Basic tests to verify the GIF project structure and API surface.
/// API aligned with SkiaSharp patterns.
/// </summary>
public class BasicApiTests
{
	[Fact]
	public void SKGifDecoder_Create_RequiresNonNullStream()
	{
		// Arrange & Act & Assert
		// With SKRuntimeEffect pattern, Create/Build throw InvalidDataException with details
		Assert.Throws<InvalidDataException>(() => SKGifDecoder.Create(null!));
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

		// Act & Assert - using explicit duration parameter
		Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!, 100));
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
	public void SKGifInfo_CanBeCreated()
	{
		// Arrange & Act
		var info = new SKGifInfo();

		// Assert
		Assert.NotNull(info);
	}

	[Fact]
	public void SKGifDisposalMethod_HasCorrectValues()
	{
		// Verify enum values align with SKCodecAnimationDisposalMethod
		Assert.Equal(0, (int)SKGifDisposalMethod.None);
		Assert.Equal(1, (int)SKGifDisposalMethod.DoNotDispose);
		Assert.Equal(2, (int)SKGifDisposalMethod.RestoreToBackground);
		Assert.Equal(3, (int)SKGifDisposalMethod.RestoreToPrevious);
	}

	[Fact]
	public void SKGifFrameInfo_DurationProperty_Exists()
	{
		// Arrange
		var frameInfo = new SKGifFrameInfo
		{
			Duration = 100
		};

		// Assert - verify Duration property exists (aligned with SKCodecFrameInfo)
		Assert.Equal(100, frameInfo.Duration);
	}
}
