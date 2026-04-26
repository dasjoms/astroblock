using Astroblock.Core.Coords;

namespace Astroblock.Core.Events;

/// <summary>
/// Raised when a loaded chunk's authoritative voxel contents change.
/// </summary>
/// <param name="Coord">Canonical chunk coordinate of the modified chunk.</param>
public sealed record ChunkModified(ChunkCoord3 Coord) : IDomainEvent;
