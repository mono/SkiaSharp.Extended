using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	[TypeConverter(typeof(SKConfettiSystemBoundsTypeConverter))]
	public struct SKConfettiSystemBounds
	{
		public SKConfettiSystemBounds(SKConfettiSystemSide side)
			: this(Rect.Zero, side)
		{
		}

		public SKConfettiSystemBounds(Rect rect)
			: this(rect, SKConfettiSystemSide.Bounds)
		{
		}

		public SKConfettiSystemBounds(Rect rect, SKConfettiSystemSide side)
		{
			Rect = rect;
			Side = side;
		}

		public Rect Rect { get; }

		public SKConfettiSystemSide Side { get; }

		public static SKConfettiSystemBounds Top =>
			new SKConfettiSystemBounds(SKConfettiSystemSide.Top);

		public static SKConfettiSystemBounds Left =>
			new SKConfettiSystemBounds(SKConfettiSystemSide.Left);

		public static SKConfettiSystemBounds Right =>
			new SKConfettiSystemBounds(SKConfettiSystemSide.Right);

		public static SKConfettiSystemBounds Bottom =>
			new SKConfettiSystemBounds(SKConfettiSystemSide.Bottom);

		public static SKConfettiSystemBounds Center =>
			new SKConfettiSystemBounds(SKConfettiSystemSide.Center);

		public static SKConfettiSystemBounds Bounds(Rect rect) =>
			new SKConfettiSystemBounds(rect);

		public static SKConfettiSystemBounds Location(Point point) =>
			new SKConfettiSystemBounds(new Rect(point, Size.Zero));
	}
}
