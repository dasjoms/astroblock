using Astroblock.Core.Coords;
using Astroblock.World.Chunks;

namespace Astroblock.Core.Interfaces;

/// <summary>
/// Authoritative voxel storage contract for the v0 prototype world state.
/// Implementations are the source of truth for chunk voxel data consumed by streaming,
/// generation, meshing, and gameplay systems.
/// </summary>
public interface IChunkStore
{
    /// <summary>
    /// Attempts to read chunk voxel data for a chunk coordinate.
    /// </summary>
    /// <param name="coord">Chunk coordinate key.</param>
    /// <param name="chunk">Resolved chunk voxel data when the coordinate exists.</param>
    /// <returns><see langword="true"/> when the chunk exists; otherwise <see langword="false"/>.</returns>
    bool TryGetChunk(ChunkCoord3 coord, out ChunkData chunk);

    /// <summary>
    /// Reads existing chunk voxel data or creates a new chunk entry if one does not exist.
    /// </summary>
    /// <param name="coord">Chunk coordinate key.</param>
    /// <returns>The existing or newly-created authoritative chunk voxel data.</returns>
    ChunkData GetOrCreateChunk(ChunkCoord3 coord);

    /// <summary>
    /// Stores chunk voxel data at the provided chunk coordinate.
    /// </summary>
    /// <param name="coord">Chunk coordinate key.</param>
    /// <param name="chunk">Chunk voxel data to store as authoritative state.</param>
    void SetChunk(ChunkCoord3 coord, ChunkData chunk);

    /// <summary>
    /// Removes chunk voxel data at the provided chunk coordinate.
    /// </summary>
    /// <param name="coord">Chunk coordinate key.</param>
    /// <returns><see langword="true"/> when an existing chunk was removed; otherwise <see langword="false"/>.</returns>
    bool RemoveChunk(ChunkCoord3 coord);
}
