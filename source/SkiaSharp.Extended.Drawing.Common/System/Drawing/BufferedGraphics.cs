namespace System.Drawing;

/// <summary>
///  Provides a graphics buffer for double buffering.
/// </summary>
public sealed partial class BufferedGraphics : IDisposable
{
	private Graphics? _graphics;
	private Graphics? _targetGraphics;
	private bool _disposed;

	internal BufferedGraphics() {}

	internal BufferedGraphics(Graphics graphics, Graphics? target)
	{
		_graphics = graphics;
		_targetGraphics = target;
	}

	/// <summary>Gets a <see cref="Drawing.Graphics"/> object that outputs to the graphics buffer.</summary>
	public Graphics Graphics => _graphics ?? throw new ObjectDisposedException(nameof(BufferedGraphics));

	/// <summary>Releases all resources used by the <see cref="BufferedGraphics"/> object.</summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_graphics?.Dispose();
			_graphics = null;
		}
	}

	/// <summary>Writes the contents of the graphics buffer to the default device.</summary>
	public void Render()
	{
		if (_targetGraphics != null)
			Render(_targetGraphics);
	}

	/// <summary>Writes the contents of the graphics buffer to the specified <see cref="Drawing.Graphics"/> object.</summary>
	/// <param name="target">A <see cref="Drawing.Graphics"/> object to which to write the contents of the graphics buffer.</param>
	public void Render(Graphics? target)
	{
		// In a full implementation this would blit the buffer to the target.
		// This is a minimal stub that prevents PNSE.
	}

	/// <summary>Writes the contents of the graphics buffer to the device context associated with the specified handle.</summary>
	/// <param name="targetDC">An <see cref="IntPtr"/> that points to the device context to write to.</param>
	public void Render(nint targetDC) { throw new PlatformNotSupportedException("Device context rendering is not supported in SkiaSharp.Extended.Drawing.Common."); }
}
