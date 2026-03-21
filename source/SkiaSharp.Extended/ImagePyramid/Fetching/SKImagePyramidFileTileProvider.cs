#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Reads tiles from the local filesystem. No disk caching is performed — the file IS the source.
/// </summary>
/// <remarks>
/// Use this provider when the image pyramid is stored locally (e.g. DZI files packaged with the app
/// or extracted to a temp directory). Because reading a local file is already as cheap as a cache
/// hit, adding a disk cache would be redundant. An in-memory render buffer in the controller still
/// applies for render-loop performance.
/// </remarks>
public sealed class SKImagePyramidFileTileProvider : ISKImagePyramidTileProvider
{
    /// <inheritdoc />
    public Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Accept plain file paths or file:// URIs
        string path;
        if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            try { path = new Uri(url).LocalPath; }
            catch { return Task.FromResult<SKImagePyramidTile?>(null); }
        }
        else
        {
            path = url;
        }

        if (!File.Exists(path)) return Task.FromResult<SKImagePyramidTile?>(null);

        try
        {
            var bytes = File.ReadAllBytes(path);
            var image = SKImage.FromEncodedData(bytes);
            return Task.FromResult<SKImagePyramidTile?>(
                image == null ? null : new SKImagePyramidTile(image, bytes));
        }
        catch (OperationCanceledException) { throw; }
        catch { return Task.FromResult<SKImagePyramidTile?>(null); }
    }

    /// <inheritdoc />
    public void Dispose() { }
}
