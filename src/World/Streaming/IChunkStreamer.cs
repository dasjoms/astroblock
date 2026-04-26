using Astroblock.Core.Coords;

namespace Astroblock.World.Streaming;

/// <summary>
/// Computes desired chunk load sets around one or more world anchors.
/// </summary>
public interface IChunkStreamer
{
    /// <summary>
    /// Produces a deterministic set of chunk coordinates that should be loaded for the provided anchor.
    /// </summary>
    /// <param name="anchorWorldX">Anchor world X coordinate in block units.</param>
    /// <param name="anchorWorldY">Anchor world Y coordinate in block units.</param>
    /// <param name="anchorWorldZ">Anchor world Z coordinate in block units.</param>
    /// <param name="radiusInChunks">Streaming radius in chunks.</param>
    /// <returns>Chunk coordinates to keep loaded.</returns>
    IReadOnlyCollection<ChunkCoord3> ComputeDesiredLoadSet(
        long anchorWorldX,
        long anchorWorldY,
        long anchorWorldZ,
        int radiusInChunks);
}
