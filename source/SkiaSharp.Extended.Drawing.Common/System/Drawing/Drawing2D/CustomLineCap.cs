namespace System.Drawing.Drawing2D;

/// <summary>
///  Encapsulates a custom user-defined line cap.
/// </summary>
public partial class CustomLineCap : System.MarshalByRefObject, System.ICloneable, System.IDisposable
{
	private GraphicsPath? _fillPath;
	private GraphicsPath? _strokePath;
	private LineCap _baseCap;
	private float _baseInset;
	private LineJoin _strokeJoin = LineJoin.Miter;
	private float _widthScale = 1.0f;
	private LineCap _startCap = LineCap.Flat;
	private LineCap _endCap = LineCap.Flat;
	private bool _disposed;

	/// <summary>Initializes a new instance of the <see cref="CustomLineCap"/> class with the specified outline and fill.</summary>
	public CustomLineCap(System.Drawing.Drawing2D.GraphicsPath? fillPath, System.Drawing.Drawing2D.GraphicsPath? strokePath)
		: this(fillPath, strokePath, LineCap.Flat, 0) { }

	/// <summary>Initializes a new instance of the <see cref="CustomLineCap"/> class with the specified outline, fill, and base cap.</summary>
	public CustomLineCap(System.Drawing.Drawing2D.GraphicsPath? fillPath, System.Drawing.Drawing2D.GraphicsPath? strokePath, System.Drawing.Drawing2D.LineCap baseCap)
		: this(fillPath, strokePath, baseCap, 0) { }

	/// <summary>Initializes a new instance of the <see cref="CustomLineCap"/> class with the specified outline, fill, base cap, and inset.</summary>
	public CustomLineCap(System.Drawing.Drawing2D.GraphicsPath? fillPath, System.Drawing.Drawing2D.GraphicsPath? strokePath, System.Drawing.Drawing2D.LineCap baseCap, float baseInset)
	{
		_fillPath = fillPath;
		_strokePath = strokePath;
		_baseCap = baseCap;
		_baseInset = baseInset;
	}

	/// <summary>Gets or sets the base <see cref="LineCap"/> from which this <see cref="CustomLineCap"/> is created.</summary>
	public System.Drawing.Drawing2D.LineCap BaseCap { get => _baseCap; set => _baseCap = value; }
	/// <summary>Gets or sets the distance between the cap and the line.</summary>
	public float BaseInset { get => _baseInset; set => _baseInset = value; }
	/// <summary>Gets or sets the <see cref="LineJoin"/> enumeration that determines how lines composing this cap are joined.</summary>
	public System.Drawing.Drawing2D.LineJoin StrokeJoin { get => _strokeJoin; set => _strokeJoin = value; }
	/// <summary>Gets or sets the amount by which to scale this <see cref="CustomLineCap"/> with respect to the width of the <see cref="Pen"/>.</summary>
	public float WidthScale { get => _widthScale; set => _widthScale = value; }

	/// <summary>Creates an exact copy of this <see cref="CustomLineCap"/>.</summary>
	public object Clone() => MemberwiseClone();

	/// <summary>Releases all resources used by this <see cref="CustomLineCap"/>.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Gets the start and end caps for this custom cap.</summary>
	/// <param name="startCap">The <see cref="LineCap"/> used at the beginning of a line.</param>
	/// <param name="endCap">The <see cref="LineCap"/> used at the end of a line.</param>
	public void GetStrokeCaps(out System.Drawing.Drawing2D.LineCap startCap, out System.Drawing.Drawing2D.LineCap endCap)
	{
		startCap = _startCap;
		endCap = _endCap;
	}

	/// <summary>Sets the start and end caps for this custom cap.</summary>
	/// <param name="startCap">The <see cref="LineCap"/> to use at the beginning of a line.</param>
	/// <param name="endCap">The <see cref="LineCap"/> to use at the end of a line.</param>
	public void SetStrokeCaps(System.Drawing.Drawing2D.LineCap startCap, System.Drawing.Drawing2D.LineCap endCap)
	{
		_startCap = startCap;
		_endCap = endCap;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="CustomLineCap"/> and optionally releases the managed resources.</summary>
	/// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
		}
	}

	/// <summary>Allows an object to try to free resources before being reclaimed by garbage collection.</summary>
	~CustomLineCap() { Dispose(false); }
}
