using System;
using System.Linq;

namespace SkiaSharp.Extended.Svg
{
    internal struct SKCircle
    {
        public SKPoint Center { get; }

        public float Radius { get; }

        public SKCircle(SKPoint center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
