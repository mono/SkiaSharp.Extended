using SkiaSharp;
using System;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Shared rendering utilities for PivotViewer, usable by both MAUI and Blazor renderers.
    /// </summary>
    public static class RenderUtils
    {
        /// <summary>
        /// Computes a destination rect that fits source dimensions uniformly into a target rect,
        /// preserving aspect ratio with letterbox/pillarbox.
        /// </summary>
        public static SKRect FitUniform(float srcWidth, float srcHeight, SKRect dest)
        {
            if (srcWidth <= 0 || srcHeight <= 0)
                return dest;

            float srcAspect = srcWidth / srcHeight;
            float destAspect = dest.Width / dest.Height;

            float fitWidth, fitHeight;
            if (srcAspect > destAspect)
            {
                // Source is wider — fit to width, letterbox top/bottom
                fitWidth = dest.Width;
                fitHeight = dest.Width / srcAspect;
            }
            else
            {
                // Source is taller — fit to height, pillarbox left/right
                fitHeight = dest.Height;
                fitWidth = dest.Height * srcAspect;
            }

            float x = dest.Left + (dest.Width - fitWidth) / 2;
            float y = dest.Top + (dest.Height - fitHeight) / 2;
            return new SKRect(x, y, x + fitWidth, y + fitHeight);
        }

        /// <summary>
        /// Truncates text to fit within maxWidth pixels, appending "…" if needed.
        /// </summary>
        public static string TruncateText(string text, SKFont font, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            float textWidth = font.MeasureText(text, out _);
            if (textWidth <= maxWidth)
                return text;

            // Binary search for the longest prefix that fits with ellipsis
            string ellipsis = "…";
            float ellipsisWidth = font.MeasureText(ellipsis, out _);
            float available = maxWidth - ellipsisWidth;
            if (available <= 0)
                return ellipsis;

            int lo = 0, hi = text.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                float w = font.MeasureText(text.AsSpan(0, mid), out _);
                if (w <= available)
                    lo = mid;
                else
                    hi = mid - 1;
            }

            return lo > 0 ? text.Substring(0, lo) + ellipsis : ellipsis;
        }

        /// <summary>
        /// Gets the display name for an item, falling back to item ID.
        /// </summary>
        public static string GetItemDisplayName(PivotViewerItem item)
        {
            var name = item["Name"];
            if (name != null && name.Count > 0 && name[0] != null)
                return name[0]!.ToString()!;
            return item.Id;
        }
    }
}
