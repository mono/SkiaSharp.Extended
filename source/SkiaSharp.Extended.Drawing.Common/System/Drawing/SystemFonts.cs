namespace System.Drawing;

/// <summary>
///  Specifies the fonts used to display text in Windows display elements.
/// </summary>
public static partial class SystemFonts
{
	private static Font? _captionFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the Caption display element.</summary>
	public static Font? CaptionFont => _captionFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font _defaultFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the Default display element.</summary>
	public static Font DefaultFont => _defaultFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font _dialogFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the Dialog display element.</summary>
	public static Font DialogFont => _dialogFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font? _iconTitleFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the IconTitle display element.</summary>
	public static Font? IconTitleFont => _iconTitleFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font? _menuFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the Menu display element.</summary>
	public static Font? MenuFont => _menuFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font? _messageBoxFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the MessageBox display element.</summary>
	public static Font? MessageBoxFont => _messageBoxFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font? _smallCaptionFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the SmallCaption display element.</summary>
	public static Font? SmallCaptionFont => _smallCaptionFont ??= new Font("Microsoft Sans Serif", 8.25f);

	private static Font? _statusFont;
	/// <summary>Gets a <see cref="Font"/> that is used to display text in the Status display element.</summary>
	public static Font? StatusFont => _statusFont ??= new Font("Microsoft Sans Serif", 8.25f);

	/// <summary>
	///  Gets a <see cref="Font"/> object that corresponds to the specified system font name.
	/// </summary>
	/// <param name="systemFontName">The name of the system font to retrieve.</param>
	/// <returns>A <see cref="Font"/> if the specified name matches a system font; otherwise, <see langword="null"/>.</returns>
	public static Font? GetFontByName(string systemFontName)
	{
		return systemFontName switch
		{
			"CaptionFont" => CaptionFont,
			"DefaultFont" => DefaultFont,
			"DialogFont" => DialogFont,
			"IconTitleFont" => IconTitleFont,
			"MenuFont" => MenuFont,
			"MessageBoxFont" => MessageBoxFont,
			"SmallCaptionFont" => SmallCaptionFont,
			"StatusFont" => StatusFont,
			_ => null,
		};
	}
}
