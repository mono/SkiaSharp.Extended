using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.PivotViewer
{
    // CxmlCollectionStateChangedEventArgs is in CxmlCollectionSource.cs

    /// <summary>Event args for item double-click.</summary>
    public class PivotViewerItemDoubleClickEventArgs : EventArgs
    {
        public PivotViewerItemDoubleClickEventArgs(PivotViewerItem item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public PivotViewerItem Item { get; }
    }

    /// <summary>Event args for link navigation.</summary>
    public class PivotViewerLinkEventArgs : EventArgs
    {
        public PivotViewerLinkEventArgs(Uri link)
        {
            Link = link ?? throw new ArgumentNullException(nameof(link));
        }

        public Uri Link { get; }
        public bool Handled { get; set; }
    }

    /// <summary>Event args for filter changes.</summary>
    public class PivotViewerFilterEventArgs : EventArgs
    {
        public PivotViewerFilterEventArgs(string filter)
        {
            Filter = filter ?? "";
        }

        public string Filter { get; }
        public bool Handled { get; set; }
    }

    /// <summary>Event args for view updates.</summary>
    public class PivotViewerViewUpdatingEventArgs : EventArgs
    {
        public PivotViewerViewUpdatingEventArgs(PivotViewerProperty? sortProperty, string? source)
        {
            SortPivotProperty = sortProperty;
            Source = source;
        }

        public PivotViewerProperty? SortPivotProperty { get; }
        public string? Source { get; }
    }

    /// <summary>Event args for item adorner commands (Silverlight pattern).</summary>
    public class PivotViewerCommandsRequestedEventArgs : EventArgs
    {
        public PivotViewerCommandsRequestedEventArgs(PivotViewerItem item, bool isSelected)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            IsItemSelected = isSelected;
            Commands = new List<IPivotViewerUICommand>();
        }

        public PivotViewerItem Item { get; }
        public bool IsItemSelected { get; }
        public IList<IPivotViewerUICommand> Commands { get; }
    }

    /// <summary>
    /// Interface for custom commands on item adorners.
    /// Matches Silverlight's IPivotViewerUICommand.
    /// </summary>
    public interface IPivotViewerUICommand
    {
        string DisplayName { get; }
        Uri? Icon { get; }
        object? ToolTip { get; }
        bool CanExecute(object? parameter);
        void Execute(object? parameter);
    }
}
