using Astroblock.Core.Coords;

namespace Astroblock.World.Streaming;

/// <summary>
/// v0 chunk streaming policy contract.
///
/// Implementations are intentionally pure, side-effect free calculators that map
/// an anchor + radius to the chunk set that should be resident.
/// Future policies (LOD, frustum culling, prediction) can replace internals
/// without changing callers that depend on this interface.
/// </summary>
public interface IChunkStreamer
{
    /// <summary>
    /// Computes the desired loaded chunk set for the provided world anchor.
    /// </summary>
    /// <param name="anchor">Anchor expressed in world block coordinates.</param>
    /// <param name="radius">Radius in chunks from the anchor chunk on each axis.</param>
    /// <returns>Deterministic set of chunk coordinates to keep loaded.</returns>
    IReadOnlyCollection<ChunkCoord3> ComputeDesiredSet(WorldAnchor anchor, int radius);
}
