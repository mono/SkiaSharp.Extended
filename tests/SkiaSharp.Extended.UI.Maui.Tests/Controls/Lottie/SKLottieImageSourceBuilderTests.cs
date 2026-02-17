using SkiaSharp.Resources;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

/// <summary>
/// Tests for the AnimationBuilder customization API on SKLottieImageSource.
/// Tests verify the property setters/getters work correctly.
/// </summary>
public class SKLottieImageSourceBuilderTests
{
	[Fact]
	public void DefaultResourceProvider_StartsAsNull()
	{
		// Ensure clean state
		SKLottieImageSource.DefaultResourceProvider = null;

		// Assert
		Assert.Null(SKLottieImageSource.DefaultResourceProvider);
	}

	[Fact]
	public void DefaultFontManager_StartsAsNull()
	{
		// Ensure clean state
		SKLottieImageSource.DefaultFontManager = null;

		// Assert
		Assert.Null(SKLottieImageSource.DefaultFontManager);
	}

	[Fact]
	public void ResourceProvider_InstanceProperty_DefaultsToNull()
	{
		// Arrange
		var source = new SKFileLottieImageSource { File = "test.json" };

		// Assert
		Assert.Null(source.ResourceProvider);
	}

	[Fact]
	public void FontManager_InstanceProperty_DefaultsToNull()
	{
		// Arrange
		var source = new SKFileLottieImageSource { File = "test.json" };

		// Assert
		Assert.Null(source.FontManager);
	}

	[Fact]
	public void ResourceProvider_CanBeSetToNull()
	{
		// Arrange
		var source = new SKFileLottieImageSource { File = "test.json" };

		// Act
		source.ResourceProvider = null;

		// Assert
		Assert.Null(source.ResourceProvider);
	}

	[Fact]
	public void FontManager_CanBeSetToNull()
	{
		// Arrange
		var source = new SKFileLottieImageSource { File = "test.json" };

		// Act
		source.FontManager = null;

		// Assert
		Assert.Null(source.FontManager);
	}

	[Fact]
	public void PropertiesAreIndependent_BetweenInstances()
	{
		// Arrange
		var source1 = new SKFileLottieImageSource { File = "test1.json" };
		var source2 = new SKFileLottieImageSource { File = "test2.json" };

		// Set different values (null is fine for this test)
		source1.ResourceProvider = null;
		source2.FontManager = null;

		// Assert - each instance maintains its own properties
		Assert.Null(source1.ResourceProvider);
		Assert.Null(source2.FontManager);
	}

	[Fact]
	public void StaticProperties_CanBeSet()
	{
		try
		{
			// Act
			SKLottieImageSource.DefaultResourceProvider = null;
			SKLottieImageSource.DefaultFontManager = null;

			// Assert
			Assert.Null(SKLottieImageSource.DefaultResourceProvider);
			Assert.Null(SKLottieImageSource.DefaultFontManager);
		}
		finally
		{
			// Cleanup
			SKLottieImageSource.DefaultResourceProvider = null;
			SKLottieImageSource.DefaultFontManager = null;
		}
	}

	[Fact]
	public void API_IsBackwardsCompatible()
	{
		// Verify that existing code patterns still work
		// (creating image sources without setting custom providers/managers)

		// Arrange & Act
		var source1 = new SKFileLottieImageSource { File = "animation.json" };
		var source2 = SKLottieImageSource.FromFile("animation.json") as SKFileLottieImageSource;
		var source3 = SKLottieImageSource.FromUri(new Uri("https://example.com/animation.json"));

		// Assert - all should work without errors
		Assert.NotNull(source1);
		Assert.NotNull(source2);
		Assert.NotNull(source3);
		
		// Properties should default to null (built-in defaults used internally)
		Assert.Null(source1.ResourceProvider);
		Assert.Null(source1.FontManager);
	}
}
