namespace System.Drawing;

/// <summary>
///  Each property of the <see cref="Brushes"/> class is a <see cref="SolidBrush"/>
///  object constructed with a specific pre-defined color.
/// </summary>
public static partial class Brushes
{
	private static Brush? _aliceBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color AliceBlue.</summary>
	public static Brush AliceBlue => _aliceBlue ??= new SolidBrush(Color.AliceBlue) { _immutable = true };

	private static Brush? _antiqueWhite;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color AntiqueWhite.</summary>
	public static Brush AntiqueWhite => _antiqueWhite ??= new SolidBrush(Color.AntiqueWhite) { _immutable = true };

	private static Brush? _aqua;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Aqua.</summary>
	public static Brush Aqua => _aqua ??= new SolidBrush(Color.Aqua) { _immutable = true };

	private static Brush? _aquamarine;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Aquamarine.</summary>
	public static Brush Aquamarine => _aquamarine ??= new SolidBrush(Color.Aquamarine) { _immutable = true };

	private static Brush? _azure;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Azure.</summary>
	public static Brush Azure => _azure ??= new SolidBrush(Color.Azure) { _immutable = true };

	private static Brush? _beige;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Beige.</summary>
	public static Brush Beige => _beige ??= new SolidBrush(Color.Beige) { _immutable = true };

	private static Brush? _bisque;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Bisque.</summary>
	public static Brush Bisque => _bisque ??= new SolidBrush(Color.Bisque) { _immutable = true };

	private static Brush? _black;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Black.</summary>
	public static Brush Black => _black ??= new SolidBrush(Color.Black) { _immutable = true };

	private static Brush? _blanchedAlmond;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color BlanchedAlmond.</summary>
	public static Brush BlanchedAlmond => _blanchedAlmond ??= new SolidBrush(Color.BlanchedAlmond) { _immutable = true };

	private static Brush? _blue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Blue.</summary>
	public static Brush Blue => _blue ??= new SolidBrush(Color.Blue) { _immutable = true };

	private static Brush? _blueViolet;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color BlueViolet.</summary>
	public static Brush BlueViolet => _blueViolet ??= new SolidBrush(Color.BlueViolet) { _immutable = true };

	private static Brush? _brown;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Brown.</summary>
	public static Brush Brown => _brown ??= new SolidBrush(Color.Brown) { _immutable = true };

	private static Brush? _burlyWood;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color BurlyWood.</summary>
	public static Brush BurlyWood => _burlyWood ??= new SolidBrush(Color.BurlyWood) { _immutable = true };

	private static Brush? _cadetBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color CadetBlue.</summary>
	public static Brush CadetBlue => _cadetBlue ??= new SolidBrush(Color.CadetBlue) { _immutable = true };

	private static Brush? _chartreuse;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Chartreuse.</summary>
	public static Brush Chartreuse => _chartreuse ??= new SolidBrush(Color.Chartreuse) { _immutable = true };

	private static Brush? _chocolate;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Chocolate.</summary>
	public static Brush Chocolate => _chocolate ??= new SolidBrush(Color.Chocolate) { _immutable = true };

	private static Brush? _coral;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Coral.</summary>
	public static Brush Coral => _coral ??= new SolidBrush(Color.Coral) { _immutable = true };

	private static Brush? _cornflowerBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color CornflowerBlue.</summary>
	public static Brush CornflowerBlue => _cornflowerBlue ??= new SolidBrush(Color.CornflowerBlue) { _immutable = true };

	private static Brush? _cornsilk;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Cornsilk.</summary>
	public static Brush Cornsilk => _cornsilk ??= new SolidBrush(Color.Cornsilk) { _immutable = true };

	private static Brush? _crimson;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Crimson.</summary>
	public static Brush Crimson => _crimson ??= new SolidBrush(Color.Crimson) { _immutable = true };

	private static Brush? _cyan;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Cyan.</summary>
	public static Brush Cyan => _cyan ??= new SolidBrush(Color.Cyan) { _immutable = true };

	private static Brush? _darkBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkBlue.</summary>
	public static Brush DarkBlue => _darkBlue ??= new SolidBrush(Color.DarkBlue) { _immutable = true };

	private static Brush? _darkCyan;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkCyan.</summary>
	public static Brush DarkCyan => _darkCyan ??= new SolidBrush(Color.DarkCyan) { _immutable = true };

	private static Brush? _darkGoldenrod;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkGoldenrod.</summary>
	public static Brush DarkGoldenrod => _darkGoldenrod ??= new SolidBrush(Color.DarkGoldenrod) { _immutable = true };

	private static Brush? _darkGray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkGray.</summary>
	public static Brush DarkGray => _darkGray ??= new SolidBrush(Color.DarkGray) { _immutable = true };

	private static Brush? _darkGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkGreen.</summary>
	public static Brush DarkGreen => _darkGreen ??= new SolidBrush(Color.DarkGreen) { _immutable = true };

	private static Brush? _darkKhaki;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkKhaki.</summary>
	public static Brush DarkKhaki => _darkKhaki ??= new SolidBrush(Color.DarkKhaki) { _immutable = true };

	private static Brush? _darkMagenta;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkMagenta.</summary>
	public static Brush DarkMagenta => _darkMagenta ??= new SolidBrush(Color.DarkMagenta) { _immutable = true };

	private static Brush? _darkOliveGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkOliveGreen.</summary>
	public static Brush DarkOliveGreen => _darkOliveGreen ??= new SolidBrush(Color.DarkOliveGreen) { _immutable = true };

	private static Brush? _darkOrange;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkOrange.</summary>
	public static Brush DarkOrange => _darkOrange ??= new SolidBrush(Color.DarkOrange) { _immutable = true };

	private static Brush? _darkOrchid;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkOrchid.</summary>
	public static Brush DarkOrchid => _darkOrchid ??= new SolidBrush(Color.DarkOrchid) { _immutable = true };

	private static Brush? _darkRed;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkRed.</summary>
	public static Brush DarkRed => _darkRed ??= new SolidBrush(Color.DarkRed) { _immutable = true };

	private static Brush? _darkSalmon;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkSalmon.</summary>
	public static Brush DarkSalmon => _darkSalmon ??= new SolidBrush(Color.DarkSalmon) { _immutable = true };

	private static Brush? _darkSeaGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkSeaGreen.</summary>
	public static Brush DarkSeaGreen => _darkSeaGreen ??= new SolidBrush(Color.DarkSeaGreen) { _immutable = true };

	private static Brush? _darkSlateBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkSlateBlue.</summary>
	public static Brush DarkSlateBlue => _darkSlateBlue ??= new SolidBrush(Color.DarkSlateBlue) { _immutable = true };

	private static Brush? _darkSlateGray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkSlateGray.</summary>
	public static Brush DarkSlateGray => _darkSlateGray ??= new SolidBrush(Color.DarkSlateGray) { _immutable = true };

	private static Brush? _darkTurquoise;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkTurquoise.</summary>
	public static Brush DarkTurquoise => _darkTurquoise ??= new SolidBrush(Color.DarkTurquoise) { _immutable = true };

	private static Brush? _darkViolet;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DarkViolet.</summary>
	public static Brush DarkViolet => _darkViolet ??= new SolidBrush(Color.DarkViolet) { _immutable = true };

	private static Brush? _deepPink;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DeepPink.</summary>
	public static Brush DeepPink => _deepPink ??= new SolidBrush(Color.DeepPink) { _immutable = true };

	private static Brush? _deepSkyBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DeepSkyBlue.</summary>
	public static Brush DeepSkyBlue => _deepSkyBlue ??= new SolidBrush(Color.DeepSkyBlue) { _immutable = true };

	private static Brush? _dimGray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DimGray.</summary>
	public static Brush DimGray => _dimGray ??= new SolidBrush(Color.DimGray) { _immutable = true };

	private static Brush? _dodgerBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color DodgerBlue.</summary>
	public static Brush DodgerBlue => _dodgerBlue ??= new SolidBrush(Color.DodgerBlue) { _immutable = true };

	private static Brush? _firebrick;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Firebrick.</summary>
	public static Brush Firebrick => _firebrick ??= new SolidBrush(Color.Firebrick) { _immutable = true };

	private static Brush? _floralWhite;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color FloralWhite.</summary>
	public static Brush FloralWhite => _floralWhite ??= new SolidBrush(Color.FloralWhite) { _immutable = true };

	private static Brush? _forestGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color ForestGreen.</summary>
	public static Brush ForestGreen => _forestGreen ??= new SolidBrush(Color.ForestGreen) { _immutable = true };

	private static Brush? _fuchsia;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Fuchsia.</summary>
	public static Brush Fuchsia => _fuchsia ??= new SolidBrush(Color.Fuchsia) { _immutable = true };

	private static Brush? _gainsboro;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Gainsboro.</summary>
	public static Brush Gainsboro => _gainsboro ??= new SolidBrush(Color.Gainsboro) { _immutable = true };

	private static Brush? _ghostWhite;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color GhostWhite.</summary>
	public static Brush GhostWhite => _ghostWhite ??= new SolidBrush(Color.GhostWhite) { _immutable = true };

	private static Brush? _gold;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Gold.</summary>
	public static Brush Gold => _gold ??= new SolidBrush(Color.Gold) { _immutable = true };

	private static Brush? _goldenrod;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Goldenrod.</summary>
	public static Brush Goldenrod => _goldenrod ??= new SolidBrush(Color.Goldenrod) { _immutable = true };

	private static Brush? _gray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Gray.</summary>
	public static Brush Gray => _gray ??= new SolidBrush(Color.Gray) { _immutable = true };

	private static Brush? _green;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Green.</summary>
	public static Brush Green => _green ??= new SolidBrush(Color.Green) { _immutable = true };

	private static Brush? _greenYellow;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color GreenYellow.</summary>
	public static Brush GreenYellow => _greenYellow ??= new SolidBrush(Color.GreenYellow) { _immutable = true };

	private static Brush? _honeydew;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Honeydew.</summary>
	public static Brush Honeydew => _honeydew ??= new SolidBrush(Color.Honeydew) { _immutable = true };

	private static Brush? _hotPink;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color HotPink.</summary>
	public static Brush HotPink => _hotPink ??= new SolidBrush(Color.HotPink) { _immutable = true };

	private static Brush? _indianRed;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color IndianRed.</summary>
	public static Brush IndianRed => _indianRed ??= new SolidBrush(Color.IndianRed) { _immutable = true };

	private static Brush? _indigo;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Indigo.</summary>
	public static Brush Indigo => _indigo ??= new SolidBrush(Color.Indigo) { _immutable = true };

	private static Brush? _ivory;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Ivory.</summary>
	public static Brush Ivory => _ivory ??= new SolidBrush(Color.Ivory) { _immutable = true };

	private static Brush? _khaki;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Khaki.</summary>
	public static Brush Khaki => _khaki ??= new SolidBrush(Color.Khaki) { _immutable = true };

	private static Brush? _lavender;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Lavender.</summary>
	public static Brush Lavender => _lavender ??= new SolidBrush(Color.Lavender) { _immutable = true };

	private static Brush? _lavenderBlush;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LavenderBlush.</summary>
	public static Brush LavenderBlush => _lavenderBlush ??= new SolidBrush(Color.LavenderBlush) { _immutable = true };

	private static Brush? _lawnGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LawnGreen.</summary>
	public static Brush LawnGreen => _lawnGreen ??= new SolidBrush(Color.LawnGreen) { _immutable = true };

	private static Brush? _lemonChiffon;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LemonChiffon.</summary>
	public static Brush LemonChiffon => _lemonChiffon ??= new SolidBrush(Color.LemonChiffon) { _immutable = true };

	private static Brush? _lightBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightBlue.</summary>
	public static Brush LightBlue => _lightBlue ??= new SolidBrush(Color.LightBlue) { _immutable = true };

	private static Brush? _lightCoral;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightCoral.</summary>
	public static Brush LightCoral => _lightCoral ??= new SolidBrush(Color.LightCoral) { _immutable = true };

	private static Brush? _lightCyan;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightCyan.</summary>
	public static Brush LightCyan => _lightCyan ??= new SolidBrush(Color.LightCyan) { _immutable = true };

	private static Brush? _lightGoldenrodYellow;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightGoldenrodYellow.</summary>
	public static Brush LightGoldenrodYellow => _lightGoldenrodYellow ??= new SolidBrush(Color.LightGoldenrodYellow) { _immutable = true };

	private static Brush? _lightGray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightGray.</summary>
	public static Brush LightGray => _lightGray ??= new SolidBrush(Color.LightGray) { _immutable = true };

	private static Brush? _lightGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightGreen.</summary>
	public static Brush LightGreen => _lightGreen ??= new SolidBrush(Color.LightGreen) { _immutable = true };

	private static Brush? _lightPink;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightPink.</summary>
	public static Brush LightPink => _lightPink ??= new SolidBrush(Color.LightPink) { _immutable = true };

	private static Brush? _lightSalmon;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightSalmon.</summary>
	public static Brush LightSalmon => _lightSalmon ??= new SolidBrush(Color.LightSalmon) { _immutable = true };

	private static Brush? _lightSeaGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightSeaGreen.</summary>
	public static Brush LightSeaGreen => _lightSeaGreen ??= new SolidBrush(Color.LightSeaGreen) { _immutable = true };

	private static Brush? _lightSkyBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightSkyBlue.</summary>
	public static Brush LightSkyBlue => _lightSkyBlue ??= new SolidBrush(Color.LightSkyBlue) { _immutable = true };

	private static Brush? _lightSlateGray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightSlateGray.</summary>
	public static Brush LightSlateGray => _lightSlateGray ??= new SolidBrush(Color.LightSlateGray) { _immutable = true };

	private static Brush? _lightSteelBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightSteelBlue.</summary>
	public static Brush LightSteelBlue => _lightSteelBlue ??= new SolidBrush(Color.LightSteelBlue) { _immutable = true };

	private static Brush? _lightYellow;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LightYellow.</summary>
	public static Brush LightYellow => _lightYellow ??= new SolidBrush(Color.LightYellow) { _immutable = true };

	private static Brush? _lime;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Lime.</summary>
	public static Brush Lime => _lime ??= new SolidBrush(Color.Lime) { _immutable = true };

	private static Brush? _limeGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color LimeGreen.</summary>
	public static Brush LimeGreen => _limeGreen ??= new SolidBrush(Color.LimeGreen) { _immutable = true };

	private static Brush? _linen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Linen.</summary>
	public static Brush Linen => _linen ??= new SolidBrush(Color.Linen) { _immutable = true };

	private static Brush? _magenta;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Magenta.</summary>
	public static Brush Magenta => _magenta ??= new SolidBrush(Color.Magenta) { _immutable = true };

	private static Brush? _maroon;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Maroon.</summary>
	public static Brush Maroon => _maroon ??= new SolidBrush(Color.Maroon) { _immutable = true };

	private static Brush? _mediumAquamarine;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumAquamarine.</summary>
	public static Brush MediumAquamarine => _mediumAquamarine ??= new SolidBrush(Color.MediumAquamarine) { _immutable = true };

	private static Brush? _mediumBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumBlue.</summary>
	public static Brush MediumBlue => _mediumBlue ??= new SolidBrush(Color.MediumBlue) { _immutable = true };

	private static Brush? _mediumOrchid;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumOrchid.</summary>
	public static Brush MediumOrchid => _mediumOrchid ??= new SolidBrush(Color.MediumOrchid) { _immutable = true };

	private static Brush? _mediumPurple;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumPurple.</summary>
	public static Brush MediumPurple => _mediumPurple ??= new SolidBrush(Color.MediumPurple) { _immutable = true };

	private static Brush? _mediumSeaGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumSeaGreen.</summary>
	public static Brush MediumSeaGreen => _mediumSeaGreen ??= new SolidBrush(Color.MediumSeaGreen) { _immutable = true };

	private static Brush? _mediumSlateBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumSlateBlue.</summary>
	public static Brush MediumSlateBlue => _mediumSlateBlue ??= new SolidBrush(Color.MediumSlateBlue) { _immutable = true };

	private static Brush? _mediumSpringGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumSpringGreen.</summary>
	public static Brush MediumSpringGreen => _mediumSpringGreen ??= new SolidBrush(Color.MediumSpringGreen) { _immutable = true };

	private static Brush? _mediumTurquoise;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumTurquoise.</summary>
	public static Brush MediumTurquoise => _mediumTurquoise ??= new SolidBrush(Color.MediumTurquoise) { _immutable = true };

	private static Brush? _mediumVioletRed;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MediumVioletRed.</summary>
	public static Brush MediumVioletRed => _mediumVioletRed ??= new SolidBrush(Color.MediumVioletRed) { _immutable = true };

	private static Brush? _midnightBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MidnightBlue.</summary>
	public static Brush MidnightBlue => _midnightBlue ??= new SolidBrush(Color.MidnightBlue) { _immutable = true };

	private static Brush? _mintCream;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MintCream.</summary>
	public static Brush MintCream => _mintCream ??= new SolidBrush(Color.MintCream) { _immutable = true };

	private static Brush? _mistyRose;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color MistyRose.</summary>
	public static Brush MistyRose => _mistyRose ??= new SolidBrush(Color.MistyRose) { _immutable = true };

	private static Brush? _moccasin;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Moccasin.</summary>
	public static Brush Moccasin => _moccasin ??= new SolidBrush(Color.Moccasin) { _immutable = true };

	private static Brush? _navajoWhite;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color NavajoWhite.</summary>
	public static Brush NavajoWhite => _navajoWhite ??= new SolidBrush(Color.NavajoWhite) { _immutable = true };

	private static Brush? _navy;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Navy.</summary>
	public static Brush Navy => _navy ??= new SolidBrush(Color.Navy) { _immutable = true };

	private static Brush? _oldLace;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color OldLace.</summary>
	public static Brush OldLace => _oldLace ??= new SolidBrush(Color.OldLace) { _immutable = true };

	private static Brush? _olive;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Olive.</summary>
	public static Brush Olive => _olive ??= new SolidBrush(Color.Olive) { _immutable = true };

	private static Brush? _oliveDrab;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color OliveDrab.</summary>
	public static Brush OliveDrab => _oliveDrab ??= new SolidBrush(Color.OliveDrab) { _immutable = true };

	private static Brush? _orange;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Orange.</summary>
	public static Brush Orange => _orange ??= new SolidBrush(Color.Orange) { _immutable = true };

	private static Brush? _orangeRed;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color OrangeRed.</summary>
	public static Brush OrangeRed => _orangeRed ??= new SolidBrush(Color.OrangeRed) { _immutable = true };

	private static Brush? _orchid;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Orchid.</summary>
	public static Brush Orchid => _orchid ??= new SolidBrush(Color.Orchid) { _immutable = true };

	private static Brush? _paleGoldenrod;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PaleGoldenrod.</summary>
	public static Brush PaleGoldenrod => _paleGoldenrod ??= new SolidBrush(Color.PaleGoldenrod) { _immutable = true };

	private static Brush? _paleGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PaleGreen.</summary>
	public static Brush PaleGreen => _paleGreen ??= new SolidBrush(Color.PaleGreen) { _immutable = true };

	private static Brush? _paleTurquoise;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PaleTurquoise.</summary>
	public static Brush PaleTurquoise => _paleTurquoise ??= new SolidBrush(Color.PaleTurquoise) { _immutable = true };

	private static Brush? _paleVioletRed;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PaleVioletRed.</summary>
	public static Brush PaleVioletRed => _paleVioletRed ??= new SolidBrush(Color.PaleVioletRed) { _immutable = true };

	private static Brush? _papayaWhip;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PapayaWhip.</summary>
	public static Brush PapayaWhip => _papayaWhip ??= new SolidBrush(Color.PapayaWhip) { _immutable = true };

	private static Brush? _peachPuff;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PeachPuff.</summary>
	public static Brush PeachPuff => _peachPuff ??= new SolidBrush(Color.PeachPuff) { _immutable = true };

	private static Brush? _peru;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Peru.</summary>
	public static Brush Peru => _peru ??= new SolidBrush(Color.Peru) { _immutable = true };

	private static Brush? _pink;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Pink.</summary>
	public static Brush Pink => _pink ??= new SolidBrush(Color.Pink) { _immutable = true };

	private static Brush? _plum;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Plum.</summary>
	public static Brush Plum => _plum ??= new SolidBrush(Color.Plum) { _immutable = true };

	private static Brush? _powderBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color PowderBlue.</summary>
	public static Brush PowderBlue => _powderBlue ??= new SolidBrush(Color.PowderBlue) { _immutable = true };

	private static Brush? _purple;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Purple.</summary>
	public static Brush Purple => _purple ??= new SolidBrush(Color.Purple) { _immutable = true };

	private static Brush? _red;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Red.</summary>
	public static Brush Red => _red ??= new SolidBrush(Color.Red) { _immutable = true };

	private static Brush? _rosyBrown;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color RosyBrown.</summary>
	public static Brush RosyBrown => _rosyBrown ??= new SolidBrush(Color.RosyBrown) { _immutable = true };

	private static Brush? _royalBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color RoyalBlue.</summary>
	public static Brush RoyalBlue => _royalBlue ??= new SolidBrush(Color.RoyalBlue) { _immutable = true };

	private static Brush? _saddleBrown;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SaddleBrown.</summary>
	public static Brush SaddleBrown => _saddleBrown ??= new SolidBrush(Color.SaddleBrown) { _immutable = true };

	private static Brush? _salmon;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Salmon.</summary>
	public static Brush Salmon => _salmon ??= new SolidBrush(Color.Salmon) { _immutable = true };

	private static Brush? _sandyBrown;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SandyBrown.</summary>
	public static Brush SandyBrown => _sandyBrown ??= new SolidBrush(Color.SandyBrown) { _immutable = true };

	private static Brush? _seaGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SeaGreen.</summary>
	public static Brush SeaGreen => _seaGreen ??= new SolidBrush(Color.SeaGreen) { _immutable = true };

	private static Brush? _seaShell;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SeaShell.</summary>
	public static Brush SeaShell => _seaShell ??= new SolidBrush(Color.SeaShell) { _immutable = true };

	private static Brush? _sienna;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Sienna.</summary>
	public static Brush Sienna => _sienna ??= new SolidBrush(Color.Sienna) { _immutable = true };

	private static Brush? _silver;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Silver.</summary>
	public static Brush Silver => _silver ??= new SolidBrush(Color.Silver) { _immutable = true };

	private static Brush? _skyBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SkyBlue.</summary>
	public static Brush SkyBlue => _skyBlue ??= new SolidBrush(Color.SkyBlue) { _immutable = true };

	private static Brush? _slateBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SlateBlue.</summary>
	public static Brush SlateBlue => _slateBlue ??= new SolidBrush(Color.SlateBlue) { _immutable = true };

	private static Brush? _slateGray;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SlateGray.</summary>
	public static Brush SlateGray => _slateGray ??= new SolidBrush(Color.SlateGray) { _immutable = true };

	private static Brush? _snow;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Snow.</summary>
	public static Brush Snow => _snow ??= new SolidBrush(Color.Snow) { _immutable = true };

	private static Brush? _springGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SpringGreen.</summary>
	public static Brush SpringGreen => _springGreen ??= new SolidBrush(Color.SpringGreen) { _immutable = true };

	private static Brush? _steelBlue;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color SteelBlue.</summary>
	public static Brush SteelBlue => _steelBlue ??= new SolidBrush(Color.SteelBlue) { _immutable = true };

	private static Brush? _tan;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Tan.</summary>
	public static Brush Tan => _tan ??= new SolidBrush(Color.Tan) { _immutable = true };

	private static Brush? _teal;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Teal.</summary>
	public static Brush Teal => _teal ??= new SolidBrush(Color.Teal) { _immutable = true };

	private static Brush? _thistle;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Thistle.</summary>
	public static Brush Thistle => _thistle ??= new SolidBrush(Color.Thistle) { _immutable = true };

	private static Brush? _tomato;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Tomato.</summary>
	public static Brush Tomato => _tomato ??= new SolidBrush(Color.Tomato) { _immutable = true };

	private static Brush? _transparent;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Transparent.</summary>
	public static Brush Transparent => _transparent ??= new SolidBrush(Color.Transparent) { _immutable = true };

	private static Brush? _turquoise;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Turquoise.</summary>
	public static Brush Turquoise => _turquoise ??= new SolidBrush(Color.Turquoise) { _immutable = true };

	private static Brush? _violet;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Violet.</summary>
	public static Brush Violet => _violet ??= new SolidBrush(Color.Violet) { _immutable = true };

	private static Brush? _wheat;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Wheat.</summary>
	public static Brush Wheat => _wheat ??= new SolidBrush(Color.Wheat) { _immutable = true };

	private static Brush? _white;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color White.</summary>
	public static Brush White => _white ??= new SolidBrush(Color.White) { _immutable = true };

	private static Brush? _whiteSmoke;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color WhiteSmoke.</summary>
	public static Brush WhiteSmoke => _whiteSmoke ??= new SolidBrush(Color.WhiteSmoke) { _immutable = true };

	private static Brush? _yellow;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color Yellow.</summary>
	public static Brush Yellow => _yellow ??= new SolidBrush(Color.Yellow) { _immutable = true };

	private static Brush? _yellowGreen;
	/// <summary>Gets a system-defined <see cref="Brush"/> object of the color YellowGreen.</summary>
	public static Brush YellowGreen => _yellowGreen ??= new SolidBrush(Color.YellowGreen) { _immutable = true };
}
