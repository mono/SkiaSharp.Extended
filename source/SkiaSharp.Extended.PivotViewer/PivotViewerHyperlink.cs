using System;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Represents a hyperlink value in PivotViewer.
    /// Matches Silverlight SL5 PivotViewerHyperlink (Text, Uri).
    /// </summary>
    public class PivotViewerHyperlink : IComparable<PivotViewerHyperlink>, IComparable, IEquatable<PivotViewerHyperlink>
    {
        public PivotViewerHyperlink(string text, Uri uri)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        /// <summary>Display text for the link.</summary>
        public string Text { get; }

        /// <summary>Link target URI.</summary>
        public Uri Uri { get; }

        public int CompareTo(PivotViewerHyperlink? other)
        {
            if (other == null) return 1;
            int cmp = string.Compare(Text, other.Text, StringComparison.Ordinal);
            if (cmp != 0) return cmp;
            return string.Compare(Uri.ToString(), other.Uri.ToString(), StringComparison.Ordinal);
        }

        public int CompareTo(object? obj) => CompareTo(obj as PivotViewerHyperlink);

        public bool Equals(PivotViewerHyperlink? other)
        {
            if (other is null) return false;
            return Text == other.Text && Uri == other.Uri;
        }

        public override bool Equals(object? obj) => Equals(obj as PivotViewerHyperlink);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Text.GetHashCode() * 397) ^ Uri.GetHashCode();
            }
        }

        public override string ToString() => $"{Text} ({Uri})";

        public static bool operator ==(PivotViewerHyperlink? left, PivotViewerHyperlink? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(PivotViewerHyperlink? left, PivotViewerHyperlink? right) =>
            !(left == right);
    }
}
