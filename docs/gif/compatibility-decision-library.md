# GIF Compatibility Decision Library

This document records all cases where the GIF specification and reference implementations disagree, documenting the decision made and the evidence supporting it.

## Decision Process

For each disagreement:

1. **Identify**: Document the specific feature or behavior where disagreement occurs
2. **Spec Excerpt**: Quote the relevant section from the W3C GIF89a specification
3. **Reference Behavior**: Test and document how each of giflib, libnsgif, and cgif behaves
4. **Proof Apps**: Create small test applications demonstrating the behavior
5. **Decision**: Choose majority behavior (2 out of 3 references)
6. **Document**: Record decision, rationale, and minority behavior

## Decision Template

```markdown
### [Feature/Behavior Name]

**Category**: [Decoder/Encoder/Extension/etc.]

**Specification Says**:
> [Exact quote from W3C GIF89a spec with section number]

**Reference Implementation Behaviors**:

- **giflib**: [Observed behavior from proof app]
- **libnsgif**: [Observed behavior from proof app]
- **cgif**: [Observed behavior from proof app]

**Proof Applications**:
- `proof-apps/[feature-name]-giflib.c` - [Description]
- `proof-apps/[feature-name]-libnsgif.c` - [Description]
- `proof-apps/[feature-name]-cgif.c` - [Description]

**Decision**: 
[Chosen behavior - should match majority (2/3) of references]

**Rationale**:
[Why this decision was made, noting majority alignment]

**Minority Behavior**:
[Document the behavior of the implementation(s) that differ]

**Implementation Notes**:
[Any special considerations for implementing this decision]

---
```

## Known Disagreements

### Example: Disposal Method Handling (Placeholder)

This section will be populated as disagreements are discovered during implementation.

**Category**: Decoder/Compositor

**Specification Says**:
> (To be filled in with actual spec excerpt when disagreements are found)

**Reference Implementation Behaviors**:
- **giflib**: (To be documented)
- **libnsgif**: (To be documented)
- **cgif**: (To be documented)

**Proof Applications**:
- (To be created as needed)

**Decision**: 
(To be determined based on majority behavior)

**Rationale**:
(To be documented)

**Minority Behavior**:
(To be documented)

---

## Adding New Decisions

When adding a new decision:

1. Use the template above
2. Include proof applications in `proof-apps/` directory
3. Reference the proof apps in the decision
4. Update the disagreement matrix
5. Add test cases validating the chosen behavior
6. Add comments in implementation referencing this decision

## Change History

| Date | Decision | Author | Notes |
|------|----------|--------|-------|
| 2026-02-17 | N/A | Initial | Template created |
