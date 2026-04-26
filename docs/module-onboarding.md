# Module Onboarding

This checklist is for engineers adding new modules under `src/`.

## Boundary checklist

1. Depend on shared contracts from `Core` and avoid cross-module implementation leakage.
2. For world/chunk/local coordinate conversion, use `Core.Coords` only. See `docs/coords-authority.md`.
3. Run boundary checks before pushing:
   - `scripts/check-coords-authority.sh`
   - `dotnet test`

## Coordinate authority note

`Core.Coords` owns conversion math as a locked contract. Reimplementing `% ChunkSize`, floor-div chunk math, or other ad-hoc conversion logic in non-Core modules is prohibited and is enforced by CI.

## Chunk lifecycle ordering (streaming)

When reconciling chunk residency, world orchestration must emit lifecycle events in this order for each update tick:

1. Emit `ChunkUnloaded` for chunks that left the desired set.
2. Emit `ChunkLoaded` for chunks that entered the desired set.
3. Emit `ChunkModified` later when block data changes within loaded chunks.

All lifecycle payloads must remain engine-agnostic and use `ChunkCoord3` only.
