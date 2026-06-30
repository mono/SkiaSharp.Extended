#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// A persistent cache decorator backed by the local filesystem. It wraps an inner provider
/// (for example <see cref="SKHttpTileProvider"/>) and stores fetched tiles on disk using
/// hashed, bucketed filenames with a configurable expiry.
/// </summary>
public sealed class SKDiskCacheTileProvider : SKCachedTileProvider
{
	private const string CacheFolder = "skimagepyramid";

	private readonly string _basePath;
	private readonly TimeSpan _expiry;

	/// <summary>
	/// The default expiry applied when none is supplied (30 days).
	/// </summary>
	public static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(30);

	/// <summary>
	/// Creates a disk-backed cache decorator.
	/// </summary>
	/// <param name="inner">The provider to wrap (an origin or another decorator).</param>
	/// <param name="basePath">The root directory under which cached tiles are stored.</param>
	/// <param name="expiry">
	/// The maximum age before a cached entry is treated as a miss. Defaults to
	/// <see cref="DefaultExpiry"/>.
	/// </param>
	public SKDiskCacheTileProvider(ISKImagePyramidTileProvider inner, string basePath, TimeSpan? expiry = null)
		: base(inner)
	{
		_basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
		_expiry = expiry ?? DefaultExpiry;
	}

	/// <inheritdoc />
	protected override Task<SKImagePyramidTileData?> ReadAsync(string key, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var path = GetPath(key);
		if (!File.Exists(path))
			return Task.FromResult<SKImagePyramidTileData?>(null);

		if (DateTime.UtcNow - File.GetLastWriteTimeUtc(path) > _expiry)
		{
			try { File.Delete(path); } catch { /* best effort */ }
			return Task.FromResult<SKImagePyramidTileData?>(null);
		}

		try
		{
			var bytes = File.ReadAllBytes(path);
			return Task.FromResult<SKImagePyramidTileData?>(new SKImagePyramidTileData(bytes));
		}
		catch
		{
			return Task.FromResult<SKImagePyramidTileData?>(null);
		}
	}

	/// <inheritdoc />
	protected override Task WriteAsync(string key, SKImagePyramidTileData data, CancellationToken ct)
	{
		var path = GetPath(key);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		// Write to a temp file then move into place so readers never observe a partial file.
		var tmp = path + ".tmp";

#if NETSTANDARD2_0
		File.WriteAllBytes(tmp, data.Data);
		if (File.Exists(path))
			File.Delete(path);
		File.Move(tmp, path);
		return Task.CompletedTask;
#else
		return WriteCoreAsync(tmp, path, data, ct);
#endif
	}

	/// <summary>
	/// Deletes every tile this provider has cached.
	/// </summary>
	public void Clear()
	{
		try
		{
			var cacheDir = Path.Combine(_basePath, CacheFolder);
			if (Directory.Exists(cacheDir))
				Directory.Delete(cacheDir, recursive: true);
		}
		catch
		{
			// Best effort.
		}
	}

#if !NETSTANDARD2_0
	private static async Task WriteCoreAsync(string tmp, string path, SKImagePyramidTileData data, CancellationToken ct)
	{
		await File.WriteAllBytesAsync(tmp, data.Data, ct).ConfigureAwait(false);
		File.Move(tmp, path, overwrite: true);
	}
#endif

	private string GetPath(string key) =>
		Path.Combine(_basePath, CacheFolder, key.Substring(0, 2), key + ".tile");
}
