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
    /// Converts chunk-space + chunk-local coordinates to world-space coordinates.
    /// </summary>
    /// <param name="cx">Chunk X index.</param>
    /// <param name="cy">Chunk Y index.</param>
    /// <param name="cz">Chunk Z index.</param>
    /// <param name="lx">Local X coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <param name="ly">Local Y coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <param name="lz">Local Z coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <returns>The corresponding world-space coordinates.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when any local axis is outside <c>[0, ChunkSize - 1]</c>.
    /// </exception>
    /// <exception cref="System.OverflowException">
    /// Thrown when the resulting world coordinate does not fit in <see cref="int"/>.
    /// </exception>
    public static (int X, int Y, int Z) ChunkLocalToWorld(long cx, long cy, long cz, int lx, int ly, int lz)
        => (
            ChunkLocalToWorldAxis(cx, lx, nameof(lx)),
            ChunkLocalToWorldAxis(cy, ly, nameof(ly)),
            ChunkLocalToWorldAxis(cz, lz, nameof(lz)));

    /// <summary>
    /// Converts chunk-space + chunk-local coordinates to world-space coordinates.
    /// </summary>
    /// <param name="chunk">Chunk coordinates.</param>
    /// <param name="lx">Local X coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <param name="ly">Local Y coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <param name="lz">Local Z coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <returns>The corresponding world-space coordinates.</returns>
    public static (int X, int Y, int Z) ChunkLocalToWorld(ChunkCoord3 chunk, int lx, int ly, int lz)
        => ChunkLocalToWorld(chunk.X, chunk.Y, chunk.Z, lx, ly, lz);

    /// <summary>
    /// Performs a round-trip conversion from world-space to chunk/local and back to world-space.
    /// </summary>
    /// <param name="wx">World X block coordinate.</param>
    /// <param name="wy">World Y block coordinate.</param>
    /// <param name="wz">World Z block coordinate.</param>
    /// <returns>The world coordinates after round-trip conversion.</returns>
    public static (int X, int Y, int Z) RoundTripWorldToChunkLocalToWorld(int wx, int wy, int wz)
    {
        var chunk = WorldToChunk(wx, wy, wz);
        var local = WorldToLocal(wx, wy, wz);
        return ChunkLocalToWorld(chunk, local.X, local.Y, local.Z);
    }

    /// <summary>
    /// Performs a round-trip conversion from chunk/local to world and back to chunk/local.
    /// </summary>
    /// <param name="cx">Chunk X index.</param>
    /// <param name="cy">Chunk Y index.</param>
    /// <param name="cz">Chunk Z index.</param>
    /// <param name="lx">Local X coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <param name="ly">Local Y coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <param name="lz">Local Z coordinate in <c>[0, ChunkSize - 1]</c>.</param>
    /// <returns>The chunk and local coordinates after round-trip conversion.</returns>
    public static (ChunkCoord3 Chunk, (int X, int Y, int Z) Local) RoundTripChunkLocalToWorldToChunkLocal(
        long cx,
        long cy,
        long cz,
        int lx,
        int ly,
        int lz)
    {
        var world = ChunkLocalToWorld(cx, cy, cz, lx, ly, lz);
        var chunk = WorldToChunk(world.X, world.Y, world.Z);
        var local = WorldToLocal(world.X, world.Y, world.Z);
        return (chunk, local);
    }

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

    private static int ChunkLocalToWorldAxis(long chunkAxis, int localAxis, string axisName)
    {
        ValidateLocalAxis(localAxis, axisName);

        checked
        {
            var worldAxis = (chunkAxis * ChunkConstants.ChunkSize) + localAxis;
            return (int)worldAxis;
        }
    }

    private static void ValidateLocalAxis(int localAxis, string axisName)
    {
        if (localAxis < 0 || localAxis >= ChunkConstants.ChunkSize)
        {
            throw new System.ArgumentOutOfRangeException(
                axisName,
                localAxis,
                $"Local axis must be in range [0, {ChunkConstants.ChunkSize - 1}].");
        }
    }
}
