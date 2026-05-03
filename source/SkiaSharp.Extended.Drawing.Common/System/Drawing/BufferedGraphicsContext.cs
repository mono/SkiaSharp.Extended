namespace System.Drawing
{
	/// <summary>
	///  Provides methods for creating graphics buffers that can be used for double buffering.
	/// </summary>
	public sealed partial class BufferedGraphicsContext : System.IDisposable
	{
		private Size _maximumBuffer = new Size(225, 96);
		private bool _disposed;

		/// <summary>Initializes a new instance of the <see cref="BufferedGraphicsContext"/> class.</summary>
		public BufferedGraphicsContext() { }

		/// <summary>Gets or sets the maximum size of the buffer to use.</summary>
		public System.Drawing.Size MaximumBuffer
		{
			get => _maximumBuffer;
			set
			{
				if (value.Width <= 0 || value.Height <= 0) throw new ArgumentException("Buffer size must be greater than zero.", nameof(value));
				_maximumBuffer = value;
			}
		}

		/// <summary>Creates a graphics buffer of the specified size using the Graphics object of the target.</summary>
		/// <param name="targetGraphics">The <see cref="Graphics"/> to match the pixel format for.</param>
		/// <param name="targetRectangle">A <see cref="Rectangle"/> indicating the size of the buffer to create.</param>
		/// <returns>A <see cref="BufferedGraphics"/> that can be used to draw to a buffer of the specified dimensions.</returns>
		public System.Drawing.BufferedGraphics Allocate(System.Drawing.Graphics targetGraphics, System.Drawing.Rectangle targetRectangle)
		{
			var width = Math.Max(1, targetRectangle.Width);
			var height = Math.Max(1, targetRectangle.Height);
			var bmp = new Bitmap(width, height);
			var g = Graphics.FromImage(bmp);
			return new BufferedGraphics(g, targetGraphics);
		}

		/// <summary>Creates a graphics buffer of the specified size using the device context handle.</summary>
		/// <param name="targetDC">An <see cref="IntPtr"/> to a device context to match the pixel format of the new buffer to.</param>
		/// <param name="targetRectangle">A <see cref="Rectangle"/> indicating the size of the buffer to create.</param>
		/// <returns>A <see cref="BufferedGraphics"/> that can be used to draw to a buffer of the specified dimensions.</returns>
		public System.Drawing.BufferedGraphics Allocate(nint targetDC, System.Drawing.Rectangle targetRectangle)
		{
			throw new System.PlatformNotSupportedException("Device context allocation is not supported in SkiaSharp.Extended.Drawing.Common.");
		}

		/// <summary>Releases all resources used by the <see cref="BufferedGraphicsContext"/>.</summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>Releases all cached buffers.</summary>
		public void Invalidate() { }

		/// <summary>Allows an object to try to free resources before being reclaimed by garbage collection.</summary>
		~BufferedGraphicsContext() { Dispose(); }
	}
}
