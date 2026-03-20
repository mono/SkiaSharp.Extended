#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles from the local file system and decodes them using SkiaSharp.
/// Accepts both plain file paths and <c>file://</c> URIs.
/// Raw bytes are buffered before decoding, which allows the bytes to be stored
/// in L2 caches without re-encoding.
/// </summary>
public class SKImagePyramidFileTileFetcher : ISKImagePyramidTileFetcher
{
    /// <inheritdoc />
    public Task<SKImagePyramidTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            string path = url;
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                path = new Uri(url).LocalPath;

            if (!File.Exists(path))
                return Task.FromResult<SKImagePyramidTile?>(null);

            cancellationToken.ThrowIfCancellationRequested();

            var bytes = File.ReadAllBytes(path);
            var image = SKImage.FromEncodedData(bytes);
            return Task.FromResult(image != null ? new SKImagePyramidTile(image, bytes) : null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Task.FromResult<SKImagePyramidTile?>(null);
        }
    }

    /// <inheritdoc />
    public void Dispose() { }
}
