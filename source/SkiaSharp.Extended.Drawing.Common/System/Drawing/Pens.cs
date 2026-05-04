namespace System.Drawing;

/// <summary>
///  Each property of the <see cref="Pens"/> class is a <see cref="Pen"/>
///  object constructed with a specific pre-defined color and a width of 1.
/// </summary>
public static partial class Pens
{
	private static Pen? _aliceBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color AliceBlue.</summary>
	public static Pen AliceBlue => _aliceBlue ??= new Pen(Color.AliceBlue) { _immutable = true };

	private static Pen? _antiqueWhite;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color AntiqueWhite.</summary>
	public static Pen AntiqueWhite => _antiqueWhite ??= new Pen(Color.AntiqueWhite) { _immutable = true };

	private static Pen? _aqua;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Aqua.</summary>
	public static Pen Aqua => _aqua ??= new Pen(Color.Aqua) { _immutable = true };

	private static Pen? _aquamarine;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Aquamarine.</summary>
	public static Pen Aquamarine => _aquamarine ??= new Pen(Color.Aquamarine) { _immutable = true };

	private static Pen? _azure;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Azure.</summary>
	public static Pen Azure => _azure ??= new Pen(Color.Azure) { _immutable = true };

	private static Pen? _beige;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Beige.</summary>
	public static Pen Beige => _beige ??= new Pen(Color.Beige) { _immutable = true };

	private static Pen? _bisque;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Bisque.</summary>
	public static Pen Bisque => _bisque ??= new Pen(Color.Bisque) { _immutable = true };

	private static Pen? _black;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Black.</summary>
	public static Pen Black => _black ??= new Pen(Color.Black) { _immutable = true };

	private static Pen? _blanchedAlmond;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color BlanchedAlmond.</summary>
	public static Pen BlanchedAlmond => _blanchedAlmond ??= new Pen(Color.BlanchedAlmond) { _immutable = true };

	private static Pen? _blue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Blue.</summary>
	public static Pen Blue => _blue ??= new Pen(Color.Blue) { _immutable = true };

	private static Pen? _blueViolet;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color BlueViolet.</summary>
	public static Pen BlueViolet => _blueViolet ??= new Pen(Color.BlueViolet) { _immutable = true };

	private static Pen? _brown;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Brown.</summary>
	public static Pen Brown => _brown ??= new Pen(Color.Brown) { _immutable = true };

	private static Pen? _burlyWood;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color BurlyWood.</summary>
	public static Pen BurlyWood => _burlyWood ??= new Pen(Color.BurlyWood) { _immutable = true };

	private static Pen? _cadetBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color CadetBlue.</summary>
	public static Pen CadetBlue => _cadetBlue ??= new Pen(Color.CadetBlue) { _immutable = true };

	private static Pen? _chartreuse;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Chartreuse.</summary>
	public static Pen Chartreuse => _chartreuse ??= new Pen(Color.Chartreuse) { _immutable = true };

	private static Pen? _chocolate;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Chocolate.</summary>
	public static Pen Chocolate => _chocolate ??= new Pen(Color.Chocolate) { _immutable = true };

	private static Pen? _coral;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Coral.</summary>
	public static Pen Coral => _coral ??= new Pen(Color.Coral) { _immutable = true };

	private static Pen? _cornflowerBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color CornflowerBlue.</summary>
	public static Pen CornflowerBlue => _cornflowerBlue ??= new Pen(Color.CornflowerBlue) { _immutable = true };

	private static Pen? _cornsilk;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Cornsilk.</summary>
	public static Pen Cornsilk => _cornsilk ??= new Pen(Color.Cornsilk) { _immutable = true };

	private static Pen? _crimson;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Crimson.</summary>
	public static Pen Crimson => _crimson ??= new Pen(Color.Crimson) { _immutable = true };

	private static Pen? _cyan;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Cyan.</summary>
	public static Pen Cyan => _cyan ??= new Pen(Color.Cyan) { _immutable = true };

	private static Pen? _darkBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkBlue.</summary>
	public static Pen DarkBlue => _darkBlue ??= new Pen(Color.DarkBlue) { _immutable = true };

	private static Pen? _darkCyan;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkCyan.</summary>
	public static Pen DarkCyan => _darkCyan ??= new Pen(Color.DarkCyan) { _immutable = true };

	private static Pen? _darkGoldenrod;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkGoldenrod.</summary>
	public static Pen DarkGoldenrod => _darkGoldenrod ??= new Pen(Color.DarkGoldenrod) { _immutable = true };

	private static Pen? _darkGray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkGray.</summary>
	public static Pen DarkGray => _darkGray ??= new Pen(Color.DarkGray) { _immutable = true };

	private static Pen? _darkGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkGreen.</summary>
	public static Pen DarkGreen => _darkGreen ??= new Pen(Color.DarkGreen) { _immutable = true };

	private static Pen? _darkKhaki;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkKhaki.</summary>
	public static Pen DarkKhaki => _darkKhaki ??= new Pen(Color.DarkKhaki) { _immutable = true };

	private static Pen? _darkMagenta;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkMagenta.</summary>
	public static Pen DarkMagenta => _darkMagenta ??= new Pen(Color.DarkMagenta) { _immutable = true };

	private static Pen? _darkOliveGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkOliveGreen.</summary>
	public static Pen DarkOliveGreen => _darkOliveGreen ??= new Pen(Color.DarkOliveGreen) { _immutable = true };

	private static Pen? _darkOrange;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkOrange.</summary>
	public static Pen DarkOrange => _darkOrange ??= new Pen(Color.DarkOrange) { _immutable = true };

	private static Pen? _darkOrchid;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkOrchid.</summary>
	public static Pen DarkOrchid => _darkOrchid ??= new Pen(Color.DarkOrchid) { _immutable = true };

	private static Pen? _darkRed;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkRed.</summary>
	public static Pen DarkRed => _darkRed ??= new Pen(Color.DarkRed) { _immutable = true };

	private static Pen? _darkSalmon;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkSalmon.</summary>
	public static Pen DarkSalmon => _darkSalmon ??= new Pen(Color.DarkSalmon) { _immutable = true };

	private static Pen? _darkSeaGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkSeaGreen.</summary>
	public static Pen DarkSeaGreen => _darkSeaGreen ??= new Pen(Color.DarkSeaGreen) { _immutable = true };

	private static Pen? _darkSlateBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkSlateBlue.</summary>
	public static Pen DarkSlateBlue => _darkSlateBlue ??= new Pen(Color.DarkSlateBlue) { _immutable = true };

	private static Pen? _darkSlateGray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkSlateGray.</summary>
	public static Pen DarkSlateGray => _darkSlateGray ??= new Pen(Color.DarkSlateGray) { _immutable = true };

	private static Pen? _darkTurquoise;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkTurquoise.</summary>
	public static Pen DarkTurquoise => _darkTurquoise ??= new Pen(Color.DarkTurquoise) { _immutable = true };

	private static Pen? _darkViolet;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DarkViolet.</summary>
	public static Pen DarkViolet => _darkViolet ??= new Pen(Color.DarkViolet) { _immutable = true };

	private static Pen? _deepPink;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DeepPink.</summary>
	public static Pen DeepPink => _deepPink ??= new Pen(Color.DeepPink) { _immutable = true };

	private static Pen? _deepSkyBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DeepSkyBlue.</summary>
	public static Pen DeepSkyBlue => _deepSkyBlue ??= new Pen(Color.DeepSkyBlue) { _immutable = true };

	private static Pen? _dimGray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DimGray.</summary>
	public static Pen DimGray => _dimGray ??= new Pen(Color.DimGray) { _immutable = true };

	private static Pen? _dodgerBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color DodgerBlue.</summary>
	public static Pen DodgerBlue => _dodgerBlue ??= new Pen(Color.DodgerBlue) { _immutable = true };

	private static Pen? _firebrick;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Firebrick.</summary>
	public static Pen Firebrick => _firebrick ??= new Pen(Color.Firebrick) { _immutable = true };

	private static Pen? _floralWhite;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color FloralWhite.</summary>
	public static Pen FloralWhite => _floralWhite ??= new Pen(Color.FloralWhite) { _immutable = true };

	private static Pen? _forestGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color ForestGreen.</summary>
	public static Pen ForestGreen => _forestGreen ??= new Pen(Color.ForestGreen) { _immutable = true };

	private static Pen? _fuchsia;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Fuchsia.</summary>
	public static Pen Fuchsia => _fuchsia ??= new Pen(Color.Fuchsia) { _immutable = true };

	private static Pen? _gainsboro;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Gainsboro.</summary>
	public static Pen Gainsboro => _gainsboro ??= new Pen(Color.Gainsboro) { _immutable = true };

	private static Pen? _ghostWhite;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color GhostWhite.</summary>
	public static Pen GhostWhite => _ghostWhite ??= new Pen(Color.GhostWhite) { _immutable = true };

	private static Pen? _gold;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Gold.</summary>
	public static Pen Gold => _gold ??= new Pen(Color.Gold) { _immutable = true };

	private static Pen? _goldenrod;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Goldenrod.</summary>
	public static Pen Goldenrod => _goldenrod ??= new Pen(Color.Goldenrod) { _immutable = true };

	private static Pen? _gray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Gray.</summary>
	public static Pen Gray => _gray ??= new Pen(Color.Gray) { _immutable = true };

	private static Pen? _green;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Green.</summary>
	public static Pen Green => _green ??= new Pen(Color.Green) { _immutable = true };

	private static Pen? _greenYellow;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color GreenYellow.</summary>
	public static Pen GreenYellow => _greenYellow ??= new Pen(Color.GreenYellow) { _immutable = true };

	private static Pen? _honeydew;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Honeydew.</summary>
	public static Pen Honeydew => _honeydew ??= new Pen(Color.Honeydew) { _immutable = true };

	private static Pen? _hotPink;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color HotPink.</summary>
	public static Pen HotPink => _hotPink ??= new Pen(Color.HotPink) { _immutable = true };

	private static Pen? _indianRed;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color IndianRed.</summary>
	public static Pen IndianRed => _indianRed ??= new Pen(Color.IndianRed) { _immutable = true };

	private static Pen? _indigo;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Indigo.</summary>
	public static Pen Indigo => _indigo ??= new Pen(Color.Indigo) { _immutable = true };

	private static Pen? _ivory;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Ivory.</summary>
	public static Pen Ivory => _ivory ??= new Pen(Color.Ivory) { _immutable = true };

	private static Pen? _khaki;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Khaki.</summary>
	public static Pen Khaki => _khaki ??= new Pen(Color.Khaki) { _immutable = true };

	private static Pen? _lavender;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Lavender.</summary>
	public static Pen Lavender => _lavender ??= new Pen(Color.Lavender) { _immutable = true };

	private static Pen? _lavenderBlush;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LavenderBlush.</summary>
	public static Pen LavenderBlush => _lavenderBlush ??= new Pen(Color.LavenderBlush) { _immutable = true };

	private static Pen? _lawnGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LawnGreen.</summary>
	public static Pen LawnGreen => _lawnGreen ??= new Pen(Color.LawnGreen) { _immutable = true };

	private static Pen? _lemonChiffon;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LemonChiffon.</summary>
	public static Pen LemonChiffon => _lemonChiffon ??= new Pen(Color.LemonChiffon) { _immutable = true };

	private static Pen? _lightBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightBlue.</summary>
	public static Pen LightBlue => _lightBlue ??= new Pen(Color.LightBlue) { _immutable = true };

	private static Pen? _lightCoral;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightCoral.</summary>
	public static Pen LightCoral => _lightCoral ??= new Pen(Color.LightCoral) { _immutable = true };

	private static Pen? _lightCyan;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightCyan.</summary>
	public static Pen LightCyan => _lightCyan ??= new Pen(Color.LightCyan) { _immutable = true };

	private static Pen? _lightGoldenrodYellow;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightGoldenrodYellow.</summary>
	public static Pen LightGoldenrodYellow => _lightGoldenrodYellow ??= new Pen(Color.LightGoldenrodYellow) { _immutable = true };

	private static Pen? _lightGray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightGray.</summary>
	public static Pen LightGray => _lightGray ??= new Pen(Color.LightGray) { _immutable = true };

	private static Pen? _lightGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightGreen.</summary>
	public static Pen LightGreen => _lightGreen ??= new Pen(Color.LightGreen) { _immutable = true };

	private static Pen? _lightPink;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightPink.</summary>
	public static Pen LightPink => _lightPink ??= new Pen(Color.LightPink) { _immutable = true };

	private static Pen? _lightSalmon;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightSalmon.</summary>
	public static Pen LightSalmon => _lightSalmon ??= new Pen(Color.LightSalmon) { _immutable = true };

	private static Pen? _lightSeaGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightSeaGreen.</summary>
	public static Pen LightSeaGreen => _lightSeaGreen ??= new Pen(Color.LightSeaGreen) { _immutable = true };

	private static Pen? _lightSkyBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightSkyBlue.</summary>
	public static Pen LightSkyBlue => _lightSkyBlue ??= new Pen(Color.LightSkyBlue) { _immutable = true };

	private static Pen? _lightSlateGray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightSlateGray.</summary>
	public static Pen LightSlateGray => _lightSlateGray ??= new Pen(Color.LightSlateGray) { _immutable = true };

	private static Pen? _lightSteelBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightSteelBlue.</summary>
	public static Pen LightSteelBlue => _lightSteelBlue ??= new Pen(Color.LightSteelBlue) { _immutable = true };

	private static Pen? _lightYellow;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LightYellow.</summary>
	public static Pen LightYellow => _lightYellow ??= new Pen(Color.LightYellow) { _immutable = true };

	private static Pen? _lime;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Lime.</summary>
	public static Pen Lime => _lime ??= new Pen(Color.Lime) { _immutable = true };

	private static Pen? _limeGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color LimeGreen.</summary>
	public static Pen LimeGreen => _limeGreen ??= new Pen(Color.LimeGreen) { _immutable = true };

	private static Pen? _linen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Linen.</summary>
	public static Pen Linen => _linen ??= new Pen(Color.Linen) { _immutable = true };

	private static Pen? _magenta;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Magenta.</summary>
	public static Pen Magenta => _magenta ??= new Pen(Color.Magenta) { _immutable = true };

	private static Pen? _maroon;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Maroon.</summary>
	public static Pen Maroon => _maroon ??= new Pen(Color.Maroon) { _immutable = true };

	private static Pen? _mediumAquamarine;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumAquamarine.</summary>
	public static Pen MediumAquamarine => _mediumAquamarine ??= new Pen(Color.MediumAquamarine) { _immutable = true };

	private static Pen? _mediumBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumBlue.</summary>
	public static Pen MediumBlue => _mediumBlue ??= new Pen(Color.MediumBlue) { _immutable = true };

	private static Pen? _mediumOrchid;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumOrchid.</summary>
	public static Pen MediumOrchid => _mediumOrchid ??= new Pen(Color.MediumOrchid) { _immutable = true };

	private static Pen? _mediumPurple;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumPurple.</summary>
	public static Pen MediumPurple => _mediumPurple ??= new Pen(Color.MediumPurple) { _immutable = true };

	private static Pen? _mediumSeaGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumSeaGreen.</summary>
	public static Pen MediumSeaGreen => _mediumSeaGreen ??= new Pen(Color.MediumSeaGreen) { _immutable = true };

	private static Pen? _mediumSlateBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumSlateBlue.</summary>
	public static Pen MediumSlateBlue => _mediumSlateBlue ??= new Pen(Color.MediumSlateBlue) { _immutable = true };

	private static Pen? _mediumSpringGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumSpringGreen.</summary>
	public static Pen MediumSpringGreen => _mediumSpringGreen ??= new Pen(Color.MediumSpringGreen) { _immutable = true };

	private static Pen? _mediumTurquoise;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumTurquoise.</summary>
	public static Pen MediumTurquoise => _mediumTurquoise ??= new Pen(Color.MediumTurquoise) { _immutable = true };

	private static Pen? _mediumVioletRed;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MediumVioletRed.</summary>
	public static Pen MediumVioletRed => _mediumVioletRed ??= new Pen(Color.MediumVioletRed) { _immutable = true };

	private static Pen? _midnightBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MidnightBlue.</summary>
	public static Pen MidnightBlue => _midnightBlue ??= new Pen(Color.MidnightBlue) { _immutable = true };

	private static Pen? _mintCream;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MintCream.</summary>
	public static Pen MintCream => _mintCream ??= new Pen(Color.MintCream) { _immutable = true };

	private static Pen? _mistyRose;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color MistyRose.</summary>
	public static Pen MistyRose => _mistyRose ??= new Pen(Color.MistyRose) { _immutable = true };

	private static Pen? _moccasin;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Moccasin.</summary>
	public static Pen Moccasin => _moccasin ??= new Pen(Color.Moccasin) { _immutable = true };

	private static Pen? _navajoWhite;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color NavajoWhite.</summary>
	public static Pen NavajoWhite => _navajoWhite ??= new Pen(Color.NavajoWhite) { _immutable = true };

	private static Pen? _navy;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Navy.</summary>
	public static Pen Navy => _navy ??= new Pen(Color.Navy) { _immutable = true };

	private static Pen? _oldLace;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color OldLace.</summary>
	public static Pen OldLace => _oldLace ??= new Pen(Color.OldLace) { _immutable = true };

	private static Pen? _olive;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Olive.</summary>
	public static Pen Olive => _olive ??= new Pen(Color.Olive) { _immutable = true };

	private static Pen? _oliveDrab;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color OliveDrab.</summary>
	public static Pen OliveDrab => _oliveDrab ??= new Pen(Color.OliveDrab) { _immutable = true };

	private static Pen? _orange;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Orange.</summary>
	public static Pen Orange => _orange ??= new Pen(Color.Orange) { _immutable = true };

	private static Pen? _orangeRed;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color OrangeRed.</summary>
	public static Pen OrangeRed => _orangeRed ??= new Pen(Color.OrangeRed) { _immutable = true };

	private static Pen? _orchid;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Orchid.</summary>
	public static Pen Orchid => _orchid ??= new Pen(Color.Orchid) { _immutable = true };

	private static Pen? _paleGoldenrod;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PaleGoldenrod.</summary>
	public static Pen PaleGoldenrod => _paleGoldenrod ??= new Pen(Color.PaleGoldenrod) { _immutable = true };

	private static Pen? _paleGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PaleGreen.</summary>
	public static Pen PaleGreen => _paleGreen ??= new Pen(Color.PaleGreen) { _immutable = true };

	private static Pen? _paleTurquoise;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PaleTurquoise.</summary>
	public static Pen PaleTurquoise => _paleTurquoise ??= new Pen(Color.PaleTurquoise) { _immutable = true };

	private static Pen? _paleVioletRed;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PaleVioletRed.</summary>
	public static Pen PaleVioletRed => _paleVioletRed ??= new Pen(Color.PaleVioletRed) { _immutable = true };

	private static Pen? _papayaWhip;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PapayaWhip.</summary>
	public static Pen PapayaWhip => _papayaWhip ??= new Pen(Color.PapayaWhip) { _immutable = true };

	private static Pen? _peachPuff;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PeachPuff.</summary>
	public static Pen PeachPuff => _peachPuff ??= new Pen(Color.PeachPuff) { _immutable = true };

	private static Pen? _peru;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Peru.</summary>
	public static Pen Peru => _peru ??= new Pen(Color.Peru) { _immutable = true };

	private static Pen? _pink;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Pink.</summary>
	public static Pen Pink => _pink ??= new Pen(Color.Pink) { _immutable = true };

	private static Pen? _plum;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Plum.</summary>
	public static Pen Plum => _plum ??= new Pen(Color.Plum) { _immutable = true };

	private static Pen? _powderBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color PowderBlue.</summary>
	public static Pen PowderBlue => _powderBlue ??= new Pen(Color.PowderBlue) { _immutable = true };

	private static Pen? _purple;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Purple.</summary>
	public static Pen Purple => _purple ??= new Pen(Color.Purple) { _immutable = true };

	private static Pen? _red;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Red.</summary>
	public static Pen Red => _red ??= new Pen(Color.Red) { _immutable = true };

	private static Pen? _rosyBrown;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color RosyBrown.</summary>
	public static Pen RosyBrown => _rosyBrown ??= new Pen(Color.RosyBrown) { _immutable = true };

	private static Pen? _royalBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color RoyalBlue.</summary>
	public static Pen RoyalBlue => _royalBlue ??= new Pen(Color.RoyalBlue) { _immutable = true };

	private static Pen? _saddleBrown;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SaddleBrown.</summary>
	public static Pen SaddleBrown => _saddleBrown ??= new Pen(Color.SaddleBrown) { _immutable = true };

	private static Pen? _salmon;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Salmon.</summary>
	public static Pen Salmon => _salmon ??= new Pen(Color.Salmon) { _immutable = true };

	private static Pen? _sandyBrown;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SandyBrown.</summary>
	public static Pen SandyBrown => _sandyBrown ??= new Pen(Color.SandyBrown) { _immutable = true };

	private static Pen? _seaGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SeaGreen.</summary>
	public static Pen SeaGreen => _seaGreen ??= new Pen(Color.SeaGreen) { _immutable = true };

	private static Pen? _seaShell;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SeaShell.</summary>
	public static Pen SeaShell => _seaShell ??= new Pen(Color.SeaShell) { _immutable = true };

	private static Pen? _sienna;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Sienna.</summary>
	public static Pen Sienna => _sienna ??= new Pen(Color.Sienna) { _immutable = true };

	private static Pen? _silver;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Silver.</summary>
	public static Pen Silver => _silver ??= new Pen(Color.Silver) { _immutable = true };

	private static Pen? _skyBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SkyBlue.</summary>
	public static Pen SkyBlue => _skyBlue ??= new Pen(Color.SkyBlue) { _immutable = true };

	private static Pen? _slateBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SlateBlue.</summary>
	public static Pen SlateBlue => _slateBlue ??= new Pen(Color.SlateBlue) { _immutable = true };

	private static Pen? _slateGray;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SlateGray.</summary>
	public static Pen SlateGray => _slateGray ??= new Pen(Color.SlateGray) { _immutable = true };

	private static Pen? _snow;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Snow.</summary>
	public static Pen Snow => _snow ??= new Pen(Color.Snow) { _immutable = true };

	private static Pen? _springGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SpringGreen.</summary>
	public static Pen SpringGreen => _springGreen ??= new Pen(Color.SpringGreen) { _immutable = true };

	private static Pen? _steelBlue;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color SteelBlue.</summary>
	public static Pen SteelBlue => _steelBlue ??= new Pen(Color.SteelBlue) { _immutable = true };

	private static Pen? _tan;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Tan.</summary>
	public static Pen Tan => _tan ??= new Pen(Color.Tan) { _immutable = true };

	private static Pen? _teal;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Teal.</summary>
	public static Pen Teal => _teal ??= new Pen(Color.Teal) { _immutable = true };

	private static Pen? _thistle;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Thistle.</summary>
	public static Pen Thistle => _thistle ??= new Pen(Color.Thistle) { _immutable = true };

	private static Pen? _tomato;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Tomato.</summary>
	public static Pen Tomato => _tomato ??= new Pen(Color.Tomato) { _immutable = true };

	private static Pen? _transparent;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Transparent.</summary>
	public static Pen Transparent => _transparent ??= new Pen(Color.Transparent) { _immutable = true };

	private static Pen? _turquoise;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Turquoise.</summary>
	public static Pen Turquoise => _turquoise ??= new Pen(Color.Turquoise) { _immutable = true };

	private static Pen? _violet;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Violet.</summary>
	public static Pen Violet => _violet ??= new Pen(Color.Violet) { _immutable = true };

	private static Pen? _wheat;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Wheat.</summary>
	public static Pen Wheat => _wheat ??= new Pen(Color.Wheat) { _immutable = true };

	private static Pen? _white;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color White.</summary>
	public static Pen White => _white ??= new Pen(Color.White) { _immutable = true };

	private static Pen? _whiteSmoke;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color WhiteSmoke.</summary>
	public static Pen WhiteSmoke => _whiteSmoke ??= new Pen(Color.WhiteSmoke) { _immutable = true };

	private static Pen? _yellow;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color Yellow.</summary>
	public static Pen Yellow => _yellow ??= new Pen(Color.Yellow) { _immutable = true };

	private static Pen? _yellowGreen;
	/// <summary>Gets a system-defined <see cref="Pen"/> object with a width of 1 and the color YellowGreen.</summary>
	public static Pen YellowGreen => _yellowGreen ??= new Pen(Color.YellowGreen) { _immutable = true };
}
