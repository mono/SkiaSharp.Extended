namespace System.Drawing;

/// <summary>
///  Encapsulates text layout information (such as alignment, orientation, and tab stops),
///  display manipulations (such as ellipsis insertion and national digit substitution),
///  and OpenType features. This class cannot be inherited.
/// </summary>
public sealed partial class StringFormat : System.MarshalByRefObject, System.ICloneable, System.IDisposable
{
	private StringAlignment _alignment;
	private StringAlignment _lineAlignment;
	private StringFormatFlags _formatFlags;
	private StringTrimming _trimming;
	private Text.HotkeyPrefix _hotkeyPrefix;
	private int _digitSubstitutionLanguage;
	private StringDigitSubstitute _digitSubstitutionMethod;
	private CharacterRange[]? _measurableRanges;
	private float _firstTabOffset;
	private float[]? _tabStops;
	private bool _disposed;

	/// <summary>
	///  Initializes a new <see cref="StringFormat"/> object.
	/// </summary>
	public StringFormat()
		: this((StringFormatFlags)0, 0)
	{
	}

	/// <summary>
	///  Initializes a new <see cref="StringFormat"/> object from the specified existing <see cref="StringFormat"/> object.
	/// </summary>
	/// <param name="format">The <see cref="StringFormat"/> object from which to initialize the new <see cref="StringFormat"/> object.</param>
	public StringFormat(System.Drawing.StringFormat format)
	{
		if (format is null) throw new ArgumentNullException(nameof(format));
		_alignment = format._alignment;
		_lineAlignment = format._lineAlignment;
		_formatFlags = format._formatFlags;
		_trimming = format._trimming;
		_hotkeyPrefix = format._hotkeyPrefix;
		_digitSubstitutionLanguage = format._digitSubstitutionLanguage;
		_digitSubstitutionMethod = format._digitSubstitutionMethod;
		_firstTabOffset = format._firstTabOffset;
		_tabStops = format._tabStops != null ? (float[])format._tabStops.Clone() : null;
		_measurableRanges = format._measurableRanges != null ? (CharacterRange[])format._measurableRanges.Clone() : null;
	}

	/// <summary>
	///  Initializes a new <see cref="StringFormat"/> object with the specified <see cref="StringFormatFlags"/> enumeration.
	/// </summary>
	/// <param name="options">The <see cref="StringFormatFlags"/> enumeration for the new <see cref="StringFormat"/> object.</param>
	public StringFormat(System.Drawing.StringFormatFlags options)
		: this(options, 0)
	{
	}

	/// <summary>
	///  Initializes a new <see cref="StringFormat"/> object with the specified <see cref="StringFormatFlags"/> enumeration and language.
	/// </summary>
	/// <param name="options">The <see cref="StringFormatFlags"/> enumeration for the new <see cref="StringFormat"/> object.</param>
	/// <param name="language">A value that indicates the language of the text.</param>
	public StringFormat(System.Drawing.StringFormatFlags options, int language)
	{
		_formatFlags = options;
		_digitSubstitutionLanguage = language;
		_trimming = StringTrimming.Character;
	}

	/// <summary>
	///  Gets or sets horizontal alignment of the string.
	/// </summary>
	public System.Drawing.StringAlignment Alignment
	{
		get => _alignment;
		set => _alignment = value;
	}

	/// <summary>
	///  Gets the language that is used when local digits are substituted for western digits.
	/// </summary>
	public int DigitSubstitutionLanguage => _digitSubstitutionLanguage;

	/// <summary>
	///  Gets the method to be used for digit substitution.
	/// </summary>
	public System.Drawing.StringDigitSubstitute DigitSubstitutionMethod => _digitSubstitutionMethod;

	/// <summary>
	///  Gets or sets a <see cref="StringFormatFlags"/> enumeration that contains formatting information.
	/// </summary>
	public System.Drawing.StringFormatFlags FormatFlags
	{
		get => _formatFlags;
		set => _formatFlags = value;
	}

	/// <summary>
	///  Gets a generic default <see cref="StringFormat"/> object.
	/// </summary>
	public static System.Drawing.StringFormat GenericDefault => new StringFormat();

	/// <summary>
	///  Gets a generic typographic <see cref="StringFormat"/> object.
	/// </summary>
	public static System.Drawing.StringFormat GenericTypographic => new StringFormat(
		StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit | StringFormatFlags.NoClip)
	{
		_trimming = StringTrimming.None,
	};

	/// <summary>
	///  Gets or sets the <see cref="Text.HotkeyPrefix"/> object for this <see cref="StringFormat"/> object.
	/// </summary>
	public System.Drawing.Text.HotkeyPrefix HotkeyPrefix
	{
		get => _hotkeyPrefix;
		set => _hotkeyPrefix = value;
	}

	/// <summary>
	///  Gets or sets the vertical alignment of the string.
	/// </summary>
	public System.Drawing.StringAlignment LineAlignment
	{
		get => _lineAlignment;
		set => _lineAlignment = value;
	}

	/// <summary>
	///  Gets or sets the <see cref="StringTrimming"/> enumeration for this <see cref="StringFormat"/> object.
	/// </summary>
	public System.Drawing.StringTrimming Trimming
	{
		get => _trimming;
		set => _trimming = value;
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="StringFormat"/> object.
	/// </summary>
	public object Clone() => new StringFormat(this);

	/// <summary>
	///  Releases all resources used by this <see cref="StringFormat"/> object.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		_disposed = true;
	}

	/// <summary>
	///  Gets the tab stops for this <see cref="StringFormat"/> object.
	/// </summary>
	/// <param name="firstTabOffset">The number of spaces between the beginning of a text line and the first tab stop.</param>
	/// <returns>An array of distances (in number of spaces) between tab stops.</returns>
	public float[] GetTabStops(out float firstTabOffset)
	{
		firstTabOffset = _firstTabOffset;
		return _tabStops ?? Array.Empty<float>();
	}

	/// <summary>
	///  Specifies the language and method to be used when local digits are substituted for western digits.
	/// </summary>
	/// <param name="language">A National Language Support (NLS) language identifier.</param>
	/// <param name="substitute">The <see cref="StringDigitSubstitute"/> specifying how digits are displayed.</param>
	public void SetDigitSubstitution(int language, System.Drawing.StringDigitSubstitute substitute)
	{
		_digitSubstitutionLanguage = language;
		_digitSubstitutionMethod = substitute;
	}

	/// <summary>
	///  Specifies an array of <see cref="CharacterRange"/> structures that represent the ranges of characters
	///  measured by a call to the <see cref="Graphics.MeasureCharacterRanges"/> method.
	/// </summary>
	/// <param name="ranges">An array of <see cref="CharacterRange"/> structures that specifies the ranges of characters.</param>
	public void SetMeasurableCharacterRanges(System.Drawing.CharacterRange[] ranges)
	{
		_measurableRanges = ranges != null ? (CharacterRange[])ranges.Clone() : null;
	}

	/// <summary>
	///  Sets tab stops for this <see cref="StringFormat"/> object.
	/// </summary>
	/// <param name="firstTabOffset">The number of spaces between the beginning of a line of text and the first tab stop.</param>
	/// <param name="tabStops">An array of distances between tab stops in the units specified by the PageUnit property.</param>
	public void SetTabStops(float firstTabOffset, float[] tabStops)
	{
		_firstTabOffset = firstTabOffset;
		_tabStops = tabStops != null ? (float[])tabStops.Clone() : null;
	}

	/// <summary>
	///  Converts this <see cref="StringFormat"/> object to a human-readable string.
	/// </summary>
	public override string ToString()
		=> $"[StringFormat, FormatFlags={_formatFlags}]";

	/// <summary>
	///  Allows a <see cref="StringFormat"/> to attempt to free resources before it is reclaimed by garbage collection.
	/// </summary>
	~StringFormat()
	{
		Dispose(false);
	}
}
