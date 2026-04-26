# Deterministic World Generation (Prototype)

`SimpleNoiseWorldGenerator` is the first prototype implementation of `IWorldGenerator`.

## Deterministic guarantees

For a fixed tuple of:
- world seed (`int`),
- chunk coordinate (`ChunkCoord3`), and
- generator config (`SimpleNoiseWorldGeneratorConfig`),

the generated `ChunkData` is byte-for-byte identical on repeated runs.

### Why this is deterministic

- The generator is pure C# and has no engine API dependencies.
- No randomness source (`Random`, time, frame counters, static mutable state) is used.
- All branching and voxel writes are driven by stable hash/noise functions fed only by `(seed, coord, local/world voxel coordinates, config)`.

## Current content profile

- Space is intentionally **mostly empty** (`ChunkSpawnThreshold` controls chunk occupancy).
- A subset of chunks generate a small noise-perturbed asteroid prototype.
- Tunables are centralized in `SimpleNoiseWorldGeneratorConfig` (no hidden magic constants).
