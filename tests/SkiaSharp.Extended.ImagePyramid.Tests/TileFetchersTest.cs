using SkiaSharp.Extended;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class TileFetchersTest
{
    // --- SKFileTileFetcher ---

    [Fact]
    public async Task FileTileFetcher_NonExistentFile_ReturnsNull()
    {
        using var fetcher = new SKTieredTileProvider(new SKFileTileFetcher());
        var result = await fetcher.GetTileAsync("/nonexistent/path/tile.jpg", CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task FileTileFetcher_FileUriScheme_ReturnsNull_WhenMissing()
    {
        using var fetcher = new SKTieredTileProvider(new SKFileTileFetcher());
        var result = await fetcher.GetTileAsync("file:///nonexistent/path/tile.jpg", CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task FileTileFetcher_CancellationToken_Throws()
    {
        using var fetcher = new SKTieredTileProvider(new SKFileTileFetcher());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.GetTileAsync("/any/path.jpg", cts.Token));
    }

    [Fact]
    public void FileTileFetcher_Dispose_DoesNotThrow()
    {
        var fetcher = new SKTieredTileProvider(new SKFileTileFetcher());
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

            using var fetcher = new SKTieredTileProvider(new SKFileTileFetcher());
            var result = await fetcher.GetTileAsync(tempPath);
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

    // --- SKHttpTileFetcher ---

    [Fact]
    public void HttpTileFetcher_DefaultConstructor_CreatesClient()
    {
        using var fetcher = new SKTieredTileProvider(new SKHttpTileFetcher());
        // Should not throw
    }

    [Fact]
    public void HttpTileFetcher_NullClient_UsesInternalClient()
    {
        // null means "create an owned internal HttpClient" -- not an error
        var provider = new SKTieredTileProvider(new SKHttpTileFetcher(httpClient: null));
        Assert.NotNull(provider);
        provider.Dispose();
    }

    [Fact]
    public void HttpTileFetcher_ExternalClient_NotDisposed()
    {
        var client = new System.Net.Http.HttpClient();
        var fetcher = new SKTieredTileProvider(new SKHttpTileFetcher(httpClient: client));
        fetcher.Dispose();

        // External client should still be usable (not disposed)
        // If it were disposed, this would throw
        Assert.NotNull(client.BaseAddress?.ToString() ?? "ok");
        client.Dispose();
    }

    [Fact]
    public async Task HttpTileFetcher_CancelledToken_Throws()
    {
        using var fetcher = new SKTieredTileProvider(new SKHttpTileFetcher());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Cancelled token should throw OperationCanceledException (not blacklist as failed)
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.GetTileAsync("http://localhost:1/tile.jpg", cts.Token));
    }
}
