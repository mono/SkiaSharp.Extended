namespace SkiaSharp.Drawing.Tests.Infrastructure;

/// <summary>
/// Validates that the SkiaSharp.Drawing assembly exposes the expected System.Drawing API surface.
/// These tests ensure the stub generation and build process haven't accidentally removed types.
/// </summary>
public class ApiSurfaceTests
{
    /// <summary>
    /// Verify core System.Drawing types exist in our assembly.
    /// </summary>
    [Theory]
    [InlineData(typeof(System.Drawing.Bitmap))]
    [InlineData(typeof(System.Drawing.Brush))]
    [InlineData(typeof(System.Drawing.Font))]
    [InlineData(typeof(System.Drawing.Graphics))]
    [InlineData(typeof(System.Drawing.Icon))]
    [InlineData(typeof(System.Drawing.Image))]
    [InlineData(typeof(System.Drawing.Pen))]
    [InlineData(typeof(System.Drawing.Region))]
    [InlineData(typeof(System.Drawing.SolidBrush))]
    [InlineData(typeof(System.Drawing.StringFormat))]
    [InlineData(typeof(System.Drawing.Drawing2D.GraphicsPath))]
    [InlineData(typeof(System.Drawing.Drawing2D.LinearGradientBrush))]
    [InlineData(typeof(System.Drawing.Drawing2D.Matrix))]
    [InlineData(typeof(System.Drawing.Imaging.BitmapData))]
    [InlineData(typeof(System.Drawing.Imaging.ImageFormat))]
    [InlineData(typeof(System.Drawing.Text.PrivateFontCollection))]
    public void CoreType_Exists(Type type)
    {
        Assert.NotNull(type);
        Assert.Equal("System.Drawing.Common", type.Assembly.GetName().Name);
    }

    /// <summary>
    /// Verify the assembly name matches the official System.Drawing.Common.
    /// </summary>
    [Fact]
    public void AssemblyName_MatchesBaseline()
    {
        var assembly = typeof(System.Drawing.Graphics).Assembly;
        Assert.Equal("System.Drawing.Common", assembly.GetName().Name);
    }

    /// <summary>
    /// Verify that implemented types work and unimplemented platform-specific methods throw.
    /// </summary>
    [Fact]
    public void ImplementedType_CanBeConstructed()
    {
        // SolidBrush is now implemented — verify it works
        using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
        Assert.Equal(System.Drawing.Color.Red.ToArgb(), brush.Color.ToArgb());
    }

    /// <summary>
    /// Verify platform-specific methods throw PlatformNotSupportedException.
    /// </summary>
    [Fact]
    public void PlatformSpecificMethod_ThrowsPlatformNotSupportedException()
    {
        Assert.Throws<PlatformNotSupportedException>(() => System.Drawing.Bitmap.FromHicon(IntPtr.Zero));
    }
}
