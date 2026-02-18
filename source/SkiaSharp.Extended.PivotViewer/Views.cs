using System;
using System.ComponentModel;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Abstract base class for PivotViewer views.
    /// Matches Silverlight's PivotViewerView.
    /// </summary>
    public abstract class PivotViewerView : INotifyPropertyChanged
    {
        private string _name = "";
        private string _id = "";
        private string? _toolTip;
        private bool _isAvailable = true;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string? ToolTip
        {
            get => _toolTip;
            set { _toolTip = value; OnPropertyChanged(nameof(ToolTip)); }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set { _isAvailable = value; OnPropertyChanged(nameof(IsAvailable)); }
        }

        public event EventHandler<PivotViewerViewUpdatingEventArgs>? PivotViewerViewUpdating;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPivotViewerViewUpdating(PivotViewerViewUpdatingEventArgs e)
            => PivotViewerViewUpdating?.Invoke(this, e);

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Grid view: items in a responsive grid, zoom changes item size.
    /// Matches Silverlight's PivotViewerGridView.
    /// </summary>
    public sealed class PivotViewerGridView : PivotViewerView
    {
        public PivotViewerGridView()
        {
            Id = "GridView";
            Name = "Grid";
        }
    }

    /// <summary>
    /// Graph/histogram view: items grouped by facet into histogram bars.
    /// Matches Silverlight's PivotViewerGraphView.
    /// </summary>
    public sealed class PivotViewerGraphView : PivotViewerView
    {
        private PivotViewerProperty? _groupByProperty;
        private PivotViewerProperty? _stackByProperty;

        public PivotViewerGraphView()
        {
            Id = "GraphView";
            Name = "Graph";
        }

        /// <summary>Property to group items by (X-axis categories).</summary>
        public PivotViewerProperty? GroupByProperty
        {
            get => _groupByProperty;
            set { _groupByProperty = value; OnPropertyChanged(nameof(GroupByProperty)); }
        }

        /// <summary>Optional property to stack/color items within each group.</summary>
        public PivotViewerProperty? StackByProperty
        {
            get => _stackByProperty;
            set { _stackByProperty = value; OnPropertyChanged(nameof(StackByProperty)); }
        }
    }
}
