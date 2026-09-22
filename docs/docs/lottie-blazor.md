# Blazor Lottie Animations

[`SKLottieView`](xref:SkiaSharp.Extended.UI.Blazor.Components.SKLottieView) plays
Lottie JSON in Blazor WebAssembly and Interactive WebAssembly with SkiaSharp.
It does not support Interactive Server or unrestricted Interactive Auto because
SkiaSharp's browser rendering host must be interactive in the browser. It uses the shared
[`SKLottiePlayer`](lottie-player.md) engine with
[`SKAnimatedSurfaceView`](animated-surface-blazor.md). The surface owns the
browser render loop and timing, so frames are updated and painted together
without a component render per frame. Its monotonic timing and rolling
frame-rate calculation are shared with the .NET MAUI animated surface.
`SKAnimatedSurfaceView` inherits the on-demand Canvas/OpenGL handling from
`SKSurfaceView`.

In DEBUG builds, the underlying animated surface draws an FPS label after each
Lottie frame. The diagnostic is not included in release builds.

## Quick Start

Install the package and add its namespaces to `_Imports.razor`:

```bash
dotnet add package SkiaSharp.Extended.UI.Blazor
```

```razor
@using SkiaSharp.Extended
@using SkiaSharp.Extended.UI.Blazor.Components
```

Render a Lottie file:

```razor
<SKLottieView Source="animations/trophy.json"
              Repeat="SKLottieRepeat.Restart()"
              style="width: 300px; height: 300px;" />
```

The default `Repeat` is `SKLottieRepeat.Never`, so an animation plays once.
The render loop runs only while the view is enabled, an animation is loaded,
and playback has not completed.

## Rendering Surface

`SKLottieView` composes and forwards the generic surface's rendering options. Canvas is the
default; select OpenGL when your app benefits from SkiaSharp's GPU-backed
Blazor view. Set `IgnorePixelScaling` when the canvas should use CSS pixels
rather than physical pixels:

```razor
<SKLottieView Source="animations/trophy.json"
              SurfaceType="@typeof(SKGLView)"
              IgnorePixelScaling="true"
              style="width: 300px; height: 300px;" />
```

`SurfaceType` can also be a custom component derived from `SKCanvasView` or
`SKGLView`.

## Playback Controls

Use `SKLottieRepeat` directly to select the repeat behavior:

```razor
@* Play once *@
<SKLottieView Source="animation.json"
              Repeat="SKLottieRepeat.Never" />

@* Restart forever *@
<SKLottieView Source="animation.json"
              Repeat="SKLottieRepeat.Restart()" />

@* Three additional restart plays *@
<SKLottieView Source="animation.json"
              Repeat="SKLottieRepeat.Restart(3)" />

@* Ping-pong once, forward then back *@
<SKLottieView Source="animation.json"
              Repeat="SKLottieRepeat.Reverse(0)" />
```

`Restart(count)` counts additional full plays. `Reverse(count)` counts
additional forward/back cycles; `Reverse(0)` therefore performs one complete
forward/back cycle. Use a negative `AnimationSpeed` to begin at the end and
play in reverse.

Pause or resume by binding `IsAnimationEnabled`; call `Restart()` through an
`@ref` to begin again with the current settings:

```razor
<SKLottieView @ref="lottieView"
              Source="animation.json"
              IsAnimationEnabled="@isPlaying" />

<button @onclick="() => isPlaying = !isPlaying">Pause or resume</button>
<button @onclick="() => lottieView?.Restart()">Restart</button>

@code {
    private SKLottieView? lottieView;
    private bool isPlaying = true;
}
```

## Events and State

`AnimationLoaded`, `AnimationFailed`, and `AnimationCompleted` report load and
playback transitions. Completion is dispatched after the frame update has been
rendered; there is deliberately no per-frame async callback.

Read `IsLoading`, `HasAnimation`, `Duration`, `Progress`, and `IsComplete`
through an `@ref`. Additional attributes such as `style` and `class` are passed
to the underlying canvas.

## Loading Sources

Strings, including the quick-start example, are URLs relative to `wwwroot` or
absolute URLs. URI loading uses the optional `HttpClient` parameter first, then
the app's registered default `HttpClient`. Supply your authenticated client
when an endpoint needs credentials:

```razor
<SKLottieView Source="@SKLottieImageSource.FromUri(new Uri("api/animation", UriKind.Relative))"
              HttpClient="@authenticatedClient" />
```

For data that is already in your app, use raw JSON, a copied byte payload, or
a stream factory. The factory must return a fresh readable stream every time;
the component disposes each stream after reading it. This keeps database, blob,
and application-service integrations in your app layer rather than in the
control API:

```csharp
var source = SKLottieImageSource.FromStream(
    cancellationToken => new ValueTask<Stream>(
        blobStore.OpenAnimationAsync(cancellationToken)));
```

Use `SKLottieImageSource.FromJson(json)` or `FromBytes(bytes)` when your app
already has the payload. Browser storage and `InputFile` are also app-layer
concerns. The sample page uses an `InputFile` picker with a 5 MiB limit and
converts the selected JSON into `FromBytes`; picker types are not part of the
library API. Call `ReloadAsync()` to intentionally reload an equal source.

## Learn More

- [Lottie Player](lottie-player.md) — shared playback semantics
- [MAUI Lottie Animations](lottie-maui.md) — use Lottie in .NET MAUI
- [Lottie](https://airbnb.design/lottie/) — animation format overview
- [API Reference](xref:SkiaSharp.Extended.UI.Blazor.Components.SKLottieView)
