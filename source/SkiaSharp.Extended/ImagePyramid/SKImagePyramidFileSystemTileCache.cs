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
/// </remarks>
public class SKImagePyramidFileSystemTileCache : ISKImagePyramidTileCache
{
    private readonly SKImagePyramidMemoryTileCache _l1;
    private readonly string _sourceDir;
    private readonly int _maxDiskTiles;
    private int _diskTileCount;
    private bool _cleanupRunning;

    /// <summary>
    /// Creates a two-tier cache using the given base directory.
    /// </summary>
    /// <param name="basePath">Root directory for tile storage (e.g. <c>FileSystem.CacheDirectory</c> on MAUI).</param>
    /// <param name="sourceId">Per-source subdirectory name. Use <see cref="ISKImagePyramidSource.SourceId"/>.</param>
    /// <param name="memoryCapacity">Maximum tiles in the L1 memory cache.</param>
    /// <param name="maxDiskTiles">Soft maximum tiles on disk before old tiles are cleaned up.</param>
    public SKImagePyramidFileSystemTileCache(
        string basePath,
        string sourceId,
        int memoryCapacity = 256,
        int maxDiskTiles = 8192)
    {
        if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("basePath is required.", nameof(basePath));
        if (string.IsNullOrEmpty(sourceId)) throw new ArgumentException("sourceId is required.", nameof(sourceId));
        if (memoryCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(memoryCapacity));
        if (maxDiskTiles <= 0) throw new ArgumentOutOfRangeException(nameof(maxDiskTiles));

        _l1 = new SKImagePyramidMemoryTileCache(memoryCapacity);
        _sourceDir = Path.Combine(basePath, "skimgpyramid", sourceId);
        _maxDiskTiles = maxDiskTiles;
    }

    /// <inheritdoc />
    public int Count => _l1.Count;

    /// <inheritdoc />
    /// <remarks>Only checks the in-memory L1 cache. Never touches the disk.</remarks>
    public bool TryGet(SKImagePyramidTileId id, out SKImagePyramidTile? tile)
        => _l1.TryGet(id, out tile);

    /// <inheritdoc />
    /// <remarks>Checks L1 first; on miss, checks the filesystem and promotes to L1 on hit.</remarks>
    public async Task<SKImagePyramidTile?> TryGetAsync(SKImagePyramidTileId id, CancellationToken ct = default)
    {
        if (_l1.TryGet(id, out var cached))
            return cached;

        string path = GetTilePath(id);
        if (!File.Exists(path))
            return null;

        try
        {
#if NETSTANDARD2_0
            var bytes = File.ReadAllBytes(path);
#else
            var bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
#endif
            var image = SKImage.FromEncodedData(bytes);
            if (image == null) return null;

            var tile = new SKImagePyramidTile(image, bytes);
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
    /// <remarks>Stores to L1 and asynchronously writes raw bytes to disk.</remarks>
    public async Task PutAsync(SKImagePyramidTileId id, SKImagePyramidTile? tile, CancellationToken ct = default)
    {
        if (tile == null) return;
        _l1.Put(id, tile);
        await WriteToDiskAsync(id, tile.RawData, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool Remove(SKImagePyramidTileId id)
    {
        bool removed = _l1.Remove(id);
        try { File.Delete(GetTilePath(id)); } catch { }
        return removed;
    }

    /// <inheritdoc />
    /// <remarks>Clears L1 and deletes all tiles in the source directory from disk.</remarks>
    public void Clear()
    {
        _l1.Clear();
        try
        {
            if (Directory.Exists(_sourceDir))
                Directory.Delete(_sourceDir, recursive: true);
        }
        catch { }
        Interlocked.Exchange(ref _diskTileCount, 0);
    }

    /// <inheritdoc />
    public void FlushEvicted() => _l1.FlushEvicted();

    /// <inheritdoc />
    public void Dispose() => _l1.Dispose();

    // ---- Private ----

    private string GetTilePath(SKImagePyramidTileId id)
        => Path.Combine(_sourceDir, id.Level.ToString(), $"{id.Col}_{id.Row}.tile");

    private async Task WriteToDiskAsync(SKImagePyramidTileId id, byte[] bytes, CancellationToken ct)
    {
        try
        {
            string tilePath = GetTilePath(id);
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
            if (count > _maxDiskTiles * 1.1 && !_cleanupRunning)
                _ = Task.Run(CleanupOldTiles, CancellationToken.None);
        }
        catch { }
    }

    private void CleanupOldTiles()
    {
        if (_cleanupRunning) return;
        _cleanupRunning = true;
        try
        {
            if (!Directory.Exists(_sourceDir)) return;
            var files = Directory.GetFiles(_sourceDir, "*.tile", SearchOption.AllDirectories);
            if (files.Length <= _maxDiskTiles) return;
            Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(a).CompareTo(File.GetLastWriteTimeUtc(b)));
            int toDelete = files.Length - (int)(_maxDiskTiles * 0.9);
            for (int i = 0; i < toDelete && i < files.Length; i++)
                try { File.Delete(files[i]); } catch { }
            Interlocked.Exchange(ref _diskTileCount, files.Length - toDelete);
        }
        catch { }
        finally { _cleanupRunning = false; }
    }
}
