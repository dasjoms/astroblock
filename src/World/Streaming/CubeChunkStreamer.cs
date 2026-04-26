using Astroblock.Core.Coords;

namespace Astroblock.World.Streaming;

/// <summary>
/// v0 streaming policy that requests a full cube of chunks centered on the anchor chunk.
///
/// This class is pure and side-effect free: it only performs coordinate calculation.
/// Future policies may add LOD/frustum-aware behavior while keeping <see cref="IChunkStreamer"/>
/// unchanged for callers.
/// </summary>
public sealed class CubeChunkStreamer : IChunkStreamer
{
    public IReadOnlyCollection<ChunkCoord3> ComputeDesiredSet(WorldAnchor anchor, int radius)
    {
        if (radius < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius must be non-negative.");
        }

        var center = anchor.ToChunkCoord();
        var desired = new HashSet<ChunkCoord3>();

        for (var x = center.X - radius; x <= center.X + radius; x++)
        {
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                for (var z = center.Z - radius; z <= center.Z + radius; z++)
                {
                    desired.Add(ChunkCoord3.FromLongs(x, y, z));
                }
            }
        }

        return desired;
    }
}
