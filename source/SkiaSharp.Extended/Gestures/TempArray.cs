using System;
using System.Buffers;

namespace SkiaSharp.Extended;

/// <summary>
/// A small stack-only wrapper around a pooled array rented from <see cref="ArrayPool{T}"/>.
/// Use it with a <c>using</c> statement to borrow a temporary buffer of a known size without
/// allocating on the managed heap, then automatically return it to the pool.
/// </summary>
/// <remarks>
/// This is a <see langword="ref struct"/> so it cannot be boxed, captured, or stored in a field —
/// which prevents the buffer from escaping its scope and being returned to the pool twice.
/// <see cref="Length"/> is the logical length requested (the underlying rented array may be larger).
/// </remarks>
internal ref struct TempArray<T>
{
	private T[]? _array;

	/// <summary>
	/// Initializes a new <see cref="TempArray{T}"/> of the requested logical length, renting the
	/// backing storage from the shared array pool.
	/// </summary>
	/// <param name="length">The number of elements the buffer must hold. Zero rents nothing.</param>
	public TempArray(int length)
	{
		if (length < 0)
			throw new ArgumentOutOfRangeException(nameof(length));

		_array = length == 0 ? Array.Empty<T>() : ArrayPool<T>.Shared.Rent(length);
		Length = length;
	}

	/// <summary>Gets the logical length of the buffer.</summary>
	public int Length { get; }

	/// <summary>Gets a reference to the element at the specified index.</summary>
	public ref T this[int index] => ref _array![index];

	/// <summary>Gets a read-only span over the logical contents of the buffer.</summary>
	public readonly ReadOnlySpan<T> Span => new ReadOnlySpan<T>(_array, 0, Length);

	/// <summary>Returns the rented buffer to the pool. Safe to call multiple times.</summary>
	public void Dispose()
	{
		var array = _array;
		_array = null;
		if (array is { Length: > 0 })
			ArrayPool<T>.Shared.Return(array);
	}
}
