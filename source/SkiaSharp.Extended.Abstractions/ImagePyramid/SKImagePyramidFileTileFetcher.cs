#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles from the local file system and decodes them using a provided
/// <see cref="ISKImagePyramidTileDecoder"/>.
/// Accepts both plain file paths and <c>file://</c> URIs.
/// </summary>
public class SKImagePyramidFileTileFetcher(ISKImagePyramidTileDecoder decoder) : ISKImagePyramidTileFetcher
{
    private readonly ISKImagePyramidTileDecoder _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));

    /// <inheritdoc />
    public Task<ISKImagePyramidTile?> FetchTileAsync(string url, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            string path = url;
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                path = new Uri(url).LocalPath;

            if (!File.Exists(path))
                return Task.FromResult<ISKImagePyramidTile?>(null);

            cancellationToken.ThrowIfCancellationRequested();

            using var fs = File.OpenRead(path);
            return Task.FromResult(_decoder.Decode(fs));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Task.FromResult<ISKImagePyramidTile?>(null);
        }
    }

    /// <inheritdoc />
    public void Dispose() { }
}
