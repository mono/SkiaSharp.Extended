using SkiaSharp;
using SkiaSharp.Extended;
using System.Net;
using System.Net.Http;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

public class CoverageBoostFinalTest
{
    // --- SKImagePyramidFileTileFetcher: OperationCanceledException rethrown (lines 96-98) ---

    [Fact]
    public async Task FileTileFetcher_CancelledTokenDuringFetch_ThrowsOperationCanceledException()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Cancellation at the start should throw OperationCanceledException
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.FetchTileAsync("/some/path.png", cts.Token));
    }

    // --- SKImagePyramidFileTileFetcher: Generic exception returns null (lines 100-102) ---

    [Fact]
    public async Task FileTileFetcher_CorruptedFilePath_ReturnsNull()
    {
        var fetcher = new SKImagePyramidFileTileFetcher();

        // A file URI with invalid characters causes an exception in Uri parsing
        // that should be caught by the generic catch and return null.
        // Use a path with embedded null bytes or illegal URI chars.
        var result = await fetcher.FetchTileAsync("file://\0invalid\0path");
        Assert.Null(result);
    }

    // --- SKImagePyramidHttpTileFetcher: non-success status code (lines 43-44) ---

    [Fact]
    public async Task HttpTileFetcher_NonSuccessStatusCode_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var client = new HttpClient(handler);
        var fetcher = new SKImagePyramidHttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    [Fact]
    public async Task HttpTileFetcher_InternalServerError_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "error");
        var client = new HttpClient(handler);
        var fetcher = new SKImagePyramidHttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    // --- SKImagePyramidHttpTileFetcher: exception path (HttpRequestException) ---

    [Fact]
    public async Task HttpTileFetcher_HttpRequestException_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Network error"));
        var client = new HttpClient(handler);
        var fetcher = new SKImagePyramidHttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    // --- SKImagePyramidHttpTileFetcher: TaskCanceledException path ---

    [Fact]
    public async Task HttpTileFetcher_TaskCanceledException_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("Timeout"));
        var client = new HttpClient(handler);
        var fetcher = new SKImagePyramidHttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    // --- Mock handlers for SKImagePyramidHttpTileFetcher testing ---

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
