namespace Astroblock.Core.Coords;

/// <summary>
/// Defines canonical chunk sizing primitives for Astroblock coordinate math.
/// This is the only legal place for chunk-size and coordinate conversion primitives.
/// </summary>
public static class ChunkConstants
{
    /// <summary>
    /// Number of blocks per chunk axis.
    /// </summary>
    public const int ChunkSize = 32;

    /// <summary>
    /// Base-2 logarithm of <see cref="ChunkSize"/>. Valid only when chunk size is a power of two.
    /// </summary>
    public const int ChunkSizeLog2 = 5;

    /// <summary>
    /// Bit mask for wrapping world block coordinates into chunk-local coordinates.
    /// Equals <c>ChunkSize - 1</c>.
    /// </summary>
    public const int ChunkMask = ChunkSize - 1;

    static ChunkConstants()
    {
        Validate();
    }

    /// <summary>
    /// Validates chunk sizing assumptions used by conversion code.
    /// Throws immediately if values are changed to an invalid state.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when <see cref="ChunkSize"/> is not a positive power of two,
    /// when <see cref="ChunkMask"/> is inconsistent, or when <see cref="ChunkSizeLog2"/> is inconsistent.
    /// </exception>
    public static void Validate()
    {
        if (!IsPowerOfTwo(ChunkSize))
        {
            throw new System.InvalidOperationException(
                $"ChunkSize must be a positive power of two, but was {ChunkSize}.");
        }

        if (ChunkMask != ChunkSize - 1)
        {
            throw new System.InvalidOperationException(
                $"ChunkMask must equal ChunkSize - 1. Expected {ChunkSize - 1}, actual {ChunkMask}.");
        }

        var expectedLog2 = CalculateLog2(ChunkSize);
        if (ChunkSizeLog2 != expectedLog2)
        {
            throw new System.InvalidOperationException(
                $"ChunkSizeLog2 must match log2(ChunkSize). Expected {expectedLog2}, actual {ChunkSizeLog2}.");
        }
    }

    private static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;

    private static int CalculateLog2(int value)
    {
        var result = 0;
        while ((value >>= 1) != 0)
        {
            result++;
        }

        return result;
    }
}
