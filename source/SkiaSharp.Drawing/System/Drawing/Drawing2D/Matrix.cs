namespace System.Drawing.Drawing2D
{
    public sealed partial class Matrix : System.MarshalByRefObject, System.IDisposable
    {
        public Matrix() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public Matrix(System.Drawing.Rectangle rect, System.Drawing.Point[] plgpts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public Matrix(System.Drawing.RectangleF rect, System.Drawing.PointF[] plgpts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public float[] Elements { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public bool IsIdentity { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public bool IsInvertible { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public float OffsetX { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public float OffsetY { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public System.Drawing.Drawing2D.Matrix Clone() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Dispose() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public override bool Equals(object? obj) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public override int GetHashCode() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Invert() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Multiply(System.Drawing.Drawing2D.Matrix matrix) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Multiply(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Reset() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Rotate(float angle) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Rotate(float angle, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void RotateAt(float angle, System.Drawing.PointF point) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void RotateAt(float angle, System.Drawing.PointF point, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Scale(float scaleX, float scaleY) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Scale(float scaleX, float scaleY, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Shear(float shearX, float shearY) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Shear(float shearX, float shearY, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void TransformPoints(System.Drawing.PointF[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void TransformPoints(System.Drawing.Point[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void TransformVectors(System.Drawing.PointF[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void TransformVectors(System.Drawing.Point[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Translate(float offsetX, float offsetY) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void Translate(float offsetX, float offsetY, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public void VectorTransformPoints(System.Drawing.Point[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected ~Matrix() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
    }
}
