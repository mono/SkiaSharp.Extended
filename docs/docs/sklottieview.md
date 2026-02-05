# SKLottieView

The Lottie view is a animated view that can playback Lottie files.

| Preview |
| :-----: |
| ![lottie](../images/ui/controls/sklottieview/lottie.gif) |

## Properties

There are several properties that can be used to control th animation playback:

| Property               | Type                   | Description |
| :--------------------- | :--------------------- | :---------- |
| **Source**             | `SKLottieImageSource`  | The Lottie image source to playback in the view. |
| **Duration**           | `TimeSpan`             | A value indicating the total duration of the animation. |
| **Progress**           | `TimeSpan`             | The current playback progress of the animation. |
| **Fps**                | `double`               | The frames per second of the animation. |
| **FrameCount**         | `int`                  | The total number of frames in the animation. |
| **CurrentFrame**       | `int`                  | The current frame number of the animation (zero-based). |
| **RepeatCount**        | `int`                  | The number of times to repeat the animation. Default is 0 (no repeat). A negative (-1) value will repeat forever. |
| **RepeatMode**         | `SKLottieRepeatMode`   | The way in which to repeat the animation. Default is `Restart`. |
| **IsAnimationEnabled** | `bool`                 | Determines whether the control will play the animation provided. |
| **IsComplete**         | `bool`                 | A value that indicates whether all systems are complete. |

## Methods

There are several convenience methods for controlling animation playback:

| Method                                                    | Description |
| :-------------------------------------------------------- | :---------- |
| **SeekToFrame(int frameNumber, bool stopPlayback)**      | Seeks to a specific frame in the animation. If `stopPlayback` is true, the animation will be paused after seeking. |
| **SeekToTime(TimeSpan time, bool stopPlayback)**         | Seeks to a specific time in the animation. If `stopPlayback` is true, the animation will be paused after seeking. |
| **SeekToProgress(double progress, bool stopPlayback)**   | Seeks to a specific progress position (0.0 to 1.0) in the animation. If `stopPlayback` is true, the animation will be paused after seeking. |
| **Pause()**                                               | Pauses the animation playback. |
| **Resume()**                                              | Resumes the animation playback. |

## Usage Examples

### Play to a Specific Frame (like goToAndStop)

To seek to a specific frame and stop the animation (similar to Lottie's `goToAndStop`):

```csharp
// Seek to frame 30 and stop
lottieView.SeekToFrame(30, stopPlayback: true);
```

### Create a Switch Animation

To create a switch animation that toggles between two states:

```csharp
bool isOn = false;

void ToggleSwitch()
{
    if (isOn)
    {
        // Go to "off" state (frame 0)
        lottieView.SeekToFrame(0, stopPlayback: true);
    }
    else
    {
        // Go to "on" state (last frame)
        lottieView.SeekToFrame(lottieView.FrameCount - 1, stopPlayback: true);
    }
    isOn = !isOn;
}
```

### Seek to a Percentage

To seek to a specific percentage of the animation:

```csharp
// Seek to 50% of the animation
lottieView.SeekToProgress(0.5, stopPlayback: false);
```

### Manual Frame-by-Frame Control

To manually control the animation frame by frame:

```csharp
// Pause the animation
lottieView.Pause();

// Step through frames
void NextFrame()
{
    var nextFrame = lottieView.CurrentFrame + 1;
    if (nextFrame < lottieView.FrameCount)
        lottieView.SeekToFrame(nextFrame);
}

void PreviousFrame()
{
    var prevFrame = lottieView.CurrentFrame - 1;
    if (prevFrame >= 0)
        lottieView.SeekToFrame(prevFrame);
}
```

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
