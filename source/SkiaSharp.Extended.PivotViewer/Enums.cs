using System;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Flags controlling how a property is displayed and used in the PivotViewer UI.
    /// Matches Silverlight SL5 PivotViewerPropertyOptions exactly.
    /// </summary>
    [Flags]
    public enum PivotViewerPropertyOptions
    {
        /// <summary>No special options.</summary>
        None = 0,

        /// <summary>Property is private and hidden from the UI.</summary>
        Private = 1,

        /// <summary>Property appears in the filter pane.</summary>
        CanFilter = 2,

        /// <summary>Property is included in word wheel / search index.</summary>
        CanSearchText = 4,

        /// <summary>Property text should wrap (used for LongString CXML type).</summary>
        WrappingText = 8
    }

    /// <summary>
    /// The data type of a PivotViewer property.
    /// Matches Silverlight SL5 PivotViewerPropertyType exactly.
    /// </summary>
    public enum PivotViewerPropertyType
    {
        /// <summary>Date/time values.</summary>
        DateTime,

        /// <summary>Numeric (decimal) values.</summary>
        Decimal,

        /// <summary>Text string values.</summary>
        Text,

        /// <summary>Hyperlink values.</summary>
        Link
    }

    /// <summary>
    /// State of CXML collection loading.
    /// Matches Silverlight SL5 CxmlCollectionState exactly.
    /// </summary>
    public enum CxmlCollectionState
    {
        /// <summary>Initial state before loading begins.</summary>
        Initialized,

        /// <summary>Currently loading data.</summary>
        Loading,

        /// <summary>Successfully loaded.</summary>
        Loaded,

        /// <summary>Loading failed.</summary>
        Failed
    }

    /// <summary>
    /// Describes how content is resized to fill its allocated space.
    /// Matches Silverlight's Stretch enum.
    /// </summary>
    public enum PivotViewerStretch
    {
        /// <summary>Content preserves its original size.</summary>
        None = 0,

        /// <summary>Content fills the destination, aspect ratio not preserved.</summary>
        Fill = 1,

        /// <summary>Content fits within the destination, preserving aspect ratio.</summary>
        Uniform = 2,

        /// <summary>Content fills the destination, preserving aspect ratio. Clips if necessary.</summary>
        UniformToFill = 3
    }
}
