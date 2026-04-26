# Astroblock

## Current implemented modules

| Module | Status | Notes |
| --- | --- | --- |
| `Core/Coords` | Implemented | Chunk constants, world↔chunk↔local conversion utilities, and `ChunkCoord3` key type are in active use. |
| `Core/Interfaces` | Implemented | `IChunkStore` defines authoritative voxel storage access. |
| `Core/Events` | Scaffold | Marker `IDomainEvent` added for future event payloads; no runtime event bus yet. |
| `World/Chunks` | Implemented | `ChunkData` contains chunk voxel payload storage. |
| `World/Storage` | Implemented | `InMemoryChunkStore` provides sparse dictionary-backed authoritative state. |
| `World/Generation` | Implemented | `IWorldGenerator` and simple deterministic noise generator are implemented with tests. |
| `World/Streaming` | Scaffold | `IChunkStreamer` contract is present; no streaming implementation yet. |


## Quick coordinate sanity demo

For quick manual validation of world/chunk/local coordinate conversions, run:

```bash
dotnet run --project src/Tools/Debug/CoordSanityDemo/CoordSanityDemo.csproj
```

The demo prints each world input, derived chunk/local values, reconstructed world output, and a PASS/FAIL marker.

## Module onboarding and coordinate boundary discipline

- Onboarding checklist: `docs/module-onboarding.md`
- Coordinate conversion authority rule: `docs/coords-authority.md`
- Static guard script: `scripts/check-coords-authority.sh`
