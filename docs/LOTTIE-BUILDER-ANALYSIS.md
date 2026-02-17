# Lottie AnimationBuilder API Analysis

## Current Implementation

### Location
`source/SkiaSharp.Extended.UI.Maui/Controls/Lottie/SKLottieImageSource.shared.cs`

### Code
```csharp
internal Skottie.AnimationBuilder CreateAnimationBuilder() =>
    Skottie.Animation.CreateBuilder()
        .SetResourceProvider(new CachingResourceProvider(new DataUriResourceProvider()))
        .SetFontManager(SKFontManager.Default);
```

### Usage
This method is called by all three image source implementations:
- `SKFileLottieImageSource.LoadAnimationAsync()`
- `SKUriLottieImageSource.LoadAnimationAsync()`
- `SKStreamLottieImageSource.LoadAnimationAsync()`

## Analysis

### Strengths ✅

1. **Centralized Configuration**: All image sources use the same builder configuration
2. **Sensible Defaults**: Provides caching and data URI support out of the box
3. **Simple**: Easy to understand and maintain
4. **Builder Pattern**: Follows SkiaSharp's fluent API style

### Weaknesses ❌

1. **Hard-coded**: No way to customize resource loading or font management
2. **Not Extensible**: Cannot inject custom providers for:
   - Custom resource locations
   - Custom font managers
   - Custom caching strategies
   - Alternative resource providers
3. **Internal Only**: Method is internal, not available even for advanced users
4. **Static Configuration**: Same settings for all animations in the app

### Real-World Use Cases Blocked

1. **Custom Font Loading**: Cannot use custom fonts from app bundle or embedded resources
2. **Network Resource Caching**: Cannot implement custom caching strategies
3. **Offline Support**: Cannot pre-load resources or use fallbacks
4. **Resource Prioritization**: Cannot implement custom resource loading priorities
5. **Testing**: Cannot mock resource providers for unit testing

## Comparison with SkiaSharp Patterns

### SkiaSharp's Builder Pattern
SkiaSharp uses builder patterns extensively:
- `SKPaint` - Properties can be set
- `SKCanvas` - Methods can be called
- `AnimationBuilder` - Fluent configuration

### Other .NET MAUI Patterns
.NET MAUI commonly uses:
- **Static defaults**: `Application.Current`, `FontManager.Default`
- **Per-instance configuration**: `ImageSource` properties
- **Handlers**: Platform-specific customization

## Recommended Improvements

### Option 1: Static + Instance Hybrid (Recommended) ⭐

Combine static defaults with per-instance overrides:

```csharp
public abstract class SKLottieImageSource : Element
{
    // Static defaults (app-wide)
    public static ResourceProvider? DefaultResourceProvider { get; set; }
    public static SKFontManager? DefaultFontManager { get; set; }
    
    // Instance overrides (per animation)
    public ResourceProvider? ResourceProvider { get; set; }
    public SKFontManager? FontManager { get; set; }
    
    internal Skottie.AnimationBuilder CreateAnimationBuilder()
    {
        var builder = Skottie.Animation.CreateBuilder();
        
        // Instance first, then static, then built-in default
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
}
```

**Usage Examples**:
```csharp
// App-wide custom configuration
SKLottieImageSource.DefaultResourceProvider = new MyCustomResourceProvider();
SKLottieImageSource.DefaultFontManager = myFontManager;

// Per-instance override
var source = new SKFileLottieImageSource 
{ 
    File = "animation.json",
    FontManager = mySpecialFontManager  // Override for this specific animation
};
```

**Pros**:
- ✅ Backwards compatible (existing code continues to work)
- ✅ App-wide defaults for consistency
- ✅ Per-instance overrides for special cases
- ✅ Easy migration path
- ✅ Follows .NET MAUI patterns

**Cons**:
- Static state (but common in MAUI)

### Option 2: Virtual Factory Method

Make the builder creation virtual for advanced customization:

```csharp
public abstract class SKLottieImageSource : Element
{
    protected virtual Skottie.AnimationBuilder CreateAnimationBuilder()
    {
        return Skottie.Animation.CreateBuilder()
            .SetResourceProvider(new CachingResourceProvider(new DataUriResourceProvider()))
            .SetFontManager(SKFontManager.Default);
    }
}
```

**Usage**:
```csharp
public class CustomLottieImageSource : SKFileLottieImageSource
{
    protected override Skottie.AnimationBuilder CreateAnimationBuilder()
    {
        return Skottie.Animation.CreateBuilder()
            .SetResourceProvider(myCustomProvider)
            .SetFontManager(myCustomFontManager);
    }
}
```

**Pros**:
- ✅ Maximum flexibility
- ✅ No breaking changes (internal → protected virtual)
- ✅ Inheritance-based customization

**Cons**:
- Requires creating derived classes
- Less discoverable

### Option 3: Configuration Object

Use a configuration object pattern:

```csharp
public class SKLottieConfiguration
{
    public ResourceProvider? ResourceProvider { get; set; }
    public SKFontManager? FontManager { get; set; }
    
    public static SKLottieConfiguration Default { get; set; } = new()
    {
        ResourceProvider = new CachingResourceProvider(new DataUriResourceProvider()),
        FontManager = SKFontManager.Default
    };
}

public abstract class SKLottieImageSource : Element
{
    public SKLottieConfiguration? Configuration { get; set; }
    
    internal Skottie.AnimationBuilder CreateAnimationBuilder()
    {
        var config = Configuration ?? SKLottieConfiguration.Default;
        
        return Skottie.Animation.CreateBuilder()
            .SetResourceProvider(config.ResourceProvider)
            .SetFontManager(config.FontManager);
    }
}
```

**Pros**:
- ✅ Clean API surface
- ✅ Grouping related configuration
- ✅ Reusable configuration objects

**Cons**:
- Additional class to maintain
- Slightly more complex

## Comparison Matrix

| Feature | Current | Option 1 | Option 2 | Option 3 |
|---------|---------|----------|----------|----------|
| Customizable | ❌ | ✅ | ✅ | ✅ |
| App-wide defaults | ❌ | ✅ | ❌ | ✅ |
| Per-instance | ❌ | ✅ | ✅ | ✅ |
| Backwards compat | N/A | ✅ | ✅ | ✅ |
| Discoverable | N/A | ✅ | ⚠️ | ✅ |
| Breaking changes | N/A | ❌ | ❌ | ❌ |
| Complexity | Low | Low | Medium | Medium |

## Recommendation

**Implement Option 1 (Static + Instance Hybrid)** because:

1. **Most flexible**: Supports both app-wide and per-instance configuration
2. **Familiar pattern**: Common in .NET MAUI and SkiaSharp
3. **Backwards compatible**: Existing code works without changes
4. **Discoverable**: Properties are visible in IntelliSense
5. **Easy migration**: Progressive enhancement from defaults to custom

## Implementation Plan

### Phase 1: Add Properties (Non-breaking)
1. Add static default properties
2. Add instance properties
3. Update `CreateAnimationBuilder()` to use them
4. All existing code continues to work

### Phase 2: Documentation
1. Document customization options
2. Provide examples for common scenarios
3. Migration guide if needed

### Phase 3: Testing
1. Test default behavior (unchanged)
2. Test static customization
3. Test instance customization
4. Test precedence (instance > static > default)

## Example Use Cases

### Custom Font Support
```csharp
// App-wide custom font manager
SKLottieImageSource.DefaultFontManager = myCustomFontManager;
```

### Custom Resource Provider
```csharp
// Custom provider for offline resources
var source = new SKFileLottieImageSource
{
    File = "animation.json",
    ResourceProvider = new OfflineResourceProvider(localResourcePath)
};
```

### Testing with Mocks
```csharp
// Unit testing with mock provider
var mockProvider = new MockResourceProvider();
var source = new SKStreamLottieImageSource
{
    Stream = GetTestStream,
    ResourceProvider = mockProvider
};
```

## Conclusion

The current implementation uses the builder pattern correctly but lacks extensibility. **Option 1 (Static + Instance Hybrid)** provides the best balance of:
- Simplicity
- Flexibility
- Backwards compatibility
- Developer experience

This follows established patterns in SkiaSharp and .NET MAUI while enabling advanced scenarios.
