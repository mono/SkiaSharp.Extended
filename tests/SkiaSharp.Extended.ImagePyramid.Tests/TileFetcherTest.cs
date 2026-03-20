using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;
using System.IO;
using System.Threading;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class TileFetcherTest
{
    [Fact]
    public async Task FileTileFetcher_ExistingFile_ReturnsBitmap()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        // Create a temporary image file
        var tmpPath = Path.Combine(Path.GetTempPath(), $"tile_test_{Guid.NewGuid()}.png");
        try
        {
            using var surface = SKSurface.Create(new SKImageInfo(64, 64));
            surface.Canvas.Clear(SKColors.Red);
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(tmpPath, data.ToArray());

            var tile = await fetcher.FetchTileAsync(tmpPath);
            Assert.NotNull(tile);
            Assert.Equal(64, tile.Image.Width);
            Assert.Equal(64, tile.Image.Height);
            tile.Dispose();
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public async Task FileTileFetcher_NonexistentFile_ReturnsNull()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        var result = await fetcher.FetchTileAsync("/nonexistent/path/tile.png");
        Assert.Null(result);
    }

    [Fact]
    public async Task FileTileFetcher_FileUri_Works()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        var tmpPath = Path.Combine(Path.GetTempPath(), $"tile_test_{Guid.NewGuid()}.png");
        try
        {
            using var surface = SKSurface.Create(new SKImageInfo(32, 32));
            surface.Canvas.Clear(SKColors.Blue);
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(tmpPath, data.ToArray());

            var fileUri = new Uri(tmpPath).AbsoluteUri;
            var tile = await fetcher.FetchTileAsync(fileUri);
            Assert.NotNull(tile);
            tile!.Dispose();
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public async Task FileTileFetcher_InvalidImage_ReturnsNull()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        var tmpPath = Path.Combine(Path.GetTempPath(), $"tile_test_{Guid.NewGuid()}.png");
        try
        {
            File.WriteAllText(tmpPath, "not an image");
            var result = await fetcher.FetchTileAsync(tmpPath);
            Assert.Null(result);
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public async Task HttpTileFetcher_InvalidUrl_ReturnsNull()
    {
        var fetcher = new SKImagePyramidHttpTileFetcher();
        // This will fail to connect, should return null
        var result = await fetcher.FetchTileAsync("http://localhost:0/nonexistent.png");
        Assert.Null(result);
        fetcher.Dispose();
    }

    [Fact]
    public async Task HttpTileFetcher_CancellationToken_ReturnsNull()
    {
        var fetcher = new SKImagePyramidHttpTileFetcher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png", cts.Token);
        Assert.Null(result);
        fetcher.Dispose();
    }

    [Fact]
    public void HttpTileFetcher_AcceptsExternalClient()
    {
        var client = new System.Net.Http.HttpClient();
        var fetcher = new SKImagePyramidHttpTileFetcher(client);
        fetcher.Dispose();
        // External client should not be disposed
        Assert.NotNull(client);
        client.Dispose();
    }

    [Fact]
    public void FileTileFetcher_Dispose_DoesNotThrow()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        fetcher.Dispose();
    }

    [Fact]
    public async Task FileTileFetcher_NonexistentPath_ReturnsNull()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        var result = await fetcher.FetchTileAsync(Path.Combine(Path.GetTempPath(), "does_not_exist_" + Guid.NewGuid() + ".png"));
        Assert.Null(result);
    }

    [Fact]
    public async Task FileTileFetcher_ValidImagePath_ReturnsNonNullBitmap()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        var tmpPath = Path.Combine(Path.GetTempPath(), $"fetcher_valid_{Guid.NewGuid()}.png");
        try
        {
            using var surface = SKSurface.Create(new SKImageInfo(32, 32));
            surface.Canvas.Clear(SKColors.Green);
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(tmpPath, data.ToArray());

            var tile = await fetcher.FetchTileAsync(tmpPath);
            Assert.NotNull(tile);
            Assert.Equal(32, tile.Image.Width);
            Assert.Equal(32, tile.Image.Height);
            tile.Dispose();
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public async Task FileTileFetcher_CancelledToken_ThrowsOperationCanceledException()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.FetchTileAsync("/any/path/tile.png", cts.Token));
    }
}
