using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing;

/// <summary>
///  Defines objects used to fill the interiors of graphical shapes such as rectangles,
///  ellipses, pies, polygons, and paths.
/// </summary>
public abstract partial class Brush : MarshalByRefObject, ICloneable, IDisposable
{
	private bool _disposed;
	internal bool _immutable;

	/// <summary>
	///  Initializes a new instance of the <see cref="Brush"/> class.
	/// </summary>
	protected Brush() { }

	/// <summary>
	///  When overridden in a derived class, creates an exact copy of this <see cref="Brush"/>.
	/// </summary>
	/// <returns>The new <see cref="Brush"/> that this method creates.</returns>
	public abstract object Clone();

	/// <summary>
	///  Releases all resources used by this <see cref="Brush"/> object.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///  Releases the unmanaged resources used by the <see cref="Brush"/> and optionally
	///  releases the managed resources.
	/// </summary>
	/// <param name="disposing">
	///  <see langword="true"/> to release both managed and unmanaged resources;
	///  <see langword="false"/> to release only unmanaged resources.
	/// </param>
	protected virtual void Dispose(bool disposing)
	{
		_disposed = true;
	}

	/// <summary>
	///  Allows a <see cref="Brush"/> object to attempt to free resources and perform other
	///  cleanup operations before the <see cref="Brush"/> object is reclaimed by garbage collection.
	/// </summary>
	~Brush()
	{
		Dispose(false);
	}

	/// <summary>
	///  In a derived class, sets a reference to a GDI+ brush object.
	/// </summary>
	/// <param name="brush">A pointer to the GDI+ brush object.</param>
	protected internal void SetNativeBrush(nint brush)
	{
		// No-op: SkiaSharp does not use GDI+ native handles.
	}

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations from this brush.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> instance configured for this brush.</returns>
	internal virtual SKPaint CreatePaint()
	{
		throw new NotImplementedException($"{GetType().Name}.CreatePaint is not implemented.");
	}

	/// <summary>
	///  Gets a value indicating whether this brush has been disposed.
	/// </summary>
	internal bool IsDisposed => _disposed;

	/// <summary>
	///  Throws an <see cref="ObjectDisposedException"/> if this brush has been disposed.
	/// </summary>
	internal void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(GetType().Name);
	}
}
