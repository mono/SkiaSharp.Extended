# GIF Implementation Disagreement Matrix

This matrix tracks all known disagreements between the GIF specification and reference implementations.

## Matrix Format

For each feature/behavior:
- ✓ = Matches spec
- ✗ = Differs from spec
- N/A = Not applicable/implemented
- ? = Untested/unknown

## Disagreements

| Feature | Spec | giflib | libnsgif | cgif | Chosen | Decision Doc |
|---------|------|--------|----------|------|--------|--------------|
| *(Example)* LZW Min Code Size | 2-8 bits | ✓ | ✓ | ✓ | ✓ | N/A - all agree |
| | | | | | | |

## Legend

- **Feature**: The specific GIF feature or behavior
- **Spec**: What the W3C GIF89a specification states
- **giflib/libnsgif/cgif**: How each reference implements it
- **Chosen**: The behavior we implement (✓ = follows spec, ✗ = follows references)
- **Decision Doc**: Link to the detailed decision in compatibility-decision-library.md

## Categories

### Decoder Behaviors
*(To be populated during implementation)*

### Encoder Behaviors
*(To be populated during implementation)*

### Extension Handling
*(To be populated during implementation)*

### LZW Codec
*(To be populated during implementation)*

### Color/Palette Handling
*(To be populated during implementation)*

### Animation/Timing
*(To be populated during implementation)*

## Update Process

1. When a disagreement is discovered during implementation or testing
2. Test all three reference implementations with proof apps
3. Document behavior in this matrix
4. Create detailed decision in compatibility-decision-library.md
5. Implement the majority behavior
6. Add test cases validating the chosen behavior

## Statistics

- Total Features Tested: 0
- Unanimous Agreement: 0
- Majority Decisions: 0
- Spec-Only Decisions: 0

*(Statistics will be updated as the matrix is populated)*
