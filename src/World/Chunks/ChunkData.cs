using Astroblock.Core.Coords;

namespace Astroblock.World.Chunks;

/// <summary>
/// Authoritative per-chunk voxel payload for the v0 prototype storage layer.
/// Uses a fixed-size dense byte buffer with one byte per block.
/// </summary>
public sealed class ChunkData
{
    /// <summary>
    /// Total number of voxels in a single chunk.
    /// </summary>
    public const int BlockCount = ChunkConstants.ChunkSize * ChunkConstants.ChunkSize * ChunkConstants.ChunkSize;

    /// <summary>
    /// Initializes a chunk with a zero-filled voxel buffer.
    /// </summary>
    public ChunkData()
        : this(new byte[BlockCount])
    {
    }

    /// <summary>
    /// Initializes a chunk from an explicit voxel buffer.
    /// </summary>
    /// <param name="blocks">
    /// Fixed voxel buffer with length exactly
    /// <see cref="ChunkConstants.ChunkSize"/> * <see cref="ChunkConstants.ChunkSize"/> * <see cref="ChunkConstants.ChunkSize"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="blocks"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the block buffer length is invalid.</exception>
    public ChunkData(byte[] blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (blocks.Length != BlockCount)
        {
            throw new ArgumentException(
                $"Chunk block array length must be {BlockCount}, but was {blocks.Length}.",
                nameof(blocks));
        }

        Blocks = blocks;
    }

    /// <summary>
    /// Fixed-size dense voxel buffer in XYZ-major linearized order.
    /// </summary>
    public byte[] Blocks { get; }

    /// <summary>
    /// Reads a voxel value from local chunk coordinates.
    /// </summary>
    public byte GetBlock(int lx, int ly, int lz) => Blocks[ToLinearIndex(lx, ly, lz)];

    /// <summary>
    /// Writes a voxel value using local chunk coordinates.
    /// </summary>
    public void SetBlock(int lx, int ly, int lz, byte value) => Blocks[ToLinearIndex(lx, ly, lz)] = value;

    private static int ToLinearIndex(int lx, int ly, int lz)
    {
        ValidateLocalAxis(lx, nameof(lx));
        ValidateLocalAxis(ly, nameof(ly));
        ValidateLocalAxis(lz, nameof(lz));

        var chunkSize = ChunkConstants.ChunkSize;
        return lx + (ly * chunkSize) + (lz * chunkSize * chunkSize);
    }

    private static void ValidateLocalAxis(int axisValue, string axisName)
    {
        if (axisValue < 0 || axisValue >= ChunkConstants.ChunkSize)
        {
            throw new ArgumentOutOfRangeException(
                axisName,
                axisValue,
                $"Local axis must be in range [0, {ChunkConstants.ChunkSize - 1}].");
        }
    }
}
