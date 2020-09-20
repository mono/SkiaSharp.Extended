# SKConfettiView

The confetti view is a container for one or more systems of particles.

| Top + Stream    | Center + Burst    | Sides + Multiple |
| :-------------: | :---------------: | :--------------: |
| ![][top-stream] | ![][center-burst] | ![][sides-spray] |

## Properties

The main property of a confetti view is the `Systems` property:

| Property        | Type                          | Description |
| :-------------- | :---------------------------- | :---------- |
| **Systems**     | `SKConfettiSystemCollection`  | The collection of [systems](#system) in the view. |
| **IsRunning**   | `bool`                        | Controls whether the all systems are running or not. |
| **IsComplete**  | `bool`                        | A value that indicates whether all systems are complete. |

## Parts

In addition to the properties on the view and all the systems, there is the overall control template that can directly influence the visual appearance of the view. The default template is defined as:

```xaml
<ControlTemplate x:Key="SKConfettiViewControlTemplate">
    <skia:SKCanvasView x:Name="PART_DrawingSurface" />
</ControlTemplate>
```

| Part                     | Description |
| :----------------------- | :---------- |
| **PART_DrawingSurface**  | This part can either be a `SKCanvasView` or a `SKGLView` and describes the actual rendering surface for the confetti. |

# System

Every confetti view consists up one or more systems (`SKConfettiSystem`). Each system is a container for an [emitter](#emitter) (`SKConfettiEmitter`) along with many properties on how the system runs.

| Property                      | Type                           | Description |
| :---------------------------- | :----------------------------- | :---------- |
| **Emitter**                   | `SKConfettiEmitter`            | An [emitter](#emitter) controls how the confetti particles are generated and at what rate for how long. |
| **EmitterBounds**             | `SKConfettiEmitterBounds`      | The emitter bounds controls where in the view the particles appear. This can be from a side (left, right, top, bottom), a point or a rectangular region. |
| **Gravity**                   | `Point`                        | The gravity is a acceleration vector that indicates the direction and strength of the gravity in the system. |
| **Colors**                    | `SKConfettiColorCollection`    | A collection of `Color` instances that determine the available colors for the confetti. |
| **Physics**                   | `SKConfettiPhysicsCollection`  | A collection of [`SKConfettiPhysics`](#physics) instances that determine the "base" mass and size of each confetti particle. |
| **Shapes**                    | `SKConfettiShapeCollection`    | A collection of [`SKConfettiShape`](#shape) instances that determine what each confetti particle looks like. |
| **StartAngle**                | `double`                       | The angle (in degrees) to form the start of the emission region. |
| **EndAngle**                  | `double`                       | The angle (in degrees) to form the end of the emission region. |
| **MinimumInitialVelocity**    | `double`                       | The minimum initial velocity of the confetti particles. |
| **MaximumInitialVelocity**    | `double`                       | The maximum initial velocity of the confetti particles. |
| **MinimumRotationVelocity**   | `double`                       | The minimum initial rotation velocity of the confetti particles. |
| **MaximumRotationVelocity**   | `double`                       | The maximum initial rotation velocity of the confetti particles. |
| **MaximumVelocity**           | `double`                       | The maximum velocity the confetti particle can reach. |
| **FadeOut**                   | `bool`                         | Whether or not the particle should fade out at the end of its life. |
| **Lifetime**                  | `double`                       | The duration in seconds for how long the particle is allowed to live. |
| **IsRunning**                 | `bool`                         | Controls whether the system is running or not. |
| **IsComplete**                | `bool`                         | A value that indicates whether the system is complete and all systems and particles are also complete. |

# Emitter

Each system has an emitter instance that controls how the confetti particles are generated and at what rate for how long.

| Property          | Type      | Description |
| :---------------- | :-------- | :---------- |
| **ParticleRate**  | `int`     | The number of particles to generate each second. |
| **MaxParticles**  | `int`     | The maximum number of particles allowed by the emitter. A value of `-1` indicates no limit. |
| **Duration**      | `double`  | The duration in seconds of how long the emitter runs for. A value of `0` indicates that all particles are emitted instantly. |
| **IsComplete**    | `bool`    | A value that indicates whether the emitter has generated all the particles and they have all disappeared. |

## Helper Emitters

To make creating emitters easier, there are a few static helper methods on the `SKConfettiEmitter` type:

| Property      | Description |
| :------------ | :---------- |
| **Burst**     | Create an emitter that generates the specified number of the particles instantly. |
| **Infinite**  | Create an emitter that releases the specified number of particles each second. |
| **Stream**    | Create an emitter that releases the specified number of particles for the specified amount of time. |

# Shapes

There are several simple types of "shapes" that the confetti can come in:

| Name              | Type                             | Description |
| :---------------- | :------------------------------- | :---------- |
| **Circle**        | `SKConfettiCircleShape`          | This is a simple circle shape. |
| **Square**        | `SKConfettiSquareShape`          | This is a simple square with equal length sides. |
| **Oval**          | `SKConfettiOvalShape`            | This is the stretched circle or ellipse shape. <br/> This has a useful property `HeightRatio` that can be used to control the width/height ratio in the range of [0..1]. |
| **Rectangle**     | `SKConfettiRectShape`            | This is a rectangle. <br/> This has a useful property `HeightRatio` that can be used to control the width/height ratio in the range of [0..1]. |

## Advanced Shapes

In addition to those, there is also a way to have custom paths as a shape:

| Name              | Type                             | Description |
| :---------------- | :------------------------------- | :---------- |
| **Paths**         | `SKConfettiPathShape`            | This is a generic shape that supports any `SKPath` instance |
| **Custom**        | `SKConfettiShape`                | This is the base type for shapes. Custom drawing code can be used by deriving from this type and overriding the `OnDraw` method. |

When making a custom shape, the instance is re-used, so all state needs to be set before drawing. The provided `SKPaint` instance is reset, so any properties set needs to be re-set.

## Custom Shapes

```csharp
public class ConfettiStar : SKConfettiShape
{
	private readonly int points;

	public ConfettiStar(int points)
	{
		this.points = points;
	}

	protected override void OnDraw(SKCanvas canvas, SKPaint paint, float size)
	{
		using var star = SKGeometry.CreateRegularStarPath(size, size / 2, points);
		canvas.DrawPath(star, paint);
	}
}
```

# Physics

Each particle can have a size and mass, and the emitter can select one of them randomly:

| Property  | Type      | Description |
| :-------- | :-------- | :---------- |
| **Mass**  | `double`  | The mass of the particle which resists the force of gravity. |
| **Size**  | `double`  | The physical size of the particle rendered. |

[top-stream]: ../../images/ui/controls/skconfettiview/top-stream.gif
[center-burst]: ../../images/ui/controls/skconfettiview/center-burst.gif
[sides-spray]: ../../images/ui/controls/skconfettiview/sides-spray.gif
