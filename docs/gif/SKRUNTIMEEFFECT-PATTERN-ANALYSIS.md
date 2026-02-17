# SKRuntimeEffect API Pattern Analysis

## Pattern Overview

The `SKRuntimeEffect` class demonstrates SkiaSharp's preferred API pattern for operations that can fail during creation.

## Key Pattern Elements

### 1. Create* Methods (Error Handling)

```csharp
public static SKRuntimeEffect CreateShader(string sksl, out string errors)
{
    using var s = new SKString(sksl);
    using var errorString = new SKString();
    var effect = GetObject(SkiaApi.sk_runtimeeffect_make_for_shader(s.Handle, errorString.Handle));
    errors = errorString?.ToString();
    if (errors?.Length == 0)
        errors = null;
    return effect;
}
```

**Characteristics**:
- Returns the object (or null if failed)
- Uses `out string errors` parameter for detailed error messages
- Allows caller to handle errors gracefully
- Suitable when errors are expected and should be handled

### 2. Build* Methods (Builder Pattern with Validation)

```csharp
public static SKRuntimeShaderBuilder BuildShader(string sksl)
{
    var effect = CreateShader(sksl, out var errors);
    ValidateResult(effect, errors);  // Throws if failed
    return new SKRuntimeShaderBuilder(effect);
}

private static void ValidateResult(SKRuntimeEffect effect, string errors)
{
    if (effect is null) {
        if (string.IsNullOrEmpty(errors))
            throw new SKRuntimeEffectBuilderException("Failed to compile...");
        else
            throw new SKRuntimeEffectBuilderException($"Failed to compile... {errors}");
    }
}
```

**Characteristics**:
- Throws exception on error (fail-fast)
- Returns a builder object for fluent configuration
- Suitable when errors are exceptional
- Provides better error messages

## Applying to GIF API

### Current GIF Decoder API

```csharp
public static SKGifDecoder Create(Stream stream)
{
    var decoder = new SKGifDecoder(stream);
    decoder.Initialize();  // Throws InvalidDataException if invalid GIF
    return decoder;
}
```

### Proposed: SKRuntimeEffect Pattern

```csharp
// Create - returns decoder with error details
public static SKGifDecoder? CreateDecoder(Stream stream, out string? errors)
{
    try
    {
        var decoder = new SKGifDecoder(stream);
        decoder.Initialize();
        errors = null;
        return decoder;
    }
    catch (Exception ex)
    {
        errors = ex.Message;
        return null;
    }
}

// Build - throws on error, returns ready-to-use decoder
public static SKGifDecoder BuildDecoder(Stream stream)
{
    var decoder = CreateDecoder(stream, out var errors);
    if (decoder == null)
    {
        throw new InvalidDataException(
            string.IsNullOrEmpty(errors) 
                ? "Failed to decode GIF file." 
                : $"Failed to decode GIF file: {errors}");
    }
    return decoder;
}

// Keep original Create for backwards compatibility
[Obsolete("Use BuildDecoder or CreateDecoder instead")]
public static SKGifDecoder Create(Stream stream) => BuildDecoder(stream);
```

### Encoder Pattern

```csharp
// Create - returns encoder with validation
public static SKGifEncoder? CreateEncoder(Stream stream, SKGifEncoderOptions? options, out string? errors)
{
    if (stream == null)
    {
        errors = "Stream cannot be null.";
        return null;
    }
    
    if (!stream.CanWrite)
    {
        errors = "Stream must be writable.";
        return null;
    }
    
    try
    {
        errors = null;
        return new SKGifEncoder(stream, options);
    }
    catch (Exception ex)
    {
        errors = ex.Message;
        return null;
    }
}

// Build - throws on error
public static SKGifEncoder BuildEncoder(Stream stream, SKGifEncoderOptions? options = null)
{
    var encoder = CreateEncoder(stream, options, out var errors);
    if (encoder == null)
    {
        throw new ArgumentException(
            string.IsNullOrEmpty(errors)
                ? "Failed to create GIF encoder."
                : $"Failed to create GIF encoder: {errors}");
    }
    return encoder;
}
```

## Benefits

1. **Error Handling Flexibility**: Create* for graceful handling, Build* for fail-fast
2. **Better Error Messages**: Detailed errors returned via `out` parameter
3. **Consistency**: Matches SkiaSharp's established pattern
4. **Discoverability**: Two clear options for different error handling needs
5. **Testing**: Create* methods easier to test (no exceptions)

## Applying to Current GIF Implementation

### Changes Required

1. **SKGifDecoder.cs**:
   - Add `CreateDecoder(Stream, out string errors)` method
   - Add `BuildDecoder(Stream)` method
   - Keep `Create(Stream)` for compatibility (or mark obsolete)

2. **SKGifEncoder.cs**:
   - Change constructor to private
   - Add `CreateEncoder(Stream, options, out string errors)` static method
   - Add `BuildEncoder(Stream, options)` static method

3. **Tests**:
   - Add tests for Create* methods (null returns, error messages)
   - Add tests for Build* methods (exceptions)
   - Update existing tests if needed

## Recommendation

Apply this pattern to the GIF API for consistency with SkiaSharp's design philosophy:
- ✅ Better error handling
- ✅ Matches SkiaSharp patterns
- ✅ More testable
- ✅ Better developer experience
- ✅ Clear separation between fail-fast and graceful error handling
