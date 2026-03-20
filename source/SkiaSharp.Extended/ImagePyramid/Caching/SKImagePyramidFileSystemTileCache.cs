#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// A two-tier tile cache: LRU memory (L1) + local filesystem (L2).
/// Tiles are stored as raw encoded bytes on disk — no re-encoding needed.
/// The filesystem tier survives app restarts and reduces network usage.
/// </summary>
/// <remarks>
/// <para>
/// Recommended base paths by platform:
/// <list type="bullet">
///   <item>MAUI: <c>FileSystem.CacheDirectory</c></item>
///   <item>Desktop .NET: <c>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkiaSharpCache")</c></item>
///   <item>Fallback: <c>Path.GetTempPath()</c></item>
/// </list>
/// Blazor WebAssembly has no real filesystem; use <c>SKImagePyramidBrowserStorageTileCache</c> from
/// <c>SkiaSharp.Extended.UI.Blazor</c> instead.
/// </para>
/// <para>
/// Set <see cref="ISKImagePyramidTileCache.ActiveSourceId"/> (the controller does this automatically on
/// <c>Load()</c>) so tiles from different image sources are stored in separate subdirectories.
/// </para>
/// </remarks>
public class SKImagePyramidFileSystemTileCache : ISKImagePyramidTileCache
{
    private readonly SKImagePyramidMemoryTileCache _l1;
    private readonly string _basePath;
    private readonly int _maxDiskTiles;
    private readonly TimeSpan _expiry;
    private volatile string? _activeSourceId;
    private int _diskTileCount;
    private int _cleanupRunning;

    /// <summary>Default tile expiry when none is specified.</summary>
    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(30);

    /// <summary>
    /// Creates a two-tier cache using the given base directory.
    /// </summary>
    /// <param name="basePath">Root directory for tile storage (e.g. <c>FileSystem.CacheDirectory</c> on MAUI).</param>
    /// <param name="memoryCapacity">Maximum tiles in the L1 memory cache.</param>
    /// <param name="maxDiskTiles">Soft maximum tiles on disk before old tiles are cleaned up.</param>
    /// <param name="expiry">
    /// Maximum age for cached tiles. Files older than this are treated as a cache miss and deleted.
    /// Pass <see langword="null"/> to use <see cref="DefaultExpiry"/> (30 days).
    /// The controller may override this with <see cref="ISKImagePyramidSource.CacheExpiry"/> when loading a source.
    /// </param>
    public SKImagePyramidFileSystemTileCache(
        string basePath,
        int memoryCapacity = 256,
        int maxDiskTiles = 8192,
        TimeSpan? expiry = null)
    {
        if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("basePath is required.", nameof(basePath));
        if (memoryCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(memoryCapacity));
        if (maxDiskTiles <= 0) throw new ArgumentOutOfRangeException(nameof(maxDiskTiles));

        _l1 = new SKImagePyramidMemoryTileCache(memoryCapacity);
        _basePath = basePath;
        _maxDiskTiles = maxDiskTiles;
        _expiry = expiry ?? DefaultExpiry;
    }

    /// <inheritdoc />
    public int Count => _l1.Count;

    /// <inheritdoc />
    /// <remarks>
    /// Used to namespace tiles by source. Set by the controller on <c>Load()</c>.
    /// Tiles stored under a different source ID will not be found.
    /// </remarks>
    public string? ActiveSourceId
    {
        get => _activeSourceId;
        set => _activeSourceId = value;
    }

    /// <summary>
    /// Maximum age for cached tiles. Files older than this are treated as a miss and deleted on next access.
    /// Can be updated at runtime (e.g. by the controller when loading a source with a shorter <see cref="ISKImagePyramidSource.CacheExpiry"/>).
    /// </summary>
    public TimeSpan Expiry { get; set; } // mutable so controller can narrow it per source

    /// <inheritdoc />
    /// <remarks>Only checks the in-memory L1 cache. Never touches the disk.</remarks>
    public bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile)
        => _l1.TryGet(id, out tile);

    /// <inheritdoc />
    /// <remarks>Checks L1 first; on miss, checks the filesystem and promotes to L1 on hit.
    /// If the on-disk tile is older than <see cref="Expiry"/>, it is deleted and treated as a miss.</remarks>
    public async Task<SKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
    {
        if (_l1.TryGet(id, out var cached))
            return cached;

        string path = GetTilePath(id);
        if (!File.Exists(path))
            return null;

        // Expiry check — delete stale tile and treat as a miss
        var effectiveExpiry = Expiry > TimeSpan.Zero ? Expiry : _expiry;
        if (DateTime.UtcNow - File.GetLastWriteTimeUtc(path) > effectiveExpiry)
        {
            try { File.Delete(path); } catch { }
            return null;
        }

        try
        {
#if NETSTANDARD2_0
            var bytes = File.ReadAllBytes(path);
#else
            var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
#endif
            var image = SKImage.FromEncodedData(bytes);
            if (image == null) return null;

            var tile = new SKImagePyramidTile(image, bytes, _activeSourceId ?? string.Empty);
            _l1.Put(id, tile);
            return tile;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    /// <remarks>Only checks the in-memory L1 cache.</remarks>
    public bool Contains(SKImagePyramidTileId id) => _l1.Contains(id);

    /// <inheritdoc />
    /// <remarks>Stores to L1 only. Use <see cref="PutAsync"/> to persist to disk.</remarks>
    public void Put(SKImagePyramidTileId id, SKImagePyramidTile? tile) => _l1.Put(id, tile);

    /// <inheritdoc />
    /// <remarks>Stores to L1 and asynchronously writes raw bytes to disk under the tile's <see cref="SKImagePyramidTile.SourceId"/> directory.</remarks>
    public async Task PutAsync(SKImagePyramidTileId id, SKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (tile == null) return;
        _l1.Put(id, tile);
        await WriteToDiskAsync(id, tile.RawData, tile.SourceId, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool Remove(SKImagePyramidTileId id)
    {
        bool removed = _l1.Remove(id);
        try { File.Delete(GetTilePath(id)); } catch { }
        return removed;
    }

    /// <inheritdoc />
    /// <remarks>Clears L1 and deletes all tiles in the active source directory from disk.</remarks>
    public void Clear()
    {
        _l1.Clear();
        string sourceDir = GetSourceDir(_activeSourceId);
        try
        {
            if (Directory.Exists(sourceDir))
                Directory.Delete(sourceDir, recursive: true);
        }
        catch { }
        Interlocked.Exchange(ref _diskTileCount, 0);
    }

    /// <inheritdoc />
    public void FlushEvicted() => _l1.FlushEvicted();

    /// <inheritdoc />
    public void Dispose() => _l1.Dispose();

    // ---- Private ----

    private string GetSourceDir(string? sourceId)
        => Path.Combine(_basePath, "skimgpyramid", string.IsNullOrEmpty(sourceId) ? "unknown" : sourceId);

    private string GetTilePath(SKImagePyramidTileId id)
        => Path.Combine(GetSourceDir(_activeSourceId), id.Level.ToString(), $"{id.Col}_{id.Row}.tile");

    private async Task WriteToDiskAsync(SKImagePyramidTileId id, byte[] bytes, string sourceId, CancellationToken ct)
    {
        try
        {
            string sourceDir = GetSourceDir(string.IsNullOrEmpty(sourceId) ? _activeSourceId : sourceId);
            string tilePath = Path.Combine(sourceDir, id.Level.ToString(), $"{id.Col}_{id.Row}.tile");
            string dir = Path.GetDirectoryName(tilePath)!;
            Directory.CreateDirectory(dir);

            // Atomic write via temp file to avoid partial reads on concurrent access
            string tmp = tilePath + ".tmp";
#if NETSTANDARD2_0
            File.WriteAllBytes(tmp, bytes);
            if (File.Exists(tilePath)) File.Delete(tilePath);
            File.Move(tmp, tilePath);
#else
            await File.WriteAllBytesAsync(tmp, bytes, ct).ConfigureAwait(false);
            File.Move(tmp, tilePath, overwrite: true);
#endif
            int count = Interlocked.Increment(ref _diskTileCount);
            if (count > _maxDiskTiles * 1.1 && Interlocked.CompareExchange(ref _cleanupRunning, 1, 0) == 0)
                _ = Task.Run(CleanupOldTiles, CancellationToken.None);
        }
        catch { }
    }

    private void CleanupOldTiles()
    {
        try
        {
            string sourceDir = GetSourceDir(_activeSourceId);
            if (!Directory.Exists(sourceDir)) return;
            var files = Directory.GetFiles(sourceDir, "*.tile", SearchOption.AllDirectories);
            if (files.Length <= _maxDiskTiles) return;
            Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(a).CompareTo(File.GetLastWriteTimeUtc(b)));
            int toDelete = files.Length - (int)(_maxDiskTiles * 0.9);
            for (int i = 0; i < toDelete && i < files.Length; i++)
                try { File.Delete(files[i]); } catch { }
            Interlocked.Exchange(ref _diskTileCount, files.Length - toDelete);
        }
        catch { }
        finally { Interlocked.Exchange(ref _cleanupRunning, 0); }
    }
}
