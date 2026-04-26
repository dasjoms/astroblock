# Astroblock Project Blueprint

## 1. Vision

Astroblock is a voxel sandbox game inspired by the freedom of Minecraft, but designed for **true 3D infinite space** and **local gravity fields** that can vary from strong planetary pull to near-zero gravity.

Primary design goals:

1. **Infinite expansion in all directions** (X/Y/Z), not just horizontal expansion with vertical slices.
2. **Cube chunk world model** (`chunkX`, `chunkY`, `chunkZ`) with deterministic generation.
3. **Gravity as a local vector field**, not a global down axis.
4. **Modular architecture** where gameplay systems are decoupled and replaceable.
5. **Performance-first data model** that scales to very large coordinates and sparse worlds.

---

## 2. Core Technical Direction

- **Engine target**: C#-based engine (Godot 4 C# or Unity C#).
- **Language**: C#.
- **World representation**: Sparse 3D voxel chunks.
- **Generation**: Deterministic by world seed + chunk coordinate.
- **Physics baseline**: Custom character motor that supports arbitrary "up" vectors.
- **Gravity baseline**: Query-based solver combining nearby detailed sources + far aggregated proxies.

> Important: Architectural choices documented here are engine-agnostic where possible. Engine-specific adapters should be thin wrappers over project-defined interfaces.

---

## 3. Non-Negotiable Architectural Principles

1. **No direct cross-feature coupling**
   - Systems communicate through interfaces, events, and small shared contracts.
2. **Data authority is explicit**
   - World voxel state lives in world storage, not in renderer/physics mirrors.
3. **Determinism for procedural systems**
   - Same seed + same coordinates must produce same generated result.
4. **Asynchronous heavy work**
   - Generation, meshing, and some analysis jobs run off main thread.
5. **Swap-friendly modules**
   - Individual implementations can be replaced without changing callers.

---

## 4. Repository Structure (Target)

```text
/src
  /Game
    /Bootstrap
    /Config
    /Composition

  /Core
    /Math
    /Coords
    /Events
    /Utilities
    /Interfaces

  /World
    /Blocks
    /Chunks
    /Storage
    /Streaming
    /Generation
    /Meshing
    /Persistence

  /Gravity
    /Sources
    /Aggregation
    /Field
    /Queries

  /Physics
    /Character
    /Collision
    /Forces

  /Entities
    /Player
    /NPC
    /Vehicles

  /Gameplay
    /Building
    /Interaction
    /Inventory

  /Rendering
    /ChunkRendering
    /LOD
    /VFX

  /Tools
    /Debug
    /Profiling

/tests
  /Unit
  /Integration
/docs
  /adr
```

### Responsibilities by major module

- **Core**: Pure utilities, numeric helpers, coordinate conversions, event primitives, and core interfaces.
- **World**: Voxel data, chunk storage, generation, streaming decisions, persistence, meshing requests.
- **Gravity**: Gravity source registration, aggregation structures, gravity query API.
- **Physics**: Character and dynamic body motion under arbitrary gravity vectors.
- **Entities**: Game actors and state; entities consume services rather than implementing world logic.
- **Gameplay**: Block interaction, crafting/inventory (future), game rules.
- **Rendering**: Render-side representations and LOD rules, no world data authority.
- **Tools**: Debug visualizations, profiling hooks, diagnostics.

---

## 5. World Coordinate Model

### 5.1 Chunk Coordinates

- Use **3D chunk indexing**: `(cx, cy, cz)` as 64-bit integers.
- Recommended chunk size at start: `32 x 32 x 32` blocks.

### 5.2 Local Block Coordinates

- Per-chunk local block index: `(bx, by, bz)` in `[0, chunkSize-1]`.
- Conversion functions must be centralized in one coordinate utility module.

### 5.3 Precision Strategy

- World data coordinates remain integer-based.
- For transforms/physics floats, use floating-origin or origin rebasing for extreme distances.

---

## 6. Gravity System Design

### 6.1 Core Concept

Gravity is a sampled vector field:

- `g(p) = sum of all relevant source contributions at world position p`
- Player up direction is `-normalize(g)` when `|g| > epsilon`

### 6.2 Source Types

1. **Detailed local sources**
   - Derived from loaded chunk data near the player.
   - Can include connected voxel clusters or authored bodies.
2. **Aggregated far-field proxies**
   - Stored in a sparse hierarchical structure (e.g., octree-like nodes).
   - Each node stores total mass + center of mass + bounds.

### 6.3 Why aggregation is required

The universe cannot be fully loaded. Far influence must be represented by compact metadata. Gravity queries should avoid loading distant chunks.

### 6.4 Query algorithm (high-level)

1. Gather nearby detailed sources (loaded zone).
2. Gather far aggregated nodes from gravity index.
3. Apply approximation threshold (`size / distance < theta`) for grouped nodes.
4. Sum all accepted contributions to produce net gravity vector.

### 6.5 Transition stability rules

- Use source switching hysteresis.
- Smooth orientation changes with damping/slerp.
- Define zero-g threshold (`epsilon`) for free-float mode.

---

## 7. Data Ownership and Boundaries

- **World.Storage** owns voxel truth.
- **World.Meshing** consumes read-only snapshots and outputs render/collision mesh data.
- **Gravity.Aggregation** owns far-field metadata.
- **Physics.Character** consumes gravity query results and collision data.
- **Rendering** consumes mesh outputs and debug overlays.

No module should mutate another module's private state directly.

---

## 8. Event and Service Interaction Rules

### 8.1 Preferred communication

- Query-style interfaces for pull data (`GetChunk`, `SampleGravity`, etc.).
- Event bus for state changes (`ChunkLoaded`, `ChunkUnloaded`, `ChunkModified`).

### 8.2 Avoid

- Cyclic dependencies between modules.
- Renderer-driven world mutation.
- Physics systems assuming global Y-down gravity.

---

## 9. Initial Interface Contracts (Draft)

These names are suggestions and can evolve, but the separation of responsibilities should remain.

- `IChunkStore`
  - Get/set block values by world or chunk-local coordinates.
  - Return chunk snapshots for meshing/queries.
- `IChunkStreamer`
  - Compute desired load set from anchors (player/cameras).
- `IWorldGenerator`
  - Deterministically generate chunk data from seed + chunk coordinate.
- `IGravitySourceProvider`
  - Return detailed local candidates and far-field aggregated candidates.
- `IGravityFieldSolver`
  - Compute net gravity at position with quality options.
- `ICharacterMotor`
  - Simulate movement under arbitrary up vector and contacts.

---

## 10. Persistence Strategy (Early)

1. Persist only modified chunks (delta from procedural baseline).
2. Persist gravity aggregation metadata required for fast far-field queries.
3. Persist player/entity state in world-space coordinates.
4. Version save format and include migration hooks from day one.

---

## 11. Performance Strategy (Early)

- Use bounded job queues for generation/meshing.
- Prioritize chunk jobs near player and view frustum.
- Cache gravity query intermediates for short time windows.
- Use object pools for temporary arrays/buffers.
- Instrument frame timings and queue depths from first prototype.

---

## 12. Testing Strategy

### Unit tests

- Coordinate conversion edge cases (negative coordinates, boundaries).
- Deterministic generation verification.
- Gravity vector composition correctness for known source setups.

### Integration tests

- Stream in/out around moving player anchor.
- Gravity transitions between two nearby masses.
- Save/load of modified chunks and gravity metadata.

### Soak/perf tests

- Long-duration streaming traversal in random directions.
- Stress with many small gravity-emitting clusters.

---

## 13. Development Workflow Expectations for Future Agents

1. Respect module boundaries in this document.
2. Add/modify interfaces before adding deep cross-module logic.
3. Update this `PROJECT.md` when architecture decisions materially change.
4. Add ADR entries in `/docs/adr` for major tradeoffs.
5. Prefer incremental vertical slices over broad unfinished scaffolding.

---

## 14. First Course of Action (Absolute First Task)

### Task 1: Build the coordinate and chunk indexing foundation

This is the first task because every other system (generation, streaming, gravity, persistence, physics) depends on unambiguous world/chunk math.

#### Deliverables

1. Define chunk constants (`ChunkSize`, derived masks/shifts if power-of-two).
2. Implement world↔chunk↔local coordinate conversion utilities.
3. Implement core chunk key type (`ChunkCoord3`) with hashing/equality.
4. Add unit tests for all edge cases (especially negatives and boundaries).
5. Add a tiny executable sanity demo/log that converts sample coordinates and round-trips them.

#### Why this is first

- Prevents subtle coordinate bugs that cascade through every subsystem.
- Enables parallel work (generation, storage, streaming) immediately after.
- Creates stable contracts for all future modules.

#### Definition of done

- All conversion tests pass.
- Coordinate utilities are engine-agnostic and located in `/src/Core/Coords`.
- No module outside Core reimplements coordinate math.

---

## 15. Immediate Next Steps After Task 1

1. Minimal `IChunkStore` with in-memory sparse dictionary.
2. Deterministic empty-space + simple asteroid generator.
3. Streaming ring/cube around player anchor.
4. Basic chunk lifecycle events.

---

## 16. Open Questions (Track in ADRs)

- Final chunk size (`16^3` vs `32^3` vs hybrid by LOD).
- Gravity mass model per block type (uniform vs material density).
- Preferred aggregation structure for far-field gravity (sparse octree vs hashed multi-resolution grid).
- Floating-origin cadence and multiplayer implications.

