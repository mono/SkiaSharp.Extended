# Morphysics Visual Documentation Guide

## Overview
This guide explains how to capture screenshots and animated GIFs of the Morphysics features for documentation purposes.

## Prerequisites
- Visual Studio 2022 or later
- .NET 9 SDK
- Android Emulator or physical device
- Screen recording software (see recommendations below)

## Sample Applications

### 1. Particles Demo (`MorphysicsParticlesPage`)
**Location**: `samples/SkiaSharpDemo/Demos/Morphysics/MorphysicsParticlesPage.xaml`

**Features to Capture**:
- Continuous particle emission
- Gravity effects (positive and negative)
- Particle collisions
- Bounce behavior (restitution)
- Burst emission

**Recommended Captures**:
1. **Screenshot**: Basic particle emission with default settings
   - Filename: `morphysics-particles-basic.png`
   
2. **GIF**: Particle burst with collisions enabled
   - Duration: 3-5 seconds
   - Show: Click burst button, particles fall and collide
   - Filename: `morphysics-particles-burst.gif`

3. **GIF**: Gravity reversal effect
   - Duration: 3-5 seconds
   - Show: Adjust gravity slider from positive to negative
   - Filename: `morphysics-particles-gravity.gif`

### 2. Vector Morphing Demo (`MorphysisMorphingPage`)
**Location**: `samples/SkiaSharpDemo/Demos/Morphysics/MorphysisMorphingPage.xaml`

**Features to Capture**:
- Shape morphing (square → circle, star → heart, etc.)
- Easing function effects
- Progress slider control
- Smooth transitions

**Recommended Captures**:
1. **Screenshot**: Halfway morph (50% progress) between square and circle
   - Filename: `morphysics-morph-halfway.png`

2. **GIF**: Complete morph animation (square → circle)
   - Duration: 4-6 seconds
   - Show: Full morph cycle with linear easing
   - Filename: `morphysics-morph-square-circle.gif`

3. **GIF**: Easing comparison (same shapes, different easing)
   - Duration: 8-10 seconds
   - Show: Morph with Linear, then EaseInOut
   - Filename: `morphysics-morph-easing-comparison.gif`

4. **GIF**: Complex shape morphing (star → heart)
   - Duration: 4-6 seconds
   - Filename: `morphysics-morph-star-heart.gif`

### 3. Physics Playground (`MorphysicsPlaygroundPage`)
**Location**: `samples/SkiaSharpDemo/Demos/Morphysics/MorphysicsPlaygroundPage.xaml`

**Features to Capture**:
- Attractor forces
- Sticky zones
- Combined physics effects
- Interactive controls

**Recommended Captures**:
1. **Screenshot**: Particles being attracted to center
   - Filename: `morphysics-playground-attractor.png`

2. **GIF**: Attractor demonstration
   - Duration: 5-8 seconds
   - Show: Particles spawning and being pulled toward center
   - Filename: `morphysics-playground-attractor.gif`

3. **GIF**: Sticky zone demonstration
   - Duration: 5-8 seconds
   - Show: Enable sticky zone, particles get captured
   - Filename: `morphysics-playground-sticky.gif`

4. **GIF**: Full playground interaction
   - Duration: 10-15 seconds
   - Show: Adjust gravity, attractor strength, spawn burst
   - Filename: `morphysics-playground-interactive.gif`

## Recommended Tools

### Windows
- **Screenshot**: Windows Snipping Tool or Snip & Sketch (Win + Shift + S)
- **GIF Recording**: 
  - [ScreenToGif](https://www.screentogif.com/) (Free, open-source)
  - [LICEcap](https://www.cockos.com/licecap/) (Free, lightweight)
  - [Peek](https://github.com/phw/peek) (Linux/Windows)

### macOS
- **Screenshot**: Built-in (Cmd + Shift + 4)
- **GIF Recording**:
  - [Kap](https://getkap.co/) (Free, open-source)
  - [GIPHY Capture](https://giphy.com/apps/giphycapture) (Free)
  - [Gifox](https://gifox.io/) (Paid)

### Android Emulator
- **Screenshot**: Emulator toolbar camera icon
- **Recording**: Emulator toolbar video icon → Convert to GIF

## Capture Process

### Screenshots
1. Launch the sample app
2. Navigate to the desired demo page
3. Set up the desired state (e.g., specific particle count, morph progress)
4. Capture using your platform's screenshot tool
5. Crop and optimize if needed
6. Save with descriptive filename

### Animated GIFs
1. Start your GIF recording software
2. Select the app window or region to record
3. Perform the interaction to demonstrate
4. Stop recording
5. Optimize GIF (reduce file size):
   - Maximum width: 800px
   - Frame rate: 15-30 fps
   - Duration: 3-15 seconds (keep it concise)
6. Save with descriptive filename

## Optimization Tips

### GIF Optimization
```bash
# Using gifsicle (install via package manager)
gifsicle -O3 --colors 256 input.gif -o output.gif

# Using ImageMagick
convert input.gif -fuzz 10% -layers Optimize output.gif
```

### Image Optimization
```bash
# Using ImageMagick for PNG
convert input.png -quality 85 -resize 800x output.png

# Using pngquant
pngquant --quality 85-95 input.png -o output.png
```

## File Organization

Store captured media in:
```
docs/images/morphysics/
├── screenshots/
│   ├── morphysics-particles-basic.png
│   ├── morphysics-morph-halfway.png
│   └── morphysics-playground-attractor.png
└── gifs/
    ├── morphysics-particles-burst.gif
    ├── morphysics-morph-square-circle.gif
    └── morphysics-playground-attractor.gif
```

## Running the Samples

### From Visual Studio
1. Open `SkiaSharp.Extended.sln`
2. Set `SkiaSharpDemo` as startup project
3. Select target platform (Android, iOS, Windows, macOS)
4. Press F5 to run
5. Navigate to Demos → Morphysics → [Choose demo]

### From Command Line
```bash
# Android
cd samples/SkiaSharpDemo
dotnet build -f net9.0-android
dotnet run -f net9.0-android

# iOS (macOS only)
dotnet build -f net9.0-ios
dotnet run -f net9.0-ios

# Windows
dotnet build -f net9.0-windows10.0.19041.0
dotnet run -f net9.0-windows10.0.19041.0

# macOS
dotnet build -f net9.0-maccatalyst
dotnet run -f net9.0-maccatalyst
```

## Checklist for Complete Documentation

- [ ] Particles Demo
  - [ ] Basic emission screenshot
  - [ ] Burst animation GIF
  - [ ] Gravity effects GIF
  - [ ] Collision demonstration GIF

- [ ] Vector Morphing Demo
  - [ ] Halfway morph screenshot
  - [ ] Square to circle GIF
  - [ ] Easing comparison GIF
  - [ ] Complex shape morph GIF

- [ ] Physics Playground
  - [ ] Attractor screenshot
  - [ ] Attractor animation GIF
  - [ ] Sticky zone GIF
  - [ ] Interactive playground GIF

- [ ] Feature Highlights
  - [ ] Deterministic physics demo
  - [ ] Scene graph hierarchy
  - [ ] MVVM binding example

## Notes

- **File Size**: Keep GIFs under 5MB for web performance
- **Duration**: Shorter is better (3-8 seconds ideal)
- **Quality**: Balance quality vs file size
- **Consistency**: Use same resolution/aspect ratio across similar captures
- **Context**: Include UI controls in frame to show interactivity
- **Timing**: Show the full interaction cycle (setup → action → result)

## Example Captions

For documentation, include descriptive captions:

```markdown
![Particles with gravity](images/morphysics/screenshots/morphysics-particles-basic.png)
*Continuous particle emission with gravity and collision detection enabled*

![Shape morphing](images/morphysics/gifs/morphysics-morph-square-circle.gif)
*Smooth vector path morphing from square to circle with easing*

![Physics playground](images/morphysics/gifs/morphysics-playground-attractor.gif)
*Interactive physics with attractor pulling particles toward center*
```
