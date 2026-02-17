# Lottie Builder API - Before and After

## API Enhancement Overview

This document provides a quick visual comparison of the Lottie `AnimationBuilder` API improvements.

## Before (Hard-coded)

```csharp
// In SKLottieImageSource.shared.cs - BEFORE
internal Skottie.AnimationBuilder CreateAnimationBuilder() =>
    Skottie.Animation.CreateBuilder()
        .SetResourceProvider(new CachingResourceProvider(new DataUriResourceProvider()))
        .SetFontManager(SKFontManager.Default);
```

**Limitations**:
- ❌ No customization possible
- ❌ Same configuration for all animations
- ❌ Cannot test with mocks
- ❌ Cannot support offline resources
- ❌ Cannot use custom fonts

**Usage**:
```csharp
// User has NO control over resource loading or fonts
var source = new SKFileLottieImageSource { File = "animation.json" };
```

---

## After (Customizable)

```csharp
// In SKLottieImageSource.shared.cs - AFTER

// New public properties
public static ResourceProvider? DefaultResourceProvider { get; set; }
public static SKFontManager? DefaultFontManager { get; set; }
public ResourceProvider? ResourceProvider { get; set; }
public SKFontManager? FontManager { get; set; }

internal Skottie.AnimationBuilder CreateAnimationBuilder()
{
    var builder = Skottie.Animation.CreateBuilder();
    
    // Priority: instance → static → built-in default
    var resourceProvider = ResourceProvider 
        ?? DefaultResourceProvider 
        ?? new CachingResourceProvider(new DataUriResourceProvider());
    builder.SetResourceProvider(resourceProvider);
    
    var fontManager = FontManager 
        ?? DefaultFontManager 
        ?? SKFontManager.Default;
    builder.SetFontManager(fontManager);
    
    return builder;
}
```

**Capabilities**:
- ✅ App-wide customization via static properties
- ✅ Per-animation customization via instance properties
- ✅ Backwards compatible (defaults unchanged)
- ✅ Testable with mock injection
- ✅ Supports offline resources
- ✅ Supports custom fonts

**Usage Examples**:

### Example 1: App-wide Custom Font Manager
```csharp
// In App.xaml.cs or MauiProgram.cs
public class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Set app-wide defaults - affects all Lottie animations
        SKLottieImageSource.DefaultFontManager = LoadCustomFontManager();
        
        MainPage = new AppShell();
    }
}

// Now all Lottie animations will use the custom font manager
var source = new SKFileLottieImageSource { File = "animation.json" };
```

### Example 2: Per-Animation Resource Provider
```csharp
// Special animation needs offline resources
var offlineSource = new SKFileLottieImageSource 
{
    File = "offline-animation.json",
    ResourceProvider = new OfflineResourceProvider(localResourcePath)
};

// Regular animation uses defaults
var regularSource = new SKFileLottieImageSource { File = "regular.json" };
```

### Example 3: Unit Testing with Mocks
```csharp
[Fact]
public async Task LoadAnimation_CallsResourceProvider()
{
    // Arrange
    var mockProvider = new MockResourceProvider();
    var source = new SKStreamLottieImageSource
    {
        Stream = GetTestStream,
        ResourceProvider = mockProvider
    };
    
    // Act
    var animation = await source.LoadAnimationAsync();
    
    // Assert
    Assert.True(mockProvider.WasCalled);
}
```

### Example 4: Priority System
```csharp
// Setup: app-wide default
SKLottieImageSource.DefaultResourceProvider = appWideProvider;

// Animation 1: uses app-wide default
var anim1 = new SKFileLottieImageSource { File = "anim1.json" };
// Uses: appWideProvider

// Animation 2: overrides with instance property
var anim2 = new SKFileLottieImageSource 
{ 
    File = "anim2.json",
    ResourceProvider = customProvider  // Override
};
// Uses: customProvider

// Animation 3: sets to null (forces built-in default)
var anim3 = new SKFileLottieImageSource 
{ 
    File = "anim3.json",
    ResourceProvider = null  // Explicit null
};
// Uses: built-in CachingResourceProvider(DataUriResourceProvider())
```

---

## Migration Guide

### No Changes Required

Existing code continues to work without modifications:
```csharp
// This still works exactly as before
var source = new SKFileLottieImageSource { File = "animation.json" };
```

### Optional Enhancements

To take advantage of new features:

**Step 1**: Decide if you need app-wide or per-animation customization

**Step 2**: Implement your custom provider or manager

**Step 3**: Configure as needed:
```csharp
// Option A: App-wide
SKLottieImageSource.DefaultResourceProvider = myProvider;

// Option B: Per-animation
source.ResourceProvider = myProvider;

// Option C: Both (instance overrides static)
SKLottieImageSource.DefaultResourceProvider = defaultProvider;
specialAnimation.ResourceProvider = specialProvider;
```

---

## Technical Details

### Priority Resolution

```
1. Check instance property (e.g., source.ResourceProvider)
   ↓ if null
2. Check static property (e.g., SKLottieImageSource.DefaultResourceProvider)
   ↓ if null
3. Use built-in default (e.g., new CachingResourceProvider(...))
```

### Thread Safety

- Static properties are shared across all instances
- Instance properties are per-object
- Both are not thread-safe by design (follow .NET conventions)
- Set during initialization, not during animation playback

### Performance

- **No overhead**: Only adds property storage (4 references per instance)
- **Lazy creation**: Providers/managers only created when needed
- **Same defaults**: When not customized, behaves identically to before

---

## See Also

- [LOTTIE-BUILDER-ANALYSIS.md](LOTTIE-BUILDER-ANALYSIS.md) - Detailed analysis with 3 alternative approaches
- [SkiaSharp AnimationBuilder Docs](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skottie.animationbuilder)
- [SkiaSharp ResourceProvider Docs](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.resources.resourceprovider)
