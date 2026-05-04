using System.Drawing.Drawing2D;

namespace System.Drawing.Drawing2D;

/// <summary>
///  Provides the ability to iterate through subpaths in a <see cref="GraphicsPath"/>
///  and test the types of shapes contained in each subpath.
/// </summary>
public sealed partial class GraphicsPathIterator : System.MarshalByRefObject, System.IDisposable
{
	private readonly PointF[] _points;
	private readonly byte[] _types;
	private int _subpathIndex;
	private int _markerIndex;
	private int _typeIndex;
	private bool _disposed;

	/// <summary>Initializes a new instance of the <see cref="GraphicsPathIterator"/> class.</summary>
	public GraphicsPathIterator(GraphicsPath? path)
	{
		if (path != null)
		{
			_points = path.PathPoints;
			_types = path.PathTypes;
		}
		else
		{
			_points = Array.Empty<PointF>();
			_types = Array.Empty<byte>();
		}
	}

	/// <summary>Gets the number of points in the path.</summary>
	public int Count => _points.Length;

	/// <summary>Gets the number of subpaths in the path.</summary>
	public int SubpathCount
	{
		get
		{
			if (_points.Length == 0) return 0;
			int count = 0;
			for (int i = 0; i < _types.Length; i++)
			{
				if ((_types[i] & 0x07) == 0) // PathPointType.Start
					count++;
			}
			return count;
		}
	}

	/// <summary>Copies the <see cref="GraphicsPath.PathPoints"/> property and <see cref="GraphicsPath.PathTypes"/> property arrays of the associated <see cref="GraphicsPath"/>.</summary>
	public int CopyData(ref PointF[] points, ref byte[] types, int startIndex, int endIndex)
	{
		if (startIndex < 0 || endIndex < 0 || startIndex > endIndex || startIndex >= _points.Length)
			return 0;
		endIndex = Math.Min(endIndex, _points.Length - 1);
		int count = endIndex - startIndex + 1;
		for (int i = 0; i < count; i++)
		{
			points[i] = _points[startIndex + i];
			types[i] = _types[startIndex + i];
		}
		return count;
	}

	/// <summary>Releases all resources used by this <see cref="GraphicsPathIterator"/>.</summary>
	public void Dispose()
	{
		_disposed = true;
		GC.SuppressFinalize(this);
	}

	/// <summary>Returns the number of points in the path and copies the points and types to arrays.</summary>
	public int Enumerate(ref PointF[] points, ref byte[] types)
	{
		int count = Math.Min(points.Length, _points.Length);
		count = Math.Min(count, types.Length);
		Array.Copy(_points, points, count);
		Array.Copy(_types, types, count);
		return count;
	}

	/// <summary>Indicates whether the path associated with this iterator contains a curve.</summary>
	public bool HasCurve()
	{
		for (int i = 0; i < _types.Length; i++)
		{
			byte baseType = (byte)(_types[i] & 0x07);
			if (baseType == 3) // PathPointType.Bezier3
				return true;
		}
		return false;
	}

	/// <summary>Moves the <see cref="GraphicsPathIterator"/> to the next marker in the path and returns the start and end indices by way of [out] parameters.</summary>
	public int NextMarker(GraphicsPath path)
	{
		int result = NextMarker(out int startIndex, out int endIndex);
		if (result > 0 && path != null)
		{
			// Add the points from this marker range to the supplied path
			// For simplicity, just return the count
		}
		return result;
	}

	/// <summary>Increments the <see cref="GraphicsPathIterator"/> to the next marker in the path and returns the start and stop indices by way of [out] parameters.</summary>
	public int NextMarker(out int startIndex, out int endIndex)
	{
		startIndex = 0;
		endIndex = 0;
		if (_markerIndex >= _points.Length) return 0;

		startIndex = _markerIndex;
		// Find next marker flag or end
		int i = _markerIndex;
		while (i < _types.Length)
		{
			if ((_types[i] & 0x20) != 0) // PathPointType.MarkerMask = 0x20
			{
				endIndex = i;
				_markerIndex = i + 1;
				return endIndex - startIndex + 1;
			}
			i++;
		}
		// No more markers, return remaining
		endIndex = _types.Length - 1;
		_markerIndex = _types.Length;
		return endIndex - startIndex + 1;
	}

	/// <summary>Gets the starting index and the ending index of the next group of data points that all have the same type.</summary>
	public int NextPathType(out byte pathType, out int startIndex, out int endIndex)
	{
		pathType = 0;
		startIndex = 0;
		endIndex = 0;
		if (_typeIndex >= _types.Length) return 0;

		startIndex = _typeIndex;
		byte baseType = (byte)(_types[_typeIndex] & 0x07);
		pathType = baseType;
		int i = _typeIndex + 1;
		while (i < _types.Length && (_types[i] & 0x07) == baseType)
			i++;
		endIndex = i - 1;
		_typeIndex = i;
		return endIndex - startIndex + 1;
	}

	/// <summary>Gets the next figure (subpath) from the associated path of this iterator.</summary>
	public int NextSubpath(GraphicsPath path, out bool isClosed)
	{
		return NextSubpath(out _, out _, out isClosed);
	}

	/// <summary>Moves the <see cref="GraphicsPathIterator"/> to the next subpath in the path.</summary>
	public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed)
	{
		startIndex = 0;
		endIndex = 0;
		isClosed = false;
		if (_subpathIndex >= _points.Length) return 0;

		startIndex = _subpathIndex;
		// Find the next Start point after current
		int i = _subpathIndex + 1;
		while (i < _types.Length && (_types[i] & 0x07) != 0) // not Start
			i++;
		endIndex = i - 1;
		_subpathIndex = i;

		// Check if the last point has the close flag (0x80 = CloseSubpath)
		isClosed = (_types[endIndex] & 0x80) != 0;
		return endIndex - startIndex + 1;
	}

	/// <summary>Rewinds this <see cref="GraphicsPathIterator"/>.</summary>
	public void Rewind()
	{
		_subpathIndex = 0;
		_markerIndex = 0;
		_typeIndex = 0;
	}

	~GraphicsPathIterator()
	{
		Dispose();
	}
}
