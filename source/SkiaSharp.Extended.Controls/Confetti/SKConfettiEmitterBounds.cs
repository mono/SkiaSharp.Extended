using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	[TypeConverter(typeof(Converters.SKConfettiEmitterBoundsTypeConverter))]
	public struct SKConfettiEmitterBounds
	{
		public SKConfettiEmitterBounds(SKConfettiEmitterSide side)
			: this(Rect.Zero, side)
		{
		}

		public SKConfettiEmitterBounds(Rect rect)
			: this(rect, SKConfettiEmitterSide.Bounds)
		{
		}

		public SKConfettiEmitterBounds(Rect rect, SKConfettiEmitterSide side)
		{
			Rect = rect;
			Side = side;
		}

		public Rect Rect { get; }

		public SKConfettiEmitterSide Side { get; }

		public static SKConfettiEmitterBounds Top =>
			new SKConfettiEmitterBounds(SKConfettiEmitterSide.Top);

		public static SKConfettiEmitterBounds Left =>
			new SKConfettiEmitterBounds(SKConfettiEmitterSide.Left);

		public static SKConfettiEmitterBounds Right =>
			new SKConfettiEmitterBounds(SKConfettiEmitterSide.Right);

		public static SKConfettiEmitterBounds Bottom =>
			new SKConfettiEmitterBounds(SKConfettiEmitterSide.Bottom);

		public static SKConfettiEmitterBounds Center =>
			new SKConfettiEmitterBounds(SKConfettiEmitterSide.Center);

		public static SKConfettiEmitterBounds Bounds(Rect rect) =>
			new SKConfettiEmitterBounds(rect);

		public static SKConfettiEmitterBounds Point(double x, double y) =>
			Point(new Point(x, y));

		public static SKConfettiEmitterBounds Point(Point point) =>
			new SKConfettiEmitterBounds(new Rect(point, Size.Zero));
	}
}
