namespace Astroblock.Core.Coords;

/// <summary>
/// Converts world-space block coordinates to chunk-space and chunk-local coordinates.
/// </summary>
public static class CoordConverter
{
    /// <summary>
    /// Converts a world-space block coordinate triplet to chunk coordinates.
    /// </summary>
    /// <param name="wx">World X block coordinate.</param>
    /// <param name="wy">World Y block coordinate.</param>
    /// <param name="wz">World Z block coordinate.</param>
    /// <returns>The chunk coordinate containing the world-space block.</returns>
    public static ChunkCoord3 WorldToChunk(int wx, int wy, int wz)
        => ChunkCoord3.FromInts(WorldToChunk(wx), WorldToChunk(wy), WorldToChunk(wz));

    /// <summary>
    /// Converts a world-space block coordinate triplet to chunk-local coordinates.
    /// </summary>
    /// <param name="wx">World X block coordinate.</param>
    /// <param name="wy">World Y block coordinate.</param>
    /// <param name="wz">World Z block coordinate.</param>
    /// <returns>
    /// Local coordinates constrained to <c>[0, ChunkSize - 1]</c> on each axis.
    /// </returns>
    public static (int X, int Y, int Z) WorldToLocal(int wx, int wy, int wz)
        => (WorldToLocal(wx), WorldToLocal(wy), WorldToLocal(wz));

    /// <summary>
    /// Converts a single world-space axis value into a chunk axis value using floor-division semantics.
    /// </summary>
    /// <remarks>
    /// Boundary examples with <see cref="ChunkConstants.ChunkSize"/> = 32:
    /// -1 -> chunk -1, local 31
    /// -32 -> chunk -1, local 0
    /// 32 -> chunk 1, local 0
    /// </remarks>
    /// <param name="worldAxis">A world-space block coordinate on one axis.</param>
    /// <returns>The containing chunk index on the same axis.</returns>
    public static int WorldToChunk(int worldAxis)
    {
        var chunkSize = ChunkConstants.ChunkSize;
        var quotient = worldAxis / chunkSize;
        var remainder = worldAxis % chunkSize;

        if (remainder < 0)
        {
            quotient--;
        }

        return quotient;
    }

    /// <summary>
    /// Converts a single world-space axis value into a local-in-chunk axis value.
    /// </summary>
    /// <param name="worldAxis">A world-space block coordinate on one axis.</param>
    /// <returns>A local axis value constrained to <c>[0, ChunkSize - 1]</c>.</returns>
    public static int WorldToLocal(int worldAxis)
    {
        var chunkSize = ChunkConstants.ChunkSize;
        var local = worldAxis % chunkSize;

        if (local < 0)
        {
            local += chunkSize;
        }

        return local;
    }
}
