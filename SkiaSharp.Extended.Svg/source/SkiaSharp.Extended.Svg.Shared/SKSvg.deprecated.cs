using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use 'SkiaSharp.Extended.Svg.SKSvg' instead. This type will be removed in the future.")]
	public class SKSvg : SkiaSharp.Extended.Svg.SKSvg
	{
		public SKSvg()
			: base()
		{
		}

		public SKSvg(float pixelsPerInch)
			: base(pixelsPerInch)
		{
		}

		public SKSvg(SKSize canvasSize)
			: base(canvasSize)
		{
		}

		public SKSvg(float pixelsPerInch, SKSize canvasSize)
			: base(pixelsPerInch, canvasSize)
		{
		}
	}
}
