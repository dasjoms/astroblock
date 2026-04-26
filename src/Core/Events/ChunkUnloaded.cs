using Astroblock.Core.Coords;

namespace Astroblock.Core.Events;

/// <summary>
/// Raised when a chunk leaves the active resident set.
/// </summary>
/// <param name="Coord">Canonical chunk coordinate of the unloaded chunk.</param>
public sealed record ChunkUnloaded(ChunkCoord3 Coord) : IDomainEvent;
