using Astroblock.Core.Coords;

namespace Astroblock.Core.Events;

/// <summary>
/// Raised when a chunk enters the active resident set.
/// </summary>
/// <param name="Coord">Canonical chunk coordinate of the loaded chunk.</param>
public sealed record ChunkLoaded(ChunkCoord3 Coord) : IDomainEvent;
