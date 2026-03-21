#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Fetches tiles over HTTP with an optional URL-keyed disk cache.
/// </summary>
/// <remarks>
/// <para>
/// Remote tile sources (DZI, IIIF) use the network by default. Pass a
/// <paramref name="cachePath"/> to persist encoded tile bytes to disk between
/// app runs, reducing network usage. Each tile is stored as its original encoded
/// bytes (JPEG, PNG, etc.) — no re-encoding on disk.
/// </para>
/// <para>
/// The disk cache uses a hash of the tile URL as the file name, so tiles from
/// different sources are naturally namespaced without any extra configuration.
/// </para>
/// <para>
/// Recommended cache path values by platform:
/// <list type="bullet">
///   <item>MAUI: <c>FileSystem.CacheDirectory</c></item>
///   <item>Desktop .NET: <c>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkiaSharpCache")</c></item>
/// </list>
/// </para>
/// </remarks>
public sealed class SKImagePyramidHttpTileProvider : ISKImagePyramidTileProvider
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly string? _cachePath;

    /// <summary>Default expiry for disk-cached tiles (30 days).</summary>
    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(30);

    /// <summary>Maximum age of a disk-cached tile before it is treated as a miss and deleted.</summary>
    public TimeSpan Expiry { get; set; }

    /// <summary>
    /// Creates an HTTP tile provider with an optional disk cache.
    /// </summary>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> to use. If <see langword="null"/>, an internal client is created
    /// and will be disposed with this instance.
    /// </param>
    /// <param name="cachePath">
    /// Optional root directory for the disk tile cache (e.g. <c>FileSystem.CacheDirectory</c> on MAUI).
    /// Pass <see langword="null"/> to disable disk caching.
    /// </param>
    /// <param name="expiry">
    /// Maximum age for disk-cached tiles. Pass <see langword="null"/> to use <see cref="DefaultExpiry"/> (30 days).
    /// </param>
    public SKImagePyramidHttpTileProvider(
        HttpClient? httpClient = null,
        string? cachePath = null,
        TimeSpan? expiry = null)
    {
        _ownsHttp = httpClient == null;
        _http = httpClient ?? new HttpClient();
        _cachePath = cachePath;
        Expiry = expiry ?? DefaultExpiry;
    }

    /// <inheritdoc />
    public async Task<SKImagePyramidTile?> GetTileAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // 1. Disk cache hit
        if (_cachePath != null)
        {
            var cached = TryReadFromDisk(url);
            if (cached != null) return cached;
        }

        // 2. HTTP fetch
        try
        {
            using var response = await _http
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return null;

#if NETSTANDARD2_0
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#else
            var bytes = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
#endif

            var image = SKImage.FromEncodedData(bytes);
            if (image == null) return null;

            // 3. Persist to disk — fire-and-forget with CancellationToken.None.
            //    Decision to store was made above after checking ct; we don't want a
            //    cancellation between fetch and store to leak tile data without persisting.
            if (_cachePath != null)
                _ = WriteToDiskAsync(url, bytes);

            return new SKImagePyramidTile(image, bytes);
        }
        catch (HttpRequestException) { return null; }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (OperationCanceledException) { return null; } // HTTP timeout — not our cancellation
        catch { return null; }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }

    // ---- Private ----

    private SKImagePyramidTile? TryReadFromDisk(string url)
    {
        string path = GetDiskPath(url);
        if (!File.Exists(path)) return null;

        if (DateTime.UtcNow - File.GetLastWriteTimeUtc(path) > Expiry)
        {
            try { File.Delete(path); } catch { }
            return null;
        }

        try
        {
            var bytes = File.ReadAllBytes(path);
            var image = SKImage.FromEncodedData(bytes);
            return image == null ? null : new SKImagePyramidTile(image, bytes);
        }
        catch { return null; }
    }

    private async Task WriteToDiskAsync(string url, byte[] bytes)
    {
        try
        {
            string path = GetDiskPath(url);
            string dir = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dir);

            // Atomic write: write to tmp then rename to avoid partial reads
            string tmp = path + ".tmp";
#if NETSTANDARD2_0
            File.WriteAllBytes(tmp, bytes);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
#else
            await File.WriteAllBytesAsync(tmp, bytes).ConfigureAwait(false);
            File.Move(tmp, path, overwrite: true);
#endif
        }
        catch { }
    }

    private string GetDiskPath(string url)
    {
        // Hash the URL to produce a short, unique, filesystem-safe filename.
        // Using FNV-1a 64-bit: fast, no collisions in practice for tile URLs.
        string hash = FnvHash64(url).ToString("x16");
        // Use first 2 hex chars as a bucket to avoid too many files in one directory.
        return Path.Combine(_cachePath!, "skimgpyramid", hash.Substring(0, 2), hash + ".tile");
    }

    private static ulong FnvHash64(string s)
    {
        const ulong OffsetBasis = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        ulong hash = OffsetBasis;
        foreach (char c in s)
        {
            hash ^= (byte)(c & 0xFF);
            hash *= Prime;
            hash ^= (byte)(c >> 8);
            hash *= Prime;
        }
        return hash;
    }
}
