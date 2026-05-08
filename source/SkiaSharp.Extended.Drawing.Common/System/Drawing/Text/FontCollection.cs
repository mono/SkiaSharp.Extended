using SkiaSharp;

namespace System.Drawing.Text;

/// <summary>Provides a base class for installed and private font collections.</summary>
public abstract partial class FontCollection : IDisposable
{
	private bool _disposed;

	internal FontCollection() {}

	/// <summary>Gets the array of <see cref="FontFamily"/> objects associated with this <see cref="FontCollection"/>.</summary>
	public FontFamily[] Families
	{
		get
		{
			var manager = SKFontManager.Default;
			var count = manager.FontFamilyCount;
			var families = new FontFamily[count];
			for (int i = 0; i < count; i++)
			{
				families[i] = new FontFamily(manager.GetFamilyName(i));
			}
			return families;
		}
	}

	/// <summary>Releases all resources used by this <see cref="FontCollection"/>.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	internal virtual void Dispose(bool disposing)
	{
		_disposed = true;
	}

	~FontCollection()
	{
		Dispose(false);
	}
}
