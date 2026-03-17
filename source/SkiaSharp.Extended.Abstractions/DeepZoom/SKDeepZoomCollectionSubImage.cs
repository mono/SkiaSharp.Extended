#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Represents a single sub-image within a DZC collection.
    /// Maps to Silverlight's MultiScaleSubImage.
    /// </summary>
    public class SKDeepZoomCollectionSubImage
    {
        /// <summary>Initializes a new <see cref="SKDeepZoomCollectionSubImage"/>.</summary>
        public SKDeepZoomCollectionSubImage(int id, int mortonIndex, int width, int height, string? source)
        {
            Id = id;
            MortonIndex = mortonIndex;
            Width = width;
            Height = height;
            Source = source;
        }

        /// <summary>Item ID from the DZC.</summary>
        public int Id { get; }

        /// <summary>Morton (Z-order) index in the mosaic grid.</summary>
        public int MortonIndex { get; }

        /// <summary>Full image width in pixels.</summary>
        public int Width { get; }

        /// <summary>Full image height in pixels.</summary>
        public int Height { get; }

        /// <summary>Optional individual DZI source path.</summary>
        public string? Source { get; }

        /// <summary>Aspect ratio (width / height).</summary>
        public double AspectRatio => Height == 0 ? 1.0 : (double)Width / Height;

        /// <summary>Viewport width in the DZC mosaic coordinate system.</summary>
        public double ViewportWidth { get; set; }

        /// <summary>Viewport X origin in the DZC mosaic coordinate system.</summary>
        public double ViewportX { get; set; }

        /// <summary>Viewport Y origin in the DZC mosaic coordinate system.</summary>
        public double ViewportY { get; set; }
    }
}
