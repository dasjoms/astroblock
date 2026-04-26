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
