#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles from the local file system and decodes them using SkiaSharp.
/// Accepts both plain file paths and <c>file://</c> URIs.
/// </summary>
public class SKImagePyramidFileTileFetcher : ISKImagePyramidTileFetcher
{
    /// <inheritdoc />
    public Task<SKImage?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            string path = url;
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                path = new Uri(url).LocalPath;

            if (!File.Exists(path))
                return Task.FromResult<SKImage?>(null);

            cancellationToken.ThrowIfCancellationRequested();

            using var fs = File.OpenRead(path);
            return Task.FromResult(SKImage.FromEncodedData(fs));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Task.FromResult<SKImage?>(null);
        }
    }

    /// <inheritdoc />
    public void Dispose() { }
}
