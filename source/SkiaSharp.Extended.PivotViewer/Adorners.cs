using System;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Base class for item adorners that provide hover/selected visual decorations.
    /// Matches Silverlight's PivotViewerItemAdorner pattern.
    /// </summary>
    public abstract class PivotViewerItemAdorner
    {
        public bool IsMouseOver { get; set; }
        public bool IsItemSelected { get; set; }
    }

    /// <summary>
    /// Default adorner with context commands support.
    /// In Silverlight, CommandsRequested lives here (NOT on the main PivotViewer control).
    /// </summary>
    public class PivotViewerDefaultItemAdorner : PivotViewerItemAdorner
    {
        /// <summary>
        /// Raised when the adorner needs to populate its command list.
        /// This is where consumers add IPivotViewerUICommand instances.
        /// </summary>
        public event EventHandler<PivotViewerCommandsRequestedEventArgs>? CommandsRequested;

        public void RequestCommands(PivotViewerItem item, bool isSelected)
        {
            var args = new PivotViewerCommandsRequestedEventArgs(item, isSelected);
            CommandsRequested?.Invoke(this, args);
        }
    }
}
