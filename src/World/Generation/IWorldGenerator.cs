using Astroblock.Core.Coords;
using Astroblock.World.Chunks;

namespace Astroblock.World.Generation;

/// <summary>
/// Pure, deterministic world generation contract.
/// Given the same <paramref name="coord"/> and <paramref name="seed"/>, implementations must
/// always produce byte-for-byte identical <see cref="ChunkData"/> output.
/// </summary>
public interface IWorldGenerator
{
    /// <summary>
    /// Generates an authoritative chunk payload for the provided coordinate and world seed.
    /// </summary>
    /// <param name="coord">Chunk coordinate to generate.</param>
    /// <param name="seed">World seed that defines deterministic generation.</param>
    /// <returns>Generated chunk voxel data.</returns>
    ChunkData Generate(ChunkCoord3 coord, int seed);
}
