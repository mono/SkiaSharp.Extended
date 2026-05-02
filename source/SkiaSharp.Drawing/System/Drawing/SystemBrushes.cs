namespace System.Drawing
{
	/// <summary>
	///  Each property of the <see cref="SystemBrushes"/> class is a <see cref="SolidBrush"/>
	///  that is the color of a Windows display element.
	/// </summary>
	public static partial class SystemBrushes
	{
		private static Brush? _activeBorder;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ActiveBorder display element.</summary>
		public static Brush ActiveBorder => _activeBorder ??= new SolidBrush(SystemColors.ActiveBorder);

		private static Brush? _activeCaption;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ActiveCaption display element.</summary>
		public static Brush ActiveCaption => _activeCaption ??= new SolidBrush(SystemColors.ActiveCaption);

		private static Brush? _activeCaptionText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ActiveCaptionText display element.</summary>
		public static Brush ActiveCaptionText => _activeCaptionText ??= new SolidBrush(SystemColors.ActiveCaptionText);

		private static Brush? _appWorkspace;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the AppWorkspace display element.</summary>
		public static Brush AppWorkspace => _appWorkspace ??= new SolidBrush(SystemColors.AppWorkspace);

		private static Brush? _buttonFace;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ButtonFace display element.</summary>
		public static Brush ButtonFace => _buttonFace ??= new SolidBrush(SystemColors.ButtonFace);

		private static Brush? _buttonHighlight;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ButtonHighlight display element.</summary>
		public static Brush ButtonHighlight => _buttonHighlight ??= new SolidBrush(SystemColors.ButtonHighlight);

		private static Brush? _buttonShadow;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ButtonShadow display element.</summary>
		public static Brush ButtonShadow => _buttonShadow ??= new SolidBrush(SystemColors.ButtonShadow);

		private static Brush? _control;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the Control display element.</summary>
		public static Brush Control => _control ??= new SolidBrush(SystemColors.Control);

		private static Brush? _controlDark;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ControlDark display element.</summary>
		public static Brush ControlDark => _controlDark ??= new SolidBrush(SystemColors.ControlDark);

		private static Brush? _controlDarkDark;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ControlDarkDark display element.</summary>
		public static Brush ControlDarkDark => _controlDarkDark ??= new SolidBrush(SystemColors.ControlDarkDark);

		private static Brush? _controlLight;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ControlLight display element.</summary>
		public static Brush ControlLight => _controlLight ??= new SolidBrush(SystemColors.ControlLight);

		private static Brush? _controlLightLight;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ControlLightLight display element.</summary>
		public static Brush ControlLightLight => _controlLightLight ??= new SolidBrush(SystemColors.ControlLightLight);

		private static Brush? _controlText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ControlText display element.</summary>
		public static Brush ControlText => _controlText ??= new SolidBrush(SystemColors.ControlText);

		private static Brush? _desktop;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the Desktop display element.</summary>
		public static Brush Desktop => _desktop ??= new SolidBrush(SystemColors.Desktop);

		private static Brush? _gradientActiveCaption;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the GradientActiveCaption display element.</summary>
		public static Brush GradientActiveCaption => _gradientActiveCaption ??= new SolidBrush(SystemColors.GradientActiveCaption);

		private static Brush? _gradientInactiveCaption;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the GradientInactiveCaption display element.</summary>
		public static Brush GradientInactiveCaption => _gradientInactiveCaption ??= new SolidBrush(SystemColors.GradientInactiveCaption);

		private static Brush? _grayText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the GrayText display element.</summary>
		public static Brush GrayText => _grayText ??= new SolidBrush(SystemColors.GrayText);

		private static Brush? _highlight;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the Highlight display element.</summary>
		public static Brush Highlight => _highlight ??= new SolidBrush(SystemColors.Highlight);

		private static Brush? _highlightText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the HighlightText display element.</summary>
		public static Brush HighlightText => _highlightText ??= new SolidBrush(SystemColors.HighlightText);

		private static Brush? _hotTrack;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the HotTrack display element.</summary>
		public static Brush HotTrack => _hotTrack ??= new SolidBrush(SystemColors.HotTrack);

		private static Brush? _inactiveBorder;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the InactiveBorder display element.</summary>
		public static Brush InactiveBorder => _inactiveBorder ??= new SolidBrush(SystemColors.InactiveBorder);

		private static Brush? _inactiveCaption;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the InactiveCaption display element.</summary>
		public static Brush InactiveCaption => _inactiveCaption ??= new SolidBrush(SystemColors.InactiveCaption);

		private static Brush? _inactiveCaptionText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the InactiveCaptionText display element.</summary>
		public static Brush InactiveCaptionText => _inactiveCaptionText ??= new SolidBrush(SystemColors.InactiveCaptionText);

		private static Brush? _info;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the Info display element.</summary>
		public static Brush Info => _info ??= new SolidBrush(SystemColors.Info);

		private static Brush? _infoText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the InfoText display element.</summary>
		public static Brush InfoText => _infoText ??= new SolidBrush(SystemColors.InfoText);

		private static Brush? _menu;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the Menu display element.</summary>
		public static Brush Menu => _menu ??= new SolidBrush(SystemColors.Menu);

		private static Brush? _menuBar;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the MenuBar display element.</summary>
		public static Brush MenuBar => _menuBar ??= new SolidBrush(SystemColors.MenuBar);

		private static Brush? _menuHighlight;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the MenuHighlight display element.</summary>
		public static Brush MenuHighlight => _menuHighlight ??= new SolidBrush(SystemColors.MenuHighlight);

		private static Brush? _menuText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the MenuText display element.</summary>
		public static Brush MenuText => _menuText ??= new SolidBrush(SystemColors.MenuText);

		private static Brush? _scrollBar;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the ScrollBar display element.</summary>
		public static Brush ScrollBar => _scrollBar ??= new SolidBrush(SystemColors.ScrollBar);

		private static Brush? _window;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the Window display element.</summary>
		public static Brush Window => _window ??= new SolidBrush(SystemColors.Window);

		private static Brush? _windowFrame;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the WindowFrame display element.</summary>
		public static Brush WindowFrame => _windowFrame ??= new SolidBrush(SystemColors.WindowFrame);

		private static Brush? _windowText;
		/// <summary>Gets a <see cref="SolidBrush"/> that is the color of the WindowText display element.</summary>
		public static Brush WindowText => _windowText ??= new SolidBrush(SystemColors.WindowText);

		/// <summary>
		///  Creates a <see cref="SolidBrush"/> from the specified <see cref="Color"/>.
		/// </summary>
		/// <param name="c">The <see cref="Color"/> from which to create the <see cref="SolidBrush"/>.</param>
		/// <returns>A <see cref="SolidBrush"/> that represents the specified color.</returns>
		public static Brush FromSystemColor(Color c) => new SolidBrush(c);
	}
}
