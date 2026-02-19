using SkiaSharp;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Color theme for PivotViewer rendering. Uses SKColor directly — no MAUI dependency.
    /// The MAUI layer converts MAUI Colors to this theme once on property change.
    /// </summary>
    public class PivotViewerTheme
    {
        /// <summary>Accent color for buttons, highlights, active elements.</summary>
        public SKColor AccentColor { get; set; } = new SKColor(100, 149, 237); // CornflowerBlue

        /// <summary>Control bar and filter pane background.</summary>
        public SKColor ControlBackground { get; set; } = new SKColor(45, 45, 48);

        /// <summary>Detail pane background.</summary>
        public SKColor SecondaryBackground { get; set; } = new SKColor(240, 240, 240);

        /// <summary>Secondary text color for subtitles, counts.</summary>
        public SKColor SecondaryForeground { get; set; } = new SKColor(128, 128, 128);

        /// <summary>Primary text color.</summary>
        public SKColor ForegroundColor { get; set; } = SKColors.Black;

        /// <summary>Text color on dark backgrounds (control bar, filter pane).</summary>
        public SKColor LightForegroundColor { get; set; } = SKColors.White;

        /// <summary>Item fallback color when no thumbnail.</summary>
        public SKColor ItemFallbackColor { get; set; } = new SKColor(100, 149, 237);

        /// <summary>Selection highlight color.</summary>
        public SKColor SelectionColor { get; set; } = SKColors.Orange;

        /// <summary>Hover highlight color.</summary>
        public SKColor HoverColor { get; set; } = new SKColor(255, 200, 0, 100);

        /// <summary>Creates a default theme.</summary>
        public static PivotViewerTheme Default => new PivotViewerTheme();
    }
}
