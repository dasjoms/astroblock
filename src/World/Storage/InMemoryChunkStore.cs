using Astroblock.Core.Coords;
using Astroblock.Core.Interfaces;
using Astroblock.World.Chunks;

namespace Astroblock.World.Storage;

/// <summary>
/// In-memory authoritative voxel storage for the v0 prototype.
/// Backed by a sparse dictionary keyed by <see cref="ChunkCoord3"/>.
/// </summary>
public sealed class InMemoryChunkStore : IChunkStore
{
    private readonly Dictionary<ChunkCoord3, ChunkData> chunks = [];

    /// <inheritdoc/>
    public bool TryGetChunk(ChunkCoord3 coord, out ChunkData chunk)
        => chunks.TryGetValue(coord, out chunk!);

    /// <inheritdoc/>
    public ChunkData GetOrCreateChunk(ChunkCoord3 coord)
    {
        if (!chunks.TryGetValue(coord, out var chunk))
        {
            chunk = new ChunkData();
            chunks[coord] = chunk;
        }

        return chunk;
    }

    /// <inheritdoc/>
    public void SetChunk(ChunkCoord3 coord, ChunkData chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        chunks[coord] = chunk;
    }

    /// <inheritdoc/>
    public bool RemoveChunk(ChunkCoord3 coord) => chunks.Remove(coord);
}
