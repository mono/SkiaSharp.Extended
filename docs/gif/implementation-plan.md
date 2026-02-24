# GIF Implementation Plan

This document outlines the implementation plan for full GIF87a/GIF89a encode/decode support in SkiaSharp.Extended.

## Project Goals

1. **Complete Specification Support**: Implement both GIF87a and GIF89a specifications including all extensions
2. **High Compatibility**: Ensure behavior matches established reference implementations
3. **SkiaSharp Integration**: Provide seamless integration with SkiaSharp's SKBitmap and SKCanvas APIs
4. **Separate Package**: Delivered as SkiaSharp.Extended.Gif NuGet package
5. **MIT License**: Use only MIT-licensed reference implementations

## Architecture Layers

### 1. Block I/O Layer
**Purpose**: Low-level reading and writing of GIF data blocks

**Components**:
- `GifReader` - Sequential block reader with buffering
- `GifWriter` - Sequential block writer with buffering
- Block type identification and routing
- Data sub-block handling

**Key Methods**:
- `ReadByte()`, `ReadBytes()`, `ReadBlock()`
- `WriteByte()`, `WriteBytes()`, `WriteBlock()`
- `SkipBlock()`, `PeekBlockType()`

### 2. LZW Codec Layer
**Purpose**: LZW compression and decompression

**Components**:
- `LzwDecoder` - Decompresses LZW-encoded image data
- `LzwEncoder` - Compresses image data using LZW
- Code table management
- Variable bit-width handling (2-12 bits)

**Key Features**:
- Clear code and end-of-information code handling
- Dynamic code table growth
- Early change optimization (follow reference majority behavior)

### 3. Decoder Layer
**Purpose**: Parse GIF files and extract frame data

**Components**:
- `GifDecoder` - Main decoder coordinating all parsing
- `LogicalScreenDescriptor` - Global GIF properties
- `ImageDescriptor` - Individual frame properties
- `ColorTable` - Global and local color tables
- Extension parsers (Graphics Control, Comment, Plain Text, Application)

**Key Methods**:
- `DecodeHeader()` - Parse GIF signature and version
- `DecodeLogicalScreen()` - Parse logical screen descriptor
- `DecodeImage()` - Parse and decompress frame data
- `DecodeExtension()` - Parse extension blocks

### 4. Compositor Layer
**Purpose**: Render GIF frames to SKBitmap with proper disposal

**Components**:
- `GifCompositor` - Manages frame rendering and disposal
- `DisposalMethod` enum - Disposal method types
- Frame buffer management
- Transparency handling

**Disposal Methods**:
- No disposal (keep frame)
- Restore to background
- Restore to previous
- Undefined (treat as no disposal per reference majority)

### 5. Quantization Layer
**Purpose**: Color reduction and palette generation for encoding

**Components**:
- `ColorQuantizer` - Reduces colors to palette
- `OctreeQuantizer` - Octree-based quantization
- `MedianCutQuantizer` - Median cut algorithm
- `PaletteBuilder` - Constructs optimal palettes

**Key Features**:
- Configurable palette size (2-256 colors)
- Dithering support (optional)
- Transparency preservation

### 6. Encoder Layer
**Purpose**: Generate GIF files from SKBitmap frames

**Components**:
- `GifEncoder` - Main encoder coordinating all writing
- Animation support with frame delays
- Loop count configuration
- Transparency key color selection

**Key Methods**:
- `AddFrame()` - Add frame with timing
- `SetLoopCount()` - Configure animation looping
- `SetTransparencyColor()` - Define transparent color
- `Save()` - Write final GIF file

### 7. Public API Layer
**Purpose**: High-level API for application developers

**Classes**:
- `SKGifDecoder` - Public decoder API
- `SKGifEncoder` - Public encoder API
- `SKGifFrame` - Represents a decoded frame
- `SKGifMetadata` - GIF metadata (size, frame count, etc.)

**Example Usage**:
```csharp
// Decoding
using var decoder = SKGifDecoder.Create(stream);
var metadata = decoder.Metadata;
for (int i = 0; i < metadata.FrameCount; i++)
{
    using var frame = decoder.GetFrame(i);
    var bitmap = frame.Bitmap;
    var delayMs = frame.DelayMs;
    // Use bitmap...
}

// Encoding
using var encoder = new SKGifEncoder(stream);
encoder.SetLoopCount(0); // Infinite loop
encoder.AddFrame(bitmap1, delayMs: 100);
encoder.AddFrame(bitmap2, delayMs: 100);
encoder.Save();
```

## Implementation Phases

### Phase 1: Project Setup
- [ ] Create SkiaSharp.Extended.Gif project
- [ ] Create SkiaSharp.Extended.Gif.Tests project
- [ ] Update solution and build files
- [ ] Set up reference library submodules
- [ ] Create documentation structure

### Phase 2: Block I/O and LZW
- [ ] Implement GifReader
- [ ] Implement GifWriter
- [ ] Implement LzwDecoder
- [ ] Implement LzwEncoder
- [ ] Unit tests for I/O and LZW

### Phase 3: Decoder
- [ ] Implement header parsing
- [ ] Implement logical screen descriptor parsing
- [ ] Implement color table parsing
- [ ] Implement image descriptor parsing
- [ ] Implement extension parsing
- [ ] Integration tests with sample GIFs

### Phase 4: Compositor
- [ ] Implement frame rendering
- [ ] Implement disposal methods
- [ ] Handle transparency
- [ ] Integration tests for animation

### Phase 5: Encoder
- [ ] Implement color quantization
- [ ] Implement GIF writing
- [ ] Implement animation support
- [ ] Round-trip tests (encode → decode)

### Phase 6: Public API
- [ ] Design public API
- [ ] Implement SKGifDecoder
- [ ] Implement SKGifEncoder
- [ ] XML documentation
- [ ] Usage examples

### Phase 7: Testing and Validation
- [ ] Reference implementation comparison
- [ ] Disagreement documentation
- [ ] Performance testing
- [ ] Memory profiling
- [ ] Compatibility matrix completion

## Testing Strategy

### Unit Tests
- Each layer tested independently
- Mock dependencies
- Edge cases and error conditions
- LZW codec correctness

### Integration Tests
- Full decode/encode workflows
- Sample GIF corpus from reference libraries
- Round-trip validation
- Cross-reference validation

### Compatibility Tests
- Side-by-side comparison with giflib
- Side-by-side comparison with libnsgif
- Side-by-side comparison with cgif
- Automated disagreement detection

### Performance Tests
- Decode performance benchmarks
- Encode performance benchmarks
- Memory usage profiling
- Large file handling

## Reference Libraries

### giflib
- **License**: MIT
- **Repository**: https://sourceforge.net/projects/giflib/
- **Usage**: Decoder and encoder reference
- **Test Suite**: `tests/` directory with regression tests

### libnsgif
- **License**: MIT
- **Repository**: https://github.com/netsurf-browser/libnsgif
- **Usage**: Decoder reference
- **Test Suite**: `test/` directory with 281 test GIFs

### cgif
- **License**: MIT
- **Repository**: https://github.com/dloebl/cgif
- **Usage**: Encoder reference
- **Test Suite**: `tests/` directory

## Compatibility Policy

As documented in [compatibility-decision-library.md](compatibility-decision-library.md):

1. When spec and all references agree → follow spec
2. When spec disagrees with majority (2+) of references → follow references
3. When references disagree among themselves → follow majority (2 out of 3)
4. All disagreements require proof apps and documentation
5. Minority behaviors documented for awareness

## Dependencies

- **SkiaSharp** (3.119.1+) - Core graphics library
- **System.IO.Streams** - Stream handling
- **No additional dependencies** - Keep it lean

## Performance Goals

- **Decode**: Match or exceed giflib performance
- **Encode**: Match or exceed cgif performance
- **Memory**: Minimize allocations, support streaming
- **Large Files**: Handle GIFs up to 2GB efficiently

## Documentation Deliverables

1. API documentation (XML comments)
2. Usage guide with examples
3. Compatibility decision library
4. Disagreement matrix
5. Architecture overview
6. Performance characteristics

## Success Criteria

- [ ] All GIF87a features implemented
- [ ] All GIF89a features implemented
- [ ] All extensions supported
- [ ] Passes reference library test suites
- [ ] Disagreements documented and resolved
- [ ] Performance within 10% of reference implementations
- [ ] API is easy to use and well-documented
- [ ] NuGet package published

## Timeline

This is a significant undertaking. Estimated effort:
- Phase 1-2: 2 weeks
- Phase 3-4: 3 weeks
- Phase 5-6: 3 weeks
- Phase 7: 2 weeks
- **Total**: ~10 weeks for full implementation

## Notes

- Prioritize decoder over encoder initially
- Build compatibility testing infrastructure early
- Document disagreements as they're discovered
- Keep proof apps small and focused
- Use reference test suites extensively
