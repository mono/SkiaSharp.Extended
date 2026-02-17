# SKRuntimeEffect Pattern Implementation Summary

## Overview

Successfully applied SkiaSharp's `SKRuntimeEffect` two-tier error handling pattern to the GIF API.

## The SKRuntimeEffect Pattern

Based on the `SKRuntimeEffect` class in SkiaSharp, this pattern provides two ways to handle operations that can fail:

### Pattern Structure

```csharp
// Tier 1: Create* - Graceful error handling
public static T? Create*(params, out string? errors)
{
    try
    {
        // Attempt creation
        errors = null;
        return new T(...);
    }
    catch (Exception ex)
    {
        errors = ex.Message;
        return null;
    }
}

// Tier 2: Build* - Fail-fast
public static T Build*(params)
{
    var obj = Create*(params, out var errors);
    if (obj == null)
    {
        throw new Exception(
            string.IsNullOrEmpty(errors)
                ? "Failed. Unknown error."
                : $"Failed: {errors}");
    }
    return obj;
}
```

## Implementation in GIF API

### SKGifDecoder

```csharp
// Create* - Returns null with error details
public static SKGifDecoder? CreateDecoder(Stream stream, out string? errors)

// Build* - Throws exception on error
public static SKGifDecoder BuildDecoder(Stream stream)

// Original - Delegates to BuildDecoder for backwards compatibility
public static SKGifDecoder Create(Stream stream) => BuildDecoder(stream);
```

### SKGifEncoder

```csharp
// Create* - Returns null with error details
public static SKGifEncoder? CreateEncoder(Stream stream, out string? errors)

// Build* - Throws exception on error
public static SKGifEncoder BuildEncoder(Stream stream)

// Constructor - Still available, throws on error
public SKGifEncoder(Stream stream)
```

## Usage Examples

### Decoder - Graceful Error Handling

```csharp
using var stream = GetUserSelectedFile();
var decoder = SKGifDecoder.CreateDecoder(stream, out var errors);

if (decoder == null)
{
    // Show error to user
    MessageBox.Show($"Invalid GIF file: {errors}");
    return;
}

using (decoder)
{
    // Process valid GIF
    DisplayAnimation(decoder);
}
```

### Decoder - Fail-Fast

```csharp
// When you expect the file to be valid
using var stream = File.OpenRead("assets/logo.gif");  
using var decoder = SKGifDecoder.BuildDecoder(stream);  // Throws if invalid

// Continue with valid decoder
DisplayAnimation(decoder);
```

### Encoder - Validate Before Writing

```csharp
using var stream = GetOutputStream();
var encoder = SKGifEncoder.CreateEncoder(stream, out var errors);

if (encoder == null)
{
    LogError($"Cannot create encoder: {errors}");
    return false;
}

using (encoder)
{
    encoder.AddFrame(frame1, 100);
    encoder.AddFrame(frame2, 100);
    encoder.Encode();
    return true;
}
```

### Encoder - Simple Usage

```csharp
// When you know the stream is valid
using var stream = File.Create("output.gif");
using var encoder = SKGifEncoder.BuildEncoder(stream);  // Throws if stream is invalid

encoder.SetLoopCount(0);
encoder.AddFrame(bitmap, 100);
encoder.Encode();
```

## Error Messages

The pattern provides detailed, actionable error messages:

### Decoder Errors
- `"Stream cannot be null."`
- `"Unexpected error decoding GIF: [details]"`
- Build* wraps with: `"Failed to decode GIF file: [details]"`

### Encoder Errors
- `"Stream cannot be null."`
- `"Stream must be writable."`
- `"Unexpected error creating GIF encoder: [details]"`
- Build* wraps with: `"Failed to create GIF encoder: [details]"`

## When to Use Each Method

### Use Create* When:
- ✅ Handling user-selected files (might be invalid)
- ✅ Want to show friendly error messages
- ✅ Need to try multiple decoders/formats
- ✅ Validation before processing
- ✅ Testing error conditions (no exceptions)

### Use Build* When:
- ✅ Loading known-good resources
- ✅ Internal operations where errors are exceptional
- ✅ Cleaner code (no null checks)
- ✅ Want exceptions for error handling

### Use Original Create/Constructor When:
- ✅ Existing code (backwards compatibility)
- ✅ Simple cases
- ✅ Following established patterns in codebase

## Testing

All three approaches are fully tested:

```csharp
// Test Create* - no exceptions
var decoder = SKGifDecoder.CreateDecoder(invalidStream, out var errors);
Assert.Null(decoder);
Assert.NotNull(errors);

// Test Build* - expects exception
Assert.Throws<InvalidDataException>(() => 
    SKGifDecoder.BuildDecoder(invalidStream));

// Test backwards compatibility
Assert.Throws<InvalidDataException>(() => 
    SKGifDecoder.Create(invalidStream));
```

## Pattern Benefits

1. **Consistency**: Matches SkiaSharp's established patterns
2. **Flexibility**: Choose error handling approach per use case
3. **Clarity**: Method names clearly indicate behavior
4. **Compatibility**: Doesn't break existing code
5. **Testability**: Create* methods easier to test

## References

- [SKRuntimeEffect source code](https://github.com/mono/SkiaSharp/blob/main/binding/Binding/SKRuntimeEffect.cs)
- [SKRuntimeEffect API docs](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skruntimeeffect)
- `docs/gif/SKRUNTIMEEFFECT-PATTERN-ANALYSIS.md` - Detailed analysis

## Conclusion

The SKRuntimeEffect pattern is now fully applied to the GIF API, providing users with flexible error handling options while maintaining backwards compatibility. This pattern should be used for future APIs in SkiaSharp.Extended that involve potentially-failing creation operations.
