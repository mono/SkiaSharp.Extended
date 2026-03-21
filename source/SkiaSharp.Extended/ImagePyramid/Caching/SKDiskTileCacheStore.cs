#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Persistent tile cache backed by the local filesystem.
/// Uses URL-derived hash keys, bucketed directories, and configurable expiry.
/// </summary>
public sealed class SKDiskTileCacheStore : ISKTileCacheStore
{
    private readonly string _basePath;
    private readonly TimeSpan _expiry;

    /// <summary>Default cache expiry (30 days).</summary>
    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(30);

    /// <summary>
    /// Creates a disk-backed cache store.
    /// </summary>
    /// <param name="basePath">Root directory for cached tiles.</param>
    /// <param name="expiry">Maximum age before a cached entry is treated as a miss. Default: 30 days.</param>
    public SKDiskTileCacheStore(string basePath, TimeSpan? expiry = null)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _expiry = expiry ?? DefaultExpiry;
    }

    /// <inheritdoc />
    public Task<SKImagePyramidTileData?> TryGetAsync(string key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        string path = GetPath(key);
        if (!File.Exists(path))
            return Task.FromResult<SKImagePyramidTileData?>(null);

        if (DateTime.UtcNow - File.GetLastWriteTimeUtc(path) > _expiry)
        {
            try { File.Delete(path); } catch { }
            return Task.FromResult<SKImagePyramidTileData?>(null);
        }

        try
        {
            var bytes = File.ReadAllBytes(path);
            return Task.FromResult<SKImagePyramidTileData?>(new SKImagePyramidTileData(bytes));
        }
        catch { return Task.FromResult<SKImagePyramidTileData?>(null); }
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, SKImagePyramidTileData data, CancellationToken ct = default)
    {
        try
        {
            string path = GetPath(key);
            string dir = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dir);

            // Atomic write: write to tmp then rename
            string tmp = path + ".tmp";
#if NETSTANDARD2_0
            File.WriteAllBytes(tmp, data.Data);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
#else
            await File.WriteAllBytesAsync(tmp, data.Data, ct).ConfigureAwait(false);
            File.Move(tmp, path, overwrite: true);
#endif
        }
        catch { }
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            string path = GetPath(key);
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken ct = default)
    {
        try
        {
            string cacheDir = Path.Combine(_basePath, "skimgpyramid");
            if (Directory.Exists(cacheDir))
                Directory.Delete(cacheDir, recursive: true);
        }
        catch { }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose() { }

    // ---- Private ----

    private string GetPath(string key)
    {
        string hash = FnvHash64(key).ToString("x16");
        return Path.Combine(_basePath, "skimgpyramid", hash.Substring(0, 2), hash + ".tile");
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
