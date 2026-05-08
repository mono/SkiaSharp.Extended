namespace System.Drawing;

/// <summary>
///  Each property of the <see cref="SystemPens"/> class is a <see cref="Pen"/>
///  that is the color of a Windows display element and a width of 1 pixel.
/// </summary>
public static partial class SystemPens
{
	private static Pen? _activeBorder;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ActiveBorder display element.</summary>
	public static Pen ActiveBorder => _activeBorder ??= new Pen(SystemColors.ActiveBorder) { _immutable = true };

	private static Pen? _activeCaption;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ActiveCaption display element.</summary>
	public static Pen ActiveCaption => _activeCaption ??= new Pen(SystemColors.ActiveCaption) { _immutable = true };

	private static Pen? _activeCaptionText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ActiveCaptionText display element.</summary>
	public static Pen ActiveCaptionText => _activeCaptionText ??= new Pen(SystemColors.ActiveCaptionText) { _immutable = true };

	private static Pen? _appWorkspace;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the AppWorkspace display element.</summary>
	public static Pen AppWorkspace => _appWorkspace ??= new Pen(SystemColors.AppWorkspace) { _immutable = true };

	private static Pen? _buttonFace;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ButtonFace display element.</summary>
	public static Pen ButtonFace => _buttonFace ??= new Pen(SystemColors.ButtonFace) { _immutable = true };

	private static Pen? _buttonHighlight;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ButtonHighlight display element.</summary>
	public static Pen ButtonHighlight => _buttonHighlight ??= new Pen(SystemColors.ButtonHighlight) { _immutable = true };

	private static Pen? _buttonShadow;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ButtonShadow display element.</summary>
	public static Pen ButtonShadow => _buttonShadow ??= new Pen(SystemColors.ButtonShadow) { _immutable = true };

	private static Pen? _control;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the Control display element.</summary>
	public static Pen Control => _control ??= new Pen(SystemColors.Control) { _immutable = true };

	private static Pen? _controlDark;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ControlDark display element.</summary>
	public static Pen ControlDark => _controlDark ??= new Pen(SystemColors.ControlDark) { _immutable = true };

	private static Pen? _controlDarkDark;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ControlDarkDark display element.</summary>
	public static Pen ControlDarkDark => _controlDarkDark ??= new Pen(SystemColors.ControlDarkDark) { _immutable = true };

	private static Pen? _controlLight;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ControlLight display element.</summary>
	public static Pen ControlLight => _controlLight ??= new Pen(SystemColors.ControlLight) { _immutable = true };

	private static Pen? _controlLightLight;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ControlLightLight display element.</summary>
	public static Pen ControlLightLight => _controlLightLight ??= new Pen(SystemColors.ControlLightLight) { _immutable = true };

	private static Pen? _controlText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ControlText display element.</summary>
	public static Pen ControlText => _controlText ??= new Pen(SystemColors.ControlText) { _immutable = true };

	private static Pen? _desktop;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the Desktop display element.</summary>
	public static Pen Desktop => _desktop ??= new Pen(SystemColors.Desktop) { _immutable = true };

	private static Pen? _gradientActiveCaption;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the GradientActiveCaption display element.</summary>
	public static Pen GradientActiveCaption => _gradientActiveCaption ??= new Pen(SystemColors.GradientActiveCaption) { _immutable = true };

	private static Pen? _gradientInactiveCaption;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the GradientInactiveCaption display element.</summary>
	public static Pen GradientInactiveCaption => _gradientInactiveCaption ??= new Pen(SystemColors.GradientInactiveCaption) { _immutable = true };

	private static Pen? _grayText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the GrayText display element.</summary>
	public static Pen GrayText => _grayText ??= new Pen(SystemColors.GrayText) { _immutable = true };

	private static Pen? _highlight;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the Highlight display element.</summary>
	public static Pen Highlight => _highlight ??= new Pen(SystemColors.Highlight) { _immutable = true };

	private static Pen? _highlightText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the HighlightText display element.</summary>
	public static Pen HighlightText => _highlightText ??= new Pen(SystemColors.HighlightText) { _immutable = true };

	private static Pen? _hotTrack;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the HotTrack display element.</summary>
	public static Pen HotTrack => _hotTrack ??= new Pen(SystemColors.HotTrack) { _immutable = true };

	private static Pen? _inactiveBorder;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the InactiveBorder display element.</summary>
	public static Pen InactiveBorder => _inactiveBorder ??= new Pen(SystemColors.InactiveBorder) { _immutable = true };

	private static Pen? _inactiveCaption;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the InactiveCaption display element.</summary>
	public static Pen InactiveCaption => _inactiveCaption ??= new Pen(SystemColors.InactiveCaption) { _immutable = true };

	private static Pen? _inactiveCaptionText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the InactiveCaptionText display element.</summary>
	public static Pen InactiveCaptionText => _inactiveCaptionText ??= new Pen(SystemColors.InactiveCaptionText) { _immutable = true };

	private static Pen? _info;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the Info display element.</summary>
	public static Pen Info => _info ??= new Pen(SystemColors.Info) { _immutable = true };

	private static Pen? _infoText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the InfoText display element.</summary>
	public static Pen InfoText => _infoText ??= new Pen(SystemColors.InfoText) { _immutable = true };

	private static Pen? _menu;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the Menu display element.</summary>
	public static Pen Menu => _menu ??= new Pen(SystemColors.Menu) { _immutable = true };

	private static Pen? _menuBar;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the MenuBar display element.</summary>
	public static Pen MenuBar => _menuBar ??= new Pen(SystemColors.MenuBar) { _immutable = true };

	private static Pen? _menuHighlight;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the MenuHighlight display element.</summary>
	public static Pen MenuHighlight => _menuHighlight ??= new Pen(SystemColors.MenuHighlight) { _immutable = true };

	private static Pen? _menuText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the MenuText display element.</summary>
	public static Pen MenuText => _menuText ??= new Pen(SystemColors.MenuText) { _immutable = true };

	private static Pen? _scrollBar;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the ScrollBar display element.</summary>
	public static Pen ScrollBar => _scrollBar ??= new Pen(SystemColors.ScrollBar) { _immutable = true };

	private static Pen? _window;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the Window display element.</summary>
	public static Pen Window => _window ??= new Pen(SystemColors.Window) { _immutable = true };

	private static Pen? _windowFrame;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the WindowFrame display element.</summary>
	public static Pen WindowFrame => _windowFrame ??= new Pen(SystemColors.WindowFrame) { _immutable = true };

	private static Pen? _windowText;
	/// <summary>Gets a <see cref="Pen"/> that is the color of the WindowText display element.</summary>
	public static Pen WindowText => _windowText ??= new Pen(SystemColors.WindowText) { _immutable = true };

	/// <summary>
	///  Creates a <see cref="Pen"/> from the specified <see cref="Color"/>.
	/// </summary>
	/// <param name="c">The <see cref="Color"/> from which to create the <see cref="Pen"/>.</param>
	/// <returns>A <see cref="Pen"/> that represents the specified color.</returns>
	public static Pen FromSystemColor(Color c) => new Pen(c);
}
