#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Reads tiles from the local filesystem. No caching — the file IS the source.
/// </summary>
/// <remarks>
/// Accepts plain file paths or <c>file://</c> URIs. Use when tiles are
/// stored locally (e.g. DZI files packaged with the app or extracted to a temp directory).
/// </remarks>
public sealed class SKFileTileFetcher : ISKTileFetcher
{
    /// <inheritdoc />
    public Task<SKImagePyramidTileData?> FetchAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        string path;
        if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            try { path = new Uri(url).LocalPath; }
            catch { return Task.FromResult<SKImagePyramidTileData?>(null); }
        }
        else
        {
            path = url;
        }

        if (!File.Exists(path))
            return Task.FromResult<SKImagePyramidTileData?>(null);

        try
        {
            var bytes = File.ReadAllBytes(path);
            return Task.FromResult<SKImagePyramidTileData?>(new SKImagePyramidTileData(bytes));
        }
        catch (OperationCanceledException) { throw; }
        catch { return Task.FromResult<SKImagePyramidTileData?>(null); }
    }

    /// <inheritdoc />
    public void Dispose() { }
}
