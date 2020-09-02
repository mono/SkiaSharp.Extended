using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	[TypeConverter(typeof(Converters.SKConfettiEmitterBoundsTypeConverter))]
	public readonly struct SKConfettiEmitterBounds
	{
		public SKConfettiEmitterBounds(SKConfettiEmitterSide side)
			: this(Rect.Zero, side)
		{
		}

		public SKConfettiEmitterBounds(double x, double y)
			: this(new Rect(x, y, 0, 0), SKConfettiEmitterSide.Bounds)
		{
		}

		public SKConfettiEmitterBounds(Point point)
			: this(new Rect(point, Size.Zero), SKConfettiEmitterSide.Bounds)
		{
		}

		public SKConfettiEmitterBounds(double x, double y, double width, double height)
			: this(new Rect(x, y, width, height), SKConfettiEmitterSide.Bounds)
		{
		}

		public SKConfettiEmitterBounds(Rect rect)
			: this(rect, SKConfettiEmitterSide.Bounds)
		{
		}

		private SKConfettiEmitterBounds(Rect rect, SKConfettiEmitterSide side)
		{
			Rect = rect;
			Side = side;
		}

		//

		public Rect Rect { get; }

		public SKConfettiEmitterSide Side { get; }

		//

		public static implicit operator SKConfettiEmitterBounds(SKConfettiEmitterSide side) =>
			new SKConfettiEmitterBounds(side);

		public static implicit operator SKConfettiEmitterBounds(Point point) =>
			new SKConfettiEmitterBounds(point);

		public static implicit operator SKConfettiEmitterBounds(Rect rect) =>
			new SKConfettiEmitterBounds(rect);

		//

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

		public static SKConfettiEmitterBounds Bounds(double x, double y, double width, double height) =>
			new SKConfettiEmitterBounds(x, y, width, height);

		public static SKConfettiEmitterBounds Bounds(Rect rect) =>
			new SKConfettiEmitterBounds(rect);

		public static SKConfettiEmitterBounds Point(double x, double y) =>
			new SKConfettiEmitterBounds(x, y);

		public static SKConfettiEmitterBounds Point(Point point) =>
			new SKConfettiEmitterBounds(point);
	}
}
