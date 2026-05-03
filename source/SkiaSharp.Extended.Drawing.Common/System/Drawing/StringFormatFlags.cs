namespace System.Drawing
{
	[System.FlagsAttribute]
	public enum StringFormatFlags
	{
		DirectionRightToLeft = 1,
		DirectionVertical = 2,
		FitBlackBox = 4,
		DisplayFormatControl = 32,
		NoFontFallback = 1024,
		MeasureTrailingSpaces = 2048,
		NoWrap = 4096,
		LineLimit = 8192,
		NoClip = 16384,
	}
}
