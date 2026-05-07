using SkiaSharp;

namespace System.Drawing;

/// <summary>Each property of the <see cref="SystemIcons"/> class is an <see cref="Icon"/> object for Windows system-wide icons.</summary>
public static partial class SystemIcons
{
	private static Icon? _application;
	private static Icon? _asterisk;
	private static Icon? _error;
	private static Icon? _exclamation;
	private static Icon? _hand;
	private static Icon? _information;
	private static Icon? _question;
	private static Icon? _shield;
	private static Icon? _warning;
	private static Icon? _winLogo;

	/// <summary>Gets an <see cref="Icon"/> object that contains the default application icon.</summary>
	public static Icon Application => _application ??= CreateIcon(SKColors.Blue);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system asterisk icon.</summary>
	public static Icon Asterisk => _asterisk ??= CreateIcon(SKColors.CornflowerBlue);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system error icon.</summary>
	public static Icon Error => _error ??= CreateIcon(SKColors.Red);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system exclamation icon.</summary>
	public static Icon Exclamation => _exclamation ??= CreateIcon(SKColors.Gold);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system hand icon.</summary>
	public static Icon Hand => _hand ??= CreateIcon(SKColors.Red);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system information icon.</summary>
	public static Icon Information => _information ??= CreateIcon(SKColors.CornflowerBlue);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system question icon.</summary>
	public static Icon Question => _question ??= CreateIcon(SKColors.DodgerBlue);
	/// <summary>Gets an <see cref="Icon"/> object that contains the shield icon.</summary>
	public static Icon Shield => _shield ??= CreateIcon(SKColors.Green);
	/// <summary>Gets an <see cref="Icon"/> object that contains the system warning icon.</summary>
	public static Icon Warning => _warning ??= CreateIcon(SKColors.Gold);
	/// <summary>Gets an <see cref="Icon"/> object that contains the Windows logo icon.</summary>
	public static Icon WinLogo => _winLogo ??= CreateIcon(SKColors.DodgerBlue);

	private static Icon CreateIcon(SKColor color)
	{
		var bitmap = new SKBitmap(32, 32, SKColorType.Bgra8888, SKAlphaType.Premul);
		bitmap.Erase(color);
		// Use the internal Icon constructor that takes SKBitmap
		var bmp = new Bitmap(32, 32);
		bmp.SKBitmapBacking?.Dispose();
		bmp.SKBitmapBacking = bitmap;
		// Save to stream and load as icon
		using var ms = new IO.MemoryStream();
		bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
		ms.Position = 0;
		return new Icon(ms);
	}
}
