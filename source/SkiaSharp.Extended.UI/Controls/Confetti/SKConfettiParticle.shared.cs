namespace SkiaSharp.Extended.UI.Controls;

internal class SKConfettiParticle
{
	private static readonly SKObjectPool<SKPaint> paintPool =
		new SKObjectPool<SKPaint>(() => new SKPaint { IsAntialias = true });

	private SKPoint acceleration = SKPoint.Empty;
	private float rotationWidth = 0f;
	private float scaleX = 1f;

	private SKPoint location;
	private float size;

	public SKPoint Location
	{
		get => location;
		set
		{
			location = value;
			Bounds = new SKRect(location.X - size, location.Y - size, size * 2, size * 2);
		}
	}

	public float Size
	{
		get => size;
		set
		{
			size = value;
			Bounds = new SKRect(location.X - size, location.Y - size, size * 2, size * 2);
		}
	}

	public float Mass { get; set; }

	public float Rotation { get; set; }

	public SKColorF Color { get; set; }

	public SKConfettiShape? Shape { get; set; }

	public SKPoint Velocity { get; set; }

	public float RotationVelocity { get; set; }

	public SKPoint MaximumVelocity { get; set; }

	public bool FadeOut { get; set; }

	public double Lifetime { get; set; }

	public SKRect Bounds { get; private set; }

	public bool IsRunning { get; private set; } = true;

	public void Draw(SKCanvas canvas)
	{
		if (!IsRunning || Shape == null)
			return;

		canvas.Save();
		canvas.Translate(Location);

		if (Rotation != 0)
		{
			canvas.RotateDegrees(Rotation);
			canvas.Scale(scaleX, 1f);
		}

		var paint = paintPool.Get();
		paint.Reset();
		paint.ColorF = Color;

		Shape.Draw(canvas, paint, Size);

		paintPool.Return(paint);

		canvas.Restore();
	}

	public void ApplyForce(SKPoint force, TimeSpan deltaTime)
	{
		if (!IsRunning)
			return;

		var secs = (float)deltaTime.TotalSeconds;
		force.X = (force.X / Mass) * secs;
		force.Y = (force.Y / Mass) * secs;

		if (force != SKPoint.Empty)
			acceleration += force;

		Velocity += acceleration;
		if (MaximumVelocity != SKPoint.Empty)
		{
			var vx = Velocity.X;
			var vy = Velocity.Y;

			vx = vx < 0
				? Math.Max(vx, -MaximumVelocity.X)
				: Math.Min(vx, MaximumVelocity.X);
			vy = vy < 0
				? Math.Max(vy, -MaximumVelocity.Y)
				: Math.Min(vy, MaximumVelocity.Y);

			Velocity = new SKPoint(vx, vy);
		}

		Location = new SKPoint(
			Location.X + Velocity.X * secs,
			Location.Y + Velocity.Y * secs);

		Lifetime -= deltaTime.TotalSeconds;
		if (Lifetime <= 0)
		{
			if (FadeOut)
			{
				var c = Color;
				var alpha = c.Alpha - secs;
				Color = c.WithAlpha(alpha);
				IsRunning = alpha > 0;
			}
			else
			{
				IsRunning = false;
			}
		}

		if (RotationVelocity != 0)
		{
			var rv = RotationVelocity * secs;

			Rotation += rv;
			if (Rotation >= 360)
				Rotation = 0f;

			rotationWidth -= rv;
			if (rotationWidth < 0)
				rotationWidth = Size;

			scaleX = Math.Abs(rotationWidth / Size - 0.5f) * 2;
		}
	}
}
