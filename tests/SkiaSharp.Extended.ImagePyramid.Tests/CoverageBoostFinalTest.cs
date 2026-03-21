using System.Threading;

using SkiaSharp;
using SkiaSharp.Extended;
using System.Net;
using System.Net.Http;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class CoverageBoostFinalTest
{
    // --- SKFileTileFetcher: OperationCanceledException rethrown ---

    [Fact]
    public async Task FileTileFetcher_CancelledTokenDuringFetch_ThrowsOperationCanceledException()
    {
        var provider = new SKTieredTileProvider(new SKFileTileFetcher());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Cancellation at the start should throw OperationCanceledException
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => provider.GetTileAsync("/some/path.png", cts.Token));
    }

    // --- SKFileTileFetcher: Generic exception returns null ---

    [Fact]
    public async Task FileTileFetcher_CorruptedFilePath_ReturnsNull()
    {
        var provider = new SKTieredTileProvider(new SKFileTileFetcher());

        // A file URI with invalid characters causes an exception in Uri parsing
        // that should be caught by the generic catch and return null.
        // Use a path with embedded null bytes or illegal URI chars.
        var result = await provider.GetTileAsync("file://\0invalid\0path", CancellationToken.None);
        Assert.Null(result);
    }

    // --- SKHttpTileFetcher via SKTieredTileProvider: non-success status code ---

    [Fact]
    public async Task HttpTileFetcher_NonSuccessStatusCode_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var client = new HttpClient(handler);
        var provider = new SKTieredTileProvider(new SKHttpTileFetcher(httpClient: client));

        var result = await provider.GetTileAsync("http://example.com/tile.png", CancellationToken.None);
        Assert.Null(result);

        provider.Dispose();
        client.Dispose();
    }

    [Fact]
    public async Task HttpTileFetcher_InternalServerError_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "error");
        var client = new HttpClient(handler);
        var provider = new SKTieredTileProvider(new SKHttpTileFetcher(httpClient: client));

        var result = await provider.GetTileAsync("http://example.com/tile.png", CancellationToken.None);
        Assert.Null(result);

        provider.Dispose();
        client.Dispose();
    }

    // --- SKHttpTileFetcher: exception path (HttpRequestException) ---

    [Fact]
    public async Task HttpTileFetcher_HttpRequestException_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Network error"));
        var client = new HttpClient(handler);
        var provider = new SKTieredTileProvider(new SKHttpTileFetcher(httpClient: client));

        var result = await provider.GetTileAsync("http://example.com/tile.png", CancellationToken.None);
        Assert.Null(result);

        provider.Dispose();
        client.Dispose();
    }

    // --- SKHttpTileFetcher: TaskCanceledException path ---

    [Fact]
    public async Task HttpTileFetcher_TaskCanceledException_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("Timeout"));
        var client = new HttpClient(handler);
        var provider = new SKTieredTileProvider(new SKHttpTileFetcher(httpClient: client));

        var result = await provider.GetTileAsync("http://example.com/tile.png", CancellationToken.None);
        Assert.Null(result);

        provider.Dispose();
        client.Dispose();
    }

    // --- Mock handlers for SKHttpTileFetcher testing ---

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }

    private class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}
