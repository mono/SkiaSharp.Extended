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
| **RepeatCount**        | `int`                  | The number of times to repeat the animation. Default is 0 (no repeat). A negative (-1) value will repeat forever. |
| **RepeatMode**         | `SKLottieRepeatMode`   | The way in which to repeat the animation. Default is `Restart`. |
| **IsAnimationEnabled** | `bool`                 | Determines whether the control will play the animation provided. |
| **IsComplete**         | `bool`                 | A value that indicates whether all systems are complete. |

### Image Source Properties

The `SKLottieImageSource` and its derived classes support the following property for loading external image assets:

| Property               | Type                   | Description |
| :--------------------- | :--------------------- | :---------- |
| **ImageAssetsFolder**  | `string`               | The folder path where external image assets referenced in the Lottie JSON file are located. This is required when your animation references external image files (e.g., `"p": "img_0.png"`) instead of embedding them as base64 data URIs. |

## Loading External Image Assets

Lottie animations can reference external image files that are stored separately from the animation JSON. To use external images:

1. Place your image files in a folder in your project (e.g., `Resources/Raw/images/`)
2. Set the `ImageAssetsFolder` property on the `Source` to the folder path where the images are located

### Example in XAML

```xaml
<controls:SKLottieView>
    <controls:SKLottieView.Source>
        <controls:SKFileLottieImageSource 
            File="animations/myanimation.json"
            ImageAssetsFolder="animations/images" />
    </controls:SKLottieView.Source>
</controls:SKLottieView>
```

### Example in C#

```csharp
var lottieView = new SKLottieView
{
    Source = new SKFileLottieImageSource
    {
        File = "animations/myanimation.json",
        ImageAssetsFolder = "animations/images"
    }
};
```

### Lottie JSON Format

When your Lottie animation references external images, the JSON will contain asset entries like this:

```json
{
  "assets": [
    {
      "id": "image_0",
      "w": 100,
      "h": 100,
      "u": "images/",
      "p": "img_0.png",
      "e": 0
    }
  ]
}
```

The `ImageAssetsFolder` property should be set to the base path where the images folder is located. The library will combine the `ImageAssetsFolder` with the `"u"` (folder) and `"p"` (filename) values from the JSON to locate the image file.

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
