using System;
using System.Collections.ObjectModel;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Represents a template for rendering items at a specific zoom level.
    /// Matches Silverlight's PivotViewerItemTemplate.
    /// </summary>
    public class PivotViewerItemTemplate
    {
        /// <summary>Maximum width (in pixels) at which this template is used.</summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// Callback to render an item using SkiaSharp.
        /// Parameters: (canvas, item, bounds)
        /// </summary>
        public Action<SKCanvas, PivotViewerItem, SKRect>? RenderAction { get; set; }
    }

    /// <summary>
    /// Collection of item templates for different zoom levels.
    /// Matches Silverlight's PivotViewerItemTemplateCollection.
    /// </summary>
    public class PivotViewerItemTemplateCollection : ObservableCollection<PivotViewerItemTemplate>
    {
        /// <summary>
        /// Select the best template for the given item width.
        /// Returns the template with the smallest MaxWidth that is >= itemWidth,
        /// or the largest template if none are large enough.
        /// </summary>
        public PivotViewerItemTemplate? SelectTemplate(int itemWidth)
        {
            if (Count == 0) return null;

            PivotViewerItemTemplate? best = null;
            PivotViewerItemTemplate? largest = null;

            foreach (var template in this)
            {
                if (largest == null || template.MaxWidth > largest.MaxWidth)
                    largest = template;

                if (template.MaxWidth >= itemWidth)
                {
                    if (best == null || template.MaxWidth < best.MaxWidth)
                        best = template;
                }
            }

            return best ?? largest;
        }
    }

    /// <summary>
    /// Multi-size image that selects the best source for the current zoom level.
    /// Matches Silverlight's PivotViewerMultiSizeImage.
    /// </summary>
    public class PivotViewerMultiSizeImage
    {
        public PivotViewerMultiSizeImageSourceCollection Sources { get; } = new PivotViewerMultiSizeImageSourceCollection();
    }

    /// <summary>
    /// A single image source for a specific size range.
    /// Matches Silverlight's PivotViewerMultiSizeImageSource.
    /// </summary>
    public class PivotViewerMultiSizeImageSource
    {
        public string? UriSource { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
    }

    public class PivotViewerMultiSizeImageSourceCollection : ObservableCollection<PivotViewerMultiSizeImageSource>
    {
    }

    /// <summary>
    /// Renders the correct sub-image from a DZC mosaic.
    /// Placed inside item templates that use collection images.
    /// Matches Silverlight's PivotViewerMultiScaleSubImageHost.
    /// </summary>
    public class PivotViewerMultiScaleSubImageHost
    {
        /// <summary>The CXML collection source.</summary>
        public CxmlCollectionSource? CollectionSource { get; set; }

        /// <summary>Image ID (from CXML Item Img attribute, e.g. "#5").</summary>
        public string? ImageId { get; set; }
    }
}
