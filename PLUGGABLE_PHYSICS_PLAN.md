# Pluggable Physics Architecture Plan

## Overview

Transform Morphysics from using a single custom physics implementation to supporting multiple physics engines through a pluggable architecture.

## Goals

1. **Pluggable Design**: Abstract physics behind interface
2. **Multiple Engines**: Support 4 physics engines:
   - DumbPhysicsEngine (current custom - baseline for benchmarks)
   - Aether.Physics2D (recommended)
   - Velcro Physics
   - Box2D.NetStandard
3. **Swappable**: Runtime or compile-time engine selection
4. **Benchmarked**: Performance comparison across all engines
5. **Backwards Compatible**: Existing API unchanged

## Architecture Design

### IPhysicsEngine Interface

```csharp
namespace SkiaSharp.Extended.UI.Maui.Morphysics.Physics;

public interface IPhysicsEngine : IDisposable
{
    // World properties
    Vector2 Gravity { get; set; }
    int ActiveParticleCount { get; }
    
    // Particle management
    void AddParticle(Particle particle);
    void RemoveParticle(Particle particle);
    void ClearParticles();
    IEnumerable<Particle> GetParticles();
    
    // Attractors
    void AddAttractor(string id, Vector2 position, float strength);
    void RemoveAttractor(string id);
    
    // Sticky zones
    void AddStickyZone(string id, Vector2 position, float radius, float stickProbability);
    void RemoveStickyZone(string id);
    
    // Simulation
    void Step(float deltaTime);
    void SetDeterministicSeed(int seed);
    
    // Configuration
    bool EnableCollisions { get; set; }
    float Restitution { get; set; }
    
    // Engine metadata
    string Name { get; }
    string Version { get; }
    PhysicsEngineCapabilities Capabilities { get; }
}

[Flags]
public enum PhysicsEngineCapabilities
{
    None = 0,
    ParticleCollisions = 1 << 0,
    Attractors = 1 << 1,
    StickyZones = 1 << 2,
    Deterministic = 1 << 3,
    Joints = 1 << 4,
    ComplexShapes = 1 << 5,
    ContinuousCollision = 1 << 6
}
```

### Physics Engine Implementations

#### 1. DumbPhysicsEngine (Baseline)

**Purpose**: Benchmark baseline, simple particle systems

**Implementation**:
- Extract current PhysicsWorld.shared.cs logic
- O(n²) collision detection
- Simple Velocity Verlet integration
- Deterministic with seeded Random

**File**: `source/.../Morphysics/Physics/DumbPhysicsEngine.cs`

#### 2. AetherPhysicsEngine (Recommended)

**NuGet**: `nkast.Aether.Physics2D` v2.2.0

**Implementation**:
- Wrap Aether.Physics2D.Dynamics.World
- Map particles to Aether bodies
- Use fixtures for collision
- Convert attractors to applied forces
- Spatial partitioning for performance

**File**: `source/.../Morphysics/Physics/AetherPhysicsEngine.cs`

**Expected Performance**:
- 10-50x faster than DumbPhysicsEngine
- Handles 500+ particles at 60 FPS

#### 3. VelcroPhysicsEngine

**NuGet**: `VelcroPhysics` (latest)

**Implementation**:
- Similar to Aether (both Box2D-based)
- Wrap Velcro World
- Map to bodies/fixtures

**File**: `source/.../Morphysics/Physics/VelcroPhysicsEngine.cs`

#### 4. Box2DPhysicsEngine

**NuGet**: `Box2D.NetStandard` (latest)

**Implementation**:
- Wrap Box2D World
- Direct Box2D port API

**File**: `source/.../Morphysics/Physics/Box2DPhysicsEngine.cs`

### PhysicsWorld Refactoring

**Current**: PhysicsWorld contains all physics logic

**New**: PhysicsWorld becomes a wrapper/facade

```csharp
public class PhysicsWorld
{
    private IPhysicsEngine engine;
    
    public PhysicsWorld(IPhysicsEngine? engine = null)
    {
        // Default to DumbPhysicsEngine for backwards compatibility
        this.engine = engine ?? new DumbPhysicsEngine();
    }
    
    // Existing public API delegates to engine
    public void AddParticle(Particle particle) => engine.AddParticle(particle);
    public void Step(float deltaTime) => engine.Step(deltaTime);
    // ... etc
    
    // New: Engine selection
    public void SetPhysicsEngine(IPhysicsEngine newEngine)
    {
        engine?.Dispose();
        engine = newEngine;
    }
    
    public IPhysicsEngine Engine => engine;
}
```

### Factory Pattern

```csharp
public static class PhysicsEngineFactory
{
    public static IPhysicsEngine Create(PhysicsEngineType type, PhysicsEngineOptions? options = null)
    {
        return type switch
        {
            PhysicsEngineType.Dumb => new DumbPhysicsEngine(options),
            PhysicsEngineType.Aether => new AetherPhysicsEngine(options),
            PhysicsEngineType.Velcro => new VelcroPhysicsEngine(options),
            PhysicsEngineType.Box2D => new Box2DPhysicsEngine(options),
            _ => throw new ArgumentException($"Unknown engine type: {type}")
        };
    }
}

public enum PhysicsEngineType
{
    Dumb,
    Aether,
    Velcro,
    Box2D
}
```

## Migration Strategy

### Phase 1: Extract Interface ✅ (This PR)

1. Create `IPhysicsEngine` interface
2. Create `DumbPhysicsEngine` (extract from PhysicsWorld)
3. Refactor `PhysicsWorld` to use `IPhysicsEngine`
4. Update tests to use new architecture
5. Verify existing functionality works

### Phase 2: Implement Aether.Physics2D (Next PR)

1. Add NuGet package
2. Implement `AetherPhysicsEngine`
3. Create adapter layer
4. Add tests
5. Benchmark vs DumbPhysicsEngine

### Phase 3: Implement Velcro & Box2D (Future PRs)

1. Add NuGet packages
2. Implement adapters
3. Add tests
4. Benchmark all engines

### Phase 4: Benchmarking Infrastructure (Final PR)

1. Create BenchmarkDotNet project
2. Benchmark scenarios:
   - 10, 50, 100, 200, 500, 1000 particles
   - With/without collisions
   - With/without attractors
3. Generate performance report
4. Update documentation

## NuGet Package Strategy

### Option A: Single Package with Optional Dependencies

**Package**: `SkiaSharp.Extended.UI.Maui`

**Dependencies**:
- SkiaSharp (required)
- nkast.Aether.Physics2D (optional, via PackageReference Condition)
- VelcroPhysics (optional)
- Box2D.NetStandard (optional)

**Pros**: Simple for users
**Cons**: Larger package, unused dependencies

### Option B: Separate Extension Packages (RECOMMENDED)

**Packages**:
1. `SkiaSharp.Extended.UI.Maui` (core, includes DumbPhysicsEngine)
2. `SkiaSharp.Extended.UI.Maui.Morphysics.Aether` (Aether adapter)
3. `SkiaSharp.Extended.UI.Maui.Morphysics.Velcro` (Velcro adapter)
4. `SkiaSharp.Extended.UI.Maui.Morphysics.Box2D` (Box2D adapter)

**Pros**: 
- Users only install what they need
- Smaller core package
- Clear dependencies

**Cons**: 
- More packages to maintain

### Recommendation: Option B

Start with core package + DumbPhysicsEngine. Users who want better performance can add extension packages.

## File Structure

```
source/SkiaSharp.Extended.UI.Maui/Controls/Morphysics/
├── Physics/                          # NEW
│   ├── IPhysicsEngine.cs             # Interface
│   ├── PhysicsEngineCapabilities.cs  # Enum
│   ├── PhysicsEngineOptions.cs       # Configuration
│   ├── PhysicsEngineFactory.cs       # Factory
│   ├── DumbPhysicsEngine.cs          # Extracted current impl
│   ├── Aether/                       # Aether.Physics2D adapter
│   │   ├── AetherPhysicsEngine.cs
│   │   └── AetherBodyAdapter.cs
│   ├── Velcro/                       # Velcro adapter
│   │   └── VelcroPhysicsEngine.cs
│   └── Box2D/                        # Box2D adapter
│       └── Box2DPhysicsEngine.cs
├── AnimatedCanvasView.shared.cs
├── PhysicsWorld.shared.cs            # REFACTORED (uses IPhysicsEngine)
├── Particle.shared.cs
├── ParticleEmitter.shared.cs
├── ... (other files unchanged)
```

## Benchmarking Plan

### Benchmark Scenarios

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class PhysicsEngineBenchmarks
{
    [Params(10, 50, 100, 200, 500, 1000)]
    public int ParticleCount { get; set; }
    
    [Params(PhysicsEngineType.Dumb, PhysicsEngineType.Aether)]
    public PhysicsEngineType EngineType { get; set; }
    
    [Benchmark]
    public void Step_WithCollisions() { ... }
    
    [Benchmark]
    public void Step_NoCollisions() { ... }
    
    [Benchmark]
    public void Step_WithAttractors() { ... }
}
```

### Performance Targets

| Engine | 100 Particles | 500 Particles | 1000 Particles |
|--------|--------------|---------------|----------------|
| Dumb | ~10 FPS | <5 FPS | Unusable |
| Aether | 60 FPS | 60 FPS | 40-50 FPS |
| Velcro | 60 FPS | 55-60 FPS | 40-50 FPS |
| Box2D | 60 FPS | 55-60 FPS | 40-50 FPS |

## Testing Strategy

### Unit Tests

1. **Interface Conformance**: Each engine implements IPhysicsEngine
2. **Particle Management**: Add/remove/clear operations
3. **Attractor Forces**: Inverse square law
4. **Sticky Zones**: Probabilistic capture
5. **Determinism**: Same seed = same results
6. **Edge Cases**: Zero particles, extreme forces, etc.

### Integration Tests

1. **Engine Swapping**: Switch engines mid-simulation
2. **API Compatibility**: Existing demos work with all engines
3. **Performance**: Each engine meets minimum FPS targets

## Timeline

### Phase 1: Interface & DumbPhysicsEngine (2-3 days) - **THIS PR**
- [ ] Create IPhysicsEngine interface
- [ ] Extract DumbPhysicsEngine
- [ ] Refactor PhysicsWorld
- [ ] Update tests
- [ ] Verify backwards compatibility

### Phase 2: Aether.Physics2D (3-4 days) - Next PR
- [ ] Add NuGet package
- [ ] Implement AetherPhysicsEngine
- [ ] Add tests
- [ ] Basic benchmarks

### Phase 3: Velcro & Box2D (2-3 days each) - Future PRs
- [ ] Implement adapters
- [ ] Add tests
- [ ] Add to benchmarks

### Phase 4: Comprehensive Benchmarking (2 days) - Final PR
- [ ] BenchmarkDotNet project
- [ ] Performance comparisons
- [ ] Documentation
- [ ] Performance visualization GIFs

**Total Estimate**: 10-16 days across multiple PRs

## Benefits

1. **Performance**: 10-50x improvement with production engines
2. **Flexibility**: Users choose engine for their needs
3. **Maintainability**: Less custom physics code to maintain
4. **Professional**: Industry-standard physics
5. **Extensibility**: Easy to add new engines
6. **Benchmarked**: Data-driven engine selection

## Risks & Mitigation

**Risk**: Breaking existing code
**Mitigation**: Maintain backwards compatibility, DumbPhysicsEngine as default

**Risk**: Complex integration
**Mitigation**: Incremental approach, one engine at a time

**Risk**: Performance regression
**Mitigation**: Comprehensive benchmarking, keep DumbPhysicsEngine

**Risk**: License issues
**Mitigation**: All engines are MIT licensed

## Decision Points

### Immediate Decisions Needed

1. **Engine Selection Mechanism**: Runtime vs compile-time?
   - **Recommendation**: Runtime (more flexible)

2. **Default Engine**: Which engine is default?
   - **Recommendation**: DumbPhysicsEngine (backwards compatible)

3. **Package Strategy**: Single vs multiple packages?
   - **Recommendation**: Multiple packages (Option B)

4. **API Breaking Changes**: Allow any?
   - **Recommendation**: No breaking changes in v1

### For This PR (Phase 1)

**Scope**: Interface + DumbPhysicsEngine + Refactoring
**Goal**: Foundation for pluggable engines
**Test**: All existing tests pass, no regressions

## Success Criteria

- [ ] IPhysicsEngine interface complete
- [ ] DumbPhysicsEngine extracted and working
- [ ] PhysicsWorld refactored to use IPhysicsEngine
- [ ] All existing tests pass
- [ ] Backwards compatibility maintained
- [ ] Documentation updated
- [ ] Ready for Phase 2 (Aether integration)

