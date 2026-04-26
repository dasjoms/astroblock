# Coordinate Conversion Authority

`Core.Coords` is the single authority for world/chunk/local coordinate math.

## Rule

- Any conversion between world coordinates and chunk/local coordinates **must** call `Core.Coords` APIs (`CoordConverter`, `ChunkConstants`, `ChunkCoord3`) instead of reimplementing math in other modules.
- Non-Core modules may compose these APIs, but must not duplicate modulo/floor-division chunk conversion logic.

## Why

- Keeps behavior consistent for negative coordinates and chunk boundaries.
- Prevents subtle drift across World/Gameplay/Rendering/Tools code.
- Preserves a stable contract so parallel module work can proceed safely.

## Enforcement

- `scripts/check-coords-authority.sh` runs a static guard that flags suspicious duplicate conversion patterns in non-Core source.
- CI executes the guard on every change.
