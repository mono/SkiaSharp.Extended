using System;
using System.Xml.Linq;

namespace SkiaSharp.Extended.Svg
{
	internal class SKSvgMask
	{
		public SKSvgMask(SKPaint stroke, SKPaint fill, XElement element)
		{
			Stroke = stroke?.Clone();
			Fill = fill?.Clone();
			Element = element;
		}

		public SKPaint Stroke { get; }

		public SKPaint Fill { get; }

		public XElement Element { get; }
	}
}
