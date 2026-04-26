namespace Astroblock.Core.Coords;

/// <summary>
/// Canonical chunk-address key for chunk stores, dictionaries, and event payloads.
/// Use this value type whenever a chunk identity must be persisted, compared, or routed.
/// </summary>
public readonly struct ChunkCoord3 : System.IEquatable<ChunkCoord3>
{
    /// <summary>
    /// Initializes a chunk coordinate from integral axis values.
    /// </summary>
    /// <param name="x">Chunk X axis index.</param>
    /// <param name="y">Chunk Y axis index.</param>
    /// <param name="z">Chunk Z axis index.</param>
    public ChunkCoord3(long x, long y, long z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Chunk X axis index.
    /// </summary>
    public long X { get; }

    /// <summary>
    /// Chunk Y axis index.
    /// </summary>
    public long Y { get; }

    /// <summary>
    /// Chunk Z axis index.
    /// </summary>
    public long Z { get; }

    /// <summary>
    /// Creates a chunk coordinate from integral axis values.
    /// This factory intentionally accepts only integral inputs to avoid hidden float conversions.
    /// </summary>
    /// <param name="x">Chunk X axis index.</param>
    /// <param name="y">Chunk Y axis index.</param>
    /// <param name="z">Chunk Z axis index.</param>
    /// <returns>A canonical <see cref="ChunkCoord3"/> value.</returns>
    public static ChunkCoord3 FromLongs(long x, long y, long z) => new(x, y, z);

    /// <summary>
    /// Creates a chunk coordinate from 32-bit integral axis values.
    /// </summary>
    /// <param name="x">Chunk X axis index.</param>
    /// <param name="y">Chunk Y axis index.</param>
    /// <param name="z">Chunk Z axis index.</param>
    /// <returns>A canonical <see cref="ChunkCoord3"/> value.</returns>
    public static ChunkCoord3 FromInts(int x, int y, int z) => new(x, y, z);

    /// <inheritdoc/>
    public bool Equals(ChunkCoord3 other) => X == other.X && Y == other.Y && Z == other.Z;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ChunkCoord3 other && Equals(other);

    /// <summary>
    /// Returns a deterministic hash code based solely on coordinate values.
    /// </summary>
    /// <returns>A stable hash code for dictionary/set usage.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            const int seed = (int)2166136261;
            const int prime = 16777619;

            var hash = seed;
            hash = (hash ^ X.GetHashCode()) * prime;
            hash = (hash ^ Y.GetHashCode()) * prime;
            hash = (hash ^ Z.GetHashCode()) * prime;
            return hash;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"({X},{Y},{Z})";

    public static bool operator ==(ChunkCoord3 left, ChunkCoord3 right) => left.Equals(right);

    public static bool operator !=(ChunkCoord3 left, ChunkCoord3 right) => !left.Equals(right);
}
