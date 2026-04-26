using Astroblock.Core.Coords;

namespace Astroblock.World.Streaming;

/// <summary>
/// Immutable anchor in world block space used as input to streaming policies.
/// </summary>
public readonly struct WorldAnchor
{
    public WorldAnchor(int worldX, int worldY, int worldZ)
    {
        WorldX = worldX;
        WorldY = worldY;
        WorldZ = worldZ;
    }

    public int WorldX { get; }

    public int WorldY { get; }

    public int WorldZ { get; }

    /// <summary>
    /// Converts this world-space anchor to the containing chunk coordinate.
    /// </summary>
    public ChunkCoord3 ToChunkCoord() => CoordConverter.WorldToChunk(WorldX, WorldY, WorldZ);
}
