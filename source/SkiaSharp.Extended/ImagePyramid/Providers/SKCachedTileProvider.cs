#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// A base class for persistent cache decorators. It wraps an inner provider and adds a
/// read-through / write-back cache: derived classes implement only the storage backend by
/// overriding <see cref="ReadAsync"/> and <see cref="WriteAsync"/>, while the caching flow
/// (check cache → on miss call inner → persist) is handled here.
/// </summary>
/// <remarks>
/// Cache reads and writes operate on encoded bytes (<see cref="SKImagePyramidTileData"/>),
/// so a backend never decodes. The built-in <see cref="SKDiskCacheTileProvider"/> stores
/// tiles on disk; samples cache to browser storage in exactly the same way.
/// </remarks>
public abstract class SKCachedTileProvider : ISKImagePyramidTileProvider
{
	private readonly ISKImagePyramidTileProvider _inner;

	/// <summary>
	/// Initializes the cache decorator around an inner provider.
	/// </summary>
	/// <param name="inner">The provider to wrap (an origin or another decorator).</param>
	protected SKCachedTileProvider(ISKImagePyramidTileProvider inner)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
	}

	/// <inheritdoc />
	public async Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		var key = ComputeKey(url);

		// 1. Cache read. A backend error is treated as a miss.
		try
		{
			var hit = await ReadAsync(key, ct).ConfigureAwait(false);
			if (hit is not null)
				return hit;
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch
		{
			// Ignore and fall through to the inner provider.
		}

		// 2. Inner provider (an origin or a nested cache).
		var data = await _inner.GetTileAsync(url, ct).ConfigureAwait(false);

		// 3. Persist. Fire-and-forget so a slow backend never blocks rendering, and use a
		//    fresh token so a cancelled request still populates the cache it just paid for.
		if (data is not null)
			_ = WriteSafeAsync(key, data);

		return data;
	}

	/// <summary>
	/// Reads cached bytes for <paramref name="key"/>, or returns <see langword="null"/> on a
	/// cache miss.
	/// </summary>
	protected abstract Task<SKImagePyramidTileData?> ReadAsync(string key, CancellationToken ct);

	/// <summary>
	/// Persists <paramref name="data"/> under <paramref name="key"/>. Called fire-and-forget;
	/// failures are ignored.
	/// </summary>
	protected abstract Task WriteAsync(string key, SKImagePyramidTileData data, CancellationToken ct);

	/// <summary>
	/// Computes a stable, key- and filesystem-safe identifier for a tile URL using a
	/// 64-bit FNV-1a hash rendered as lower-case hex.
	/// </summary>
	/// <param name="url">The tile URL.</param>
	/// <returns>A 16-character lower-case hex string.</returns>
	protected static string ComputeKey(string url)
	{
		const ulong OffsetBasis = 14695981039346656037UL;
		const ulong Prime = 1099511628211UL;

		var hash = OffsetBasis;
		foreach (var c in url)
		{
			hash ^= (byte)(c & 0xFF);
			hash *= Prime;
			hash ^= (byte)(c >> 8);
			hash *= Prime;
		}

		return hash.ToString("x16");
	}

	/// <inheritdoc />
	public virtual void Dispose() => _inner.Dispose();

	private async Task WriteSafeAsync(string key, SKImagePyramidTileData data)
	{
		try
		{
			await WriteAsync(key, data, CancellationToken.None).ConfigureAwait(false);
		}
		catch
		{
			// A failed cache write is not fatal — the tile was still returned to the caller.
		}
	}
}
