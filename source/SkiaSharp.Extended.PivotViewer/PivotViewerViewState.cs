namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Mutable view state for PivotViewer rendering. Shared between MAUI/Blazor and the renderer.
    /// Contains UI state that is not part of the core controller (visual-only concerns).
    /// </summary>
    public class PivotViewerViewState
    {
        /// <summary>Item currently under the pointer (for hover highlight).</summary>
        public PivotViewerItem? HoverItem { get; set; }
    }
}
