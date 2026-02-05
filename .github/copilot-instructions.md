# GitHub Copilot Instructions for SkiaSharp.Extended

This document provides instructions for GitHub Copilot when working with the SkiaSharp.Extended repository.

## Repository Overview

SkiaSharp.Extended is a collection of additional libraries and controls for SkiaSharp, a cross-platform 2D graphics API for .NET. The repository includes:

- **SkiaSharp.Extended** - Core extension library with utilities like PathBuilder, SKGeometry, etc.
- **SkiaSharp.Extended.UI.Maui** - MAUI controls including Lottie animations, Confetti effects, and gesture surfaces

## Project Structure

```
SkiaSharp.Extended/
├── source/
│   ├── SkiaSharp.Extended/              # Core library (netstandard2.0, net9.0)
│   └── SkiaSharp.Extended.UI.Maui/      # MAUI controls
│       ├── Controls/
│       │   ├── Confetti/                # Confetti particle system
│       │   ├── Lottie/                  # Lottie animation support
│       │   └── Gestures/                # Gesture and dynamic surface views
│       └── Utils/                       # Utility classes
├── samples/
│   └── SkiaSharpDemo/                   # MAUI demo app
├── tests/                               # Test projects
└── docs/                                # Documentation
```

## Coding Conventions

### Naming Conventions
- All control classes start with `SK` prefix (e.g., `SKConfettiView`, `SKGestureSurfaceView`)
- Event args classes end with `EventArgs` suffix
- Shared source files use `.shared.cs` extension
- Platform-specific files use `.android.cs`, `.ios.cs`, `.windows.cs`, etc.

### File Naming
- `*.shared.cs` - Cross-platform code
- `*.shared.xaml` - Shared XAML resources
- `*.android.cs` - Android-specific code
- `*.ios.cs` - iOS-specific code  
- `*.windows.cs` - Windows-specific code

### Code Style
- File-scoped namespaces
- Nullable reference types enabled
- LangVersion 10.0
- Implicit usings enabled for MAUI projects

### Control Patterns

Controls follow a consistent pattern using `TemplatedView`:

```csharp
namespace SkiaSharp.Extended.UI.Controls;

public class SKMyControl : SKSurfaceView  // or TemplatedView
{
    public static readonly BindableProperty MyProperty = BindableProperty.Create(
        nameof(MyPropertyName),
        typeof(PropertyType),
        typeof(SKMyControl),
        defaultValue,
        propertyChanged: OnMyPropertyChanged);

    public SKMyControl()
    {
        // Register resources for styling
        ResourceLoader<Themes.SKMyControlResources>.EnsureRegistered(this);
    }

    public PropertyType MyPropertyName
    {
        get => (PropertyType)GetValue(MyProperty);
        set => SetValue(MyProperty, value);
    }

    protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
    {
        // Drawing logic
    }

    private static void OnMyPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // Property change handler
    }
}
```

### XAML Resource Pattern

Each control has a corresponding resources file:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    xmlns:local="clr-namespace:SkiaSharp.Extended.UI.Controls"
                    xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"
                    x:Class="SkiaSharp.Extended.UI.Controls.Themes.SKMyControlResources">

    <ControlTemplate x:Key="SKMyControlControlTemplate">
        <skia:SKCanvasView x:Name="PART_DrawingSurface" />
    </ControlTemplate>

    <Style x:Key="SKMyControlStyle" TargetType="local:SKMyControl">
        <Setter Property="ControlTemplate"
                Value="{StaticResource SKMyControlControlTemplate}" />
    </Style>

    <Style TargetType="local:SKMyControl"
           ApplyToDerivedTypes="True"
           BasedOn="{StaticResource SKMyControlStyle}" />
</ResourceDictionary>
```

## Build Instructions

### Prerequisites
- .NET 9 SDK or later
- MAUI workload installed: `dotnet workload install maui`
- On Linux, only Android builds are supported: `dotnet workload install maui-android`

### Build Commands
```bash
# Restore packages (use nuget.org source if internal feeds are unavailable)
dotnet restore source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj --source https://api.nuget.org/v3/index.json

# Build for Android
dotnet build source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj -f net9.0-android35.0

# Build for all platforms (Windows only)
dotnet build source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj
```

### Target Frameworks
- `net9.0` - Reference assembly
- `net9.0-android35.0` - Android
- `net9.0-ios18.0` - iOS (macOS/Windows only)
- `net9.0-maccatalyst18.0` - Mac Catalyst (macOS only)
- `net9.0-windows10.0.19041.0` - Windows (Windows only)

## Common Patterns

### ResourceLoader Pattern
Used to ensure XAML resources are loaded:
```csharp
public SKMyControl()
{
    ResourceLoader<Themes.SKMyControlResources>.EnsureRegistered(this);
}
```

### Animation Pattern
For animated controls, inherit from `SKAnimatedSurfaceView`:
```csharp
public class SKMyAnimatedControl : SKAnimatedSurfaceView
{
    protected override void Update(TimeSpan deltaTime)
    {
        // Update animation state
    }

    protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
    {
        // Draw current frame
    }
}
```

### Touch/Gesture Pattern
Enable touch events and handle them:
```csharp
// In constructor or initialization
canvasView.EnableTouchEvents = true;
canvasView.Touch += OnTouch;

private void OnTouch(object? sender, SKTouchEventArgs e)
{
    switch (e.ActionType)
    {
        case SKTouchAction.Pressed:
            // Handle press
            break;
        case SKTouchAction.Moved:
            // Handle move
            break;
        case SKTouchAction.Released:
            // Handle release
            break;
    }
    e.Handled = true;
}
```

## Dependencies

### NuGet Packages
- `SkiaSharp` (3.119.1+)
- `SkiaSharp.Skottie` (3.119.1+) - For Lottie animations
- `SkiaSharp.Views.Maui.Controls` (3.119.1+)
- `Microsoft.Maui.Controls` (9.x)

### Project References
- `SkiaSharp.Extended.UI.Maui` → `SkiaSharp.Extended`

## Testing

### Test Structure
Tests are located in the `tests/` directory and use xUnit framework.

### Running Tests
```bash
dotnet test tests/SkiaSharp.Extended.Tests/SkiaSharp.Extended.Tests.csproj
```

## Common Issues

### 1. NuGet Restore Failures
If restore fails with Azure DevOps feed errors, use:
```bash
dotnet restore --source https://api.nuget.org/v3/index.json
```

### 2. MAUI Workload Not Found
Install the appropriate workload:
```bash
# Windows/macOS - full MAUI
dotnet workload install maui

# Linux - Android only
dotnet workload install maui-android
```

### 3. Deprecated SkiaSharp APIs
Use modern APIs:
- `SKMatrix.CreateIdentity()` instead of `SKMatrix.MakeIdentity()`
- `SKMatrix.CreateTranslation()` instead of `SKMatrix.MakeTranslation()`
- `SKFont` for text measurement instead of `SKPaint.TextSize`

## Resources

- [SkiaSharp Documentation](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)
- [.NET MAUI Documentation](https://docs.microsoft.com/en-us/dotnet/maui/)
- [SkiaSharp.Views.Maui](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.views.maui)
