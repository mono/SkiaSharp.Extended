# GIF Implementation Architecture

## Overview

This document describes the architecture of the GIF encoder/decoder implementation.

## Core Principles

1. **Hybrid Decoder Strategy**: Use SKCodec when possible for performance, fallback to pure C# for full features
2. **Layered Architecture**: Clear separation between I/O, codec, composition, and public API
3. **Test-Driven Development**: Write tests before/during implementation
4. **Performance First**: Optimize critical paths, benchmark against reference implementations
5. **Memory Efficiency**: Minimize allocations, support streaming where possible

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                      Public API Layer                        │
│  SKGifDecoder, SKGifEncoder, SKGifFrame, SKGifInfo          │
└─────────────────────────────────────────────────────────────┘
                              │
      ┌───────────────────────┴───────────────────────┐
      │                                               │
┌─────────────────────┐                   ┌──────────────────────┐
│  Hybrid Decoder      │                   │   Pure C# Encoder    │
│                      │                   │                      │
│ ┌─────────────────┐ │                   │  ┌────────────────┐  │
│ │ SKCodec Wrapper │ │                   │  │  Quantization  │  │
│ │  (Performance)  │ │                   │  │    Engine      │  │
│ └─────────────────┘ │                   │  └────────────────┘  │
│         │            │                   │          │           │
│ ┌─────────────────┐ │                   │  ┌────────────────┐  │
│ │  Pure C# Dec    │ │                   │  │  Frame Builder │  │
│ │  (Full Features)│ │                   │  │                │  │
│ └─────────────────┘ │                   │  └────────────────┘  │
└─────────────────────┘                   └──────────────────────┘
           │                                          │
┌──────────┴──────────────────────────────────────────┴──────────┐
│              Frame Compositor & Image Processing               │
│   Disposal Methods, Transparency, Interlacing, Color Tables   │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────┴────────────────────────────────────┐
│                        LZW Codec Layer                            │
│                 LzwDecoder    │    LzwEncoder                     │
└────────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────┴────────────────────────────────────┐
│                      Block I/O Layer                              │
│          GifReader (Parse blocks)  │  GifWriter (Write blocks)   │
└────────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────┴────────────────────────────────────┐
│                        Stream Layer                               │
│                    System.IO.Stream                              │
└────────────────────────────────────────────────────────────────────┘
```

## Component Details

### Block I/O Layer

**Purpose**: Low-level reading and writing of GIF data blocks

**Components**:
- `GifReader`: Sequential block reader
- `GifWriter`: Sequential block writer
- Block models: Header, Logical Screen Descriptor, Image Descriptor, Extensions

**Key Responsibilities**:
- Read/write GIF signature and version
- Parse/write logical screen descriptor
- Handle color tables (global and local)
- Read/write all extension blocks
- Handle sub-block chains
- Validate structure

**Error Handling**:
- Throw `InvalidDataException` for malformed GIF data
- Validate all block sizes and offsets
- Handle truncated files gracefully

### LZW Codec Layer

**Purpose**: Compress and decompress image data using LZW algorithm

**Components**:
- `LzwDecoder`: Decompresses LZW-encoded data
- `LzwEncoder`: Compresses data using LZW

**Key Features**:
- Variable bit widths (2-12 bits)
- Clear code and end-of-information code
- Dynamic code table growth
- Deferred clear code (GIF89a spec)

**Performance Considerations**:
- Pre-allocate code table
- Minimize allocations in hot path
- Use spans for zero-copy operations

### Hybrid Decoder Layer

**SKCodec Wrapper** (Performance Path):
- Wraps SkiaSharp's native `SKCodec`
- Fast decoding using native libgif
- Automatic format support
- **Limitations**: No loop count, no extension metadata

**Pure C# Decoder** (Feature Path):
- Full GIF87a/89a support
- All extensions accessible
- Loop count (NETSCAPE)
- Comments, application data
- **Trade-off**: Slower than native

**Selection Strategy**:
```csharp
if (options?.UseNativeCodec ?? true)
{
    // Try SKCodec first
    if (CanUseSKCodec(stream))
        return new SKCodecGifDecoder(stream);
}
// Fallback to pure C#
return new ManagedGifDecoder(stream);
```

### Frame Compositor

**Purpose**: Composite GIF frames with proper disposal methods

**Disposal Methods**:
1. **None (0)**: No disposal specified
2. **DoNotDispose (1)**: Leave frame in place
3. **RestoreToBackground (2)**: Clear to background color
4. **RestoreToPrevious (3)**: Restore to previous frame state

**Algorithm**:
```
1. Start with background canvas
2. For each frame:
   a. Apply disposal method from previous frame
   b. Decode current frame image data
   c. Composite at frame offset with transparency
   d. Return composited result
```

### Encoder Layer

**Color Quantization**:
- Octree quantizer (default)
- Median cut (optional)
- NeuQuant (optional, higher quality)

**Encoding Pipeline**:
```
1. Analyze all frames
2. Choose global vs local palettes
3. Quantize colors per strategy
4. Write GIF header and logical screen
5. Write NETSCAPE loop extension
6. For each frame:
   a. Write graphics control extension
   b. Write image descriptor
   c. Write local color table (if needed)
   d. LZW compress image data
   e. Write compressed data blocks
7. Write trailer
```

### Public API Layer

**SKGifDecoder**:
- Factory: `Create(Stream, options)`
- Properties: `Info`, `GifInfo`, `FrameInfo[]`, `FrameCount`
- Methods: `GetFrame(index)`, `GetPixels(index, info, pixels)`

**SKGifEncoder**:
- Constructor: `new SKGifEncoder(Stream, options)`
- Methods: `AddFrame(bitmap, frameInfo)`, `SetLoopCount(count)`, `Encode()`
- Disposal: `Dispose()` writes trailer

## Error Handling Strategy

### Decoder Errors
- `InvalidDataException`: Malformed GIF structure
- `NotSupportedException`: Unsupported features
- `ArgumentException`: Invalid arguments
- `IOException`: Stream I/O errors

### Encoder Errors
- `ArgumentException`: Invalid input (e.g., null bitmap)
- `InvalidOperationException`: Invalid state (e.g., already encoded)
- `IOException`: Stream write errors

## Memory Management

### Decoder
- Use `ArrayPool<T>` for temporary buffers
- Return pooled arrays in finally blocks
- Avoid allocating per-frame unless necessary
- Support streaming decode (don't load all frames at once)

### Encoder
- Pool color table buffers
- Reuse LZW dictionary across frames
- Stream write (don't buffer entire GIF in memory)

## Testing Strategy

### Unit Tests
- Each component tested independently
- Mock dependencies where needed
- Test edge cases and error conditions
- Target: >90% coverage

### Integration Tests
- End-to-end decode/encode
- Round-trip tests
- Compatibility with reference implementations
- Real-world GIF files

### Benchmark Tests
- Decode performance vs SKCodec vs giflib
- Encode performance vs cgif
- Memory usage benchmarks
- Large file benchmarks

## Performance Targets

### Decoder
- SKCodec wrapper: Match native performance (within 10%)
- Pure C# decoder: Within 2x of native
- Memory: <2x frame size for decoding

### Encoder
- Within 3x of cgif for small images
- Within 2x of cgif for large images
- Memory: <3x output size during encoding

## Implementation Phases

1. ✅ Infrastructure
2. 🟡 Block I/O + LZW (Current)
3. ⬜ Hybrid Decoder
4. ⬜ Encoder
5. ⬜ Testing
6. ⬜ Benchmarking
7. ⬜ Optimization
8. ⬜ Documentation

## Decision Log

### Use Hybrid Decoder
**Decision**: Implement both SKCodec wrapper and pure C# decoder
**Rationale**: Best performance when possible, full features when needed
**Date**: 2026-02-17

### Use Octree Quantization as Default
**Decision**: Default to Octree, offer alternatives
**Rationale**: Good balance of speed and quality
**Date**: TBD

### Stream-based API
**Decision**: Use `Stream` for I/O, not byte arrays
**Rationale**: Memory efficient, supports large files
**Date**: 2026-02-17
