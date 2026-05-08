using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing;

/// <summary>
///  Defines a brush of a single color. Brushes are used to fill graphics shapes,
///  such as rectangles, ellipses, pies, polygons, and paths.
/// </summary>
public sealed partial class SolidBrush : Brush
{
	private Color _color;

	/// <summary>
	///  Initializes a new <see cref="SolidBrush"/> object of the specified color.
	/// </summary>
	/// <param name="color">
	///  A <see cref="Color"/> structure that represents the color of this brush.
	/// </param>
	public SolidBrush(Color color)
	{
		_color = color;
	}

	/// <summary>
	///  Gets or sets the color of this <see cref="SolidBrush"/> object.
	/// </summary>
	/// <value>A <see cref="Color"/> structure that represents the color of this brush.</value>
	/// <exception cref="ObjectDisposedException">This brush has been disposed.</exception>
	public Color Color
	{
		get
		{
			ThrowIfDisposed();
			return _color;
		}
		set
		{
			ThrowIfDisposed();
			if (_immutable) throw new ArgumentException("Cannot modify an immutable Brush.");
			_color = value;
		}
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="SolidBrush"/> object.
	/// </summary>
	/// <returns>The <see cref="SolidBrush"/> object that this method creates.</returns>
	public override object Clone() => new SolidBrush(_color);

	/// <summary>
	///  Releases the unmanaged resources used by the <see cref="SolidBrush"/> and
	///  optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">
	///  <see langword="true"/> to release both managed and unmanaged resources;
	///  <see langword="false"/> to release only unmanaged resources.
	/// </param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations with this brush's color.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with <see cref="SKPaintStyle.Fill"/> and the brush color.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();
		return new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SkiaConversions.ToSKColor(_color),
			IsAntialias = true,
		};
	}
}
