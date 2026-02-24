# GIF Support for SkiaSharp.Extended

This directory contains documentation and reference materials for GIF87a/GIF89a encode/decode support in SkiaSharp.Extended.

## Overview

The GIF implementation provides full support for:
- **GIF87a** specification
- **GIF89a** specification with all extensions
- Encoding (generation) and decoding (reading) of GIF files
- Animation support
- Transparency and disposal methods
- Color quantization and palette optimization

## Specifications

Primary reference: [W3C GIF89a Specification](https://www.w3.org/Graphics/GIF/spec-gif89a.txt)

## Reference Implementations

For compatibility validation and disagreement resolution, we reference three MIT-licensed libraries:

1. **giflib** - Classic C library for GIF encoding/decoding
2. **libnsgif** - NetSurf's GIF decoder library
3. **cgif** - Modern C library for GIF encoding

## Compatibility Policy

When the specification and reference implementations disagree on behavior:

1. **Majority Rule**: Follow the behavior demonstrated by the majority of the three reference implementations
2. **Evidence Required**: Each disagreement must be documented with:
   - Relevant spec excerpt
   - Small proof application for each reference implementation
   - Observed behavior from running the proof apps
3. **Documentation**: All disagreements and decisions are preserved in the [Compatibility Decision Library](compatibility-decision-library.md)

## Directory Structure

- `compatibility-decision-library.md` - Documented decisions for spec/implementation disagreements
- `disagreement-matrix.md` - Matrix of known disagreements across references
- `proof-apps/` - Small test applications demonstrating reference behaviors

## Implementation Architecture

The GIF implementation is organized into layers:

1. **Block I/O** - Low-level reading/writing of GIF data blocks
2. **LZW Codec** - Compression and decompression
3. **Decoder** - GIF file parsing and frame extraction
4. **Compositor** - Frame rendering with disposal methods
5. **Quantizer** - Color quantization and palette generation
6. **Encoder** - GIF file generation
7. **Public API** - High-level SKGifDecoder and SKGifEncoder classes

## Testing Strategy

- **Conformance Tests**: Validate against GIF87a/89a specifications
- **Cross-Reference Tests**: Compare output with reference implementations
- **Round-Trip Tests**: Encode → Decode validation
- **Compatibility Matrix**: Systematic validation against reference libraries

## See Also

- [Compatibility Decision Library](compatibility-decision-library.md)
- [Disagreement Matrix](disagreement-matrix.md)
- [Implementation Plan](implementation-plan.md)
