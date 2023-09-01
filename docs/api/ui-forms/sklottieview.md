# SKLottieView

The Lottie view is a animated view that can playback Lottie files.

| Preview |
| :-----: |
| ![lottie](../../images/ui/controls/sklottieview/lottie.gif) |

## Properties

There are several properties that can be used to control th animation playback:

| Property               | Type                   | Description |
| :--------------------- | :--------------------- | :---------- |
| **Source**             | `SKLottieImageSource`  | The Lottie [image source](#source) to playback in the view. |
| **Duration**           | `TimeSpan`             | A value indicating the total duration of the animation. |
| **Progress**           | `TimeSpan`             | The current playback progress of the animation. |
| **RepeatCount**        | `int`                  | The number of times to repeat the animation. Default is 0 (no repeat). A negative (-1) value will repeat forever. |
| **RepeatMode**         | `SKLottieRepeatMode`   | The way in which to repeat the animation. Default is `Restart`. |
| **IsAnimationEnabled** | `bool`                 | Determines whether the control will play the animation provided. |
| **IsComplete**         | `bool`                 | A value that indicates whether all systems are complete. |

## Events

There are a few events that can be used to be notified of animation loading events:

| Event                   | Type            | Description |
| :---------------------- | :-------------- | :---------- |
| **AnimationLoaded**     | `EventHandler`  | Invoked when the animation has loaded successfully. |
| **AnimationFailed**     | `EventHandler`  | Invoked when there was an error loading the animation. |
| **AnimationCompleted**  | `EventHandler`  | Invoked when the animation is finished playing (after all the repeats). Infinite animations never complete so will not trigger the event. |

## Parts

In addition to the properties on the view, there is the overall control template that can directly influence the visual appearance of the view. The default template is defined as:

```xaml
<ControlTemplate x:Key="SKLottieViewControlTemplate">
    <skia:SKCanvasView x:Name="PART_DrawingSurface" />
</ControlTemplate>
```

| Part                     | Description |
| :----------------------- | :---------- |
| **PART_DrawingSurface**  | This part can either be a `SKCanvasView` or a `SKGLView` and describes the actual rendering surface for the animation. |
