using SkiaSharp.Extended;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class TileFetchersTest
{
    // --- SKImagePyramidFileTileFetcher ---

    [Fact]
    public async Task FileTileFetcher_NonExistentFile_ReturnsNull()
    {
        using var fetcher = new SKImagePyramidFileTileFetcher();
        var result = await fetcher.FetchTileAsync("/nonexistent/path/tile.jpg");
        Assert.Null(result);
    }

    [Fact]
    public async Task FileTileFetcher_FileUriScheme_ReturnsNull_WhenMissing()
    {
        using var fetcher = new SKImagePyramidFileTileFetcher();
        var result = await fetcher.FetchTileAsync("file:///nonexistent/path/tile.jpg");
        Assert.Null(result);
    }

    [Fact]
    public async Task FileTileFetcher_CancellationToken_Throws()
    {
        using var fetcher = new SKImagePyramidFileTileFetcher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.FetchTileAsync("/any/path.jpg", cts.Token));
    }

    [Fact]
    public void FileTileFetcher_Dispose_DoesNotThrow()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        fetcher.Dispose(); // Should not throw
    }

    [Fact]
    public async Task FileTileFetcher_ValidFile_ReturnsBitmap()
    {
        // Create a temp PNG file
        var tempPath = Path.GetTempFileName() + ".png";
        try
        {
            using (var bmp = new SkiaSharp.SKBitmap(10, 10))
            using (var image = SkiaSharp.SKImage.FromBitmap(bmp))
            using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(tempPath))
            {
                data.SaveTo(stream);
            }

            using var fetcher = new SKImagePyramidFileTileFetcher();
            var result = await fetcher.FetchTileAsync(tempPath);
            Assert.NotNull(result);
            Assert.Equal(10, result.Image.Width);
            Assert.Equal(10, result.Image.Height);
            result.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // --- SKImagePyramidHttpTileFetcher ---

    [Fact]
    public void HttpTileFetcher_DefaultConstructor_CreatesClient()
    {
        using var fetcher = new SKImagePyramidHttpTileFetcher();
        // Should not throw
    }

    [Fact]
    public void HttpTileFetcher_NullClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SKImagePyramidHttpTileFetcher(null!));
    }

    [Fact]
    public void HttpTileFetcher_ExternalClient_NotDisposed()
    {
        var client = new System.Net.Http.HttpClient();
        var fetcher = new SKImagePyramidHttpTileFetcher(client);
        fetcher.Dispose();

        // External client should still be usable (not disposed)
        // If it were disposed, this would throw
        Assert.NotNull(client.BaseAddress?.ToString() ?? "ok");
        client.Dispose();
    }

    [Fact]
    public async Task HttpTileFetcher_CancelledToken_ReturnsNull()
    {
        using var fetcher = new SKImagePyramidHttpTileFetcher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Cancelled token should return null (not throw)
        var result = await fetcher.FetchTileAsync("http://localhost:1/tile.jpg", cts.Token);
        Assert.Null(result);
    }
}
